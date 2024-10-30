using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.Data.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Admin
{
    public class UsersViewModel : ReactiveObject
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected ILogger Logger { get; }
        protected IUserBusinessFacade UserFacade { get; }
        public ReactiveCommand<DataGridRequest<Guid, User>, RepositoryResultSet<Guid, User>?> FetchUsers { get; }
        public UsersViewModel(IUserBusinessFacade userFacade, ILogger<UsersViewModel> logger)
        {
            UserFacade = userFacade;
            Logger = logger;
            FetchUsers = ReactiveCommand.CreateFromTask<DataGridRequest<Guid, User>, RepositoryResultSet<Guid, User>?>(DoFetchUsers);
        }
        protected async Task<RepositoryResultSet<Guid, User>?> DoFetchUsers(DataGridRequest<Guid, User> request, CancellationToken token = default)
        {
            try
            {
                return await UserFacade.Get(page: request.Pager, filter: request.Filter, orderBy: request.OrderBy, token: token);
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
