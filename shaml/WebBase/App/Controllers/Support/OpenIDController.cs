using System.Web.Security;
using WebBase.AppServices;
using Shaml.Core;

namespace WebBase.Controllers
{
    [GenericLogger]
	public class OpenIDController : OpenIDController_Base
	{
		public OpenIDController()
			: this( null, null ) {}

		public OpenIDController( IFormsAuthentication formsAuth, MembershipProvider provider )
			: base( formsAuth, provider ) {}

		protected override string GetUserNameFromOpenIdIdentity( string openIdIdentity )
		{
            Shaml.Core.OpenIDMembershipProvider.PgOpenIDMembershipProvider idprov = Provider as Shaml.Core.OpenIDMembershipProvider.PgOpenIDMembershipProvider;
            string user = idprov.GetUserNameByOpenID(openIdIdentity);
            return user;
		}

        protected override bool AddOpenIdIdentityToUserName(string user, string openIdIdentity)
        {
            Shaml.Core.OpenIDMembershipProvider.PgOpenIDMembershipProvider idprov = Provider as Shaml.Core.OpenIDMembershipProvider.PgOpenIDMembershipProvider;
            MembershipCreateStatus status;
            idprov.AddIdToUserName(user, openIdIdentity, out status);
            return status == MembershipCreateStatus.Success;
        }

        protected override bool RemoveOpenIdIdentityFromUserName(string user, string openIdIdentity)
        {
            Shaml.Core.OpenIDMembershipProvider.PgOpenIDMembershipProvider idprov = Provider as Shaml.Core.OpenIDMembershipProvider.PgOpenIDMembershipProvider;
            MembershipCreateStatus status;
            idprov.RemoveIdFromUserName(user, openIdIdentity, out status);
            return status == MembershipCreateStatus.Success;
        }

        protected override System.Collections.Generic.List<string> ListOpenIdIdentitiesForUserName(string user)
        {
            Shaml.Core.OpenIDMembershipProvider.PgOpenIDMembershipProvider idprov = Provider as Shaml.Core.OpenIDMembershipProvider.PgOpenIDMembershipProvider;
            return idprov.GetOpenIDIdentifiersForUserName(user);
        }

        protected override string HandleUnknownUser(DotNetOpenAuth.OpenId.RelyingParty.OpenIdRelyingParty openid)
        {
            var resp = openid.GetResponse().GetExtension<DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.ClaimsResponse>();
            if (resp == null)
            {
                return null;
            }
            else
            {
                return resp.Nickname;
            }
        }

        protected override void AddNeededExtensions(ref DotNetOpenAuth.OpenId.RelyingParty.IAuthenticationRequest request)
        {
            var claim = new DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.ClaimsRequest();
            claim.Nickname = DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.DemandLevel.Request;
            claim.Email = DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.DemandLevel.Request;
            request.AddExtension(claim);
        }

	}
}
