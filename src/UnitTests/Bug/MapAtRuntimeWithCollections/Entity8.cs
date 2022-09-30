namespace OmmitedDatabaseModel3WithCollections;

public class Entity8 : BaseEntity
{
    public Entity8()
    {
        this.Entities20 = new List<Entity20>();
        this.Entities22 = new List<Entity22>();
        this.Entities3 = new List<Entity3>();
        this.Entities11 = new List<Entity11>();
        this.Entities17 = new List<Entity17>();
    }

    public ICollection<Entity20> Entities20 { get; set; }
    public ICollection<Entity17> Entities17 { get; set; }
    public Entity17 Entity17 { get; set; }
    public ICollection<Entity22> Entities22 { get; set; }
    public Entity22 Entity22 { get; set; }
    public ICollection<Entity3> Entities3 { get; set; }
    public ICollection<Entity11> Entities11 { get; set; }
    public Entity11 Entity11 { get; set; }
}
