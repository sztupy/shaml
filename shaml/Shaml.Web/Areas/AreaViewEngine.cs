using System.Web.Mvc;

namespace Shaml.Web.Areas
{
    /// <summary>
    /// Replacement view engine to support the organization of views into areas.  Taken from the
    /// sample provided by Phil Haack at http://haacked.com/archive/2008/11/04/areas-in-aspnetmvc.aspx.
    /// It's been modified to allow area views to sit within subfolders under /Views
    /// </summary>
    public class AreaViewEngine : WebFormViewEngine
    {
        public AreaViewEngine()
        {
            MasterLocationFormats = new[] {
                "~/App/Views/{0}.master",
                "~/App/Views/{1}/{0}.master",
                "~/App/Views/Shared/{0}.master",
            };

            AreaMasterLocationFormats = new[] {
                "~/App/Views/{2}/{1}/{0}.master",
                "~/App/Views/{2}/Shared/{0}.master",
            };

            ViewLocationFormats = new[] { 
                "~/App/Views/{0}.aspx",
                "~/App/Views/{0}.ascx",
                "~/App/Views/{1}/{0}.aspx",
                "~/App/Views/{1}/{0}.ascx",
                "~/App/Views/Shared/{0}.aspx",
                "~/App/Views/Shared/{0}.ascx",
            };

            AreaViewLocationFormats = new[] {
                "~/App/Views/{2}/{1}/{0}.aspx",
                "~/App/Views/{2}/{1}/{0}.ascx",
                "~/App/Views/{2}/Shared/{0}.aspx",
                "~/App/Views/{2}/Shared/{0}.ascx",
            };

            PartialViewLocationFormats = ViewLocationFormats;
            AreaPartialViewLocationFormats = AreaViewLocationFormats;
        }
    }
}
