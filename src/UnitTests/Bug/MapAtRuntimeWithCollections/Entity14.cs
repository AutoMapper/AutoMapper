namespace OmmitedDatabaseModel3WithCollections;

public class Entity14 : BaseEntity
{
    public Entity14()
    {
        this.Entities12 = new List<Entity12>();
        this.Entities1 = new List<Entity1>();
    }

    public ICollection<Entity12> Entities12 { get; set; }
    public ICollection<Entity1> Entities1 { get; set; }
}
