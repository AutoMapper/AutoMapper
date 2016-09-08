
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapperSamples.EF.Model
{
    /// <summary>
    /// copied from BreezeJS samples: https://github.com/Breeze/breeze.js.samples/blob/master/net/NorthBreeze/Model_NorthwindIB_NH/Entities/
    /// </summary>
    public class OrderDetail
    {
        public virtual Guid OrderID { get; set; }
        public virtual Guid ProductID { get; set; }
        public virtual decimal UnitPrice { get; set; }
        public virtual short Quantity { get; set; }
        public virtual float Discount { get; set; }
        public virtual int RowVersion { get; set; }

        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }

        public override int GetHashCode()
        {
            if (OrderID == Guid.Empty) return base.GetHashCode(); //transient instance
            if (ProductID == Guid.Empty) return base.GetHashCode(); //transient instance
            return OrderID.GetHashCode() ^ ProductID.GetHashCode();

        }

        public override bool Equals(object obj)
        {
            var x = obj as OrderDetail;
            if (x == null) return false;
            if (OrderID == Guid.Empty && x.OrderID == Guid.Empty) return ReferenceEquals(this, x);
            if (ProductID == Guid.Empty && x.ProductID == Guid.Empty) return ReferenceEquals(this, x);
            return (OrderID == x.OrderID) && (ProductID == x.ProductID);

        }
    }
}
