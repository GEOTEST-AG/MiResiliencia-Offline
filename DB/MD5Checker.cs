using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ResTB.DB
{
    public class MD5Checker
    {
        /// <summary>
        /// Checks that the default values in the database are correct and have not been changed.
        /// </summary>
        /// <returns>true if the MD5 Hash is correct</returns>
        public static bool CheckMD5Hash(string checkHash, out string md5hash)
        {
            using (ResTBContext db = new DB.ResTBContext())
            {
                StringBuilder query = new StringBuilder();
                query.Append("select md5(CAST((array_agg(total.* order by total.\"id\"))AS text)) as md5 ");
                query.Append("from ( ");
                query.Append("select 1 as id, md5(CAST((array_agg(f.* order by f.\"ID\"))AS text)) as md5 ");
                query.Append("from \"ObjectparameterPerProcess\" f ");
                query.Append("union ");
                query.Append("select 2,  md5(CAST((array_agg(spa.* order by spa.\"NatHazardId\", spa.\"IKClassesId\"))AS text)) ");
                query.Append("from \"Standard_PrA\" spa ");
                query.Append("union ");
                query.Append("SELECT 3, md5(CAST((array_agg(wtp.* order by wtp.\"ID\"))AS text)) ");
                query.Append("from \"WillingnessToPay\" wtp ");
                query.Append("union ");
                query.Append("SELECT 3, md5(CAST((array_agg(o.* order by o.\"ID\"))AS text)) ");
                query.Append("from \"Objectparameter\" o ");
                query.Append("where o.\"ID\" < 100 ");
                query.Append(") as total ");
                
                md5hash = db.Database.SqlQuery<string>(query.ToString()).FirstOrDefault();

                if (md5hash == checkHash) return true;
                else return false;


            }
        }

    }
}
