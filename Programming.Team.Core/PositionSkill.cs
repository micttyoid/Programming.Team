using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public partial class PositionSkill : Entity<Guid>
{

    public Guid PositionId { get; set; }

    public Guid SkillId { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual Position Position { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
