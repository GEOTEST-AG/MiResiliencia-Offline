using NHibernate.Spatial.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Models
{
    public class MyPostGisGeometryType : PostGisGeometryType
    {
        protected override void SetDefaultSRID(GeoAPI.Geometries.IGeometry geometry)
        {
            geometry.SRID = 3857;
        }
    }
}