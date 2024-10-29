using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public partial class Company : Entity<Guid>
{

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? Url { get; set; }

    public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
}
