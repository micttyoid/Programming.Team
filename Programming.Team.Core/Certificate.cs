using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public partial class Certificate : Entity<Guid>
{

    public Guid IssuerId { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly ValidFromDate { get; set; }

    public DateOnly? ValidToDate { get; set; }

    public string? Url { get; set; }

    public string? Description { get; set; }

    public virtual CertificateIssuer Issuer { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
