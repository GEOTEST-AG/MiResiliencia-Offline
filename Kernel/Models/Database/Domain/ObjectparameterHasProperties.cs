namespace ResTB_API.Models.Database.Domain
{
    public class ObjectparameterHasProperties
    {
        public virtual int ID { get; set; }
        public virtual string Property { get; set; }
        public virtual bool isOptional { get; set; }
        public virtual Objectparameter Objectparameter { get; set; }

    }

}