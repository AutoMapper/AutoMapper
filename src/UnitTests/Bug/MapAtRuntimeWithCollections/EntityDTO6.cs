namespace OmmitedDTOModel3WithCollections;

public class EntityDTO6 : BaseEntity
{
    public EntityDTO6()
    {
        this.Entities12 = new List<EntityDTO12>();
    }

    public Guid Entity5Id { get; set; }
    public EntityDTO5 Entity5 { get; set; }
    public Guid Entity20Id { get; set; }
    public EntityDTO20 Entity20 { get; set; }
    public ICollection<EntityDTO12> Entities12 { get; set; }
}
