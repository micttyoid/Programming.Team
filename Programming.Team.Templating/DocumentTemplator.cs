using Microsoft.Extensions.Logging;
using Programming.Team.Core;
using Programming.Team.Templating.Core;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Templating
{
    public class DocumentTemplator : IDocumentTemplator
    {
        protected ILogger Logger { get; }
        public DocumentTemplator(ILogger<DocumentTemplator> logger)
        {
            Logger = logger;
        }
        public async Task<string> ApplyTemplate(string template, Resume resume, CancellationToken token = default)
        {
            try
            {
                var templator = Template.Parse(template);
                return await templator.RenderAsync(templator);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<byte[]> RenderLatex(string latex, CancellationToken token = default)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new MultipartFormDataContent
                    {
                        { new StringContent(latex), "input" }
                    };

                    HttpResponseMessage response = await client.PostAsync("https://latexonline.cc/compile", content);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsByteArrayAsync();
                }
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
