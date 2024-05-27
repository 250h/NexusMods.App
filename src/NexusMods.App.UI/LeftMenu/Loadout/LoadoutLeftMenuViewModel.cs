using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData.Kernel;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Diagnostics;
using NexusMods.App.UI.Controls.Navigation;
using NexusMods.App.UI.LeftMenu.Items;
using NexusMods.App.UI.Pages.Diagnostics;
using NexusMods.App.UI.Pages.LoadoutGrid;
using NexusMods.App.UI.Pages.ModLibrary;
using NexusMods.App.UI.Resources;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Icons;
using ReactiveUI;

namespace NexusMods.App.UI.LeftMenu.Loadout;

public class LoadoutLeftMenuViewModel : AViewModel<ILoadoutLeftMenuViewModel>, ILoadoutLeftMenuViewModel
{
    public IApplyControlViewModel ApplyControlViewModel { get; }

    public ReadOnlyObservableCollection<ILeftMenuItemViewModel> Items { get; }
    public WorkspaceId WorkspaceId { get; }

    public LoadoutLeftMenuViewModel(
        LoadoutContext loadoutContext,
        WorkspaceId workspaceId,
        IWorkspaceController workspaceController,
        IServiceProvider serviceProvider)
    {
        var diagnosticManager = serviceProvider.GetRequiredService<IDiagnosticManager>();

        WorkspaceId = workspaceId;
        ApplyControlViewModel = new ApplyControlViewModel(loadoutContext.LoadoutId, serviceProvider);

        var loadoutItem = new IconViewModel
        {
            Name = Language.LoadoutLeftMenuViewModel_LoadoutGridEntry,
            Icon = IconValues.Collections,
            NavigateCommand = ReactiveCommand.Create<NavigationInformation>(info =>
                {
                    var pageData = new PageData
                    {
                        FactoryId = LoadoutGridPageFactory.StaticId,
                        Context = new LoadoutGridContext { LoadoutId = loadoutContext.LoadoutId },
                    };

                    var behavior = workspaceController.GetOpenPageBehavior(pageData, info, Optional<PageIdBundle>.None);
                    workspaceController.OpenPage(WorkspaceId, pageData, behavior);
                }
            ),
        };
        
        var modLibraryItem = new IconViewModel
        {
            Name = Language.FileOriginsPageTitle,
            Icon = IconValues.ModLibrary,
            NavigateCommand = ReactiveCommand.Create<NavigationInformation>(info =>
                {
                    var pageData = new PageData
                    {
                        FactoryId = FileOriginsPageFactory.StaticId,
                        Context = new FileOriginsPageContext
                        {
                            LoadoutId = loadoutContext.LoadoutId,
                        },
                    };

                    var behavior = workspaceController.GetOpenPageBehavior(pageData, info, Optional<PageIdBundle>.None);
                    workspaceController.OpenPage(WorkspaceId, pageData, behavior);
                }
            ),
        };

        var diagnosticItem = new IconViewModel
        {
            Name = Language.LoadoutLeftMenuViewModel_LoadoutLeftMenuViewModel_Diagnostics,
            Icon = IconValues.MonitorDiagnostics,
            NavigateCommand = ReactiveCommand.Create<NavigationInformation>(info =>
            {
                var pageData = new PageData
                {
                    FactoryId = DiagnosticListPageFactory.StaticId,
                    Context = new DiagnosticListPageContext
                    {
                        LoadoutId = loadoutContext.LoadoutId,
                    },
                };

                var behavior = workspaceController.GetOpenPageBehavior(pageData, info, Optional<PageIdBundle>.None);
                workspaceController.OpenPage(WorkspaceId, pageData, behavior);
            }),
        };
        

        var items = new ILeftMenuItemViewModel[]
        {
            loadoutItem,
            modLibraryItem,
            diagnosticItem,
        };

        Items = new ReadOnlyObservableCollection<ILeftMenuItemViewModel>(new ObservableCollection<ILeftMenuItemViewModel>(items));

        this.WhenActivated(disposable =>
        {
            diagnosticManager
                .CountDiagnostics(loadoutContext.LoadoutId)
                .OnUI()
                .Select(counts =>
                {
                    var badges = new List<string>(capacity: 3);
                    if (counts.NumCritical != 0)
                        badges.Add(counts.NumCritical.ToString());
                    if (counts.NumWarnings != 0)
                        badges.Add(counts.NumWarnings.ToString());
                    if (counts.NumSuggestions != 0)
                        badges.Add(counts.NumSuggestions.ToString());
                    return badges.ToArray();
                })
                .BindToVM(diagnosticItem, vm => vm.Badges)
                .DisposeWith(disposable);
        });
    }
}
