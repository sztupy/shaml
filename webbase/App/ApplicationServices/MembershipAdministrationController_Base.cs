using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using dotNetMembership=System.Web.Security.Membership;
using dotNetRoles=System.Web.Security.Roles;

namespace WebBase.ApplicationServices
{
	public abstract class MembershipAdministrationController_Base : Controller
	{
		#region Utility & protected methods.

		[NonAction]
		protected RedirectToRouteResult RedirectToUserPage( MembershipUser user )
		{
			var rvd = new RouteValueDictionary( new{
			                                       	controller = ControllerContext.RouteData.Values["controller"],
			                                       	action = "UserDetails",
			                                       	id = new Guid(user.ProviderUserKey.ToString())
			                                       } );
			return RedirectToRoute( rvd );
		}

		[NonAction]
		protected virtual List<T> ToList<T>( IEnumerable source )
		{
			var list = new List<T>();
			foreach( T t in source )
				list.Add( t );
			return list;
		}

		[NonAction]
		protected virtual List<TTo> ToList<TFrom, TTo>( IEnumerable source, Func<TFrom, TTo> convert )
		{
			var list = new List<TTo>();
			foreach( TFrom t in source )
				list.Add( convert( t ) );
			return list;
		}

		#endregion

		#region Role management.

		[NonAction]
		protected virtual void OnBeforeCreateRole( string role ) { }

		[NonAction]
		protected virtual void OnAfterCreateRole( string role ) { }

		[NonAction]
		protected virtual void OnBeforeDeleteRole( string id ) { }

		[NonAction]
		protected virtual void OnAfterDeleteRole( string id ) { }

		[NonAction]
		protected virtual void OnBeforeAddUserToRole( string userName, string roleName ) { }

		[NonAction]
		protected virtual void OnAfterAddUserToRole( string userName, string roleName ) { }

		[NonAction]
		protected virtual void OnBeforeRemoveUserFromRole( string userName, string roleName ) { }

		[NonAction]
		protected virtual void OnAfterRemoveUserFromRole( string userName, string roleName ) { }

		public virtual RedirectToRouteResult CreateRole( string role )
		{
			OnBeforeCreateRole( role );
			dotNetRoles.CreateRole( role );
			OnAfterCreateRole( role );
			return RedirectToAction( "Index" );
		}

		public virtual RedirectToRouteResult DeleteRole( string id )
		{
			OnBeforeDeleteRole( id );
			dotNetRoles.DeleteRole( id );
			OnAfterDeleteRole( id );
			return RedirectToAction( "Index" );
		}

		public virtual ViewResult Role( string id )
		{
			ViewData["Title"] = "Role (" + id + ")";
			var userNames = dotNetRoles.GetUsersInRole( id );
			var role = new MembershipRoleViewData{
			                                     	Name = id,
			                                     	Users = ToList<string, MembershipUser>( userNames, u => Membership.GetUser( u ) )
			                                     };
			ViewData["Role"] = role;
			return View( "UsersInRole" );
		}

		public virtual RedirectToRouteResult AddUserToRole( Guid userId, string roleName )
		{
			var user = Membership.GetUser( userId );
			OnBeforeAddUserToRole( user.UserName, roleName );
			dotNetRoles.AddUserToRole( user.UserName, roleName );
			OnAfterAddUserToRole( user.UserName, roleName );
			return RedirectToUserPage( user );
		}

		public virtual RedirectToRouteResult RemoveUserFromRole( Guid userId, string roleName )
		{
			var user = Membership.GetUser( userId );
			OnBeforeRemoveUserFromRole( user.UserName, roleName );
			dotNetRoles.RemoveUserFromRole( user.UserName, roleName );
			OnAfterRemoveUserFromRole( user.UserName, roleName );
			return RedirectToUserPage( user );
		}

		#endregion

		#region List/display users.

		public virtual ViewResult Index( int? pageIndex, int? pageSize )
		{
			ViewData["Title"] = "Membership Administration";
			int totalRecords;
			var members = Membership.GetAllUsers( pageIndex ?? 0, pageSize ?? 25, out totalRecords );
			ViewData["Users"] = ToList<MembershipUser>( members );
			ViewData["Roles"] = dotNetRoles.GetAllRoles().ToList();
			return View( "ListUsers" );
		}

