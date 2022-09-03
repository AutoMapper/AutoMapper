namespace OmmitedDatabaseModel3WithCollections;

public class Entity3 : BaseEntity
{
    public Entity3()
    {
        this.Entities4 = new List<Entity4>();
        this.Entities8 = new List<Entity8>();
    }
    public ICollection<Entity4> Entities4 { get; set; }
    public ICollection<Entity8> Entities8 { get; set; }
}
