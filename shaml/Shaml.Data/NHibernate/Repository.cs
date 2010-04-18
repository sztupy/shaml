using System.Linq;
using System.Collections.Generic;
using NHibernate;
using System.Reflection;
using Shaml.Core;
using Shaml.Core.PersistenceSupport;
using NHibernate.Criterion;
using System.Collections.Specialized;
using System;
using Shaml.Core.PersistenceSupport.NHibernate;
using Shaml.Core.DomainModel;
using System.Collections;
using NHibernate.Metadata;
using System.ComponentModel;

namespace Shaml.Data.NHibernate
{
    /// <summary>
    /// Since nearly all of the domain objects you create will have a type of int Id, this 
    /// most freqently used base GenericDao leverages this assumption.  If you want an entity
    /// with a type other than int, such as string, then use 
    /// <see cref="GenericDaoWithTypedId{T, IdT}" />.
    /// </summary>
    public class Repository<T> : RepositoryWithTypedId<T, int>, IRepository<T> { }

    /// <summary>
    /// Provides a fully loaded DAO which may be created in a few ways including:
    /// * Direct instantiation; e.g., new GenericDao<Customer, string>
    /// * Spring configuration; e.g., <object id="CustomerDao" type="Shaml.Data.NHibernateSupport.GenericDao&lt;CustomerAlias, string>, Shaml.Data" autowire="byName" />
    /// </summary>
    public class RepositoryWithTypedId<T, IdT> : IRepositoryWithTypedId<T, IdT>
    {
        protected virtual ISession Session {
            get {
                string factoryKey = SessionFactoryAttribute.GetKeyFrom(this);
                return NHibernateSession.CurrentFor(factoryKey);
            }
        }

        public virtual IDbContext DbContext {
            get {
                if (dbContext == null) {
                    string factoryKey = SessionFactoryAttribute.GetKeyFrom(this);
                    dbContext = new DbContext(factoryKey);
                }

                return dbContext;
            }
        }

        protected void AddOrderingsToCriteria(ICriteria criteria, params IPropertyOrder<T>[] order)
        {
            foreach (IPropertyOrder<T> i in order)
            {
                if (i.IsValid)
                {
                    if (i.Desc)
                    {
                        criteria.AddOrder(Order.Desc(i.PropertyName));
                    }
                    else
                    {
                        criteria.AddOrder(Order.Asc(i.PropertyName));
                    }
                }
            }
        }

        protected void AddOrderingsToCriteria(DetachedCriteria criteria, params IPropertyOrder<T>[] order)
        {
            foreach (IPropertyOrder<T> i in order)
            {
                if (i.IsValid)
                {
                    if (i.Desc)
                    {
                        criteria.AddOrder(Order.Desc(i.PropertyName));
                    }
                    else
                    {
                        criteria.AddOrder(Order.Asc(i.PropertyName));
                    }
                }
            }
        }


        public virtual T Get(IdT id) {
            return Session.Get<T>(id);
        }

        public virtual IList<T> GetAll() {
            ICriteria criteria = Session.CreateCriteria(typeof(T));
            return criteria.List<T>();
        }

        public virtual IList<T> GetAll(int pageSize, int page, params IPropertyOrder<T>[] order)
        {
            ICriteria criteria;
            if ((pageSize <= 0) || (page < 0))
            {
                criteria = Session.CreateCriteria(typeof(T));
            }
            else
            {
                criteria = Session.CreateCriteria(typeof(T)).SetMaxResults(page).SetFirstResult(pageSize * page);
            }
            AddOrderingsToCriteria(criteria, order);
            return criteria.List<T>();
        }

        public virtual IList<T> GetAll(int pageSize, int page, out long numResults, params IPropertyOrder<T>[] order)
        {
            ICriteria criteria;
            if ((pageSize <= 0) || (page < 0))
            {
                criteria = Session.CreateCriteria(typeof(T));
            }
            else
            {
                criteria = Session.CreateCriteria(typeof(T)).SetMaxResults(page).SetFirstResult(pageSize * page);
            }
            AddOrderingsToCriteria(criteria, order);
            IMultiCriteria multicriteria = Session.CreateMultiCriteria()
                        .Add(criteria)
                        .Add(Session.CreateCriteria(typeof(T)).SetProjection(Projections.RowCountInt64()));

            IList results = multicriteria.List();
            numResults = (long)((IList)results[1])[0];
            return ((IList)results[0]).Cast<T>().ToList<T>();
        }

        private IDictionary<string,object> MakeDictionary(object withProperties)
        {
            IDictionary<string,object> dic = new Dictionary<string, object>();
            var properties = System.ComponentModel.TypeDescriptor.GetProperties(withProperties);
            foreach (PropertyDescriptor property in properties)
            {
                dic.Add(property.Name, property.GetValue(withProperties));
            }
            return dic;
        }

        public virtual IList<T> FindAll(object propertyValuePairs)
        {
            return FindAll(propertyValuePairs, 0, 0);
        }

