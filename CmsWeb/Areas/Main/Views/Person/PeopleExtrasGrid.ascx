﻿<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<CmsData.Person>" %>
<% if (Model.PeopleExtras.Count > 0)
   { %>
<table class="grid">
<thead>
    <tr>
        <th>Field</th>
        <th>Time</th>
        <th>Value</th>
    </tr>
</thead>
<tbody>
    <% foreach (var c in Model.PeopleExtras)
       { %>
    <tr>
        <td valign="top"><%=c.Field%></td>
        <td valign="top"><%=c.TransactionTime.ToString("M/d/yy h:mm tt")%></td>
        <% if (c.StrValue.HasValue())
           { %>        
        <td><%=c.StrValue%></td>
        <% }
           else if (c.Data.HasValue())
           { %>
        <td><%=Util.SafeFormat(c.Data)%></td>
        <% }
           else if (c.DateValue.HasValue)
           { %>
        <td><%=c.DateValue.FormatDate()%></td>
        <% }
           else
           { %>   
        <td><%=c.IntValue%> <%=c.IntValue2%></td>
        <% } %>
    </tr>
    <% }
       foreach (var c in Model.Family.FamilyExtras)
       { %>
    <tr>
        <td valign="top"><%=c.Field%></td>
        <td><%=Util.SafeFormat(c.Data) %></td>
    </tr>
    <% } %>
</tbody>
</table>
<% } %>
