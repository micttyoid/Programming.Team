using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public interface IResumeConfiguration
    {
        double? MatchThreshold { get; set; }
        int? TargetLengthPer10Percent { get; set; }
        bool HideSkillsNotInJD { get; set; }
        double? BulletsPer20Percent { get; set; }
    }
    public class ResumeConfiguration : IResumeConfiguration
    {
        public double? MatchThreshold { get; set; }
        public int? TargetLengthPer10Percent { get; set; }
        public bool HideSkillsNotInJD { get; set; } = true;
        public double? BulletsPer20Percent { get; set; }
    }
}
