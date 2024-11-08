using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Programming.Team.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Input;

namespace Programming.Team.Data.Core
{
    public interface IContextFactory
    {
        DbContext CreateContext();
        Task<ClaimsPrincipal?> GetPrincipal();
    }
    public static class ClaimsPrincipalExtentension
    {
        public static string? GetUserId(this ClaimsPrincipal? principal)
        {
            return principal?.Claims.FirstOrDefault(p => p.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        }
    }
    public interface IUnitOfWork : IAsyncDisposable
    {
        DbContext Context { get; }
        Task Rollback();
        Task Commit(CancellationToken token = default);
    }
    public interface IRepository
    {
        IUnitOfWork CreateUnitOfWork();
    }
    public interface IIRepository<in TEntity, TKey> : IRepository
        where TEntity : Entity<TKey>, new()
        where TKey : struct
    {
        Task Delete(TKey id, IUnitOfWork? work = null, CancellationToken token = default);
        Task Delete(TEntity entity, IUnitOfWork? work = null, CancellationToken token = default);
        Task Add(TEntity entity, IUnitOfWork? work = null, CancellationToken token = default);

    }
    public interface IRepository<TEntity, TKey> : IIRepository<TEntity, TKey>
        where TEntity : Entity<TKey>, new()
        where TKey : struct
    {
        Task<TEntity> Update(TEntity entity,
            IUnitOfWork? work = null, CancellationToken token = default);
        Task<RepositoryResultSet<TKey, TEntity>> Get(IUnitOfWork? work = null,
            Pager? page = null,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Expression<Func<TEntity, object>>? properites = null,
            CancellationToken token = default);
        Task<int> Count(IUnitOfWork? work = null,
            Expression<Func<TEntity, bool>>? filter = null,
            CancellationToken token = default);
        Task<TEntity?> GetByID(TKey key, IUnitOfWork? work = null,
            Expression<Func<TEntity, object>>? properites = null, CancellationToken token = default);

    }
}
