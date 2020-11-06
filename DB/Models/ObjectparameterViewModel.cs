using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB.DB.Models
{
    public class ObjectparameterViewModel
    {
        public MappedObject MappedObject { get; set; }
        public Objectparameter MergedObjectparameter { get; set; }
        public List<ObjectparameterHasProperties> HasProperties { get; set; }

    }
}