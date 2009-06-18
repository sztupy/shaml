using System.Web.Mvc;
using WebBase.AppServices;
using Shaml.Core;

namespace WebBase.Controllers
{
	[Authorize( Roles="Administrator" )]
    [GenericLogger]
	[HandleError]
    public class MembershipAdministrationController : MembershipAdministrationController_Base{}
}
