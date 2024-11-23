using DynamicData;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Programming.Team.ViewModels.Resume
{
    public class ResumeBuilderViewModel : ReactiveObject
    {
        public ResumeConfigurationViewModel Configuration { get; } = new ResumeConfigurationViewModel();
        public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
        protected ILogger Logger { get; }
        protected IResumeBuilder Builder { get; }
        public ReactiveCommand<Unit, Posting?> Build { get; }
        public ReactiveCommand<Unit, Unit> Load { get; }
        protected IBusinessRepositoryFacade<DocumentTemplate, Guid> DocumentTemplateFacade { get; }
        public ObservableCollection<DocumentTemplate> DocumentTemplates { get; } = new ObservableCollection<DocumentTemplate>();
        public ResumeBuilderViewModel(IBusinessRepositoryFacade<DocumentTemplate, Guid>  documentTemplateFacade, ILogger<ResumeBuilderViewModel> logger, IResumeBuilder builder)
        {
            Logger = logger;
            DocumentTemplateFacade = documentTemplateFacade;
            Builder = builder;
            Build = ReactiveCommand.CreateFromTask(DoBuild);
            Load = ReactiveCommand.CreateFromTask(DoLoad);
        }
        private DocumentTemplate? selectedTemplate;
        public DocumentTemplate? SelectedTemplate
        {
            get => selectedTemplate;
            set => this.RaiseAndSetIfChanged(ref selectedTemplate, value);
        }
        private string postingText = string.Empty;
        public string PostingText
        {
            get => postingText;
            set => this.RaiseAndSetIfChanged(ref postingText, value);
        }
        private string name = string.Empty;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
        protected async Task<Posting?> DoBuild(CancellationToken token)
        {
            try
            {
                var userId = await DocumentTemplateFacade.GetCurrentUserId();
                if (userId == null)
                    return null;
                var resume = await Builder.BuildResume(userId.Value, token);
                var posting = await Builder.BuildPosting(userId.Value, SelectedTemplate!.Id, Name, PostingText, resume, Configuration.GetConfiguration(), token: token);
                await Alert.Handle("Resume Built").GetAwaiter();
                return posting;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
            return null;
        }
        protected async Task DoLoad(CancellationToken token)
        {
            try
            {
                DocumentTemplates.Clear();
                var dts = await DocumentTemplateFacade.Get(orderBy: o => o.OrderBy(e => e.Name), token: token);
                DocumentTemplates.AddRange(dts.Entities);
                SelectedTemplate = DocumentTemplates.First();
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                await Alert.Handle(ex.Message).GetAwaiter();
            }
        }
    }
}
