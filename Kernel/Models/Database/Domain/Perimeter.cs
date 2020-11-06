using System;
using System.Text;
using System.Collections.Generic;
using GeoAPI.Geometries;
using System.Linq;

namespace ResTB_API.Models.Database.Domain
{

    public class Perimeter
    {
        public virtual int Id { get; set; }
        public virtual Project Project { get; set; }
        public virtual IGeometry geometry { get; set; }

    }
}
