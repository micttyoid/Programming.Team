using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class AddCompanyViewModel : AddEntityViewModel<Guid, Company>, ICompany
    {
        public AddCompanyViewModel(IBusinessRepositoryFacade<Company, Guid> facade, ILogger<AddEntityViewModel<Guid, Company, IBusinessRepositoryFacade<Company, Guid>>> logger) : base(facade, logger)
        {
        }
        private string name = null!;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }

        private string? city;
        public string? City
        {
            get => city;
            set => this.RaiseAndSetIfChanged(ref city, value);
        }

        private string? state;
        public string? State
        {
            get => state;
            set => this.RaiseAndSetIfChanged(ref state, value);
        }

        private string? country;
        public string? Country
        {
            get => country;
            set => this.RaiseAndSetIfChanged(ref country, value);
        }

        private string? url;

        public string? Url
        {
            get => url;
            set => this.RaiseAndSetIfChanged(ref url, value);
        }


        protected override Task Clear()
        {
            Name = "";
            Description = null;
            City = null;
            State = null;
            Country = null;
            Url = null;
            return Task.CompletedTask;
        }

        protected override Task<Company> ConstructEntity()
        {
            return Task.FromResult(new Company()
            {
                Name = Name,
                Description = Description,
                City = City,
                State = State,
                Country = Country,
                Url = Url
            });
        }
        public override void SetText(string text)
        {
            Name = text;
        }
    }
    public class SearchSelectCompanyViewModel : EntitySelectSearchViewModel<Guid, Company, AddCompanyViewModel>
    {
        public SearchSelectCompanyViewModel(IBusinessRepositoryFacade<Company, Guid> facade, AddCompanyViewModel addViewModel, ILogger<EntitySelectSearchViewModel<Guid, Company, IBusinessRepositoryFacade<Company, Guid>, AddCompanyViewModel>> logger) : base(facade, addViewModel, logger)
        {
        }

        protected override async Task<IEnumerable<Company>> DoSearch(string? text, CancellationToken token = default)
        {
            
            if (string.IsNullOrWhiteSpace(text))
                return [];
            SearchString = text;
            var result = await Facade.Get(page: new Pager() { Page = 1, Size = 5 },
                filter: q => q.Name.StartsWith(text), token: token);
            if (result != null)
                return result.Entities;
            return [];
        }
    }
    public class PositionViewModel : EntityViewModel<Guid, Position>, IPosition
    {
        private Company company = null!;
        public Company Company
        {
            get => company;
            set => this.RaiseAndSetIfChanged(ref company, value);
        }
        private Guid companyId;
        [Required]
        public Guid CompanyId
        {
            get => companyId;
            set => this.RaiseAndSetIfChanged(ref companyId, value);
        }

        private DateOnly startDate;
        [Required]
        public DateOnly StartDate
        {
            get => startDate;
            set
            {
                this.RaiseAndSetIfChanged(ref startDate, value);
                this.RaisePropertyChanged(nameof(StartDateTime));
            }
        }
        public DateTime? StartDateTime
        {
            get => StartDate.ToDateTime(TimeOnly.MinValue);
            set
            {
                StartDate = DateOnly.FromDateTime(value ?? DateTime.Today);
            }
        }
        private DateOnly? endDate;
        public DateOnly? EndDate
        {
            get => endDate;
            set
            {
                this.RaiseAndSetIfChanged(ref endDate, value);
                this.RaisePropertyChanged(nameof(EndDateTime));
            }
        }
        public DateTime? EndDateTime
        {
            get => EndDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                if (value != null)
                    EndDate = DateOnly.FromDateTime(value.Value);
                else EndDate = null;
            }

        }
        private string? title;
        public string? Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }

        private string? sortOrder;
        public string? SortOrder
        {
            get => sortOrder;
            set => this.RaiseAndSetIfChanged(ref sortOrder, value);
        }
        private Guid userId;
        public Guid UserId
        {
            get => userId;
            set => this.RaiseAndSetIfChanged(ref userId, value);
        }

        public PositionViewModel(ILogger logger, IBusinessRepositoryFacade<Position, Guid> facade, Guid id) : base(logger, facade, id)
        {
        }

        public PositionViewModel(ILogger logger, IBusinessRepositoryFacade<Position, Guid> facade, Position entity) : base(logger, facade, entity)
        {
        }
        protected override IEnumerable<Expression<Func<Position, object>>>? PropertiesToLoad()
        {
            yield return e => e.Company;
        }
        protected override Task<Position> Populate()
        {
            return Task.FromResult(new Position()
            {
                Id = Id,
                CompanyId = CompanyId,
                UserId = UserId,
                Description = Description,
                SortOrder = sortOrder,
                Title = Title,
                StartDate = StartDate,
                EndDate = EndDate,

            });
        }

        protected override Task Read(Position entity)
        {
            Id = entity.Id;
            CompanyId = entity.CompanyId;
            Company = entity.Company;
            UserId = entity.UserId;
            Description = entity.Description;
            SortOrder = entity.SortOrder;
            Title = entity.Title;
            StartDate = entity.StartDate;
            EndDate = entity.EndDate;
            Company = entity.Company;
            return Task.CompletedTask;
        }
    }
    public class AddPositionViewModel : AddUserPartionedEntity<Guid, Position>, IPosition
    {
        public SearchSelectCompanyViewModel CompanyViewModel { get; }
        protected readonly CompositeDisposable disposable = new CompositeDisposable();
        public AddPositionViewModel(IBusinessRepositoryFacade<Position, Guid> facade,
            ILogger<AddEntityViewModel<Guid, Position, IBusinessRepositoryFacade<Position, Guid>>> logger,
            SearchSelectCompanyViewModel companyViewModel) : base(facade, logger)
        {
            CompanyViewModel = companyViewModel;
            CompanyViewModel.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
            {
                if (p.Sender != null && p.Sender.Selected != null)
                    CompanyId = p.Sender.Selected.Id;
                else
                    CompanyId = Guid.Empty;
            }).DisposeWith(disposable);
        }

        private Guid companyId;
        [Required]
        public Guid CompanyId
        {
            get => companyId;
            set => this.RaiseAndSetIfChanged(ref companyId, value);
        }

        private DateOnly startDate;
        [Required]
        public DateOnly StartDate
        {
            get => startDate;
            set
            {
                this.RaiseAndSetIfChanged(ref startDate, value);
                this.RaisePropertyChanged(nameof(StartDateTime));
            }
        }
        public DateTime? StartDateTime
        {
            get => StartDate.ToDateTime(TimeOnly.MinValue);
            set
            {
                StartDate = DateOnly.FromDateTime(value ?? DateTime.Today);
            }
        }
        private DateOnly? endDate;
        public DateOnly? EndDate
        {
            get => endDate;
            set
            {
                this.RaiseAndSetIfChanged(ref endDate, value);
                this.RaisePropertyChanged(nameof(EndDateTime));
            }
        }
        public DateTime? EndDateTime
        {
            get => EndDate?.ToDateTime(TimeOnly.MinValue);
            set
            {
                if (value != null)
                    EndDate = DateOnly.FromDateTime(value.Value);
                else EndDate = null;
            }

        }
        private string? title;
        public string? Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        private string? description;
        public string? Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }

        private string? sortOrder;
        public string? SortOrder
        {
            get => sortOrder;
            set => this.RaiseAndSetIfChanged(ref sortOrder, value);
        }


        protected override Task Clear()
        {
            CompanyId = Guid.Empty;
            StartDate = DateOnly.FromDateTime(DateTime.Now);
            EndDate = null;
            Title = null;
            Description = null;
            SortOrder = null;
            return Task.CompletedTask;
        }

        protected override Task<Position> ConstructEntity()
        {
            return Task.FromResult(new Position()
            {
                CompanyId = CompanyId,
                StartDate = StartDate,
                EndDate = EndDate,
                Title = Title,
                Description = Description,
                SortOrder = SortOrder,
                UserId = UserId
            });
        }
        ~AddPositionViewModel()
        {
            disposable.Dispose();
        }
    }
    public class PositionsViewModel : EntitiesDefaultViewModel<Guid, Position, PositionViewModel, AddPositionViewModel>
    {
        public PositionsViewModel(AddPositionViewModel addViewModel, IBusinessRepositoryFacade<Position, Guid> facade, ILogger<EntitiesViewModel<Guid, Position, PositionViewModel, IBusinessRepositoryFacade<Position, Guid>>> logger) : base(addViewModel, facade, logger)
        {
        }
        protected override IEnumerable<Expression<Func<Position, object>>>? PropertiesToLoad()
        {
            yield return e => e.Company;
        }
        protected override Func<IQueryable<Position>, IOrderedQueryable<Position>>? OrderBy()
        {
            return e => e.OrderByDescending(c => c.StartDate).ThenByDescending(c => c.SortOrder).ThenByDescending(c => c.EndDate);
        }
        protected override Task<PositionViewModel> Construct(Position entity, CancellationToken token)
        {
            return Task.FromResult(new PositionViewModel(Logger, Facade, entity));
        }
    }
}
