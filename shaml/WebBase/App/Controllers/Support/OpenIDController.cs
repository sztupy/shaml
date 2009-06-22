using System.Web.Security;
using WebBase.AppServices;
using DotNetOpenAuth.OpenId.Extensions;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using Shaml.Core;
using System.Collections.Generic;
using System;
using DotNetOpenAuth.OpenId.RelyingParty;

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

        protected override string HandleUnknownUser(IAuthenticationResponse response)
        {
            string username = response.ClaimedIdentifier.ToString();
            string email = response.ClaimedIdentifier.ToString();
            string comment = null;
            var sreg = response.GetExtension<DotNetOpenAuth.OpenId.Extensions.SimpleRegistration.ClaimsResponse>();
            if (sreg != null)
            {
                if (sreg.Nickname != null)
                {
                    comment = sreg.Nickname;
                }
                if (sreg.Email != null)
                {
                    email = sreg.Email;
                }
            }
            var ax = response.GetExtension<DotNetOpenAuth.OpenId.Extensions.AttributeExchange.FetchResponse>();
            if (ax != null)
            {
                if (ax.Attributes.Contains(WellKnownAttributes.Contact.Email))
                {
                    IList<string> emailAddresses = ax.Attributes[WellKnownAttributes.Contact.Email].Values;
                    email = emailAddresses.Count > 0 ? emailAddresses[0] : email;
                }
                if (ax.Attributes.Contains(WellKnownAttributes.Name.Alias))
                {
                    IList<string> aliasNames = ax.Attributes[WellKnownAttributes.Name.Alias].Values;
                    comment = aliasNames.Count > 0 ? aliasNames[0] : comment;
                }
            }
            try
            {
                var user = Membership.CreateUser(username, Guid.NewGuid().ToString(), email);
                Shaml.Core.OpenIDMembershipProvider.PgOpenIDMembershipProvider idprov = Provider as Shaml.Core.OpenIDMembershipProvider.PgOpenIDMembershipProvider;
                MembershipCreateStatus status;
                idprov.AddIdToUser(user, response.ClaimedIdentifier, out status);
                if (status == MembershipCreateStatus.Success)
                {
                    if (String.IsNullOrEmpty(comment)) {
                      user.Comment = email;
                    } else {
                      user.Comment = comment;
                    }
                    Provider.UpdateUser(user);
                    return user.UserName;
                }
                else
                {
                    Provider.DeleteUser(user.UserName, true);
                }
            }
            catch (MembershipCreateUserException)
            {
                return null;
            }
            return null;
        }

        protected override void AddNeededExtensions(ref DotNetOpenAuth.OpenId.RelyingParty.IAuthenticationRequest request)
        {
            var sreg = new ClaimsRequest();
            sreg.Nickname = DemandLevel.Require;
            sreg.Email = DemandLevel.Require;
            request.AddExtension(sreg);

            var ax = new FetchRequest();
            ax.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Email, true));
            ax.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.Alias, true));
            request.AddExtension(ax);
        }

	}
}
