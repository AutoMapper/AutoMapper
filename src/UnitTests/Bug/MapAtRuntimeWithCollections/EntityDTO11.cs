namespace OmmitedDTOModel3WithCollections;

public class EntityDTO11 : BaseEntity
{
    public EntityDTO11()
    {
        this.Entities10 = new List<EntityDTO10>();
        this.Entities8 = new List<EntityDTO8>();
    }
    public ICollection<EntityDTO10> Entities10 { get; set; }
    public ICollection<EntityDTO8> Entities8 { get; set; }
}
