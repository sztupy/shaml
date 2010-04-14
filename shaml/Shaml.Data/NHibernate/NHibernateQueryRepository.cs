using System.Collections.Generic;
using NHibernate;
using System.Reflection;
using System.Linq;
using Shaml.Core;
using Shaml.Core.PersistenceSupport;
using NHibernate.Criterion;
using System.Collections.Specialized;
using System;
using Shaml.Core.PersistenceSupport.NHibernate;
using System.Collections;

namespace Shaml.Data.NHibernate
{
    /// <summary>
    /// Since nearly all of the domain objects you create will have a type of int Id, this 
    /// most freqently used base GenericDao leverages this assumption.  If you want an entity 
    /// with a type other than int, such as string, then use 
    /// <see cref="GenericDaoWithTypedId{T, IdT}" />.
    /// </summary>
    public class NHibernateQueryRepository<T> : NHibernateQueryRepositoryWithTypedId<T, int>, INHibernateQueryRepository<T> { }

    /// <summary>
    /// Provides a fully loaded DAO which may be created in a few ways including:
    /// * Direct instantiation; e.g., new GenericDao<Customer, string>
    /// * Spring configuration; e.g., <object id="CustomerDao" type="Shaml.Data.NHibernateSupport.GenericDao&lt;CustomerAlias, string>, Shaml.Data" autowire="byName" />
    /// </summary>
    public class NHibernateQueryRepositoryWithTypedId<T, IdT> : NHibernateRepositoryWithTypedId<T, IdT>, INHibernateQueryRepositoryWithTypedId<T, IdT>
    {

        public T FindOneByQuery(string query)
        {
            IList<T> foundList = FindByQuery(query);

            if (foundList.Count > 1)
            {
                throw new NonUniqueResultException(foundList.Count);
            }
            else if (foundList.Count == 1)
            {
                return foundList[0];
            }

            return default(T);
        }

        public IList<T> FindByQuery(string query)
        {
            return FindByQuery(query, 0, 0);
        }

        public IList<T> FindByQuery(string query, int pageSize, int page)
        {
            IQuery q = Session.CreateQuery(query);
            if ((pageSize > 0) && (page >= 0))
            {
                q.SetFirstResult(page * pageSize);
                q.SetMaxResults(pageSize);
            }
            return q.List<T>();
        }

        public IList<T> FindByQuery(string query, int pageSize, int page, out long numResults)
        {
            IMultiQuery mq = Session.CreateMultiQuery();
            IQuery q = Session.CreateQuery(query);
            if ((pageSize > 0) && (page >= 0))
            {
                q.SetFirstResult(page * pageSize);
                q.SetMaxResults(pageSize);
            }
            mq.Add(q);
            mq.Add(Session.CreateQuery("select count(*) " + query));
            IList results = mq.List();
            numResults = (long)((IList)results[1])[0];
            return ((IList)results[0]).Cast<T>().ToList<T>();
        }

        public T FindOneByCriteria(object criteria)
        {
            IList<T> foundList = FindByCriteria(criteria);

            if (foundList.Count > 1)
            {
                throw new NonUniqueResultException(foundList.Count);
            }
            else if (foundList.Count == 1)
            {
                return foundList[0];
            }

            return default(T);
        }

        public IList<T> FindByCriteria(object criteria)
        {
            return FindByCriteria(criteria, 0, 0);
        }

        public IList<T> FindByCriteria(object criteria, int pageSize, int page)
        {
            Check.Require(criteria is DetachedCriteria, "The criteria should be a DetachedCrtieria!");
            DetachedCriteria cr = CriteriaTransformer.Clone(criteria as DetachedCriteria);

            if ((pageSize >= 0) && (page > 0))
            {
                cr.SetFirstResult(page*pageSize);
                cr.SetMaxResults(pageSize);
            }
            return cr.GetExecutableCriteria(Session).List<T>();
        }

        public IList<T> FindByCriteria(object criteria, int pageSize, int page, out long numResults)
        {
            Check.Require(criteria is DetachedCriteria, "The criteria should be a DetachedCrtieria!");
            DetachedCriteria cr = CriteriaTransformer.Clone(criteria as DetachedCriteria);
            DetachedCriteria crmaxres = CriteriaTransformer.Clone(criteria as DetachedCriteria);

            if ((pageSize >= 0) && (page > 0))
            {
                cr.SetFirstResult(page * pageSize);
                cr.SetMaxResults(pageSize);
            }
            crmaxres.SetProjection(Projections.RowCountInt64());

            IMultiCriteria mc = Session.CreateMultiCriteria();
            mc.Add(cr);
            mc.Add(crmaxres);

            IList results = mc.List();
            numResults = (long)((IList)results[1])[0];
            return ((IList)results[0]).Cast<T>().ToList<T>();
        }
    }
}
