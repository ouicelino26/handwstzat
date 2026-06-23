namespace HandWStat.Models.Analytics;

public enum SortDirection
{
    Asc,
    Desc
}

public readonly record struct TableSortState(string Column, SortDirection Direction = SortDirection.Desc)
{
    public bool IsActive(string column) => string.Equals(Column, column, StringComparison.OrdinalIgnoreCase);

    public bool IsDescending => Direction == SortDirection.Desc;

    public TableSortState Toggle(string column, SortDirection defaultDirection = SortDirection.Desc)
    {
        if (!IsActive(column))
        {
            return new TableSortState(column, defaultDirection);
        }

        return new TableSortState(
            column,
            IsDescending ? SortDirection.Asc : SortDirection.Desc);
    }

    public string Glyph(string column)
    {
        if (!IsActive(column))
        {
            return string.Empty;
        }

        return IsDescending ? "↓" : "↑";
    }

    public string SortButtonLabel(string column, string label)
    {
        if (!IsActive(column))
        {
            return $"Trier {label} par ordre croissant";
        }

        var currentOrder = IsDescending ? "décroissant" : "croissant";
        var nextOrder = IsDescending ? "croissant" : "décroissant";
        return $"{label}, tri {currentOrder}. Activer pour trier par ordre {nextOrder}.";
    }
}

public static class TextSearchHelper
{
    public static bool Contains(string? value, string query)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains(query, StringComparison.OrdinalIgnoreCase);
    }
}
