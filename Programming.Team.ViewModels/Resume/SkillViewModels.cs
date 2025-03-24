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
public class AddSkillViewModel : AddEntityViewModel<Guid, Skill>, ISkill
{
    // TODO: do something to this old C# style
    public AddSkillViewModel(IBusinessRepositoryFacade<Skill, Guid> facade, ILogger<AddEntityViewModel<Guid, Skill, IBusinessRepositoryFacade<Skill, Guid>>> logger) : base(facade, logger)
    {
    }
    private string name = null!;
    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    protected override Task Clear()
    {
        Name = string.Empty;
        return Task.CompletedTask;
    }

    protected override Task<Skill> ConstructEntity()
    {
        return Task.FromResult(new Skill()
        {
            Name = Name
        });
    }
    public override void SetText(string text)
    {
        Name = text;
    }
}
public class SkillViewModel : EntityViewModel<Guid, Skill>, ISkill
{
    public SkillViewModel(ILogger logger, IBusinessRepositoryFacade<Skill, Guid> facade, Guid id) : base(logger, facade, id)
    {
    }

    public SkillViewModel(ILogger logger, IBusinessRepositoryFacade<Skill, Guid> facade, Skill entity) : base(logger, facade, entity)
    {
    }
    private string name = null!;
    public string Name
    {
        get => name;
        set => this.RaiseAndSetIfChanged(ref name, value);
    }

    protected override Task<Skill> Populate()
    {
        return Task.FromResult(new Skill()
        {
            Id = Id,
            Name = Name
        });
    }

    protected override Task Read(Skill entity)
    {
        Id = entity.Id;
        Name = entity.Name;
        return Task.CompletedTask;
    }
}


// TODO: Why does Skill always need to associated with positionId?
//       and why do we "RawSkill" that also has positionId?
public class AddExpSkillViewModel : AddEntityViewModel<Guid, ExpSkill>, IExpSkill
{
    public SearchSelectSkillViewModel SkillSelectorViewModel { get; }
    private readonly CompositeDisposable disposables = new CompositeDisposable();
    public AddExpSkillViewModel(SearchSelectSkillViewModel skillViewModel, IBusinessRepositoryFacade<ExpSkill, Guid> facade, ILogger<AddEntityViewModel<Guid, ExpSkill, IBusinessRepositoryFacade<ExpSkill, Guid>>> logger) : base(facade, logger)
    {
        SkillSelectorViewModel = skillViewModel;
        skillViewModel.WhenPropertyChanged(p => p.Selected).Subscribe(p =>
        {
            if (p.Sender.Selected == null)
                SkillId = Guid.Empty;
            else
                SkillId = p.Sender.Selected.Id;
        }).DisposeWith(disposables);
    }
    public override bool CanAdd => SkillSelectorViewModel.Selected != null;
    private Guid positionId;
    public Guid PositionId
    {
        get => positionId;
        set => this.RaiseAndSetIfChanged(ref positionId, value);
    }

    private Guid skillId;
    public Guid SkillId
    {
        get => skillId;
        set
        {
            this.RaiseAndSetIfChanged(ref skillId, value);
            this.RaisePropertyChanged(nameof(CanAdd));
        }
    }

    private string? description;
    public string? Description
    {
        get => description;
        set => this.RaiseAndSetIfChanged(ref description, value);
    }


    protected override Task Clear()
    {
        SkillId = Guid.Empty;
        Description = null;
        SkillSelectorViewModel.Selected = null;
        return Task.CompletedTask;
    }

    protected override Task<ExpSkill> ConstructEntity()
    {
        return Task.FromResult(new ExpSkill()
        {
            Id = Id,
            PositionId = PositionId,
            SkillId = SkillId,
            Description = Description
        });
    }
    ~AddExpSkillViewModel()
    {
        disposables.Dispose();
    }
}
public class ExpSkillViewModel : EntityViewModel<Guid, ExpSkill>, IExpSkill
{
    private bool isOpen;
    public bool IsOpen
    {
        get => isOpen;
        set => this.RaiseAndSetIfChanged(ref isOpen, value);
    }
    private Guid positionId;
    public Guid PositionId
    {
        get => positionId;
        set => this.RaiseAndSetIfChanged(ref positionId, value);
    }

    private Guid skillId;
    public Guid SkillId
    {
        get => skillId;
        set => this.RaiseAndSetIfChanged(ref skillId, value);
    }

    private string? description;
    public string? Description
    {
        get => description;
        set => this.RaiseAndSetIfChanged(ref description, value);
    }

    public ReactiveCommand<Unit, Unit> Cancel { get; }
    public ReactiveCommand<Unit, Unit> Edit { get; }

    // Look at these ...
    public ExpSkillViewModel(ILogger logger, IBusinessRepositoryFacade<ExpSkill, Guid> facade, Guid id) : base(logger, facade, id)
    {
        Cancel = ReactiveCommand.CreateFromTask(DoCancel);
        Edit = ReactiveCommand.Create(() => { IsOpen = true; });
    }

    public ExpSkillViewModel(ILogger logger, IBusinessRepositoryFacade<ExpSkill, Guid> facade, ExpSkill entity) : base(logger, facade, entity)
    {
        Cancel = ReactiveCommand.CreateFromTask(DoCancel);
        Edit = ReactiveCommand.Create(() => { IsOpen = true; });
    }
    protected async Task DoCancel(CancellationToken token)
    {
        IsOpen = false;
        await Load.Execute().GetAwaiter();
    }

    protected override Func<IQueryable<ExpSkill>, IQueryable<ExpSkill>>? PropertiesToLoad()
    {
        return q => q.Include(e => e.Skill).Include(e => e.Position);
    }

    private Skill skill = null!;
    public Skill Skill
    {
        get => skill;
        set => this.RaiseAndSetIfChanged(ref skill, value);
    }
    protected override Task<ExpSkill> Populate()
    {
        return Task.FromResult(new ExpSkill()
        {
            Id = Id,
            PositionId = PositionId,
            SkillId = SkillId,
            Description = Description
        });
    }

    protected override Task Read(ExpSkill entity)
    {
        Id = entity.Id;
        PositionId = entity.PositionId;
        SkillId = entity.SkillId;
        Description = entity.Description;
        Skill = entity.Skill;
        return Task.CompletedTask;
    }
}

public class SearchSelectSkillViewModel : EntitySelectSearchViewModel<Guid, Skill, AddSkillViewModel>
{
    public SearchSelectSkillViewModel(IBusinessRepositoryFacade<Skill, Guid> facade, AddSkillViewModel addViewModel, ILogger<EntitySelectSearchViewModel<Guid, Skill, IBusinessRepositoryFacade<Skill, Guid>, AddSkillViewModel>> logger) : base(facade, addViewModel, logger)
    {
    }

    protected override async Task<IEnumerable<Skill>> DoSearch(string? text, CancellationToken token = default)
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

