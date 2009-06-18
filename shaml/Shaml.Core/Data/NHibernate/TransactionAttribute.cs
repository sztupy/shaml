﻿using System.Web.Mvc;
using Shaml.Data.NHibernate;

namespace Shaml.Web.NHibernate
{
	public class TransactionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext) {
			NHibernateSession.Current.BeginTransaction();
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext) {
            if (filterContext.Exception == null && NHibernateSession.Current.Transaction.IsActive) {
                NHibernateSession.Current.Transaction.Commit();
            }
		}
	}
}
