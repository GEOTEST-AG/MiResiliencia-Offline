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

            //string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //return connectionString;
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
                    //HttpContext.Current != null
                    //? getFactory<WebSessionContext>()
                    //: getFactory<ThreadStaticSessionContext>();

                    //ISessionFactory sessionFactory = Fluently.Configure()
                    //.Database(PostgreSQLConfiguration
                    //            .Standard
                    //            .ConnectionString(GetConnectionString())
                    //            //.ShowSql()
                    //            .Dialect<NHibernate.Spatial.Dialect.PostGisDialect>()
                    //          )
                    //.Mappings(m => m.FluentMappings.AddFromAssemblyOf<ProjectMap>())
                    ////.ExposeConfiguration(x => x.SetInterceptor(new LoggingInterceptor()))
                    //.CurrentSessionContext<WebSessionContext>()
                    //.BuildSessionFactory();

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
                //ISession session;

                //if (NHibernate.Context.CurrentSessionContext.HasBind(Factory))
                //{
                //    session = Factory.GetCurrentSession();
                //}
                //else
                //{
                //    session = Factory.OpenSession();
                //    NHibernate.Context.CurrentSessionContext.Bind(session);
                //}

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

        //const string SessionKey = "NhibernateSessionPerRequest";
        //public static ISession openSession()
        //{
        //    var context = HttpContext.Current;

        //    //Check whether there is an already open ISession for this request
        //    if (context != null && context.Items.Contains(SessionKey))
        //    {
        //        //Return the open ISession
        //        return (ISession)context.Items[SessionKey];
        //    }
        //    else
        //    {
        //        //Create a new ISession and store it in HttpContext
        //        var newSession = Factory.OpenSession();
        //        if (context != null)
        //            context.Items[SessionKey] = newSession;

        //        return newSession;
        //    }
        //}

        ///// <summary>
        ///// closes active session and opens new one
        ///// </summary>
        ///// <returns></returns>
        //public static ISession NewSession(bool closeOldSession = false)
        //{
        //    if (_activeSession != null && _activeSession.IsOpen && closeOldSession)
        //    {
        //        _activeSession.Close();
        //    }
        //    _activeSession = Factory.OpenSession();
        //    return _activeSession;
        //}

    }
}