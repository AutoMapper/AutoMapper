namespace OmmitedDTOModel3WithCollections;

public class EntityDTO10 : BaseEntity
{
    public EntityDTO10()
    {
        this.Entities11 = new List<EntityDTO11>();
    }
    public ICollection<EntityDTO11> Entities11 { get; set; }
}
