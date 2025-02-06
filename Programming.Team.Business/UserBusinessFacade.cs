﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data;
using Programming.Team.Data.Core;
using Programming.Team.PurchaseManager.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.Business
{
    public class UserBusinessFacade : BusinessRepositoryFacade<User, Guid, IUserRepository>, IUserBusinessFacade
    {
        public UserBusinessFacade(IUserRepository repository, ILogger<User> logger) : base(repository, logger)
        {
        }

        public Task<User?> GetByObjectIdAsync(string objectId, IUnitOfWork? work = null, Expression<Func<User, object>>? properties = null, CancellationToken token = default)
        {
            return Repository.GetByObjectIdAsync(objectId, work, properties, token);
        }
        public Task AddRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.AddRecruiter(targetUserId, recruiterId, work, token);
        }

        public Task RemoveRecruiter(Guid targetUserId, Guid recruiterId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.RemoveRecruiter(targetUserId, recruiterId, work, token);
        }
        public override async Task<User> Update(User entity, IUnitOfWork? work = null, Func<IQueryable<User>, IQueryable<User>>? properites = null, CancellationToken token = default)
        {
            var user = await GetByID(entity.Id, work, token: token);
            entity.ResumeGenerationsLeft = user?.ResumeGenerationsLeft ?? throw new InvalidDataException();
            return await base.Update(entity, work, properites, token);
        }

        public async Task<bool> UtilizeResumeGeneration(Guid userId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            var user = await GetByID(userId, properites: q => q.Include(e => e.Roles), work: work, token: token);
            if(!user!.Roles.Any(e => e.Name == "Admin"))
                return await Repository.UtilizeResumeGeneration(userId, work, token);
            return true;
        }
    }
    public class RoleBusinessFacade : BusinessRepositoryFacade<Role, Guid, IRoleRepository>, IRoleBusinessFacade
    {
        public RoleBusinessFacade(IRoleRepository repository, ILogger<Role> logger) : base(repository, logger)
        {
        }

        public Task<Guid[]> GetUserIds(Guid roleId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.GetUserIds(roleId, work, token);
        }

        public Task SetSelectedUsersToRole(Guid roleId, Guid[] userIds, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.SetSelectedUsersToRole(roleId, userIds, work, token);
        }
    }
    public class PackageBusinessFacade : BusinessRepositoryFacade<Package, Guid>
    {
        protected IPurchaseManager PurchaseManager { get; }
        public PackageBusinessFacade(IPurchaseManager purchaseManager, IRepository<Package, Guid> repository, ILogger<Package> logger) : base(repository, logger)
        {
            PurchaseManager = purchaseManager;
        }
        public override async Task Add(Package entity, IUnitOfWork? work = null, CancellationToken token = default)
        {
            await base.Add(entity, work, token);
            await PurchaseManager.ConfigurePackage(entity, token);
        }
        public override async Task<Package> Update(Package entity, IUnitOfWork? work = null, Func<IQueryable<Package>, IQueryable<Package>>? properites = null, CancellationToken token = default)
        {
            var pack = await base.Update(entity, work, properites, token);
            await PurchaseManager.ConfigurePackage(pack, token);
            return pack;
        }
    }
    public class SectionTemplateBusinessFacade : BusinessRepositoryFacade<SectionTemplate, Guid, ISectionTemplateRepository>, ISectionTemplateBusinessFacade
    {
        public SectionTemplateBusinessFacade(ISectionTemplateRepository repository, ILogger<SectionTemplate> logger) : base(repository, logger)
        {
        }

        public Task<SectionTemplate[]> GetBySection(ResumePart sectionId, IUnitOfWork? work = null, CancellationToken token = default)
        {
            return Repository.GetBySection(sectionId, work, token);
        }
    }
}
