using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB.DB.Models
{
    public class PostGISHatObjektparameter
    {
        public int ID { get; set; }
        public int PostGISID { get; set; }
        public Objectparameter Objektparameter { get; set; }
    }
}