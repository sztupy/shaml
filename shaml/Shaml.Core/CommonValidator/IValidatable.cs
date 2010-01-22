using System.Collections.Generic;

namespace Shaml.Core.CommonValidator
{
    public interface IValidatable
    {
        bool IsValid();
        ICollection<IValidationResult> ValidationResults();
    }
}
