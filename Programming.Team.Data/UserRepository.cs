﻿using Microsoft.EntityFrameworkCore;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Data
{
    public class UserRepository : Repository<User, Guid>, IUserRepository
    {
        public UserRepository(IContextFactory contextFactory) : base(contextFactory)
        {
        }

        public async Task<User?> GetByObjectIdAsync(string objectId, IUnitOfWork? work = null, Expression<Func<User, object>>? properties = null, CancellationToken token = default)
        {
            User? user = null;
            await Use(async (w, t) =>
            {
                var query = w.ResumesContext.Users.AsQueryable();
                if(properties != null)
                    query = query.Include(properties);
                user = await query.SingleOrDefaultAsync(x => x.ObjectId == objectId);
            }, work, token);
            return user;
        }
    }
    public class RoleRepository : Repository<Role, Guid>, IRoleRepository
    {
        public RoleRepository(IContextFactory contextFactory) : base(contextFactory)
        {
        }

        public async Task<Guid[]> GetUserIds(Guid roleId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            Guid[] userIds = [];
            await Use(async (w, t) => 
            {
                userIds = await w.ResumesContext.Users.Where(u => u.Roles.Any(r => r.Id == roleId)).Select(u => u.Id).ToArrayAsync(token);
            },work, token);
            return userIds;
        }

        public async Task SetSelectedUsersToRole(Guid roleId, Guid[] userIds, IUnitOfWork? work = null, CancellationToken token = default)
        {
            await Use(async (w, t) =>
            {
                var role = await w.ResumesContext.Roles.Include(c => c.Users).SingleOrDefaultAsync(w => w.Id == roleId);
                if(role != null)
                {
                    
                    var userId = await GetCurrentUserId(w, t);
                    role.Users.Clear();
                    role.UpdateDate = DateTime.UtcNow;
                    role.UpdatedByUserId = userId;
                    foreach(var id in userIds)
                    {
                        role.Users.Add(await w.ResumesContext.Users.SingleAsync(w => w.Id == id));
                    }
                    
                }
            }, work, token, true);
        }
    }
}
