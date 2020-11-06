using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResTB.Map.Tools
{
    public class DBConnectionTool : BaseTool
    {

        public string Host { get; set; }
        public int Port { get; set; }
        public string DBName { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        private string _configDBConnectionName = "ResTBLocalDB";
        public string ConfigDBConnectionName { get { return _configDBConnectionName; } set { _configDBConnectionName = value; } }

        public DBConnectionTool(AxMapWinGIS.AxMap axMap, MapControlTools mapControlTool) : base(axMap, mapControlTool)
        {
            try
            {
                if (ConfigurationManager.AppSettings["UseOfflineDB"] == "true") ConfigDBConnectionName = "ResTBLocalDB";
                else ConfigDBConnectionName = "ResTBOnlineDB";


                string appconnection = ConfigurationManager.ConnectionStrings[ConfigDBConnectionName].ConnectionString;
                ParseEFConnectionString(appconnection);
            }
            catch (Exception e)
            {
                Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.MissingDBConfiguration, InMethod = "DBConnectionTool", InnerException = e };
                On_Error(error);
            }
        }

        public DBConnectionTool(AxMapWinGIS.AxMap axMap, MapControlTools mapControlTool, string configDBConnectionName) : base(axMap, mapControlTool)
        {
            try
            {
                ConfigDBConnectionName = configDBConnectionName;
                string appconnection = ConfigurationManager.ConnectionStrings[ConfigDBConnectionName].ConnectionString;
                ParseEFConnectionString(appconnection);
            }
            catch (Exception e)
            {
                Events.MapControl_Error error = new Events.MapControl_Error() { ErrorCode = Events.ErrorCodes.MissingDBConfiguration, InMethod = "DBConnectionTool", InnerException = e };
                On_Error(error);
            }
        }

        public DBConnectionTool(AxMapWinGIS.AxMap axMap, MapControlTools mapControlTool, string EFConnectionString, bool parseEFConnection) : base(axMap, mapControlTool)
        {
            if (parseEFConnection) ParseEFConnectionString(EFConnectionString);
        }

        public void ParseEFConnectionString(string eFConnectionString)
        {
            // EFConnectionString like server=localhost;Port=5432;Database=restb;User Id=postgres;Password=
            string[] parts = eFConnectionString.Split(';');
            foreach (string parameter in parts)
            {
                string[] paraparts = parameter.Split('=');

                if (paraparts.Length == 2)
                {
                    switch (paraparts[0].ToLower())
                    {
                        case "server":
                            Host = paraparts[1];
                            break;
                        case "port":
                            int _port;
                            Int32.TryParse(paraparts[1], out _port);
                            if (_port > 0) Port = Port;
                            break;
                        case "database":
                            DBName = paraparts[1];
                            break;
                        case "user id":
                            User = paraparts[1];
                            break;
                        case "password":
                            Password = paraparts[1];
                            break;
                    }
                }
            }
        }

        public string GetGdalConnectionString()
        {
            return "PG:host=" + Host + " dbname=" + DBName + " user=" + User + " password=" + Password;
        }
    }
}
