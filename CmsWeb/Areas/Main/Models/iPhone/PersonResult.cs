﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Web.Mvc;
using System.Xml.Linq;
using UtilityExtensions;
using System.Linq;
using CmsData;

namespace CmsWeb.Models.iPhone
{
    public class PersonResult : ActionResult
    {
        private List<PeopleInfo> items;
        private int count;
        public PersonResult(int id)
        {
            this.items = items.ToList();
        }
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "text/xml";
            var settings = new XmlWriterSettings();
            settings.Encoding = new System.Text.UTF8Encoding(false);

            using (var w = XmlWriter.Create(context.HttpContext.Response.OutputStream, settings))
            {
                w.WriteStartElement("People");
                w.WriteAttributeString("count", count.ToString());

                foreach (var p in items)
                {
                    w.WriteStartElement("Person");
                    w.WriteAttributeString("peopleid", p.PeopleId.ToString());
                    w.WriteAttributeString("name", p.Name);
                    w.WriteAttributeString("address", p.Address);
                    w.WriteAttributeString("citystatezip", p.CityStateZip);
                    w.WriteAttributeString("age", p.Age.ToString());
                    w.WriteAttributeString("birthdate", p.BirthDate);
                    w.WriteAttributeString("homephone", p.HomePhone);
                    w.WriteAttributeString("cellphone", p.CellPhone);
                    w.WriteAttributeString("workphone", p.WorkPhone);
                    w.WriteAttributeString("memberstatus", p.MemberStatus);
                    w.WriteAttributeString("email", p.Email);
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }
        }
    }
}