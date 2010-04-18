using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shaml.Core.PersistenceSupport
{
    /// <summary>
    /// Allows storing propertynames for ordering. Should check propertyname is valid
    /// before usage
    /// </summary>
    public interface IPropertyOrder<T>
    {
        /// <summary>
        /// Should return true if the property exists for the appropriate entity
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Whether the ordering is a descending or an ascending one
        /// </summary>
        bool Desc { get; }

        /// <summary>
        /// The name of the property the ordering is defined
        /// </summary>
        string PropertyName { get; }
    }

    /// <summary>
    /// Holds an expression that will be converted to a Criteria later
    /// </summary>
    public interface IExpression
    {
        object GetExpression();
    }

    /// <summary>
    /// An Expression Builder that has to be declared in the Repository classes
    /// </summary>
    public interface IExpressionBuilder
    {
        // Simple Expressions
        IExpression Eq(string propertyName, object value);
        IExpression NEq(string propertyName, object value);
        IExpression Ge(string propertyName, object value);
        IExpression Gt(string propertyName, object value);
        IExpression Le(string propertyName, object value);
        IExpression Lt(string propertyName, object value);
        IExpression Like(string propertyName, object value);
        IExpression Like(string propertyName, object value, bool ignoreCase);

        // Property Expressions
        IExpression EqProperty(string leftProperty, string rightProperty);
        IExpression NEqProperty(string leftProperty, string rightProperty);
        IExpression GeProperty(string leftProperty, string rightProperty);
        IExpression GtProperty(string leftProperty, string rightProperty);
        IExpression LeProperty(string leftProperty, string rightProperty);
        IExpression LtProperty(string leftProperty, string rightProperty);

        // In, Between
        IExpression Between(string propertyName, object lo, object hi);
        IExpression In(string propertyName, object[] values);

        // Null checking
        IExpression Null(string propertyName);
        IExpression NotNull(string propertyName);

        // Logic
        IExpression Not(IExpression value);
        IExpression And(params IExpression[] values);
        IExpression Or(params IExpression[] values);
    }
}
