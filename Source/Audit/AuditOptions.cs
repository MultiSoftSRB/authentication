using System.Linq.Expressions;

namespace MultiSoftSRB.Audit;

public class AuditOptions
{
    private readonly HashSet<Type> _excludedEntities = new();
    private readonly Dictionary<Type, HashSet<string>> _excludedProperties = new();

    public void ExcludeEntity<TEntity>()
    {
        _excludedEntities.Add(typeof(TEntity));
    }
    
    // In AuditOptions class
    public void ExcludeProperty<TEntity>(Expression<Func<TEntity, object>> propertyExpression)
    {
        if (propertyExpression.Body is MemberExpression memberExpression)
        {
            string propertyName = memberExpression.Member.Name;
            var entityType = typeof(TEntity);
        
            if (!_excludedProperties.TryGetValue(entityType, out var properties))
            {
                properties = new HashSet<string>();
                _excludedProperties[entityType] = properties;
            }
        
            properties.Add(propertyName);
        }
        else if (propertyExpression.Body is UnaryExpression unaryExpression && 
                 unaryExpression.Operand is MemberExpression operandExpression)
        {
            // This handles value types that get boxed to object
            string propertyName = operandExpression.Member.Name;
            var entityType = typeof(TEntity);
        
            if (!_excludedProperties.TryGetValue(entityType, out var properties))
            {
                properties = new HashSet<string>();
                _excludedProperties[entityType] = properties;
            }
        
            properties.Add(propertyName);
        }
        else
        {
            throw new ArgumentException("Expression must be a property access expression", nameof(propertyExpression));
        }
    }

    public void ExcludeProperty<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
    {
        if (propertyExpression.Body is MemberExpression memberExpression)
        {
            string propertyName = memberExpression.Member.Name;
            var entityType = typeof(TEntity);
            
            if (!_excludedProperties.TryGetValue(entityType, out var properties))
            {
                properties = new HashSet<string>();
                _excludedProperties[entityType] = properties;
            }
            
            properties.Add(propertyName);
        }
        else
        {
            throw new ArgumentException("Expression must be a property access expression", nameof(propertyExpression));
        }
    }
    

    public bool IsEntityExcluded(Type entityType)
    {
        return _excludedEntities.Contains(entityType);
    }

    public bool IsPropertyExcluded(Type entityType, string propertyName)
    {
        if (_excludedProperties.TryGetValue(entityType, out var properties))
        {
            return properties.Contains(propertyName);
        }
        return false;
    }
}