using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CmsData.Codes;
using Dapper;
using HandlebarsDotNet;
using IronPython.Hosting;
using UtilityExtensions;

namespace CmsData
{
    public partial class QueryFunctions
    {
        public int QueryCountDate(string s, object startdt)
        {
            var start = startdt.ToDate();
            if (start == null)
                throw new Exception("bad date: " + startdt);
            db.QbStartDateOverride = start;
            db.QbEndDateOverride = start;

            try
            {
                var qb = db.PeopleQuery2(s);
                if (qb == null)
                    return 0;
                return qb.Count();
            }
            finally
            {
                db.QbStartDateOverride = null;
                db.QbEndDateOverride = null;
                db.QbDivisionOverride = null;
            }
        }

        public int QueryCountDateRange(string s, object startdt, int days)
        {
            var start = startdt.ToDate();
            if (start == null)
                throw new Exception("bad date: " + startdt);
            var enddt = start.Value.AddDays(days);
            db.QbStartDateOverride = start;
            db.QbEndDateOverride = enddt;

            try
            {
                var qb = db.PeopleQuery2(s);
                if (qb == null)
                    return 0;
                return qb.Count();
            }
            finally
            {
                db.QbStartDateOverride = null;
                db.QbEndDateOverride = null;
                db.QbDivisionOverride = null;
            }
        }

        public int QueryCountDivDate(string s, string division, object startdt)
        {
            return QueryCountDivDate(s, null, division, startdt);
        }

        public int QueryCountDivDate(string s, int? progid, string division, object startdt)
        {
            var start = startdt.ToDate();
            if (start == null)
                throw new Exception("bad date: " + startdt);
            var divid = (from dd in db.Divisions
                         where dd.Name == division
                         where progid == null || dd.ProgId == progid
                         select dd.Id).SingleOrDefault();
            db.QbStartDateOverride = start;
            db.QbEndDateOverride = start;
            db.QbDivisionOverride = divid;

            try
            {
                var qb = db.PeopleQuery2(s);
                if (qb == null)
                    return 0;
                return qb.Count();
            }
            finally
            {
                db.QbStartDateOverride = null;
                db.QbEndDateOverride = null;
                db.QbDivisionOverride = null;
            }
        }

        public int QueryCountDivDateRange(string s, string division, object startdt, int days)
        {
            return QueryCountProgIdDivDateRange(s, null, division, startdt, days);
        }

        public int QueryCountProgIdDivDateRange(string s, int? progid, string division, object startdt, int days)
        {
            var start = startdt.ToDate();
            if (start == null)
                throw new Exception("bad date: " + startdt);
            var enddt = start.Value.AddDays(days);
            var divid = (from dd in db.Divisions
                         where dd.Name == division
                         where progid == null || dd.ProgId == progid
                         select dd.Id).SingleOrDefault();
            db.QbStartDateOverride = start;
            db.QbEndDateOverride = enddt;
            db.QbDivisionOverride = divid;

            try
            {
                var qb = db.PeopleQuery2(s);
                if (qb == null)
                    return 0;
                return qb.Count();
            }
            finally
            {
                db.QbStartDateOverride = null;
                db.QbEndDateOverride = null;
                db.QbDivisionOverride = null;
            }
        }
    }
}