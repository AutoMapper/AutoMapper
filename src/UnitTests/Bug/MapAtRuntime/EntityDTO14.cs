namespace OmmitedDTOModel3;

public class EntityDTO14 : BaseEntity
{
    public EntityDTO14()
    {
        this.Entities12 = new EntityDTO12();
        this.Entities1 = new EntityDTO1();
    }

    //public Address Address { get; set; }
    public EntityDTO12 Entities12 { get; set; }
    public EntityDTO1 Entities1 { get; set; }
}
