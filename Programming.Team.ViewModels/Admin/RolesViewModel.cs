using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Admin
{
    public class ManageRolesViewModel : ReactiveObject
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected IBusinessRepositoryFacade<Role, Guid> RoleFacade { get; }
        protected ILogger Logger { get; }
        public ReactiveCommand<DataGridRequest<Guid, Role>, RepositoryResultSet<Guid, Role>?> FetchRoles { get; }
        public ManageRolesViewModel(IBusinessRepositoryFacade<Role, Guid> roleFacade, ILogger<ManageRolesViewModel> logger)
        {
            RoleFacade = roleFacade;
            Logger = logger;
            FetchRoles = ReactiveCommand.CreateFromTask<DataGridRequest<Guid, Role>, RepositoryResultSet<Guid, Role>?>(DoFetchRoles);
        }
        protected async Task<RepositoryResultSet<Guid, Role>?> DoFetchRoles(DataGridRequest<Guid, Role> request, CancellationToken token = default)
        {
            try
            {
                return await RoleFacade.Get(page: request.Pager, filter: request.Filter, orderBy: request.OrderBy, token: token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return null;
        }
    }

}
