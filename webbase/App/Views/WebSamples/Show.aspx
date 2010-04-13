<%@ Page Inherits="System.Web.Mvc.ViewPage<WebSample>" Title="Create WebSample" Language="C#" MasterPageFile="~/App/Views/Shared/Site.master" AutoEventWireup="true" %>

<asp:Content ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<div>
 <h2>WebSample Details</h2>
 <ul>
<!-- __BEGIN__PROPERTY__ -->
  <li>
   <label for="WebSample_Property">Property:</label>
   <span id="WebSample_Property"><%= Server.HtmlEncode(ViewData.Model.Property.ToString()) %></span>
  </li>
<!-- __END__PROPERTY__ -->
 </ul>
</div>
</asp:Content>