		public virtual ViewResult UserDetails( Guid id )
		{
			var user = Membership.GetUser( id );
			ViewData["Title"] = "User Details (" + user.UserName + ")";
			var data = new MembershipUserAndRolesViewData{
			                                             	RolesEnabled = dotNetRoles.Enabled,
			                                             	RequiresQuestionAndAnswer = Membership.RequiresQuestionAndAnswer,
			                                             	User = user,
			                                             	AllRoles = dotNetRoles.GetAllRoles().OrderBy( x => x ).ToList(),
			                                             	UsersRoles = dotNetRoles.GetRolesForUser( user.UserName ).OrderBy( x => x ).ToList()
			                                             };
			ViewData["User"] = data;
			return View( "DisplayUser" );
		}

		#endregion

		#region Edit user.

		[NonAction]
		protected virtual void OnBeforeUpdateUser( string userName ) { }

		[NonAction]
		protected virtual void OnAfterUpdateUser( string userName ) { }

		[NonAction]
		protected virtual void OnBeforeUnlockUser( string id ) { }

		[NonAction]
		protected virtual void OnAfterUnlockUser( string id ) { }

		[NonAction]
		protected virtual void OnBeforeDeleteUser( string id ) { }

		[NonAction]
		protected virtual void OnAfterDeleteUser( string id ) { }

		public virtual RedirectToRouteResult SaveExistingUser()
		{
			var userName = Request.Form["UserName"];
			OnBeforeUpdateUser( userName );
			var user = Membership.GetUser( userName );
			UpdateModel( user, new[]{ "Email", "Comment" } );
			user.IsApproved = ( ( Request.Form["IsApproved"] ?? "" ).Trim().Equals( "on", StringComparison.CurrentCultureIgnoreCase ) );
			Membership.UpdateUser( user );
			OnAfterUpdateUser( userName );
			return RedirectToUserPage( user );
		}

		public virtual RedirectToRouteResult UnlockUser( Guid id )
		{
			var user = Membership.GetUser( id );
			OnBeforeUnlockUser( user.UserName );
			user.UnlockUser();
			OnAfterUnlockUser( user.UserName );
			return RedirectToUserPage( user );
		}

		public virtual RedirectToRouteResult DeleteUser( Guid id )
		{
			var user = Membership.GetUser( id );
			OnBeforeDeleteUser( user.UserName );
			if( user.UserName == Membership.GetUser().UserName )
				throw new InvalidOperationException("Cannot delete user account while logged in as that user account.");
			Membership.DeleteUser( user.UserName, true ); 
			OnAfterDeleteUser( user.UserName );
			return RedirectToAction( "Users" );
		}

		#endregion

		#region User creation.

		[NonAction]
		protected virtual void OnBeforeCreateUser( string userName, string emailAddress, string password, string passwordConfirm, string passwordQuestion, string passwordAnswer ) { }

		[NonAction]
		protected virtual void OnAfterCreateUser( string userName, string emailAddress, string password, string passwordConfirm, string passwordQuestion, string passwordAnswer ) { }

		[NonAction]
		protected virtual void OnCreateUserError( string userName, string emailAddress, string password, string passwordConfirm, string passwordQuestion, string passwordAnswer, string error ) { }

		public virtual ViewResult CreateUser()
		{
			ViewData["Title"] = "Create Account";
			ViewData["RequiresQuestionAndAnswer"] = Membership.RequiresQuestionAndAnswer;
			return View();
		}

