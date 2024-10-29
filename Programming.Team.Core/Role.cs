using Programming.Team.Core;
using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public partial class Role : Entity<Guid>
{

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
