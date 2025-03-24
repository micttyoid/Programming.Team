
using DynamicData.Binding;
using Microsoft.Extensions.Logging;
using Programming.Team.Business.Core;
using Programming.Team.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Programming.Team.AI.Core;
using System.Collections.ObjectModel;
using DynamicData;
using System.Windows.Input;
using System.Diagnostics;

namespace Programming.Team.ViewModels.Resume;
public class ExpSkillTurgidViewModel : EntitiesDefaultViewModel<Guid, ExpSkill, ExpSkillViewModel, AddExpSkillViewModel>
{
    public ReactiveCommand<Unit, Unit> ExtractSkills { get; }
    public ReactiveCommand<Unit, Unit> AssociateSkills { get; }
    public ICommand ToggleOpen => ReactiveCommand.Create(() => IsOpen = !IsOpen);
    protected IResumeEnricher Enricher { get; }
    protected IBusinessRepositoryFacade<Skill, Guid> SkillFacade { get; }
    public Guid PositionId
    {
        get => AddViewModel.PositionId;
        set
        {
            AddViewModel.PositionId = value;
            SuggestAddSkillsVM.PositionId = value;
        }
    }
    private string description = string.Empty;
    public string Description
    {
        get => description;
        set => this.RaiseAndSetIfChanged(ref description, value);
    }
    public SuggestAddSkillsForPositionViewModel SuggestAddSkillsVM { get; }
    public ExpSkillTurgidViewModel(AddExpSkillViewModel addViewModel, SuggestAddSkillsForPositionViewModel suggestAddSkillsVM,
        IBusinessRepositoryFacade<ExpSkill, Guid> facade, IBusinessRepositoryFacade<Skill, Guid> skillFacade,
        ILogger<EntitiesViewModel<Guid, ExpSkill, ExpSkillViewModel, IBusinessRepositoryFacade<ExpSkill, Guid>>> logger,
        IResumeEnricher enricher) : base(addViewModel, facade, logger)
    {
        ExtractSkills = ReactiveCommand.CreateFromTask(DoExtractSkills);
        AssociateSkills = ReactiveCommand.CreateFromTask(DoAssociateSkills);
        Enricher = enricher;
        SkillFacade = skillFacade;
        SuggestAddSkillsVM = suggestAddSkillsVM;
    }
    private bool isOpen;
    public bool IsOpen
    {
        get => isOpen;
        set => this.RaiseAndSetIfChanged(ref isOpen, value);
    }
    // TODO: looks troublesome
    public ObservableCollection<RawSkillViewModel> RawSkills { get; } = new();
    protected async Task DoAssociateSkills(CancellationToken token)
    {
        try
        {
            foreach (var raw in RawSkills.Where(r => r.IsSelected).ToArray())
            {
                var skillRes = await SkillFacade.Get(page: new Pager() { Page = 1, Size = 1 }, filter: q => q.Name == raw.Name, token: token);
                var skill = skillRes.Entities.FirstOrDefault();
                if (skill == null)
                {
                    skill = new Skill()
                    {
                        Name = raw.Name
                    };
                    await SkillFacade.Add(skill, token: token);
                }
                var ps = new ExpSkill()
                {
                    PositionId = PositionId,
                    SkillId = skill.Id
                };
                await Facade.Add(ps, token: token);
                RawSkills.Remove(raw);
            }
            await Load.Execute().GetAwaiter();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            await Alert.Handle(ex.Message);
        }
    }
    protected async Task DoExtractSkills(CancellationToken token)
    {
        RawSkills.Clear();
        try
        {
            var skills = await Enricher.ExtractSkills(Description, token);
            if (skills?.Length > 0)
            {
                var sks = skills.ToList();
                sks.RemoveAll(s => Entities.Any(e => string.Compare(e.Skill.Name, s, StringComparison.OrdinalIgnoreCase) == 0));
                RawSkills.AddRange(sks.Select(s => new RawSkillViewModel(Logger, PositionId, s)));
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            await Alert.Handle(ex.Message);
        }
    }
    protected override async Task<ExpSkillViewModel> Construct(ExpSkill entity, CancellationToken token)
    {
        var vm = new ExpSkillViewModel(Logger, Facade, entity);

        return vm;
    }
    protected override Func<IQueryable<ExpSkill>, IOrderedQueryable<ExpSkill>>? OrderBy()
    {
        return e => e.OrderBy(c => c.Skill.Name);
    }
    protected override Func<IQueryable<ExpSkill>, IQueryable<ExpSkill>>? PropertiesToLoad()
    {
        return e => e.Include(x => x.Skill);
    }
    protected override async Task<Expression<Func<ExpSkill, bool>>?> FilterCondition()
    {
        return e => e.PositionId == PositionId;
    }
}

// TODO: This part seems to be what i was asked to fix
// why is this raw skill that holds positionId
public class RawSkillViewModel : ReactiveObject
{
    private string name = string.Empty;
    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }
    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set => this.RaiseAndSetIfChanged(ref isSelected, value);
    }
    public Guid PositionId { get; set; }
    public RawSkillViewModel(ILogger logger, Guid positionId, string name)
    {
        Name = name.Trim();
        PositionId = positionId;
    }
}

