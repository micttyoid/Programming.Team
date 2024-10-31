using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Programming.Team.ViewModels.Admin
{
    public class ManageRolesViewModel : ManageEntityViewModelWithAdd<Guid, Role, AddRoleViewModel>
    {
        public ManageRolesViewModel(IBusinessRepositoryFacade<Role, Guid> facade, AddRoleViewModel addVM, ILogger<ManageEntityViewModel<Guid, Role, IBusinessRepositoryFacade<Role, Guid>>> logger) : base(facade, addVM, logger)
        {
        }
    }
    public class AddRoleViewModel : AddEntityViewModel<Guid, Role>, IRole
    {
        private string name = null!;

        public AddRoleViewModel(IBusinessRepositoryFacade<Role, Guid> facade, ILogger<AddEntityViewModel<Guid, Role, IBusinessRepositoryFacade<Role, Guid>>> logger) : base(facade, logger)
        {
        }
        [Required]
        public string Name 
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        protected override void Clear()
        {
            Name = "";
        }

        protected override Role ConstructEntity()
        {
            return new Role() { Name = Name };
        }
    }
}
