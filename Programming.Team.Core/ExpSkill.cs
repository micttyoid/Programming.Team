using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public interface IExpSkill : IEntity<Guid>
{
    Guid PositionId { get; set; }

    Guid SkillId { get; set; }

    string? Description { get; set; }

}
public partial class ExpSkill : Entity<Guid>, IExpSkill
{

    public Guid PositionId { get; set; }

    public Guid SkillId { get; set; }

    public string? Description { get; set; }


    public virtual Position Position { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
