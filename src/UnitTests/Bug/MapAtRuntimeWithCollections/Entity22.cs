namespace OmmitedDatabaseModel3WithCollections;

public class Entity22 : BaseEntity
{
    public Entity22()
    {
        this.Entities20 = new List<Entity20>();
        this.Entities24 = new List<Entity24>();
    }
    public ICollection<Entity20> Entities20 { get; set; }
    public ICollection<Entity24> Entities24 { get; set; }
}
