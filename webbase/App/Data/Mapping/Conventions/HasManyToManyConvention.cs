using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace WebBase.Data.Mapping.Conventions {
    public class HasManyToManyConvention : IHasManyToManyConvention
    {
        public void Apply(IManyToManyCollectionInstance instance) {   
            instance.Cascade.SaveUpdate();
        }
    }
}
