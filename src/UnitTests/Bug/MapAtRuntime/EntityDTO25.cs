namespace OmmitedDTOModel3;

public class EntityDTO25 : BaseEntity
{
    public EntityDTO25()
    {
        this.Entities19 = new EntityDTO19();
    }

    public EntityDTO19 Entities19 { get; set; }
    public Guid? Entity8Id { get; set; }
    public EntityDTO8 Entity8 { get; set; }
    public Guid? Entity17Id { get; set; }
    public EntityDTO17 Entity17 { get; set; }
}
