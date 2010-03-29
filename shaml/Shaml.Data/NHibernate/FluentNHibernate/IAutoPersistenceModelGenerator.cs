using FluentNHibernate.Automapping;
using System;

namespace Shaml.Data.NHibernate.FluentNHibernate
{
	[CLSCompliant(false)]
	public interface IAutoPersistenceModelGenerator
    {
        AutoPersistenceModel Generate();
    }
}
