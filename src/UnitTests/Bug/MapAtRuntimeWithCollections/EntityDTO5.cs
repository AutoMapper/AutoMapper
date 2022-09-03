namespace OmmitedDTOModel3WithCollections;

public class EntityDTO5 : BaseEntity
{
    public EntityDTO5()
    {
        this.Entities6 = new List<EntityDTO6>();
        this.TimeSlots = new List<EntityDTO23>();
        this.Entities5 = new List<EntityDTO5>();
    }

    public Guid? Entity8Id { get; set; }
    public EntityDTO8 Entity8 { get; set; }
    public Guid? Entity17Id { get; set; }
    public EntityDTO17 Entity17 { get; set; }
    public Guid? Entity5Id { get; set; }
    public EntityDTO5 Entity5Exception { get; set; }
    public ICollection<EntityDTO5> Entities5 { get; set; }
    public List<EntityDTO6> Entities6 { get; set; }
    public ICollection<EntityDTO23> TimeSlots { get; set; }
}
