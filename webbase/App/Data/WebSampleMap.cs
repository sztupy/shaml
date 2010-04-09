using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using Shaml.Data.NHibernate.FluentNHibernate;
using Shaml.Membership.Core;
using WebBase.Core;

namespace WebBase.Data.Mapping
{
   public class WebSampleMap : IAutoMappingOverride<WebSample>
   {
       public void Override(AutoMapping<WebSample> mapping)
       {
         // __BEGIN__PROPERTY__
         // mapping.Map(x => x.Property).CustomType("StringClob").CustomSqlType("text");
         // __END__PROPERTY__
       }
   }
}
