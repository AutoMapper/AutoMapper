namespace OmmitedDTOModel3WithCollections;

public class EntityDTO20 : BaseEntity
{
    public EntityDTO20()
    {
        this.Entities8 = new List<EntityDTO8>();
        this.Entities26 = new List<EntityDTO26>();
        this.Entities12 = new List<EntityDTO12>();
        this.Entities17 = new List<EntityDTO17>();
        this.Entities21 = new List<EntityDTO21>();
        this.Entities16 = new List<EntityDTO16>();
    }

    public Guid Entity3Id { get; set; }
    public EntityDTO3 Entity3 { get; set; }
    public Guid Entity22Id { get; set; }
    public EntityDTO22 Entity22 { get; set; }
    public ICollection<EntityDTO8> Entities8 { get; set; }
    public ICollection<EntityDTO26> Entities26 { get; set; }
    public ICollection<EntityDTO12> Entities12 { get; set; }
    public ICollection<EntityDTO17> Entities17 { get; set; }
    public ICollection<EntityDTO21> Entities21 { get; set; }
    public ICollection<EntityDTO16> Entities16 { get; set; }
}
