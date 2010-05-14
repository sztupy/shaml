using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shaml.Core.PersistenceSupport
{
    /// <summary>
    /// Adds basic expression query support for IPaginatableRepository
    /// 
    /// Since nearly all of the domain objects you create will have a type of int Id, this 
    /// base Idao leverages this assumption.  If you want an entity with a type 
    /// other than int, such as string, then use <see cref="IExpressionRepositoryWithTypedId{T, IdT}" />.
    /// </summary>
    public interface IExpressionRepository<T> : IExpressionRepositoryWithTypedId<T, int>, IPaginatableRepository<T> { }

    public interface IExpressionRepositoryWithTypedId<T, IdT> : IPaginatableRepositoryWithTypedId<T, IdT>
    {
        /// <summary>
        /// Looks for zero or more instances using the <see cref="IDictionary{string, object}"/> provided.
        /// The key of the collection should be the property name and the value should be
        /// the value of the property to filter by.
        /// </summary>
        IList<T> FindByExpression(IExpression expression);

        /// <summary>
        /// Looks for zero or more instances using the <see cref="IDictionary{string, object}"/> provided.
        /// The key of the collection should be the property name and the value should be
        /// the value of the property to filter by. Paginated.
        /// PageSize and page can be 0, which means no pagination will occur. 
        /// </summary>
        IList<T> FindByExpression(IExpression expression, int pageSize, int page, params IPropertyOrder<T>[] ordering);

        /// <summary>
        /// Looks for zero or more instances using the <see cref="IDictionary{string, object}"/> provided.
        /// The key of the collection should be the property name and the value should be
        /// the value of the property to filter by. Paginated with the number of results.
        /// PageSize and page can be 0, which means no pagination will occur. 
        /// </summary>
        IList<T> FindByExpression(IExpression expression, int pageSize, int page, out long numResults, params IPropertyOrder<T>[] ordering);

        /// <summary>
        /// Looks for a single instance using the property/values provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        T FindOneByExpression(IExpression expression);

        /// <summary>
        /// Creates an Expression Builder to create Expression objects
        /// </summary>
        /// <returns>The IExpressionBuilder instance</returns>
        IExpressionBuilder CreateExpressionBuilder();
    }
}
