namespace OmmitedDatabaseModel3WithCollections;

public class Entity12 : BaseEntity
{
    public Entity12()
    {
        this.Entities20 = new List<Entity20>();
        this.Entities14 = new List<Entity14>();
        this.Entities16 = new List<Entity16>();
    }
    public ICollection<Entity20> Entities20 { get; set; }
    public ICollection<Entity16> Entities16 { get; set; }
    public ICollection<Entity14> Entities14 { get; set; }
}
