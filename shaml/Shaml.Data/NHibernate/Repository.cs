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
using NHibernate.Type;

namespace Shaml.Data.NHibernate
{
    /// <summary>
    /// Since nearly all of the domain objects you create will have a type of int Id, this 
    /// most freqently used base GenericDao leverages this assumption.  If you want an entity
    /// with a type other than int, such as string, then use 
    /// <see cref="GenericDaoWithTypedId{T, IdT}" />.
    /// </summary>
    public class Repository<T> : RepositoryWithTypedId<T, int>, IExpressionRepository<T> { }

    /// <summary>
    /// Provides a fully loaded DAO which may be created in a few ways including:
    /// * Direct instantiation; e.g., new GenericDao<Customer, string>
    /// * Spring configuration; e.g., <object id="CustomerDao" type="Shaml.Data.NHibernateSupport.GenericDao&lt;CustomerAlias, string>, Shaml.Data" autowire="byName" />
    /// </summary>
    public class RepositoryWithTypedId<T, IdT> : IExpressionRepositoryWithTypedId<T, IdT>
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
                criteria = Session.CreateCriteria(typeof(T)).SetMaxResults(pageSize).SetFirstResult(pageSize * page);
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
                criteria = Session.CreateCriteria(typeof(T)).SetMaxResults(pageSize).SetFirstResult(pageSize * page);
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
            ICriteria nores = CriteriaTransformer.Clone(criteria);
            if ((pageSize > 0) && (page >= 0)) {
                criteria.SetFirstResult(page * pageSize).SetMaxResults(pageSize);
            }
            multicriteria.Add(criteria);
            multicriteria.Add(nores.SetProjection(Projections.RowCountInt64()));
                        
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

        #region IPropertyOrder based functions

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


        #endregion

        #region IExpression based functions

        public IList<T> FindByExpression(IExpression expression)
        {
            return FindByExpression(expression, 0, 0);
        }

        public IList<T> FindByExpression(IExpression expression, int pageSize, int page, params IPropertyOrder<T>[] ordering)
        {
            Check.Require(expression is CriterionExpression, "expression needs to be a CriterionExpression");

            ICriteria criteria = Session.CreateCriteria(typeof(T));
            AddOrderingsToCriteria(criteria, ordering);
            criteria.Add((expression as CriterionExpression).GetExpression() as ICriterion);
                        
            if ((pageSize > 0) && (page >= 0))
            {
                criteria.SetFirstResult(page * pageSize).SetMaxResults(pageSize);
            }
            return criteria.List<T>();
        }

        public IList<T> FindByExpression(IExpression expression, int pageSize, int page, out long numResults, params IPropertyOrder<T>[] ordering)
        {
            Check.Require(expression is CriterionExpression, "expression needs to be a CriterionExpression");

            ICriteria criteria = Session.CreateCriteria(typeof(T));
            AddOrderingsToCriteria(criteria, ordering);
            criteria.Add((expression as CriterionExpression).GetExpression() as ICriterion);
            ICriteria nores = CriteriaTransformer.Clone(criteria);
            if ((pageSize > 0) && (page >= 0))
            {
                criteria.SetFirstResult(page * pageSize).SetMaxResults(pageSize);
            }
            nores.SetProjection(Projections.RowCountInt64());
           
            // TODO: Multicriteria doesn't work well with SQLite. Disabled
           
            //IMultiCriteria multicriteria = Session.CreateMultiCriteria();
            //multicriteria.Add(criteria);
            //multicriteria.Add(nores);
            //IList results = multicriteria.List();
            //numResults = (long)((IList)results[1])[0];
            //return ((IList)results[0]).Cast<T>().ToList<T>();
            numResults = nores.UniqueResult<long>();
            return criteria.List<T>();
        }

