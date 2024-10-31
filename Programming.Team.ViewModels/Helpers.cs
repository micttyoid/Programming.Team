﻿using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using Programming.Team.ViewModels;
using Programming.Team.ViewModels.Admin;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Programming.Team.ViewModels
{
    public class DataGridRequest<TKey, TEntity>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        public Pager? Pager { get; set; }
        public Expression<Func<TEntity, bool>>? Filter { get; set; }
        public Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? OrderBy { get; set; }
    }
    public class ViewModelResult<TKey, TEntity, TViewModel> : IPagedResult<TViewModel>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TViewModel : IEntityViewModel<TKey, TEntity>
    {
        public IEnumerable<TViewModel> Entities { get; set; } = null!;
        public int? Count { get; set; }
        public int? PageSize { get; set; }
        public int? Page { get; set; }
    }
    public interface IEntityViewModel<TKey, TEntity> : IEntity<TKey>, IHandleObservableErrors, IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, Splat.IEnableLogger
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        Interaction<string, bool> Alert { get; }
        ICommand Load { get; }
        ICommand Add { get; }
        ICommand Delete { get; }
        ICommand Update { get; }
        bool IsSelected { get; set; }
    }
    public abstract class EntityViewModel<TKey, TEntity, TFacade> : ReactiveObject, IEntityViewModel<TKey, TEntity>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected TFacade Facade { get; }
        public ICommand Load { get; }
        public ICommand Add { get; }
        public ICommand Delete { get; }
        public ICommand Update { get; }
        protected ILogger Logger { get; }
        protected EntityViewModel(ILogger logger, TFacade facade)
        {
            Logger = logger;
            Facade = facade;
            Load = ReactiveCommand.CreateFromTask(DoLoad);
            Add = ReactiveCommand.CreateFromTask(DoAdd);
            Delete = ReactiveCommand.CreateFromTask(DoDelete);
            Update = ReactiveCommand.CreateFromTask(DoUpdate);
        }
        public EntityViewModel(ILogger logger, TFacade facade, TKey id)
            : this(logger, facade)
        {
            Id = id;
        }
        public EntityViewModel(ILogger logger, TFacade facade, TEntity entity)
            : this(logger, facade, entity.Id)
        {
            Read(entity);
        }
        protected virtual async Task DoLoad(CancellationToken token)
        {
            try
            {
                var entity = await Facade.GetByID(Id, token: token);
                if (entity != null)
                    Read(entity);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected virtual async Task DoAdd(CancellationToken token)
        {
            try
            {
                var entity = Populate();
                await Facade.Add(entity, token: token);
                entity.Id = Id;
                await DoLoad(token);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected virtual async Task DoDelete(CancellationToken token)
        {
            try
            {
                await Facade.Delete(Id, token: token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected virtual async Task DoUpdate(CancellationToken token)
        {
            try
            {
                var entity = Populate();
                entity = await Facade.Update(entity, token: token);
                entity.Id = Id;
                Read(entity);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        private TKey id;
        public TKey Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => this.RaiseAndSetIfChanged(ref isSelected, value);
        }
        protected abstract TEntity Populate();
        protected abstract void Read(TEntity entity);
    }
    public abstract class EntityViewModel<TKey, TEntity> : EntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        public EntityViewModel(ILogger logger, IBusinessRepositoryFacade<TEntity, TKey> facade, TKey id)
            : base(logger, facade, id)
        { }
        public EntityViewModel(ILogger logger, IBusinessRepositoryFacade<TEntity, TKey> facade, TEntity entity)
            : base(logger, facade, entity) { }
    }
    public abstract class SelectEntitiesViewModel<TKey, TEntity, TViewModel, TFacade> : ReactiveObject
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TViewModel : IEntityViewModel<TKey, TEntity>
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected ILogger Logger { get; }
        protected TFacade Facade { get; }
        public ReactiveCommand<DataGridRequest<TKey, TEntity>, ViewModelResult<TKey, TEntity, TViewModel>?> Fetch { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        public ObservableCollection<TViewModel> Selected { get; } = new ObservableCollection<TViewModel>();
        public SelectEntitiesViewModel(TFacade facade, ILogger<SelectEntitiesViewModel<TKey, TEntity, TViewModel, TFacade>> logger)
        {
            Logger = logger;
            Facade = facade;
            Fetch = ReactiveCommand.CreateFromTask<DataGridRequest<TKey, TEntity>, ViewModelResult<TKey, TEntity, TViewModel>?>(DoFetch);
        }
        protected virtual async Task<ViewModelResult<TKey, TEntity, TViewModel>?> DoFetch(DataGridRequest<TKey, TEntity> request, CancellationToken token = default)
        {
            try
            {
                var resp = await Facade.Get(page: request.Pager, filter: request.Filter, orderBy: request.OrderBy, token: token);
                if (resp == null)
                    return null;
                ViewModelResult<TKey, TEntity, TViewModel> vmr = new ViewModelResult<TKey, TEntity, TViewModel>();
                vmr.Page = resp.Page;
                vmr.PageSize = resp.PageSize;
                vmr.Count = resp.Count;
                List<TViewModel> vms = new List<TViewModel>();
                foreach (var vm in vmr.Entities)
                {
                    var existing = Selected.SingleOrDefault(e => e.Id.Equals(vm.Id));
                    if (existing == null)
                    {
                        WireupViewModel(vm);
                        vms.Add(vm);
                    }
                    else
                    {
                        vms.Add(existing);
                    }
                }
                vmr.Entities = vms;
                return vmr;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return null;
        }
        public virtual async Task SetSelected(TKey[] ids, CancellationToken token = default)
        {
            try
            {
                foreach (var item in Selected.Where(e => !ids.Any(i => e.Id.Equals(i))).ToArray())
                {
                    Selected.Remove(item);
                }
                if (ids.Length == 0)
                    return;
                var missingIds = ids.Where(i => !Selected.Any(e => e.Id.Equals(i))).ToArray();
                var rs = await Facade.Get(filter: q => missingIds.Contains(q.Id), token: token);
                foreach (var e in rs.Entities)
                {
                    var vm = ConstructViewModel(e);
                    vm.IsSelected = true;
                    WireupViewModel(vm);
                    Insert(vm);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected abstract TViewModel ConstructViewModel(TEntity entity);
        protected virtual void WireupViewModel(TViewModel viewModel)
        {
            viewModel.WhenPropertyChanged(p => p.IsSelected).Subscribe(p =>
            {
                if (p.Value)
                    Insert(p.Sender);
                else
                    Selected.Remove(p.Sender);
            }).DisposeWith(disposable);
        }
        protected readonly object synch = new object();
        protected virtual void Insert(TViewModel vm)
        {
            lock (synch)
            {
                if (!Selected.Any(s => s.Id.Equals(vm.Id)))
                    Selected.Add(vm);
            }
        }
        ~SelectEntitiesViewModel()
        {
            disposable.Dispose();
        }
    }
    public abstract class SelectEntitiesViewModel<TKey, TEntity, TViewModel> : SelectEntitiesViewModel<TKey, TEntity, TViewModel, IBusinessRepositoryFacade<TEntity, TKey>>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TViewModel : IEntityViewModel<TKey, TEntity>
    {
        protected SelectEntitiesViewModel(IBusinessRepositoryFacade<TEntity, TKey> facade,
            ILogger<SelectEntitiesViewModel<TKey, TEntity, TViewModel, IBusinessRepositoryFacade<TEntity, TKey>>> logger) :
            base(facade, logger)
        {
        }
    }
    public interface IManageEntityViewModel<TKey, TEntity> : IHandleObservableErrors, IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, Splat.IEnableLogger
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        Interaction<string, bool> Alert { get; }
        ReactiveCommand<DataGridRequest<TKey, TEntity>, RepositoryResultSet<TKey, TEntity>?> Fetch { get; }
    }
    public interface IManageEntityViewModel<TKey, TEntity, out TAddVM> : IManageEntityViewModel<TKey, TEntity>
        where TKey: struct
        where TEntity: Entity<TKey>, new()
        where TAddVM: IAddEntityViewModel<TKey, TEntity>
    {
        Func<Task>? Reload { get; set; }
        public TAddVM AddViewModel { get; }
    }
    public class ManageEntityViewModel<TKey, TEntity, TFacade> : ReactiveObject, IManageEntityViewModel<TKey, TEntity>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();

        public ReactiveCommand<DataGridRequest<TKey, TEntity>, RepositoryResultSet<TKey, TEntity>?> Fetch { get; }
        protected ILogger Logger { get; }
        protected TFacade Facade { get; }
        public ManageEntityViewModel(TFacade facade, ILogger<ManageEntityViewModel<TKey, TEntity, TFacade>> logger)
        {
            Logger = logger;
            Facade = facade;
            Fetch = ReactiveCommand.CreateFromTask<DataGridRequest<TKey, TEntity>, RepositoryResultSet<TKey, TEntity>?>(DoFetch);
        }
        protected virtual async Task<RepositoryResultSet<TKey, TEntity>?> DoFetch(DataGridRequest<TKey, TEntity> request, CancellationToken token)
        {
            try
            {
                return await Facade.Get(page: request.Pager, filter: request.Filter, orderBy: request.OrderBy, token: token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return null;
        }
    }
    public class ManageEntityViewModel<TKey, TEntity, TFacade, TAddVM> : ManageEntityViewModel<TKey, TEntity, TFacade>, IManageEntityViewModel<TKey, TEntity, TAddVM>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
        where TAddVM : IAddEntityViewModel<TKey, TEntity>
    {
        public ManageEntityViewModel(TFacade facade, TAddVM addVM, ILogger<ManageEntityViewModel<TKey, TEntity, TFacade>> logger) : base(facade, logger)
        {
            AddViewModel = addVM;
            AddViewModel.Added += async (s, e) =>
            {
                if (Reload != null)
                    await Reload();
            };
        }

        public Func<Task>? Reload { get; set; }

        public TAddVM AddViewModel{ get; }
    }
    public class ManageEntityViewModelWithAdd<TKey, TEntity, TAddVM> : ManageEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>, TAddVM>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TAddVM : IAddEntityViewModel<TKey, TEntity>
    {
        public ManageEntityViewModelWithAdd(IBusinessRepositoryFacade<TEntity, TKey> facade, TAddVM addVM, ILogger<ManageEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>> logger) : base(facade, addVM, logger)
        {
        }
    }
    public class ManageEntityViewModel<TKey, TEntity> : ManageEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        public ManageEntityViewModel(IBusinessRepositoryFacade<TEntity, TKey> facade, ILogger<ManageEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>> logger) : base(facade, logger)
        {
        }
    }
    public interface IAddEntityViewModel<TKey, TEntity> : IEntity<TKey>, IHandleObservableErrors, IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, Splat.IEnableLogger
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        Interaction<string, bool> Alert { get; }
        ICommand Add { get; }
        event EventHandler<TEntity>? Added;
    }
    public abstract class AddEntityViewModel<TKey, TEntity, TFacade> : ReactiveObject, IAddEntityViewModel<TKey, TEntity>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
        where TFacade : IBusinessRepositoryFacade<TEntity, TKey>
    {
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();

        public ICommand Add { get; }

        private TKey id;
        public TKey Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }

        public event EventHandler<TEntity>? Added;
        protected TFacade Facade { get; }
        protected ILogger Logger { get; }
        public AddEntityViewModel(TFacade facade, ILogger<AddEntityViewModel<TKey, TEntity, TFacade>> logger)
        {
            Facade = facade;
            Logger = logger;
            Add = ReactiveCommand.CreateFromTask(DoAdd);
        }
        protected virtual async Task DoAdd(CancellationToken token)
        {
            try
            {
                var e = ConstructEntity();
                await Facade.Add(e, token: token);
                Added?.Invoke(this, e);
                Clear();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
        protected abstract TEntity ConstructEntity();
        protected abstract void Clear();
    }
    public abstract class AddEntityViewModel<TKey, TEntity> : AddEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>
        where TKey : struct
        where TEntity : Entity<TKey>, new()
    {
        protected AddEntityViewModel(IBusinessRepositoryFacade<TEntity, TKey> facade, ILogger<AddEntityViewModel<TKey, TEntity, IBusinessRepositoryFacade<TEntity, TKey>>> logger) : base(facade, logger)
        {
        }
    }
}
