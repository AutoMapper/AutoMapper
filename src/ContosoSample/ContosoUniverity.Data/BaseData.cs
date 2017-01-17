using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Data
{
    abstract public class BaseData
    {
        [NotMapped]
        public EntityStateType EntityState { get; set; }
    }
}