        public T FindOneByExpression(IExpression expression)
        {
            IList<T> foundList = FindByExpression(expression);

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

        public IExpressionBuilder CreateExpressionBuilder()
        {
            return new CriterionExpressionBuilder();
        }

        private class CriterionExpressionBuilder : IExpressionBuilder
        {
            public CriterionExpressionBuilder()
            {
            }

            public IExpression Eq(string propertyName, object value)
            {
                return new CriterionExpression("Eq", propertyName, value);
            }

            public IExpression NEq(string propertyName, object value)
            {
                return new CriterionExpression("NEq", propertyName, value);
            }

            public IExpression Ge(string propertyName, object value)
            {
                return new CriterionExpression("Ge", propertyName, value);
            }

            public IExpression Gt(string propertyName, object value)
            {
                return new CriterionExpression("Gt", propertyName, value);
            }

            public IExpression Le(string propertyName, object value)
            {
                return new CriterionExpression("Le", propertyName, value);
            }

            public IExpression Lt(string propertyName, object value)
            {
                return new CriterionExpression("Lt", propertyName, value);
            }

            public IExpression Like(string propertyName, object value)
            {
                return new CriterionExpression("Like", propertyName, value, false);
            }

            public IExpression Like(string propertyName, object value, bool ignoreCase)
            {
                return new CriterionExpression("Like", propertyName, value, ignoreCase);
            }

            public IExpression FlagEq(string propertyName, object value)
            {
                return new CriterionExpression("FlagEq", propertyName, value);
            }

            public IExpression FlagIn(string propertyName, object value)
            {
                return new CriterionExpression("FlagIn", propertyName, value);
            }

            public IExpression EqProperty(string leftProperty, string rightProperty)
            {
                return new CriterionExpression("EqProperty", leftProperty, rightProperty);
            }

            public IExpression NEqProperty(string leftProperty, string rightProperty)
            {
                return new CriterionExpression("NEqProperty", leftProperty, rightProperty);
            }

            public IExpression GeProperty(string leftProperty, string rightProperty)
            {
                return new CriterionExpression("GeProperty", leftProperty, rightProperty);
            }

            public IExpression GtProperty(string leftProperty, string rightProperty)
            {
                return new CriterionExpression("GtProperty", leftProperty, rightProperty);
            }

            public IExpression LeProperty(string leftProperty, string rightProperty)
            {
                return new CriterionExpression("LeProperty", leftProperty, rightProperty);
            }

            public IExpression LtProperty(string leftProperty, string rightProperty)
            {
                return new CriterionExpression("GeProperty", leftProperty, rightProperty);
            }

            public IExpression Between(string propertyName, object lo, object hi)
            {
                return new CriterionExpression("Between", propertyName, hi, lo);
            }

            public IExpression In(string propertyName, object[] values)
            {
                return new CriterionExpression("In", propertyName, values);
            }

            public IExpression Null(string propertyName)
            {
                return new CriterionExpression("Null", propertyName);
            }

            public IExpression NotNull(string propertyName)
            {
                return new CriterionExpression("NotNull", propertyName);
            }

            public IExpression Not(IExpression value)
            {
                return new CriterionExpression("Not", value);
            }

            public IExpression And(params IExpression[] values)
            {
                return new CriterionExpression("And", values, null);
            }

            public IExpression Or(params IExpression[] values)
            {
                return new CriterionExpression("Or", values, null);
            }
        }

        private class CriterionExpression : IExpression
        {
            private string t;
            private object[] p;
            public object GetExpression()
            {
                Check.Ensure(p.Length >= 1, "At least one parameter is expected");
                switch (t) {
                    case "Eq": return Expression.Eq(p[0] as string, p[1]);
                    case "NEq": return Expression.Not(Expression.Eq(p[0] as string, p[1]));
                    case "Ge": return Expression.Ge(p[0] as string, p[1]);
                    case "Gt": return Expression.Gt(p[0] as string, p[1]);
                    case "Le": return Expression.Le(p[0] as string, p[1]);
                    case "Lt": return Expression.Lt(p[0] as string, p[1]);
                    case "FlagEq": return Expression.Eq(
                            Projections.SqlProjection("({alias}." + (p[0] as string) + " & " + ((int)p[1]).ToString() + ") as " + (p[0] as string) + "Result", new[] { (p[0] as string) + "Result" }, new IType[] { NHibernateUtil.Int32 }),
                        ((int)p[1]));
                    case "FlagIn": return Expression.Gt(
                            Projections.SqlProjection("({alias}."+(p[0] as string)+" & "+ ((int)p[1]).ToString() +") as "+(p[0] as string)+"Result",new[] {(p[0] as string)+"Result"}, new IType[] {NHibernateUtil.Int32}),
                        0);
                    case "Like": if ((p[2] as bool?) == false)
                        {
                            return Expression.Like(p[0] as string, p[1]);
                        }
                        else
                        {
                            return Expression.InsensitiveLike(p[0] as string, p[1]);
                        }
                    case "EqProperty": return Expression.EqProperty(p[0] as string, p[1] as string);
                    case "NEqProperty": return Expression.NotEqProperty(p[0] as string, p[1] as string);
                    case "GeProperty": return Expression.GeProperty(p[0] as string, p[1] as string);
                    case "GtProperty": return Expression.GtProperty(p[0] as string, p[1] as string);
                    case "LeProperty": return Expression.LeProperty(p[0] as string, p[1] as string);
                    case "LtProperty": return Expression.LtProperty(p[0] as string, p[1] as string);
                    case "Between": return Expression.Between(p[0] as string, p[1], p[2]);
                    case "In": return Expression.In(p[0] as string, p[1] as object[]);
                    case "Null": return Expression.IsNull(p[0] as string);
                    case "NotNull": return Expression.IsNotNull(p[0] as string);
                    case "Not": return Expression.Not((p[0] as CriterionExpression).GetExpression() as ICriterion);
                    case "And":
                        {
                            var conj = Expression.Conjunction();
                            foreach (IExpression ce in p[0] as IExpression[])
                            {
                                conj.Add(ce.GetExpression() as ICriterion);
                            }
                            return conj;
                        }
                    case "Or":
                        {
                            var conj = Expression.Disjunction();
                            foreach (IExpression ce in p[0] as IExpression[])
                            {
                                conj.Add(ce.GetExpression() as ICriterion);
                            }
                            return conj;
                        }
                    default:
                        throw new NotImplementedException();
                }
            }

            public CriterionExpression(string type, params object[] parameters)
            {
                t = type;
                p = parameters;
            }
        }

        #endregion
    }
}
