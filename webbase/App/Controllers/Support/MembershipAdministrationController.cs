using System.Web.Mvc;
using WebBase.ApplicationServices;
using Shaml.Core;
using Shaml.Web;

namespace WebBase.Controllers
{
//	[Authorize( Roles="Administrator" )]
    [GenericLogger]
	[HandleError]
    public class MembershipAdministrationController : MembershipAdministrationController_Base{}
}
