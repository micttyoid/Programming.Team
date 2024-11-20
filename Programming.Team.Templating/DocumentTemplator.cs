﻿using Microsoft.Extensions.Logging;
using Programming.Team.Core;
using Programming.Team.Templating.Core;
using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static System.Net.WebRequestMethods;

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
                var context = new ScriptObject();
                context.Import(resume);
                var scriptContext = new TemplateContext { MemberRenamer = member => member.Name };
                scriptContext.PushGlobal(context);

                return await templator.RenderAsync(scriptContext);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public Task<byte[]> RenderLatex(string latex, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
