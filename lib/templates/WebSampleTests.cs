using NUnit.Framework;
using WebBase.Core;
using NUnit.Framework.SyntaxHelpers;
using Shaml.Testing;

namespace Tests.Blog.Core
{
	[TestFixture]
    public class WebSampleTests
    {
        [Test]
        public void CanCompareWebSamples() {
            WebSample instance = new WebSample();

            WebSample instanceToCompareTo = new WebSample();

            Assert.That(instance.Equals(instanceToCompareTo));
        }
    }
}
