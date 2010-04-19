using NHibernate.Validator.Constraints;
using Shaml.Core.DomainModel;
using Shaml.Core.PersistenceSupport;
using Shaml.Membership.Core;
using System;

namespace WebBase.Core
{
    public class PropertyType : Entity { }
    public class WebSample : Entity
    {
        public WebSample() { }

        // __BEGIN__PROPERTY__
        [NotNull]
        public virtual PropertyType Property { get; set; }

        // __END__PROPERTY__
    }
}
