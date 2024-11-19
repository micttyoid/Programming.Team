using Microsoft.Extensions.Logging;
using Programming.Team.AI.Core;
using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task EnrichResume(Resume resume, Posting posting, CancellationToken token = default)
        {
            try
            {
                var config = !string.IsNullOrWhiteSpace(posting.Configuration) ? 
                    JsonSerializer.Deserialize<ResumeConfiguration>(posting.Configuration) ?? throw new InvalidDataException() : new ResumeConfiguration();
                string? postingSkills = await ChatGPT.GetRepsonse("extract skills in json format {\\\"skill\\\":\\\"name\\\"}",
                    posting.Details, token: token);
                if(postingSkills != null)
                {
                    postingSkills = postingSkills.Replace("```json", "").Replace("```", "");
                    var skills = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(postingSkills);
                    if (skills != null && config.HideSkillsNotInJD)
                    {
                        HashSet<SkillRollup> globalSkills = new HashSet<SkillRollup>();
                        foreach(var skill in skills)
                        {
                            foreach(var skillRollup in resume.Skills.Where(s => string.Equals(s.Skill.Name, skill["skill"], StringComparison.OrdinalIgnoreCase)))
                            {
                                globalSkills.Add(skillRollup);
                            }

                        }
                        resume.Skills = globalSkills.ToList();
                    }
                }
                

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
