using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;
using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Programming.Team.AI
{
    public class ResumeEnricher : IResumeEnricher
    {
        protected ILogger Logger { get; }
        protected IChatGPT ChatGPT { get; }
        public ResumeEnricher(ILogger<ResumeEnricher> logger, IChatGPT chatGPT) 
        {
            Logger = logger;
            ChatGPT = chatGPT;
        }
        public async Task EnrichResume(Resume resume, Posting posting, IProgress<string>? progress = null, CancellationToken token = default)
        {
            try
            {
                var config = !string.IsNullOrWhiteSpace(posting.Configuration) ? 
                    JsonSerializer.Deserialize<ResumeConfiguration>(posting.Configuration) ?? throw new InvalidDataException() : new ResumeConfiguration();
                
                if (config.HideSkillsNotInJD)
                {
                    progress?.Report("Filtering Skills");
                    string? postingSkills = await ChatGPT.GetRepsonse("extract as many skill keywords as possible from user message in json dictionary format [{\"skill\":\"name\"}]",
                        posting.Details, token: token);
                    if (postingSkills != null)
                    {
                        postingSkills = postingSkills.Replace("```json", "").Replace("```", "").Trim().ReplaceLineEndings();
                        try
                        {
                            var skills = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(postingSkills);
                            if (skills != null)
                            {
                                HashSet<SkillRollup> globalSkills = new HashSet<SkillRollup>();
                                foreach (var skill in skills)
                                {
                                    foreach (var skillRollup in resume.Skills.Where(s => string.Equals(s.Skill.Name.Replace("\\", ""), skill["skill"], StringComparison.OrdinalIgnoreCase)))
                                    {
                                        
                                        globalSkills.Add(skillRollup);
                                    }

                                }
                                resume.Skills = globalSkills.OrderByDescending(e => e.YearsOfExperience).ToList();
                            }
                        }
                        catch { }
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(resume.User.Bio))
                {
                    progress?.Report("Tailoring Bio");
                    resume.User.Bio = await ChatGPT.GetRepsonse($"Output a LaTex snippet that will be added to an existing latex document - do not generate opening or closing article, document or sections. The user message is a biography: tailor/summarize it highlighting how it pertains the following job description, write three paragraphs and 6 bullet points - stick to what you know, don't make things up:  {JsonSerializer.Serialize(posting.Details)}", JsonSerializer.Serialize(resume.User.Bio), token: token);
                    resume.User.Bio = resume.User.Bio?.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                }
                foreach (var skill in resume.Skills.Select(p => p.Skill).Union(resume.Positions.SelectMany(p => p.PositionSkills.Select(p => p.Skill))))
                {
                    skill.Name = skill.Name.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                }
                foreach(var rec in resume.Reccomendations)
                {
                    rec.Body = rec.Body.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                    rec.Name = rec.Name.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                }
                foreach(var edu in resume.Educations)
                {
                    edu.Description = edu.Description?.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                }
                foreach(var pub in resume.Publications)
                {
                    pub.Title = pub.Title.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                    pub.Description = pub.Description?.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                }
                int count = resume.Positions.Count;
                int numberProcessed = 0;
                await Parallel.ForEachAsync(resume.Positions, token, async (position, t) =>
                {
                    if (!string.IsNullOrWhiteSpace(position.Description))
                    {
                        var match = await ChatGPT.GetRepsonse($"Indicate a percent match, only responding with a single value in \\\"%\\\", for the user message to the following job description: {JsonSerializer.Serialize(posting.Details)}", JsonSerializer.Serialize(position.Description), token: t);
                        if (match != null)
                        {
                            double mtch = double.Parse(match.Replace("%", "")) / 100;
                            if (mtch >= (config.MatchThreshold ?? 0.45))
                            {
                                double length = mtch * 10 * (config.TargetLengthPer10Percent ?? 200);
                                double bullets = mtch * 5;
                                position.Description = await ChatGPT.GetRepsonse($"Output a LaTex snippet, with proper escaping, that will be added to an existing latex document - do not generate opening or closing article, document sections or headers. Tailor user message - which is a description of a job experience, resulting in a total text length of no more than {length} characters, to the following job requirement sticking to the facts included in the user message, do not be creative IF A TECHNOLOGY IS NOT MENTIONED IN THE USER MESSAGE DO NOT INCLUDE IT IN THE SUMMARY!!!! include a short paragraph and {Math.Round(bullets)} bullet points: {JsonSerializer.Serialize(posting.Details)}", JsonSerializer.Serialize(position.Description), token: t);
                                position.Description = position.Description?.Replace("#", "\\#").Replace("$", "\\$").Replace("&", "\\&").Replace("%", "\\%");
                            }
                            else
                                position.Description = "";
                        }
                    }
                    int currentCount = Interlocked.Increment(ref numberProcessed);
                    progress?.Report($"{currentCount}/{count} Positions Processed");
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
