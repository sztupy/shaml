<%@ Page Title="Create WebSample" Language="C#" MasterPageFile="~/App/Views/Shared/Site.master" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewPage<WebSample>" %>

<asp:Content ContentPlaceHolderID="MainContentPlaceHolder" runat="server">

<div>
 <h2>Delete WebSample</h2>
 <p>Are you sure?</p>
 <% using (Html.BeginForm("DeleteConfirmed","WebSamples",new { Id = ViewData.Model.Id })) { %>
    <% // Html.AntiForgeryToken() %>
    <input type="submit" value="Yes" />
    <input type="button" name="No" value="No" onclick=""javascript:history.go(-1);"}
 <% } %>
</div>

</asp:Content>
