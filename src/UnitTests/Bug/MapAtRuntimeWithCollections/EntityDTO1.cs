namespace OmmitedDTOModel3WithCollections;

public class EntityDTO1 : BaseEntity
{
    public EntityDTO1()
    {
        this.Entities2 = new List<EntityDTO2>();
    }
    public Guid Entity17Id { get; set; }
    public EntityDTO17 Entity17 { get; set; }
    public Guid? Entity22Id { get; set; }
    public EntityDTO22 Entity22 { get; set; }
    public Guid? Entity20Id { get; set; }
    public EntityDTO20 Entity20 { get; set; }
    public Guid? Entity12Id { get; set; }
    public EntityDTO12 Entity12 { get; set; }
    public Guid Entity14Id { get; set; }
    public EntityDTO14 Entity14 { get; set; }
    public Guid Entity8Id { get; set; }
    public EntityDTO8 Entity8 { get; set; }
    public ICollection<EntityDTO2> Entities2 { get; set; }
}
