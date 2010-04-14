using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shaml.Core.PersistenceSupport.NHibernate
{
    /// <summary>
    /// Extends the NHibernate data repository interface with an interface that supports Querying
    /// using a query string, or a DetachedCriteria object. the latter will check that the parameter is
    /// really a DetachedCriteria at runtime
    /// </summary>
    public interface INHibernateQueryRepository<T> : INHibernateQueryRepositoryWithTypedId<T, int>, INHibernateRepository<T> { }

    public interface INHibernateQueryRepositoryWithTypedId<T, IdT> : INHibernateRepositoryWithTypedId<T, IdT>
    {
        /// <summary>
        /// Looks for a single instance using the query string provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        T FindOneByQuery(string query);

        /// <summary>
        /// Looks for zero or more instances using the query string provided.
        /// </summary>
        IList<T> FindByQuery(string query);

        /// <summary>
        /// Looks for zero or more instances using the query string provided. Paginated.
        /// PageSize and page can be 0, which means no pagination will occur.
        /// </summary>
        IList<T> FindByQuery(string query, int pageSize, int page);

        /// <summary>
        /// Looks for zero or more instances using the query string provided. Paginated with the number of results.
        /// PageSize and page can be 0, which means no pagination will occur. 
        /// </summary>
        IList<T> FindByQuery(string query, int pageSize, int page, out long numResults);

        /// <summary>
        /// Looks for a single instance using the DetachedCriteria provided.
        /// </summary>
        /// <exception cref="NonUniqueResultException" />
        T FindOneByCriteria(object criteria);

        /// <summary>
        /// Looks for zero or more instances using the DetachedCriteria provided.
        /// </summary>
        IList<T> FindByCriteria(object criteria);

        /// <summary>
        /// Looks for zero or more instances using the DetachedCriteria provided. Paginated.
        /// PageSize and page can be 0, which means no pagination will occur.
        /// </summary>
        IList<T> FindByCriteria(object criteria, int pageSize, int page, params IPropertyOrder<T>[] order);

        /// <summary>
        /// Looks for zero or more instances using the DetachedCriteria provided. Paginated with the number of results.
        /// PageSize and page can be 0, which means no pagination will occur.
        /// </summary>
        IList<T> FindByCriteria(object criteria, int pageSize, int page, out long numResults, params IPropertyOrder<T>[] order);
    }
}
