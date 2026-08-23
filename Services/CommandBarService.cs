namespace HandWStat.Services;

public sealed class CommandBarService
{
    private Func<Task>? _filterAction;
    private int? _itemCount;
    private string? _itemLabel;

    public event Action? Changed;

    public bool HasFilter => _filterAction is not null;
    public int? ItemCount => _itemCount;
    public string? ItemLabel => _itemLabel;

    public void Register(Func<Task> filterAction, int? itemCount = null, string? itemLabel = null)
    {
        _filterAction = filterAction;
        _itemCount = itemCount;
        _itemLabel = itemLabel;
        Changed?.Invoke();
    }

    public void UpdateCount(int? count, string? label = null)
    {
        _itemCount = count;
        _itemLabel = label ?? _itemLabel;
        Changed?.Invoke();
    }

    public void Clear()
    {
        _filterAction = null;
        _itemCount = null;
        _itemLabel = null;
        Changed?.Invoke();
    }

    public Task InvokeFilterAsync() => _filterAction?.Invoke() ?? Task.CompletedTask;
}
