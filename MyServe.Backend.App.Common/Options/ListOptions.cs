using Serilog;

namespace MyServe.Backend.Common.Options;

public abstract class ListOptions
{
    public const string OrderByAsc = "ASC";
    public const string OrderByDesc = "DESC";
    
    private string? _orderBy ;

    protected ListOptions()
    {
        
    }

    public string Search { get; set; } = string.Empty;
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;

    public string OrderBy
    {
        get => string.IsNullOrWhiteSpace(_orderBy) ? DefaultColumn : _orderBy;
        set
        {
            if (!Columns.Contains(value))
            {
                _orderBy = DefaultColumn;
                Log.Logger.Warning($"Wrong column name has been provided for {GetType().Name} as a value for {value}");
            }
            
            _orderBy = value;
        }
    }

    public string OrderDirection { get; set; } = OrderByAsc;
    
    protected abstract HashSet<string> Columns { get; }
    protected abstract string DefaultColumn { get; }
}