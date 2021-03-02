using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Kernel;
using NHibernate;
using NHibernate.Context;
using NHibernate.SqlCommand;
using ResTB_API.Models.Database.Map;
using System.Configuration;
using System.Diagnostics;

namespace ResTB_API.Helpers
{
    public class DBManager
    {
        private static ISessionFactory _factory = null;


        /// <summary>
        /// Create Connectionstring to DB
        /// </summary>
        /// <returns></returns>
        static string GetConnectionString()
        {
            string conn = "";
            if (!Globals.ISONLINE)
                conn = ConfigurationManager.ConnectionStrings["ResTBLocalDB"].ConnectionString;
            else
                conn = ConfigurationManager.ConnectionStrings["ResTBOnlineDB"].ConnectionString;

            return conn;
        }

        /// <summary>
        /// SessionFactory singleton
        /// </summary>
        private static ISessionFactory Factory
        {
            get
            {
                if (_factory == null)
                {
                    ISessionFactory sessionFactory = getFactory();
                    _factory = sessionFactory;
                }

                return _factory;
            }
            set
            {
                _factory = value;
            }
        }

        private static ISessionFactory getFactory() //<T>() where T : ICurrentSessionContext
        {
            ISessionFactory sessionFactory = Fluently.Configure()
                                   .Database(PostgreSQLConfiguration
                                               .Standard
                                               .ConnectionString(GetConnectionString())
                                               //.ShowSql()
                                               .Dialect<NHibernate.Spatial.Dialect.PostGisDialect>()
                                             )
                                   .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ProjectMap>())
                                   //.ExposeConfiguration(x => x.SetInterceptor(new LoggingInterceptor()))
                                   //.CurrentSessionContext<T>()
                                   .BuildSessionFactory();

            return sessionFactory;
        }

        public class LoggingInterceptor : EmptyInterceptor
        {
            public override SqlString OnPrepareStatement(SqlString sql)
            {

                Debug.WriteLine(sql);

                return sql;
            }
        }

        //private static ISession _activeSession = null;
        private static ISession _activeSession;
        public static ISession ActiveSession
        {
            get
            {
                if (_activeSession == null || !_activeSession.IsOpen)
                {
                    _activeSession = Factory.OpenSession();
                }
                return _activeSession;

            }
        }

        public static void Unbind()
        {
            CurrentSessionContext.Unbind(Factory);
        }

     
    }
}