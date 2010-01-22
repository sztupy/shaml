using Shaml.Core.DomainModel;

namespace Shaml.Core.PersistenceSupport
{
    public interface IEntityDuplicateChecker
    {
        bool DoesDuplicateExistWithTypedIdOf<IdT>(IEntityWithTypedId<IdT> entity);
    }
}
