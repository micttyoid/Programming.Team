using System;
using System.Collections.Generic;

namespace Programming.Team.Core;

public partial class DocumentType : Entity<int>
{
    public string Name { get; set; } = null!;

    public virtual ICollection<DocumentTemplate> DocumentTemplates { get; set; } = new List<DocumentTemplate>();
}
