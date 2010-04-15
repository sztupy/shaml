<%@ Page Title="Create WebSample" Language="C#" MasterPageFile="~/App/Views/Shared/Site.master" Inherits="System.Web.Mvc.ViewPage<List<WebSample>>" %>
<%@ Import Namespace="WebBase.Core" %>
<%@ Import Namespace="WebBase.Controllers" %>
<%@ Import Namespace="Shaml.Web.HtmlHelpers" %>

<asp:Content ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
  <h1>WebSamples</h1>
  <% if (ViewContext.TempData["message"] != null) { %>
  <p id="pageMessage"><%= ViewContext.TempData["message"].ToString() %></p>
  <% } %>
  <table>
   <thead>
    <tr>
<!-- __BEGIN__PROPERTY__ -->
     <th><%= Html.SwitchOrderLink("Property","Property") %></th>
<!-- __END__PROPERTY__ -->
     <th colspan="3">Action</th>
    </tr>
   </thead>
  <% foreach (WebSample websample in ViewData.Model) { %>
    <tr>
<!-- __BEGIN__PROPERTY__ -->
	   <td><%= websample.Property %></td>
<!-- __END__PROPERTY__ -->
     <td><%= Html.ActionLink("Details", "Show", "WebSamples", new { Id = websample.Id }, new { }) %></td>
     <td><%= Html.ActionLink("Edit", "Edit", "WebSamples", new { Id = websample.Id }, new { }) %></td>
     <td><%= Html.ActionLink("Delete", "Delete", "WebSamples", new { Id = websample.Id }, new { }) %></td>
    </tr>
   <%} %>
   </table>
  <p><%= Html.RenderPagination(ViewData["Pagination"] as PaginationData); %></p>
  <p><%= Html.ActionLink("Create new WebSample", "Create", "WebSamples") %></p>
</asp:Content>
