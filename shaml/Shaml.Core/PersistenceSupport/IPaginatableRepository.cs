using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shaml.Core.PersistenceSupport
{
    /// <summary>
    /// Adds pagination and ordering support for IRepository.
    /// 
    /// Since nearly all of the domain objects you create will have a type of int Id, this 
    /// base Idao leverages this assumption.  If you want an entity with a type 
    /// other than int, such as string, then use <see cref="IPaginatableRepositoryWithTypedId{T, IdT}" />.
    /// </summary>
    public interface IPaginatableRepository<T> : IPaginatableRepositoryWithTypedId<T, int>, IRepository<T> { }

    public interface IPaginatableRepositoryWithTypedId<T, IdT> : IRepositoryWithTypedId<T, IdT>
    {
        /// <summary>
        /// Returns all of the items of a given type paginated, with additional ordering.
        /// PageSize and page can be 0, which means no pagination will occur. 
        /// </summary>
        IList<T> GetAll(int pageSize, int page, params IPropertyOrder<T>[] ordering);

        /// <summary>
        /// Returns all of the items of a given type paginated and the number of results.
        /// PageSize and page can be 0, which means no pagination will occur. 
        /// </summary>
        IList<T> GetAll(int pageSize, int page, out long numResults, params IPropertyOrder<T>[] ordering);

        /// <summary>
        /// Looks for zero or more instances using the Anonymous Type provided.
        /// The key of the collection should be the property name and the value should be
        /// the value of the property to filter by. Paginated.
        /// PageSize and page can be 0, which means no pagination will occur. 
        /// </summary>
        IList<T> FindAll(object propertyValuePairs, int pageSize, int page, params IPropertyOrder<T>[] ordering);

        /// <summary>
        /// Looks for zero or more instances using the Anonymous Type provided.
        /// The key of the collection should be the property name and the value should be
        /// the value of the property to filter by. Paginated with the number of results.
        /// PageSize and page can be 0, which means no pagination will occur. 
        /// </summary>
        IList<T> FindAll(object propertyValuePairs, int pageSize, int page, out long numResults, params IPropertyOrder<T>[] ordering);

        /// <summary>
        /// Looks for zero or more instances using the <see cref="IDictionary{string, object}"/> provided.
        /// The key of the collection should be the property name and the value should be
        /// the value of the property to filter by. Paginated.
        /// PageSize and page can be 0, which means no pagination will occur. 
        /// </summary>
        IList<T> FindAll(IDictionary<string, object> propertyValuePairs, int pageSize, int page, params IPropertyOrder<T>[] ordering);

        /// <summary>
        /// Looks for zero or more instances using the <see cref="IDictionary{string, object}"/> provided.
        /// The key of the collection should be the property name and the value should be
        /// the value of the property to filter by. Paginated with the number of results.
        /// PageSize and page can be 0, which means no pagination will occur. 
        /// </summary>
        IList<T> FindAll(IDictionary<string, object> propertyValuePairs, int pageSize, int page, out long numResults, params IPropertyOrder<T>[] ordering);

        /// <summary>
        /// Creates a new IPropertyOrder object, that sotres how the results shold be ordered
        /// </summary>
        /// <param name="propertyName">The name of the property to order by</param>
        /// <param name="isDesc">Whetther to order by ASC or DESC</param>
        /// <returns>The IPropertyOrder instance</returns>
        IPropertyOrder<T> CreateOrder(string propertyName, bool isDesc);
    }
}
