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
using System.Windows.Input;

namespace Programming.Team.ViewModels.Admin
{
    public class ManageRolesViewModel : ReactiveObject
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected IBusinessRepositoryFacade<Role, Guid> RoleFacade { get; }
        protected ILogger Logger { get; }
        public AddRoleViewModel AddRoleVM { get; }
        public Func<Task>? Reload { get; set; }
        public ReactiveCommand<DataGridRequest<Guid, Role>, RepositoryResultSet<Guid, Role>?> FetchRoles { get; }
        public ManageRolesViewModel(IBusinessRepositoryFacade<Role, Guid> roleFacade, ILogger<ManageRolesViewModel> logger)
        {
            RoleFacade = roleFacade;
            Logger = logger;
            AddRoleVM = new AddRoleViewModel(roleFacade, logger);
            AddRoleVM.RoleAdded += async (o, e) =>
            {
                if (Reload != null)
                    await Reload();
            };
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
    public class AddRoleViewModel : ReactiveObject
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected ILogger Logger { get; }
        protected IBusinessRepositoryFacade<Role, Guid> RoleFacade { get; }
        private string name = "";
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
        public ICommand AddRole { get; }
        public event EventHandler? RoleAdded;
        public AddRoleViewModel(IBusinessRepositoryFacade<Role, Guid> roleFacade, ILogger logger)
        {
            RoleFacade = roleFacade;
            Logger = logger;
            AddRole = ReactiveCommand.CreateFromTask(DoAddRole);
        }
        protected async Task DoAddRole(CancellationToken token = default)
        {
            try
            {
                Role role = new Role();
                role.Name = Name;
                await RoleFacade.Add(role, token: token);
                Name = "";
                RoleAdded?.Invoke(this, new EventArgs());
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
}
