using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
//using System.Data.Odbc;
//using System.Web.Script.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using Cliver.Bot;

namespace Cliver.CrawlerHost.Models
{
    public class DbApi:CrawlerHostDataContext
    {
        public DbApi()
            : base(ConnectionString)
        {
            try
            {
                if (!ProgramRoutines.IsWebContext)
                   Cliver.Bot.Log.Main.Write("DbApi ConnectionString: " + ConnectionString);
            }
            catch (Exception e)
            {
                string m = "The app could not connect the database with string:" + ConnectionString + "\r\nBe sure " + Cliver.CrawlerHost.Api.CrawlerHost_CONGIG_FILE_NAME + " file exists and is correct.\r\n\r\n" + e.Message;
                if (!ProgramRoutines.IsWebContext)
                    LogMessage.Exit(m);
                else
                    throw new Exception(m);
            }
        }
        
        public static readonly string ConnectionString = Cliver.CrawlerHost.Api.GetConnectionString(DATABASE_CONNECTION_STRING_NAME);
        public const string DATABASE_CONNECTION_STRING_NAME = "CliverCrawlerHostConnectionString";

        public static void RenewContext(ref DbApi context)
        {
            if (context != null)
                context.Dispose();
            context = new DbApi();
        }
    }
}
