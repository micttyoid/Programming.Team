﻿using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.AI.Core
{
    public interface IResumeEnricher
    {
        Task EnrichResume(Resume resume, Posting posting, CancellationToken token = default);
    }
    public interface IChatGPT
    {
        Task<string?> GetRepsonse(string systemPrompt, string userPrompt, int maxTokens = 22, CancellationToken token = default);
    }
}