		public virtual RedirectToRouteResult SaveNewUser()
		{
			var userName = Request.Form["MembershipUser.UserName"];
			var password = Request.Form["MembershipUser.Password"];
			var passwordConfirm = Request.Form["MembershipUser.ConfirmPassword"];
			var email = Request.Form["MembershipUser.EmailAddress"];
			var pwdQuestion = Request.Form["MembershipUser.PasswordQuestion"];
			var pwdAnswer = Request.Form["MembershipUser.PasswordAnswer"];
			var isApproved = ( ( Request.Form["MembershipUser.IsApproved"] ?? "" ).Trim().Equals( "on", StringComparison.CurrentCultureIgnoreCase ) );

			OnBeforeCreateUser( userName, email, password, passwordConfirm, pwdQuestion, pwdAnswer );
			MembershipCreateStatus status;
			MembershipUser user = null;

			if( password == passwordConfirm )
			{
				var providerUserKey = AssociateNewUserToProviderUserKey( userName, email );
				if( providerUserKey == Guid.Empty )
					user = Membership.CreateUser( userName, password, email, pwdQuestion, pwdAnswer, isApproved, out status );
				else
					user = Membership.CreateUser( userName, password, email, pwdQuestion, pwdAnswer, isApproved, providerUserKey, out status );

				OnAfterCreateUser( userName, email, password, passwordConfirm, pwdQuestion, pwdAnswer );
			}
			else
				status = MembershipCreateStatus.InvalidPassword;

			if( status == MembershipCreateStatus.Success )
				return RedirectToUserPage( user );

			OnCreateUserError( userName, email, password, passwordConfirm, pwdQuestion, pwdAnswer, status.ToString() );
			TempData["MembershipCreateStatus"] = status;
			TempData["MembershipUser.UserName"] = userName;
			TempData["MembershipUser.EmailAddress"] = email;
			TempData["MembershipUser.PasswordQuestion"] = pwdQuestion;
			TempData["MembershipUser.PasswordAnswer"] = pwdAnswer;
			TempData["MembershipUser.IsApproved"] = isApproved;
			return RedirectToAction( "CreateUser" );
		}

		[NonAction]
		protected virtual Guid AssociateNewUserToProviderUserKey( string userName, string email )
		{
			return Guid.Empty;
		}

		#endregion

		#region Password management.

		[NonAction]
		protected virtual void OnBeforeResetPassword( string userName, string passwordAnswer ) { }

		[NonAction]
		protected virtual void OnAfterResetPassword( string email, string userName, string newPassword ) { }

		[NonAction]
		protected virtual void OnBeforePasswordChange( string userName, string currentPassword, string newPassword, string newPasswordConfirm ) { }

		[NonAction]
		protected virtual void OnAfterPasswordChange( string userName, string currentPassword, string newPassword, string newPasswordConfirm ) { }

		public virtual RedirectToRouteResult ChangePassword()
		{
			var userName = Request.Form["UserName"];
			var oldPwd = Request.Form["OldPassword"];
			var newPwd = Request.Form["NewPassword"];
			var newPwdConfirm = Request.Form["NewPasswordConfirm"];

			OnBeforePasswordChange( userName, oldPwd, newPwd, newPwdConfirm );

			var user = Membership.GetUser( userName );
			if( newPwd == newPwdConfirm )
				user.ChangePassword( oldPwd, newPwd );
			else
				throw new MembershipPasswordException( "\"New Password\" does not match \"Confirm New Password\"." );

			OnAfterPasswordChange( userName, oldPwd, newPwd, newPwdConfirm );

			return RedirectToUserPage( user );
		}

		public virtual ViewResult ResetPassword()
		{
			ViewData["Title"] = "Reset Password";

            var userName = Request.Form["UserName"];
            var pwdAnswer = Request.Form["PasswordAnswer"].ToString();

			OnBeforeResetPassword( userName, pwdAnswer );

			var user = Membership.GetUser( userName );
			var pwd = user.ResetPassword( );

			OnAfterResetPassword( user.Email, userName, pwdAnswer );

			ViewData["Password"] = pwd;
			return View( "ResetPassword" );
		}

		#endregion
	}

	#region ViewData models.

	public class MembershipRoleViewData
	{
		public string Name { get; set; }
		public List<MembershipUser> Users { get; set; }
	}

	public class MembershipUserAndRolesViewData
	{
		public bool RolesEnabled { get; set; }
		public bool RequiresQuestionAndAnswer { get; set; }
		public MembershipUser User { get; set; }
		public List<string> AllRoles { get; set; }
		public List<string> UsersRoles { get; set; }
	}

	#endregion
}
