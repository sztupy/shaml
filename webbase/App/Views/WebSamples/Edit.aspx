<%@ Page Title="Create WebSample" Language="C#" MasterPageFile="~/App/Views/Shared/Site.Master" AutoEventWireup="true" 
	Inherits="System.Web.Mvc.ViewPage<WebBase.Controllers.WebSamplesController.WebSampleFormViewModel>" %>

<asp:Content ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
<div>
 <h2>Edit WebSample</h2>
 <% Html.RenderPartial("WebSampleForm", ViewData); %>
</div>
</asp:Content>

