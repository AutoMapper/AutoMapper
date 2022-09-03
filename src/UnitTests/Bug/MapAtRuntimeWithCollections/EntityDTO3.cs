namespace OmmitedDTOModel3WithCollections;

public class EntityDTO3 : BaseEntity
{
    public EntityDTO3()
    {
        this.Entities4 = new List<EntityDTO4>();
        this.Entities8 = new List<EntityDTO8>();
    }
    public ICollection<EntityDTO4> Entities4 { get; set; }
    public ICollection<EntityDTO8> Entities8 { get; set; }
}
