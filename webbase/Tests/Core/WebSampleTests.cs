using NUnit.Framework;
using WebBase.Core;
using Shaml.Testing;
using Shaml.Testing.NUnit;

namespace Tests.Blog.Core
{
	[TestFixture]
    public class WebSampleTests
    {
        [Test]
        public void CanCompareWebSamples() {
            WebSample instance = new WebSample();
            EntityIdSetter.SetIdOf<int>(instance, 1);

            WebSample instanceToCompareTo = new WebSample();
            EntityIdSetter.SetIdOf<int>(instanceToCompareTo, 1);

            instance.ShouldEqual(instanceToCompareTo);
        }
    }
}
