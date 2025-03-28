namespace MultiSoftSRB.Entities.Audit;

public class PropertyChange
{
    public string PropertyName { get; set; } = null!;
    public object? OldValue { get; set; }
    public object? NewValue { get; set; }
}