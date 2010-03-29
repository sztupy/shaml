﻿using System.Web.Mvc;

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
                "~/{0}.master",
                "~/Shared/{0}.master",
                "~/Views/{1}/{0}.master",
                "~/Views/Shared/{0}.master",
            };

            AreaMasterLocationFormats = new[] {
                "~/Views/{2}/{1}/{0}.master",
                "~/Views/{2}/Shared/{0}.master",
            };

            ViewLocationFormats = new[] { 
                "~/{0}.aspx",
                "~/{0}.ascx",
                "~/Views/{1}/{0}.aspx",
                "~/Views/{1}/{0}.ascx",
                "~/Views/Shared/{0}.aspx",
                "~/Views/Shared/{0}.ascx",
            };

            AreaViewLocationFormats = new[] {
                "~/Views/{2}/{1}/{0}.aspx",
                "~/Views/{2}/{1}/{0}.ascx",
                "~/Views/{2}/Shared/{0}.aspx",
                "~/Views/{2}/Shared/{0}.ascx",
            };

            PartialViewLocationFormats = ViewLocationFormats;
            AreaPartialViewLocationFormats = AreaViewLocationFormats;
        }
    }
}
