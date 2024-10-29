using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public partial class Posting : Entity<Guid>
{

    public Guid DocumentTemplateId { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Details { get; set; } = null!;

    public string? RenderedLaTex { get; set; }

    public byte[]? RenderedPdf { get; set; }

    public string? Configuration { get; set; }

    public virtual DocumentTemplate DocumentTemplate { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
