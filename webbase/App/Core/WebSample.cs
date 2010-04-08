using NHibernate.Validator.Constraints;
using Shaml.Core.DomainModel;
using Shaml.Core.PersistenceSupport;
using System;

namespace WebBase.Core
{
    public class PropertyType { }
    public class WebSample : Entity
    {
        public WebSample() { }

        // __BEGIN__PROPERTY__
        public virtual PropertyType Property { get; set; }
        // __END__PROPERTY__
    }
}
