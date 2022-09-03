namespace OmmitedDatabaseModel3WithCollections;

public class Entity25 : BaseEntity
{
    public Entity25()
    {
        this.Entities19 = new List<Entity19>();
    }

    public ICollection<Entity19> Entities19 { get; set; }
    public Guid? Entity8Id { get; set; }
    public Entity8 Entity8 { get; set; }
    public Guid? Entity17Id { get; set; }
    public Entity17 Entity17 { get; set; }
}
