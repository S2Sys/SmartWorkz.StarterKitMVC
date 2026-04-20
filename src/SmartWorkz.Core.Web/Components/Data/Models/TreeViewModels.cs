namespace SmartWorkz.Core.Web.Components.Data.Models;

/// <summary>Hierarchical node for TreeViewComponent.</summary>
public class TreeNode
{
    /// <summary>Unique identifier for the node.</summary>
    public required string Id { get; set; }

    /// <summary>Display text for the node.</summary>
    public required string Label { get; set; }

    /// <summary>Parent node ID (null for root nodes).</summary>
    public string? ParentId { get; set; }

    /// <summary>Icon CSS class (e.g., "fa-folder", "fa-file").</summary>
    public string? IconClass { get; set; }

    /// <summary>Is this a leaf node (no children).</summary>
    public bool IsLeaf { get; set; } = false;

    /// <summary>Initially expanded state.</summary>
    public bool IsExpanded { get; set; } = false;

    /// <summary>Disable interaction with this node.</summary>
    public bool IsDisabled { get; set; } = false;

    /// <summary>Custom data associated with the node.</summary>
    public object? Data { get; set; }

    /// <summary>Child nodes.</summary>
    public List<TreeNode> Children { get; set; } = [];

    /// <summary>Load children asynchronously (lazy loading).</summary>
    public Func<string, Task<List<TreeNode>>>? LoadChildrenAsync { get; set; }
}

/// <summary>Configuration for TreeViewComponent.</summary>
public class TreeViewOptions
{
    /// <summary>Allow selecting multiple nodes.</summary>
    public bool AllowMultiSelect { get; set; } = false;

    /// <summary>Enable drag-drop reordering (future feature).</summary>
    public bool AllowDragDrop { get; set; } = false;

    /// <summary>Show expand/collapse toggles.</summary>
    public bool ShowToggle { get; set; } = true;

    /// <summary>Show checkboxes for selection.</summary>
    public bool ShowCheckboxes { get; set; } = false;

    /// <summary>Enable search/filter functionality.</summary>
    public bool AllowSearch { get; set; } = false;

    /// <summary>Indentation per level (in pixels).</summary>
    public int IndentationPixels { get; set; } = 20;

    /// <summary>Collapse siblings when expanding (only one open per parent).</summary>
    public bool ExpandSingleBranch { get; set; } = false;
}

/// <summary>Fired when tree node is selected.</summary>
public class TreeNodeSelectedEventArgs
{
    public required TreeNode SelectedNode { get; set; }
    public List<TreeNode> AllSelectedNodes { get; set; } = [];
    public bool IsMultiSelect { get; set; }
}
