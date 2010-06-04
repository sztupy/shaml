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
using System.Text.RegularExpressions;

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

        public T FindOneByQuery(string query, params object[] parameters)
        {
            IList<T> foundList = FindByQuery(query,1,0,parameters);

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

        public IList<T> FindByQuery(string query, params object[] parameters)
        {
            return FindByQuery(query, 0, 0,parameters);
        }

        public IList<T> FindByQuery(string query, int pageSize, int page, params object[] parameters)
        {
            IQuery q = Session.CreateQuery(query);
            if ((pageSize > 0) && (page >= 0))
            {
                q.SetFirstResult(page * pageSize);
                q.SetMaxResults(pageSize);
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                q.SetEntity(i, parameters[i]);
            }
            return q.List<T>();
        }

        public IList<T> FindByQuery(string query, int pageSize, int page, out long numResults, params object[] parameters)
        {
            IMultiQuery mq = Session.CreateMultiQuery();
            IQuery q = Session.CreateQuery(query);
            if ((pageSize > 0) && (page >= 0))
            {
                q.SetFirstResult(page * pageSize);
                q.SetMaxResults(pageSize);
            }
            for (int i=0; i < parameters.Length; i++)
            {
                q.SetEntity(i, parameters[i]);
            }
            mq.Add(q);

            // Change select part of the original query

            string newquery = "select count(*) " + Regex.Replace(query, "select .*? from", "from");

            IQuery q2 = Session.CreateQuery("select count(*) " + query);
            for (int i = 0; i < parameters.Length; i++)
            {
                q2.SetEntity(i, parameters[i]);
            }
            mq.Add(q2);
            IList results = mq.List();
            numResults = (long)((IList)results[1])[0];
            return ((IList)results[0]).Cast<T>().ToList<T>();
        }

        public T FindOneByCriteria(object criteria)
        {
            IList<T> foundList = FindByCriteria(criteria,1,0);

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

        public IList<T> FindByCriteria(object criteria, int pageSize, int page, params IPropertyOrder<T>[] order)
        {
            Check.Require(criteria is DetachedCriteria, "The criteria should be a DetachedCrtieria!");
            DetachedCriteria cr = CriteriaTransformer.Clone(criteria as DetachedCriteria);
            AddOrderingsToCriteria(cr, order);

            if ((pageSize >= 0) && (page > 0))
            {
                cr.SetFirstResult(page*pageSize);
                cr.SetMaxResults(pageSize);
            }
            return cr.GetExecutableCriteria(Session).List<T>();
        }

        public IList<T> FindByCriteria(object criteria, int pageSize, int page, out long numResults, params IPropertyOrder<T>[] order)
        {
            Check.Require(criteria is DetachedCriteria, "The criteria should be a DetachedCrtieria!");
            DetachedCriteria cr = CriteriaTransformer.Clone(criteria as DetachedCriteria);
            DetachedCriteria crmaxres = CriteriaTransformer.Clone(criteria as DetachedCriteria);

            AddOrderingsToCriteria(cr, order);


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
