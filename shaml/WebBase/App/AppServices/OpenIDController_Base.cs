using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using WebBase.Controllers;

namespace WebBase.AppServices
{
	public abstract class OpenIDController_Base : Controller
	{
		protected OpenIDController_Base()
			: this( null, null ) {}

		protected OpenIDController_Base( IFormsAuthentication formsAuth, MembershipProvider provider )
		{
            FormsAuth = formsAuth ?? new FormsAuthenticationService();
			Provider = provider ?? Membership.Provider;
		}

		public IFormsAuthentication FormsAuth { get; private set; }
		public MembershipProvider Provider { get; private set; }

		public ActionResult Index() {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/OpenId/Login?ReturnUrl=Index");
            }
            else
            {
                return View(ListOpenIdIdentitiesForUserName(User.Identity.Name));
            }
		}

		public ActionResult Logout() {
			FormsAuthentication.SignOut();
			return Redirect("/Home");
		}

		public ActionResult Login() {
			// Stage 1: display login form to user
			return View("Login");
		}

        [ValidateInput(false)]
		[HandleError( ExceptionType = typeof(System.Net.WebException) )]
        public virtual ActionResult Authenticate(string openid_identifier, string returnUrl)
		{
			//### set page title & declare variables
			ViewData["Title"] = "Login via OpenID";
			var errors = new List<string>();
			var rememberMe = false;
			var openid = new OpenIdRelyingParty();
            var response = openid.GetResponse();

			//### stage 1: display login form to user
            if (response == null && Request.HttpMethod != "POST")
                return RedirectToAction("Index");

			if( response == null )
			{
				//### stage 2: user submitting Identifier
				Identifier id;
                if (Identifier.TryParse(openid_identifier, out id))
                {
                    try
                    {
                        var request = openid.CreateRequest(openid_identifier);
                        AddNeededExtensions(ref request);
                        return request.RedirectingResponse.AsActionResult();
                    }
                    catch (ProtocolException ex)
                    {
                        errors.Add(ex.Message);
                        ViewData["errors"] = errors;
                        ViewData["openid_identifier"] = openid_identifier;
                        return View("Login");
                    }
                }
                else
                {
                    errors.Add(ErrorMessages.InvalidIdentifierSpecified);
                    ViewData["errors"] = errors;
                    ViewData["openid_identifier"] = openid_identifier;
                    return View("Login");
                }
			}
			else
			{
				//### stage 3: OpenID Provider sending assertion response
				switch(response.Status)
				{
					case AuthenticationStatus.Authenticated:

						//### if someone is already logged in then assign a new openid
                        if (User.Identity.IsAuthenticated)
                        {
                            string username = GetUserNameFromOpenIdIdentity(response.ClaimedIdentifier);
                            if (string.IsNullOrEmpty(username))
                            {
                                if (User.Identity.IsAuthenticated)
                                {
                                    AddOpenIdIdentityToUserName(User.Identity.Name, response.ClaimedIdentifier);
                                    return RedirectToAction("Index");
                                }
                                else
                                {
                                    errors.Add(ErrorMessages.AssociationFailure);
                                    ViewData["errors"] = errors;
                                    ViewData["openid_identifier"] = response.ClaimedIdentifier;
                                    return View("Login");
                                }
                            }
                            else
                            {
                                errors.Add(ErrorMessages.DuplicateIDFailure);
                                ViewData["errors"] = errors;
                                ViewData["openid_identifier"] = response.ClaimedIdentifier;
                                return View("Login");
                            }
                        }
                        // else try to log him in
                        else
                        {
                            string username = GetUserNameFromOpenIdIdentity(response.ClaimedIdentifier);
                            if (string.IsNullOrEmpty(username))
                            {
                                username = HandleUnknownUser(response);
                                if (string.IsNullOrEmpty(username))
                                {
                                    errors.Add(ErrorMessages.AssociationFailure);
                                    ViewData["errors"] = errors;
                                    ViewData["openid_identifier"] = response.ClaimedIdentifier;
                                    return View("Login");
                                }
                            }
                            FormsAuthentication.RedirectFromLoginPage(username, rememberMe);
                        }
						break;

					case AuthenticationStatus.Canceled:
						errors.Add( ErrorMessages.CanceledAtProvider );
						ViewData["errors"] = errors;
						ViewData["openid_identifier"] = response.ClaimedIdentifier;
						return View("Login");
					case AuthenticationStatus.Failed:
						errors.Add( ErrorMessages.UnknownFailure + response.Exception.Message );
						ViewData["errors"] = errors;
						ViewData["openid_identifier"] = response.ClaimedIdentifier;
						return View("Login");
				}
			}
			return new EmptyResult();
		}

        [Authorize]
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult RemoveOpenIdAccount(string openIdIdentifier)
        {
            if (!string.IsNullOrEmpty(openIdIdentifier))
            {
                RemoveOpenIdIdentityFromUserName(User.Identity.Name, openIdIdentifier);
            }
            return RedirectToAction("Index");
        }

		public virtual ViewResult XRDS()
		{
			return View("XRDS","Empty");
		}

		[NonAction]
        protected abstract string GetUserNameFromOpenIdIdentity(string openIdIdentity);

        [NonAction]
        protected abstract bool AddOpenIdIdentityToUserName(string UserName, string openIdIdentity);

        [NonAction]
        protected abstract bool RemoveOpenIdIdentityFromUserName(string UserName, string openIdIdentity);

        [NonAction]
        protected abstract List<string> ListOpenIdIdentitiesForUserName(string UserName);

        [NonAction]
        protected abstract string HandleUnknownUser(IAuthenticationResponse response);

        [NonAction]
        protected abstract void AddNeededExtensions(ref IAuthenticationRequest request);

		#region Nested type: ErrorMessages

		public struct ErrorMessages
		{
			public static string AssociationFailure = "Could not associate OpenID to a user in this system.";
            public static string DuplicateIDFailure = "Your identity is already in use by another user!";
			public static string CanceledAtProvider = "Login canceled by OpenID provider.";
			public static string InvalidIdentifierSpecified = "You enter your OpenID Url.";
			public static string InvalidIdentityServerSpecified = "OpenID provider not on whitelist.";
			public static string UnknownFailure = "OpenID authentication failed: ";
		}

		#endregion
	}
}