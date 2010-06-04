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
    public class NHibernateRepository<T> : NHibernateRepositoryWithTypedId<T, int>, INHibernateRepository<T> { }

    /// <summary>
    /// Provides a fully loaded DAO which may be created in a few ways including:
    /// * Direct instantiation; e.g., new GenericDao<Customer, string>
    /// * Spring configuration; e.g., <object id="CustomerDao" type="Shaml.Data.NHibernateSupport.GenericDao&lt;CustomerAlias, string>, Shaml.Data" autowire="byName" />
    /// </summary>
    public class NHibernateRepositoryWithTypedId<T, IdT> : RepositoryWithTypedId<T, IdT>, INHibernateRepositoryWithTypedId<T, IdT>
    {
        public virtual T Get(IdT id, Enums.LockMode lockMode) {
            return Session.Get<T>(id, ConvertFrom(lockMode));
        }

        public virtual T Load(IdT id) {
            return Session.Load<T>(id);
        }

        public virtual T Load(IdT id, Enums.LockMode lockMode) {
            return Session.Load<T>(id, ConvertFrom(lockMode));
        }

        public virtual IList<T> FindAll(T exampleInstance, params string[] propertiesToExclude) {
            return FindAll(exampleInstance, 0, 0, propertiesToExclude);
        }

        public virtual IList<T> FindAll(T exampleInstance, int pageSize, int page, params string[] propertiesToExclude)
        {
            ICriteria criteria = Session.CreateCriteria(typeof(T));
            Example example = Example.Create(exampleInstance);

            foreach (string propertyToExclude in propertiesToExclude)
            {
                example.ExcludeProperty(propertyToExclude);
            }
            criteria.Add(example);
            if ((pageSize > 0) && (page >= 0))
            {
                criteria.SetFirstResult(pageSize * page);
                criteria.SetMaxResults(pageSize);
            }
            return criteria.List<T>();
        }

        public virtual IList<T> FindAll(T exampleInstance, int pageSize, int page, out long numResults, params string[] propertiesToExclude)
        {
            IMultiCriteria mcriteria = Session.CreateMultiCriteria();
            ICriteria criteria = Session.CreateCriteria(typeof(T));
            Example example = Example.Create(exampleInstance);

            foreach (string propertyToExclude in propertiesToExclude)
            {
                example.ExcludeProperty(propertyToExclude);
            }

            criteria.Add(example);
            if ((pageSize > 0) && (page >= 0))
            {
                criteria.SetFirstResult(pageSize * page);
                criteria.SetMaxResults(pageSize);
            }
            mcriteria.Add(criteria);
            mcriteria.Add(Session.CreateCriteria(typeof(T)).SetProjection(Projections.RowCountInt64()));

            IList results = mcriteria.List();
            numResults = (long)((IList)results[1])[0];
            return ((IList)results[0]).Cast<T>().ToList<T>();
        }

        public virtual T FindOne(T exampleInstance, params string[] propertiesToExclude) {
            IList<T> foundList = FindAll(exampleInstance, 1,0, propertiesToExclude);

            if (foundList.Count > 1) {
                throw new NonUniqueResultException(foundList.Count);
            }
            else if (foundList.Count == 1) {
                return foundList[0];
            }

            return default(T);
        }

        public virtual T Save(T entity) {
            Session.Save(entity);
            return entity;
        }

        public virtual T Update(T entity) {
            Session.Update(entity);
            return entity;
        }

        public virtual void Evict(T entity) {
            Session.Evict(entity);
        }

        /// <summary>
        /// Translates a domain layer lock mode into an NHibernate lock mode via reflection.  This is 
        /// provided to facilitate developing the domain layer without a direct dependency on the 
        /// NHibernate assembly.
        /// </summary>
        private LockMode ConvertFrom(Enums.LockMode lockMode) {
            FieldInfo translatedLockMode = typeof(LockMode).GetField(lockMode.ToString(),
                BindingFlags.Public | BindingFlags.Static);

            Check.Ensure(translatedLockMode != null, "The provided lock mode , '" + lockMode + ",' " +
                    "could not be translated into an NHibernate.LockMode. This is probably because " +
                    "NHibernate was updated and now has different lock modes which are out of synch " +
                    "with the lock modes maintained in the domain layer.");

            return (LockMode)translatedLockMode.GetValue(null);
        }
    }
}
