namespace SmartWorkz.Core.Web.Services;

using SmartWorkz.Core.Web.Components.Data.Models;
using System.Reflection;

/// <summary>Service for table data processing and manipulation.</summary>
public interface ITableDataService
{
    Task<PagedResult<T>> ApplyFiltersAsync<T>(IEnumerable<T> data, TableRequest request)
        where T : class;

    Task<List<T>> ApplySortingAsync<T>(IEnumerable<T> data, TableRequest request)
        where T : class;

    Task<PagedResult<T>> ApplyPaginationAsync<T>(List<T> data, TableRequest request)
        where T : class;
}

/// <summary>Service for tree view operations.</summary>
public interface ITreeViewService
{
    List<TreeNode> FlattenTree(TreeNode root);
    TreeNode? FindNodeById(List<TreeNode> roots, string nodeId);
    List<TreeNode> SearchNodes(List<TreeNode> roots, string searchQuery);
    List<TreeNode> FilterByProperty(List<TreeNode> roots, string propertyName, object value);
}

/// <summary>Service for formatting cell values in tables and lists.</summary>
public interface IDataFormatterService
{
    string FormatCurrency(decimal? value, string currencySymbol = "$");
    string FormatDate(DateTime? date, string format = "MMM dd, yyyy");
    string FormatPercentage(decimal? value, int decimalPlaces = 1);
    string FormatBytes(long bytes);
    string FormatBoolean(bool value, string trueText = "Yes", string falseText = "No");
    string FormatTimeSpan(TimeSpan span);
}

/// <summary>Default implementation of ITableDataService.</summary>
public sealed class TableDataService : ITableDataService
{
    /// <summary>Apply filters to data based on table request.</summary>
    public Task<PagedResult<T>> ApplyFiltersAsync<T>(IEnumerable<T> data, TableRequest request)
        where T : class
    {
        var filtered = data.AsEnumerable();

        if (!string.IsNullOrEmpty(request.FilterValue) && !string.IsNullOrEmpty(request.FilterColumn))
        {
            var property = typeof(T).GetProperty(request.FilterColumn,
                BindingFlags.IgnoreCase | BindingFlags.Public);

            if (property != null)
            {
                filtered = filtered.Where(item =>
                {
                    var value = property.GetValue(item);
                    return value?.ToString()?.Contains(request.FilterValue,
                        StringComparison.OrdinalIgnoreCase) == true;
                });
            }
        }

        return Task.FromResult(new PagedResult<T>
        {
            Items = filtered.ToList(),
            TotalCount = filtered.Count(),
            CurrentPage = request.Page,
            PageSize = request.PageSize
        });
    }

    /// <summary>Apply sorting to data based on table request.</summary>
    public Task<List<T>> ApplySortingAsync<T>(IEnumerable<T> data, TableRequest request)
        where T : class
    {
        if (string.IsNullOrEmpty(request.SortBy))
            return Task.FromResult(data.ToList());

        var property = typeof(T).GetProperty(request.SortBy,
            BindingFlags.IgnoreCase | BindingFlags.Public);

        if (property == null)
            return Task.FromResult(data.ToList());

        var sorted = request.SortDescending
            ? data.OrderByDescending(x => property.GetValue(x))
            : data.OrderBy(x => property.GetValue(x));

        return Task.FromResult(sorted.ToList());
    }

    /// <summary>Apply pagination to data.</summary>
    public Task<PagedResult<T>> ApplyPaginationAsync<T>(List<T> data, TableRequest request)
        where T : class
    {
        var totalCount = data.Count;
        var totalPages = (totalCount + request.PageSize - 1) / request.PageSize;
        var validPage = Math.Max(1, Math.Min(request.Page, totalPages));

        var items = data
            .Skip((validPage - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = validPage,
            PageSize = request.PageSize
        });
    }
}

/// <summary>Default implementation of ITreeViewService.</summary>
public sealed class TreeViewService : ITreeViewService
{
    /// <summary>Flatten hierarchical tree to single list.</summary>
    public List<TreeNode> FlattenTree(TreeNode root)
    {
        var result = new List<TreeNode> { root };

        if (root.Children != null)
        {
            foreach (var child in root.Children)
            {
                result.AddRange(FlattenTree(child));
            }
        }

        return result;
    }

    /// <summary>Find node by ID in tree.</summary>
    public TreeNode? FindNodeById(List<TreeNode> roots, string nodeId)
    {
        foreach (var root in roots)
        {
            var found = FindNodeByIdRecursive(root, nodeId);
            if (found != null)
                return found;
        }

        return null;
    }

    private TreeNode? FindNodeByIdRecursive(TreeNode node, string nodeId)
    {
        if (node.Id == nodeId)
            return node;

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                var found = FindNodeByIdRecursive(child, nodeId);
                if (found != null)
                    return found;
            }
        }

        return null;
    }

    /// <summary>Search nodes by label/query.</summary>
    public List<TreeNode> SearchNodes(List<TreeNode> roots, string searchQuery)
    {
        var results = new List<TreeNode>();

        foreach (var root in roots)
        {
            SearchNodesRecursive(root, searchQuery, results);
        }

        return results;
    }

    private void SearchNodesRecursive(TreeNode node, string query, List<TreeNode> results)
    {
        if (node.Label.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            results.Add(node);
        }

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                SearchNodesRecursive(child, query, results);
            }
        }
    }

    /// <summary>Filter nodes by property value.</summary>
    public List<TreeNode> FilterByProperty(List<TreeNode> roots, string propertyName, object value)
    {
        var results = new List<TreeNode>();

        foreach (var root in roots)
        {
            FilterByPropertyRecursive(root, propertyName, value, results);
        }

        return results;
    }

    private void FilterByPropertyRecursive(TreeNode node, string propertyName, object filterValue, List<TreeNode> results)
    {
        if (node.Data != null)
        {
            var property = node.Data.GetType().GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public);

            if (property != null)
            {
                var nodeValue = property.GetValue(node.Data);
                if (nodeValue?.Equals(filterValue) == true)
                {
                    results.Add(node);
                }
            }
        }

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                FilterByPropertyRecursive(child, propertyName, filterValue, results);
            }
        }
    }
}

/// <summary>Default implementation of IDataFormatterService.</summary>
public sealed class DataFormatterService : IDataFormatterService
{
    /// <summary>Format decimal as currency.</summary>
    public string FormatCurrency(decimal? value, string currencySymbol = "$")
    {
        if (!value.HasValue)
            return "-";

        return $"{currencySymbol}{value:N2}";
    }

    /// <summary>Format date.</summary>
    public string FormatDate(DateTime? date, string format = "MMM dd, yyyy")
    {
        if (!date.HasValue)
            return "-";

        return date.Value.ToString(format);
    }

    /// <summary>Format as percentage.</summary>
    public string FormatPercentage(decimal? value, int decimalPlaces = 1)
    {
        if (!value.HasValue)
            return "-";

        return $"{value:F{decimalPlaces}}%";
    }

    /// <summary>Format bytes as human-readable.</summary>
    public string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:F2} {sizes[order]}";
    }

    /// <summary>Format boolean value.</summary>
    public string FormatBoolean(bool value, string trueText = "Yes", string falseText = "No")
    {
        return value ? trueText : falseText;
    }

    /// <summary>Format TimeSpan.</summary>
    public string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalSeconds < 1)
            return "Just now";
        if (span.TotalMinutes < 1)
            return $"{(int)span.TotalSeconds}s ago";
        if (span.TotalHours < 1)
            return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalDays < 1)
            return $"{(int)span.TotalHours}h ago";

        return $"{(int)span.TotalDays}d ago";
    }
}
