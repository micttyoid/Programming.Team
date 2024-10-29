using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public partial class DocumentTemplate : Entity<Guid>
{
    public int DocumentTypeId { get; set; }

    public string Name { get; set; } = null!;

    public string Template { get; set; } = null!;

    public virtual DocumentType DocumentType { get; set; } = null!;

    public virtual ICollection<Posting> Postings { get; set; } = new List<Posting>();
}
