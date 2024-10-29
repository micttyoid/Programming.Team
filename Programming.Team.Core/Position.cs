using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public partial class Position : Entity<Guid>
{

    public Guid UserId { get; set; }

    public Guid CompanyId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? SortOrder { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<PositionSkill> PositionSkills { get; set; } = new List<PositionSkill>();

    public virtual User User { get; set; } = null!;
}
