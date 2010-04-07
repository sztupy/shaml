using NUnit.Framework;
using WebBase.Controllers;
using System.Web.Routing;
using MvcContrib.TestHelper;
using WebBase.ApplicationServices;
using WebBase;
using WebBase.Config;

namespace Tests.WebBase
{
    [TestFixture]
    public class RouteRegistrarTests
    {
        [SetUp]
        public void SetUp()
        {
            RouteTable.Routes.Clear();
            RouteRegistrar.RegisterRoutesTo(RouteTable.Routes);
        }

        [Test]
        public void CanVerifyRouteMaps()
        {
            "~/".Route().ShouldMapTo<HomeController>(x => x.Index());
            "~/OpenId".Route().ShouldMapTo<OpenIDController>(x => x.Index());
            "~/Account/LogOn".Route().ShouldMapTo<AccountController>(x => x.LogOn());
            "~/Account/LogOff".Route().ShouldMapTo<AccountController>(x => x.LogOff());
            "~/MembershipAdministration".Route().ShouldMapTo<MembershipAdministrationController>(x => x.Index(null,null));
        }
    }
}