        public virtual IList<T> FindAll(object propertyValuePairs, int pageSize, int page, params IPropertyOrder<T>[] order)
        {
            IDictionary<string, object> dic = MakeDictionary(propertyValuePairs);
            return FindAll(dic, pageSize, page, order);
        }

        public virtual IList<T> FindAll(object propertyValuePairs, int pageSize, int page, out long numResults, params IPropertyOrder<T>[] order)
        {
            IDictionary<string, object> dic = MakeDictionary(propertyValuePairs);
            return FindAll(dic, pageSize, page, out numResults, order);
        }

        public virtual T FindOne(object propertyValuePairs)
        {
            IList<T> foundList = FindAll(propertyValuePairs);

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

        public virtual IList<T> FindAll(IDictionary<string, object> propertyValuePairs) {
            return FindAll(propertyValuePairs, 0, 0);
        }

        public virtual IList<T> FindAll(IDictionary<string, object> propertyValuePairs, int pageSize, int page, params IPropertyOrder<T>[] order)
        {
            Check.Require(propertyValuePairs != null && propertyValuePairs.Count > 0,
                "propertyValuePairs was null or empty; " +
                "it has to have at least one property/value pair in it");

            ICriteria criteria = Session.CreateCriteria(typeof(T));
            AddOrderingsToCriteria(criteria, order);

            foreach (string key in propertyValuePairs.Keys)
            {
                if (propertyValuePairs[key] != null)
                {
                    criteria.Add(Expression.Eq(key, propertyValuePairs[key]));
                }
                else
                {
                    criteria.Add(Expression.IsNull(key));
                }
            }
            if ((pageSize > 0) && (page >= 0))
            {
                criteria.SetFirstResult(page * pageSize).SetMaxResults(pageSize);
            }
            return criteria.List<T>();
        }

        public virtual IList<T> FindAll(IDictionary<string, object> propertyValuePairs, int pageSize, int page, out long numResults, params IPropertyOrder<T>[] order)
        {
            Check.Require(propertyValuePairs != null && propertyValuePairs.Count > 0,
                "propertyValuePairs was null or empty; " +
                "it has to have at least one property/value pair in it");

            IMultiCriteria multicriteria = Session.CreateMultiCriteria();
            ICriteria criteria = Session.CreateCriteria(typeof(T));
            AddOrderingsToCriteria(criteria, order);

            foreach (string key in propertyValuePairs.Keys)
            {
                if (propertyValuePairs[key] != null)
                {
                    criteria.Add(Expression.Eq(key, propertyValuePairs[key]));
                }
                else
                {
                    criteria.Add(Expression.IsNull(key));
                }
            }
            if ((pageSize > 0) && (page >= 0)) {
                criteria.SetFirstResult(page * pageSize).SetMaxResults(pageSize);
            }
            multicriteria.Add(criteria);
            multicriteria.Add(Session.CreateCriteria(typeof(T)).SetProjection(Projections.RowCountInt64()));
                        
            IList results = multicriteria.List();
            numResults = (long)((IList)results[1])[0];
            return ((IList)results[0]).Cast<T>().ToList<T>();
        }

        public virtual T FindOne(IDictionary<string, object> propertyValuePairs) {
            IList<T> foundList = FindAll(propertyValuePairs);

            if (foundList.Count > 1) {
                throw new NonUniqueResultException(foundList.Count);
            }
            else if (foundList.Count == 1) {
                return foundList[0];
            }

            return default(T);
        }

        public virtual void Delete(T entity) {
            Session.Delete(entity);
        }

        /// <summary>
        /// Although SaveOrUpdate _can_ be invoked to update an object with an assigned Id, you are 
        /// hereby forced instead to use Save/Update for better clarity.
        /// </summary>
        public virtual T SaveOrUpdate(T entity) {
            Check.Require(!(entity is IHasAssignedId<IdT>),
                "For better clarity and reliability, Entities with an assigned Id must call Save or Update");

            Session.SaveOrUpdate(entity);
            return entity;
        }

        private IDbContext dbContext;

        public IPropertyOrder<T> CreateOrder(string propertyName, bool isDesc)
        {
            return new PropertyOrder<T>(Session.SessionFactory, propertyName, isDesc);
        }

        /// <summary>
        /// Returns an IPropertyOrder element, that has checked previously whether the property actually exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class PropertyOrder<TT> : IPropertyOrder<T>
        {
            public virtual bool IsValid { get; private set; }
            public virtual bool Desc { get; private set; }
            public virtual string PropertyName { get; private set; }

            public PropertyOrder(ISessionFactory sessionFactory, string propertyname, bool desc)
            {
                
                PropertyName = propertyname;
                Desc = desc;
                if (String.IsNullOrEmpty(propertyname))
                {
                    IsValid = false;
                }
                else
                {
                    Type type = typeof(TT);
                    IClassMetadata meta = sessionFactory.GetClassMetadata(type);
                    IsValid = meta.PropertyNames.Contains<string>(propertyname);
                }
            }
        }
    }
}
