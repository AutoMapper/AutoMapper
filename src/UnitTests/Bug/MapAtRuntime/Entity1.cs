namespace OmmitedDatabaseModel3;

public class Entity1 : BaseEntity
{
    public Entity1()
    {
        this.Entities2 = new Entity2();
    }
    public Guid Entity17Id { get; set; }
    public Entity17 Entity17 { get; set; }
    public Guid? Entity22Id { get; set; }
    public Entity22 Entity22 { get; set; }
    public Guid? Entity20Id { get; set; }
    public Entity20 Entity20 { get; set; }
    public Guid? Entity12Id { get; set; }
    public Entity12 Entity12 { get; set; }
    public Guid Entity14Id { get; set; }
    public Entity14 Entity14 { get; set; }
    public Guid Entity8Id { get; set; }
    public Entity8 Entity8 { get; set; }
    public Entity2 Entities2 { get; set; }
}
