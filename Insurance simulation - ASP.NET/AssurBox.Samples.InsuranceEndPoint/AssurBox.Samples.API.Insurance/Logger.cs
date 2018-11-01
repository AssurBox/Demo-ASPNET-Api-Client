using AssurBox.Samples.API.Insurance.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AssurBox.Samples.API.Insurance
{
    public static class Logger
    {
        public static void Log(string title, string content = "")
        {
            using (DAL.ApiDataContext ctx = new DAL.ApiDataContext())
            {
                Logs log = new Logs
                {
                    Title = title,
                    Content = content,
                    LogDate = DateTime.UtcNow
                };
                ctx.Logs.Add(log);
                ctx.SaveChanges();

            }
        }
    }
}