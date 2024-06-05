using System;

namespace Maggsoft.Core.Model.DataTables;

/// <summary>
/// </summary>
/// <param name="name">example Operators.Equal</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property,
    AllowMultiple = true)]
public class DTFilterOperation(string name) : Attribute
{
    /// <summary>
    /// Operators.Equal = "eq";
    /// Operators.NotEqual = "neq";
    /// Operators.IsNull = "isnull";
    /// Operators.IsNotNull = "isnotnull";
    /// Operators.StartsWith = "startswith";
    /// Operators.Contains = "contains";
    /// Operators.EndsWith = "endswith";
    /// Operators.DoesNotContain = "doesnotcontain";
    /// Operators.GreaterThan = "gt";
    /// Operators.GreaterThanOrEqual = "gte";
    /// Operators.LessThan = "lt";
    /// Operators.LessThanOrEqual = "lte";
    /// </summary>
    public string Name { get; } = name;
}
