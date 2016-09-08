using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperSamples.EF.Model
{
    /// <summary>
    /// copied from BreezeJS samples: https://github.com/Breeze/breeze.js.samples/blob/master/net/NorthBreeze/Model_NorthwindIB_NH/Entities/
    /// </summary>
    public class Product
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual Guid Id { get; set; }
        public virtual string ProductName { get; set; }
        public virtual string QuantityPerUnit { get; set; }
        public virtual decimal? UnitPrice { get; set; }
        public virtual short? UnitsInStock { get; set; }
        public virtual short? UnitsOnOrder { get; set; }
        public virtual short? ReorderLevel { get; set; }
        public virtual bool Discontinued { get; set; }
        public virtual DateTime? DiscontinuedDate { get; set; }
        public virtual int RowVersion { get; set; }
    }
}
