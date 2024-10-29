using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public partial class CertificateIssuer : Entity<Guid>
{

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Url { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}
