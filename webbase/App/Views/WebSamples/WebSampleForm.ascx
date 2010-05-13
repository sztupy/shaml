<%@ Import Namespace="WebBase.Core" %>
<%@ Import Namespace="WebBase.Controllers" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.Mvc.ViewUserControl<WebBase.Controllers.WebSamplesController.WebSampleFormViewModel>" %>

<% Html.EnableClientValidation(); %>
<%= Html.ValidationSummary() %>
<% using (Html.BeginForm()) { %>
  <%= Html.AntiForgeryToken() %>
  <%= Html.Hidden("WebSample.Id", (Model.WebSample != null) ? Model.WebSample.Id : 0) %>
  <ul>
<!-- __BEGIN__PROPERTY__ -->
   <li>
     <label for="WebSample_Property">Property:</label>
     <div>
       <%= Html.TextBoxFor(x => x.WebSample.Property ) %>
       <%= Html.ValidationMessageFor(x => x.WebSample.Property) %>
     </div>
   </li>
<!-- __END__PROPERTY__ -->
   <li>
    <input type="submit" name="btnSave" value="Save WebSample" />
    <button name="btnCancel" onClick="window.location.href = '/WebSamples';">Cancel</button>
   </li>
  </ul>
<% } %>

