<%@ Page Language="C#" MasterPageFile="~/App/Views/Shared/Site.master" AutoEventWireup="true"
    Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="WebBase.Controllers" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h2> 
      S#aml Architecture Project
    </h2> 
    <h3> 
      What next?
    </h3> 
    <p> 
      Your S#aml Architecture project is now setup and ready to go.  The only tasks remaining
      are to:
      <ol> 
        <li> 
          Create your database and set the connection string within NHibernate.config
        </li> 
        <li> 
          Import the database schemas found in the db directory
        </li> 
        <li> 
          Open WebBase.Tests.dll via NUnit and make sure all the tests are turning green.
        </li> 
        <li> 
          Add your first entity with CRUD scaffolding via <tt>shaml generate resource <i>res-name</i></tt> 
        </li> 
      </ol> 
    </p> 
    <p> 
      If you need direction on what to do next, take a look at <a href="http://code.google.com/p/shaml-architecture/">the project homepage</a>,
      or ask questions at <a href="http://stackoverflow.com/questions/tagged/shaml">Stack Overflow</a> (tag questions with
      <tt>shaml</tt> or <tt>s#aml</tt>)
    </p> 
</asp:Content>
