namespace OmmitedDTOModel3WithCollections;

public class EntityDTO14 : BaseEntity
{
    public EntityDTO14()
    {
        this.Entities12 = new List<EntityDTO12>();
        this.Entities1 = new List<EntityDTO1>();
    }

    //public Address Address { get; set; }
    public ICollection<EntityDTO12> Entities12 { get; set; }
    public ICollection<EntityDTO1> Entities1 { get; set; }
}
