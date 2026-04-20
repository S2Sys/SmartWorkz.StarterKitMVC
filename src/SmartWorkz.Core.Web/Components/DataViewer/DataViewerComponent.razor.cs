using Microsoft.AspNetCore.Components;
using SmartWorkz.Core.Shared.Grid;
using SmartWorkz.Core.Web.Components.DataContext;
using SmartWorkz.Core.Web.Components.Grid;
using SmartWorkz.Core.Web.Components.ListView;
using SmartWorkz.Core.Web.Services.DataView;

namespace SmartWorkz.Core.Web.Components.DataViewer;

public partial class DataViewerComponent<T> : ComponentBase where T : class
{
    [Parameter]
    public IEnumerable<T>? DataSource { get; set; }

    [Parameter]
    public List<GridColumn>? Columns { get; set; }

    [Parameter]
    public RenderFragment<T>? ItemTemplate { get; set; }

    [Parameter]
    public ViewType DefaultView { get; set; } = ViewType.Grid;

    [Parameter]
    public bool AutoFetch { get; set; } = true;

    public IDataContext<T>? DataContext { get; set; }
    public ViewConfiguration? ListViewConfig { get; set; }
    public ViewType CurrentView { get; set; }

    private GridComponent<T>? gridComponent;
    private ListViewComponent<T>? listComponent;

    protected override async Task OnInitializedAsync()
    {
        CurrentView = DefaultView;
        DataContext = new DataContext<T>();
        ListViewConfig = new ViewConfiguration
        {
            VisibleColumns = Columns?.Select(c => c.PropertyName).ToList() ?? [],
            ItemsPerRow = 2
        };

        if (AutoFetch && DataSource != null)
        {
            await DataContext.Initialize(DataSource);
        }
    }

    private void OnViewToggle(string? viewValue)
    {
        if (viewValue == "grid")
            CurrentView = ViewType.Grid;
        else if (viewValue == "list")
            CurrentView = ViewType.List;

        StateHasChanged();
    }
}

public enum ViewType
{
    Grid = 0,
    List = 1,
    Map = 2
}
