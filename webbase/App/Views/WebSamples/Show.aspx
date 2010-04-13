<%@ Page Title="Create WebSample" Language="C#" MasterPageFile="~/App/Views/Shared/Site.Master" AutoEventWireup="true" 
	Inherits="System.Web.Mvc.ViewPage<WebBase.Controllers.WebSamplesController.WebSampleFormViewModel>" %>

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
