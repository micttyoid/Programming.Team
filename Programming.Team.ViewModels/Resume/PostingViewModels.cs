using DynamicData;
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class PostingsViewModel : ManageEntityViewModel<Guid, Posting>
    {
        public PostingsViewModel(IBusinessRepositoryFacade<Posting, Guid> facade, ILogger<ManageEntityViewModel<Guid, Posting, IBusinessRepositoryFacade<Posting, Guid>>> logger) : base(facade, logger)
        {
        }

        protected override async Task<Expression<Func<Posting, bool>>?> GetBaseFilterCondition()
        {
            var userId = await Facade.GetCurrentUserId();
            return e => e.UserId == userId;
        }
    }
    public class PostingLoaderViewModel : EntityLoaderViewModel<Guid, Posting, PostingViewModel, IBusinessRepositoryFacade<Posting, Guid>>
    {
        protected IBusinessRepositoryFacade<DocumentTemplate, Guid> DocumentTemplateFacade { get; }
        public PostingLoaderViewModel(IBusinessRepositoryFacade<DocumentTemplate, Guid> documentTemplateFacade, IBusinessRepositoryFacade<Posting, Guid> facade, ILogger<EntityLoaderViewModel<Guid, Posting, PostingViewModel, IBusinessRepositoryFacade<Posting, Guid>>> logger) : base(facade, logger)
        {
            DocumentTemplateFacade = documentTemplateFacade;
        }

        protected override PostingViewModel Construct(Posting entity)
        {
            return new PostingViewModel(DocumentTemplateFacade, Logger, Facade, entity);
        }
    }
    public class PostingViewModel : EntityViewModel<Guid, Posting>, IPosting
    {
        protected readonly CompositeDisposable disposables = new CompositeDisposable();
        public ResumeConfigurationViewModel ConfigurationViewModel { get; } = new ResumeConfigurationViewModel();
        public ObservableCollection<DocumentTemplate> DocumentTemplates { get; } = new ObservableCollection<DocumentTemplate>();
        protected IBusinessRepositoryFacade<DocumentTemplate, Guid> DocumentTemplateFacade { get; }
        ~PostingViewModel()
        {
            disposables.Dispose();
        }
        public PostingViewModel(IBusinessRepositoryFacade<DocumentTemplate, Guid>  documentTemplateFacade, ILogger logger, IBusinessRepositoryFacade<Posting, Guid> facade, Guid id) : base(logger, facade, id)
        {
            DocumentTemplateFacade = documentTemplateFacade;
            WireUpEvents();
        }

        public PostingViewModel(IBusinessRepositoryFacade<DocumentTemplate, Guid> documentTemplateFacade, ILogger logger, IBusinessRepositoryFacade<Posting, Guid> facade, Posting entity) : base(logger, facade, entity)
        {
            DocumentTemplateFacade = documentTemplateFacade;
            WireUpEvents();
        }
        protected void WireUpEvents()
        {
            this.WhenPropertyChanged(p => p.SelectedTemplate).Subscribe(p =>
            {
                if (p.Sender.SelectedTemplate != null)
                    p.Sender.DocumentTemplateId = p.Sender.SelectedTemplate.Id;
            }).DisposeWith(disposables);
            this.WhenPropertyChanged(p => p.Configuration).Subscribe(p =>
            {
                ConfigurationViewModel.Load(p.Sender.Configuration);
            }).DisposeWith(disposables);
        }
        private DocumentTemplate? selectedTemplate;
        public DocumentTemplate? SelectedTemplate
        {
            get => selectedTemplate;
            set => this.RaiseAndSetIfChanged(ref selectedTemplate, value);
        }
        private Guid documentTemplateId;
        public Guid DocumentTemplateId
        {
            get => documentTemplateId;
            set => this.RaiseAndSetIfChanged(ref documentTemplateId, value);
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        private string details = string.Empty;
        public string Details
        {
            get => details;
            set => this.RaiseAndSetIfChanged(ref details, value);
        }

        private string? renderedLaTex;
        public string? RenderedLaTex
        {
            get => renderedLaTex;
            set => this.RaiseAndSetIfChanged(ref renderedLaTex, value);
        }

        private string? configuration;
        public string? Configuration
        {
            get => configuration;
            set => this.RaiseAndSetIfChanged(ref configuration, value);
        }

        public Guid UserId { get; set; }
        protected override async Task<Posting?> DoLoad(CancellationToken token)
        {
            try
            {
                DocumentTemplates.Clear();
                var dts = await DocumentTemplateFacade.Get(orderBy: o => o.OrderBy(e => e.Name), token: token);
                DocumentTemplates.AddRange(dts.Entities);
                
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return await base.DoLoad(token);
        }
        protected override Task<Posting> Populate()
        {
            return Task.FromResult(new Posting()
            {
                Id = Id,
                Name = Name,
                Details = Details,
                RenderedLaTex = RenderedLaTex,
                DocumentTemplateId = DocumentTemplateId,
                Configuration = ConfigurationViewModel.GetSerializedConfiguration()
            });
        }

        protected override Task Read(Posting entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Details = entity.Details;
            Configuration = entity.Configuration;
            RenderedLaTex = entity.RenderedLaTex;
            SelectedTemplate = DocumentTemplates.SingleOrDefault(d => d.Id == entity.DocumentTemplateId);
            return Task.CompletedTask;
        }
    }
    public class ResumeConfigurationViewModel : ReactiveObject, IResumeConfiguration
    {
        public void Load(string? configuration)
        {
            var config = configuration != null ? JsonSerializer.Deserialize<ResumeConfiguration>(configuration) ?? new ResumeConfiguration() : new ResumeConfiguration();
            MatchThreshold = config.MatchThreshold;
            TargetLengthPer10Percent = config.TargetLengthPer10Percent;
            HideSkillsNotInJD = config.HideSkillsNotInJD;
        }
        public ResumeConfiguration GetConfiguration()
        {
            return new ResumeConfiguration()
            {
                MatchThreshold = MatchThreshold,
                TargetLengthPer10Percent = TargetLengthPer10Percent,
                HideSkillsNotInJD = HideSkillsNotInJD
            };
        }
        public string GetSerializedConfiguration()
        {
            return JsonSerializer.Serialize(GetConfiguration());
        }
        private double? matchThreshold;
        public double? MatchThreshold
        {
            get => matchThreshold;
            set => this.RaiseAndSetIfChanged(ref matchThreshold, value);
        }

        private int? targetLengthPer10Percent;
        public int? TargetLengthPer10Percent
        {
            get => targetLengthPer10Percent;
            set => this.RaiseAndSetIfChanged(ref targetLengthPer10Percent, value);
        }

        private bool hideSkillsNotInJD = false; // Defaulting to false
        public bool HideSkillsNotInJD
        {
            get => hideSkillsNotInJD;
            set => this.RaiseAndSetIfChanged(ref hideSkillsNotInJD, value);
        }

    }
}