// TODO: This part seems to be what i was asked to fix
public class SuggestAddSkillsForPositionViewModel : ReactiveObject
{
    public Interaction<string, bool> Alert { get; } = new Interaction<string, bool>();
    public Guid PositionId { get; set; }
    public ReactiveCommand<Unit, Unit> SuggestSkills { get; }
    public ReactiveCommand<Unit, Unit> AddSelectedSkills { get; }
    public ObservableCollection<SkillViewModel> Skills { get; } = new ObservableCollection<SkillViewModel>();
    protected ISkillsBusinessFacade SkillFacade { get; }
    protected IBusinessRepositoryFacade<ExpSkill, Guid> ExpSkillFacade { get; }
    protected ILogger Logger { get; }
    public SuggestAddSkillsForPositionViewModel(ISkillsBusinessFacade skillFacade, IBusinessRepositoryFacade<ExpSkill, Guid> expSkillFacade, ILogger<SuggestAddSkillsForPositionViewModel> logger)
    {
        SuggestSkills = ReactiveCommand.CreateFromTask(DoSuggestSkills);
        AddSelectedSkills = ReactiveCommand.CreateFromTask(DoAddSelectedSkills);
        SkillFacade = skillFacade;
        ExpSkillFacade = expSkillFacade;
        Logger = logger;
    }
    protected async Task DoSuggestSkills(CancellationToken token)
    {
        try
        {
            Skills.Clear();
            foreach (var skill in await SkillFacade.GetSkillsExcludingPosition(PositionId, token: token))
            {
                var vm = new SkillViewModel(Logger, SkillFacade, skill);
                await vm.Load.Execute().GetAwaiter();
                Skills.Add(vm);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            await Alert.Handle(ex.Message).GetAwaiter();
        }
    }
    protected async Task DoAddSelectedSkills(CancellationToken token)
    {
        try
        {
            foreach (var skill in Skills.Where(s => s.IsSelected).ToArray())
            {
                var expskill = new ExpSkill()
                {
                    PositionId = PositionId,
                    SkillId = skill.Id
                };
                // Look at this lagsana. It's almost hide and seek.
                //
                // ExpSkillFacade is ...
                // (Programming.Team.Business.Core/Plumbing.cs)
                // derived from:          IBusinessRepositoryFacade<ExpSkill, Guid>,
                // which is derived from: IIBusinessRepositoryFacade<in TEntity, TKey>,
                // which is derived from: IBusinessRepositoryFacade
                //
                // And all these is just to use a method "Add" of IIBusinessRepositoryFacade
                //
                // Also file naming is not consistent at all
                await ExpSkillFacade.Add(expskill, token: token);
                Skills.Remove(skill);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            await Alert.Handle(ex.Message).GetAwaiter();
        }
    }
}
