namespace OmmitedDTOModel3WithCollections;

public class EntityDTO25 : BaseEntity
{
    public EntityDTO25()
    {
        this.Entities19 = new List<EntityDTO19>();
    }

    public ICollection<EntityDTO19> Entities19 { get; set; }
    public Guid? Entity8Id { get; set; }
    public EntityDTO8 Entity8 { get; set; }
    public Guid? Entity17Id { get; set; }
    public EntityDTO17 Entity17 { get; set; }
}
