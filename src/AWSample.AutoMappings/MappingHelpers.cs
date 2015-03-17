using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSample.AutoMappings
{
    public static class MappingHelpers
    {
        [DbFunction("Edm", "TruncateTime")]
        public static DateTime? TruncateTime(DateTime? date)
        {
            return date.HasValue ? date.Value.Date : (DateTime?)null;
        }
    }
}
