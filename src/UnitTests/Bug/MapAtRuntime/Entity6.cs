namespace OmmitedDatabaseModel3;

public class Entity6 : BaseEntity
{
    public Entity6()
    {
        this.Entities12 = new Entity12();
    }

    public Guid Entity5Id { get; set; }
    public Entity5 Entity5 { get; set; }
    public Guid Entity20Id { get; set; }
    public Entity20 Entity20 { get; set; }
    public Entity12 Entities12 { get; set; }
}
