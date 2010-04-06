using NHibernate.Validator;
using Shaml.Core.DomainModel;
using Shaml.Core.PersistenceSupport;
using System;

namespace WebBase.Core
{
    public class WebSample : Entity
    {
        public WebSample() { }
		
__BEGIN__PROPERTY__
        public virtual PropertyType Property { get; set; }
__END__PROPERTY__
    }
}
