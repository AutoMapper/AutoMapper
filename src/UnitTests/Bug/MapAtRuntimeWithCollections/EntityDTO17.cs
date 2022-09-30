namespace OmmitedDTOModel3WithCollections;

public class EntityDTO17 :BaseEntity
{
    public EntityDTO17()
    {
        this.Entities20 = new List<EntityDTO20>();
        this.Entities8 = new List<EntityDTO8>();
        this.Entities5 = new List<EntityDTO5>();
        this.Entities18 = new List<EntityDTO18>();
    }

    public ICollection<EntityDTO20> Entities20 { get; set; }
    public ICollection<EntityDTO8> Entities8 { get; set; }
    public ICollection<EntityDTO5> Entities5 { get; set; }
    public ICollection<EntityDTO18> Entities18 { get; set; }
}
