﻿using System;
using System.Collections.Generic;

namespace Programming.Team.Core;
public interface IPosition : IEntity<Guid>
{
    Guid UserId { get; set; }

    Guid CompanyId { get; set; }

    DateOnly StartDate { get; set; }

    DateOnly? EndDate { get; set; }

    string? Title { get; set; }

    string? Description { get; set; }

    string? SortOrder { get; set; }
}
public partial class Position : Entity<Guid>, IPosition
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
