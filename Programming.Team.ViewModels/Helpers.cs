using Programming.Team.Core;
using System.Linq.Expressions;

namespace Programming.Team.ViewModels
{
    public class DataGridRequest<TKey, TEntity>
        where TKey: struct
        where TEntity: Entity<TKey>, new()
    {
        public Pager? Pager { get; set; }
        public Expression<Func<TEntity, bool>>? Filter{ get; set; }
        public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; set; }
    }
}
