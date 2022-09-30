namespace OmmitedDatabaseModel3WithCollections;

public class Entity20 : BaseEntity
{
    public Entity20()
    {
        this.Entities8 = new List<Entity8>();
        this.Entities26 = new List<Entity26>();
        this.Entities12 = new List<Entity12>();
        this.Entities17 = new List<Entity17>();
        this.Entities21 = new List<Entity21>();
        this.Entities16 = new List<Entity16>();
    }

    public Guid Entity3Id { get; set; }
    public Entity3 Entity3 { get; set; }
    public Guid Entity22Id { get; set; }
    public Entity22 Entity22 { get; set; }
    public ICollection<Entity8> Entities8 { get; set; }
    public Entity8 Entity8 { get; set; }
    public ICollection<Entity26> Entities26 { get; set; }
    public ICollection<Entity12> Entities12 { get; set; }
    public ICollection<Entity17> Entities17 { get; set; }
    public Entity17 Entitys17 { get; set; }
    public ICollection<Entity21> Entities21 { get; set; }
    public ICollection<Entity16> Entities16 { get; set; }
}
