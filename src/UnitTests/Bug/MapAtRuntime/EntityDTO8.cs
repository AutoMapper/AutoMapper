namespace OmmitedDTOModel3;

public class EntityDTO8 : BaseEntity
{
    public EntityDTO8()
    {
        this.Entities20 = new EntityDTO20();
        this.Entities22 = new EntityDTO22();
        this.Entities3 = new EntityDTO3();
        this.Entities11 = new EntityDTO11();
        this.Entities17 = new EntityDTO17();
    }

    public EntityDTO20 Entities20 { get; set; }
    public EntityDTO17 Entities17 { get; set; }
    public EntityDTO22 Entities22 { get; set; }
    public EntityDTO3 Entities3 { get; set; }
    public EntityDTO11 Entities11 { get; set; }
}
