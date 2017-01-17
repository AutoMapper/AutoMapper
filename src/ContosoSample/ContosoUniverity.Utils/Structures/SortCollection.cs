using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.Utils.Structures
{
    public class SortCollection
    {
        public SortCollection() { }
        public SortCollection(ICollection<SortDescription> sortDescriptions)
        {
            this.SortDescriptions = sortDescriptions;
            this.Skip = 0;
            this.Take = 20;
        }

        public SortCollection(ICollection<SortDescription> sortDescriptions, int skip, int take)
        {
            this.SortDescriptions = sortDescriptions;
            this.Skip = skip;
            this.Take = take;
        }

        public ICollection<SortDescription> SortDescriptions { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
