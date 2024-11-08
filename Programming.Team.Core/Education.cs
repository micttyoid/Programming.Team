using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public interface IEducation : IEntity<Guid>, IUserPartionedEntity
{
    Guid InstitutionId { get; set; }

    string? Major { get; set; }

    DateOnly StartDate { get; set; }

    DateOnly? EndDate { get; set; }

    string? Description { get; set; }

    bool Graduated { get; set; }
}
public partial class Education : Entity<Guid>
{
   

    public Guid InstitutionId { get; set; }

    public Guid UserId { get; set; }

    public string? Major { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Description { get; set; }

    public bool Graduated { get; set; }

    public virtual Institution Institution { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
