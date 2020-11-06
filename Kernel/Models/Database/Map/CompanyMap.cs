using FluentNHibernate.Mapping;
using ResTB_API.Models.Database.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResTB_API.Models.Database.Map
{
    //public class CompanyMap : ClassMap<Company>
    //{

    //    public CompanyMap()
    //    {
    //        Schema("public");
    //        Table("\"Company\"");
    //        LazyLoad();
    //        ReadOnly();
    //        Id(x => x.ID).GeneratedBy.Identity().Column("\"ID\"");
    //        Map(x => x.CompanyName).Column("\"CompanyName\"");

    //        References<Company>(x => x.SuperCompany).Column("\"MySuperCompany_ID\"");
    //        //HasManyToMany(x => x.Projects).Table("\"ProjectCompany\"")
    //        //    .ChildKeyColumns.Add("\"Project_Id\"")
    //        //    .ParentKeyColumns.Add("\"Company_ID\"")
    //        //    .Not.LazyLoad();
    //    }
    //}
}