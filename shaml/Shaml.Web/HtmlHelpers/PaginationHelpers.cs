using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Shaml.Web.HtmlHelpers
{
    static public class PaginationHelpers
    {
        static public MvcHtmlString RenderPagination(this HtmlHelper helper, PaginationData data)
        {
            if (data != null)
            {
                return MvcHtmlString.Create(data.Render(helper));
            }
            else
            {
                return MvcHtmlString.Empty;
            }
        }
    }

    public abstract class PaginationData
    {
        public virtual int Page { get; protected set;  }
        public virtual int PageSize { get; protected set; }
        public virtual long MaxResults { get; protected set; }
        public virtual int LastPage { get; protected set; }

        public virtual string PreviousText { get; set; }
        public virtual string NextText { get; set; }

        public virtual int CurrentPage { get; private set; }
        public virtual RouteValueDictionary CurrentRoute { get; private set; }

        public PaginationData(int page, int pageSize, long maxResults)
        {
            if (page < 0)
            {
                Page = 0;
            }
            else
            {
                Page = page;
            }
            if (pageSize <= 0)
            {
                PageSize = 1;
            }
            else
            {
                PageSize = pageSize;
            }
            MaxResults = maxResults;
            if ((MaxResults % PageSize) == 0)
            {
                LastPage = (int)(MaxResults / PageSize);
            }
            else
            {
                LastPage = (int)(MaxResults / pageSize) + 1;
            }
            if (LastPage == 0)
            {
                LastPage = 1;
            }
            if (Page >= LastPage)
            {
                Page = LastPage - 1;
            }
            PreviousText = "« Previous";
            NextText = "Next »";
        }

        public virtual void FillPageData(HtmlHelper helper)
        {
            string currentPageString = helper.ViewContext.RouteData.Values["Page"] as string;
            CurrentPage = 0;
            int result;
            if (Int32.TryParse(currentPageString, out result))
            {
                CurrentPage = result;
            }

            CurrentRoute = new RouteValueDictionary();
            foreach (var p in helper.ViewContext.RouteData.Values)
            {
                CurrentRoute.Add(p.Key, p.Value);
            }
            foreach (string var in helper.ViewContext.HttpContext.Request.QueryString)
            {
                CurrentRoute.Add(var, helper.ViewContext.HttpContext.Request.QueryString[var]);
            }
        }

        abstract public string Render(HtmlHelper helper);
    }

    // Creates a Paginator that writes out the pages like this: 1 2 3 ... 5 6 7 ... 11 12 13
    // (Where 6 is the current page)
    public class ThreeWayPaginationData : PaginationData
    {
        public virtual int PagesPerBlock { get; protected set; }

        public ThreeWayPaginationData(int page, int pageSize, long maxResults) : base(page, pageSize, maxResults)
        {
            PagesPerBlock = 5;
        }

        public ThreeWayPaginationData(int page, int pageSize, long maxResults, int pagesPerBlock) :
            base(page, pageSize, maxResults)
        {
            PagesPerBlock = pagesPerBlock;
        }

        public override string Render(HtmlHelper helper)
        {
            StringBuilder str = new StringBuilder();

            if (LastPage <= 1)
            {
                return "";
            }

            // First block: Previous link
            FillPageData(helper);
            if (Page <= 0)
            {
                //str.Append(PreviousText);
            }
            else
            {
                CurrentRoute["Page"] = Page - 1;
                str.Append(helper.RouteLink(PreviousText, CurrentRoute));
            }
            str.Append(" ");
            int cpage = 0;

            // Second block: first few links
            while (cpage < PagesPerBlock)
            {
                if (cpage >= LastPage) break;
                CurrentRoute["Page"] = cpage;
                if (cpage == Page)
                {
                    str.Append(cpage + 1);
                } else 
                {
                    str.Append(helper.RouteLink((cpage+1).ToString(), CurrentRoute));
                }
                str.Append(" ");
                cpage++;
            }
            // Third block: middle links
            if (cpage < Page - PagesPerBlock + 1)
            {
                str.Append("... ");
                cpage = Page - PagesPerBlock + 1;
            }
            while (cpage < Page+PagesPerBlock)
            {
                if (cpage >= LastPage) break;
                CurrentRoute["Page"] = cpage;
                if (cpage == Page)
                {
                    str.Append(cpage + 1);
                }
                else
                {
                    str.Append(helper.RouteLink((cpage + 1).ToString(), CurrentRoute));
                }
                str.Append(" ");
                cpage++;
            }            

            // Fourth block: end links
            if (cpage < LastPage - PagesPerBlock + 1)
            {
                str.Append("... ");
                cpage = LastPage - PagesPerBlock + 1;
            }
            while (cpage < LastPage)
            {
                CurrentRoute["Page"] = cpage;
                if (cpage == Page)
                {
                    str.Append(cpage + 1);
                }
                else
                {
                    str.Append(helper.RouteLink((cpage + 1).ToString(), CurrentRoute));
                }
                str.Append(" ");
                cpage++;
            }     
            // Fifth block: Next link
            if (Page >= LastPage-1)
            {
                //str.Append(NextText);
            }
            else
            {
                CurrentRoute["Page"] = Page + 1;
                str.Append(helper.RouteLink(NextText, CurrentRoute));
            }
            return str.ToString();
        }
    }
}
