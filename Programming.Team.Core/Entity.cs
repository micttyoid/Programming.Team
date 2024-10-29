using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Core
{
    public abstract class Entity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public Guid? CreatedByUserId { get; set; }

        public Guid? UpdatedByUserId { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public bool IsDeleted { get; set; }

        public virtual User? CreatedByUser { get; set; } = null!;

        public virtual User? UpdatedByUser { get; set; } = null!;
    }
    public class RepositoryResultSet<TKey, TEntity>
        where TEntity : Entity<TKey>, new()
        where TKey : struct
    {
        public IEnumerable<TEntity> Entities { get; set; } = null!;
        public int? Count { get; set; }
        public int? PageSize { get; set; }
        public int? Page { get; set; }


    }
    public struct Pager
    {
        public int Size { get; set; }
        public int Page { get; set; }
    }
}
