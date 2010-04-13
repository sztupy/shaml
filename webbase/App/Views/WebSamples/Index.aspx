<%@ Page Title="Create WebSample" Language="C#" MasterPageFile="~/App/Views/Shared/Site.Master" AutoEventWireup="true" 
	Inherits="System.Web.Mvc.ViewPage<WebBase..Controllers.WebSamplesController.WebSampleFormViewModel>" %>
<%@ Import Namespace="WebBase.Core #>" %>
<%@ Import Namespace="WebBase.Controllers" %>
<asp:Content ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
  <h1>WebSamples</h1>
  <% if (ViewContext.TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()] != null) { %>
  <p id="pageMessage"><%= ViewContext.TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()]%></p>
  <% } %>
  <table>
   <thead>
    <tr>
<!-- __BEGIN__PROPERTY__ -->
     <th>Property</th>
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
  <p><%= Html.ActionLink("Create new WebSample", "Create", "WebSamples") %></p>
</asp:Content>
