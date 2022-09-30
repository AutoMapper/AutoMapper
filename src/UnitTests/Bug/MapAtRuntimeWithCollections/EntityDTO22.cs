namespace OmmitedDTOModel3WithCollections;

public class EntityDTO22 : BaseEntity
{
    public EntityDTO22()
    {
        this.Entities20 = new List<EntityDTO20>();
        this.Entities24 = new List<EntityDTO24>();
    }
    public ICollection<EntityDTO20> Entities20 { get; set; }
    public ICollection<EntityDTO24> Entities24 { get; set; }
}
