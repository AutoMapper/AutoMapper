namespace OmmitedDatabaseModel3;

public class Entity3 : BaseEntity
{
    public Entity3()
    {
        this.Entities4 = new Entity4();
        this.Entities8 = new Entity8();
    }
    public Entity4 Entities4 { get; set; }
    public Entity8 Entities8 { get; set; }
}
