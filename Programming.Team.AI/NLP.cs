using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using java.io;
using Microsoft.Extensions.Logging;
using opennlp.tools.ml.maxent;
using opennlp.tools.sentdetect;
using Programming.Team.AI.Core;

namespace Programming.Team.AI
{
    public class NLP : INLP
    {
        protected ILogger Logger { get; }
        protected SentenceDetectorME SentanceDetector { get; }
        public NLP(ILogger<NLP> logger, SentenceDetectorME sentanceDetector)
        {
            Logger = logger;
            SentanceDetector = sentanceDetector;
        }
        public Task<string[]> IdentifyParagraphs(string text, CancellationToken token = default)
        {
            try
            {
                text = text.Replace("\r\n", "\n").Replace("\r", "\n");

                // Load OpenNLP sentence model (ensure en-sent.bin is in the same directory)
                

                // Split into sentences
                string[] sentences = SentanceDetector.sentDetect(text);

                // Reconstruct paragraphs using double newlines as indicators
                return Task.FromResult(Regex.Split(text, @"\n\s*\n"));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
