using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoMapper.Internal
{
    public class MemberNameReplacer
    {
        public MemberNameReplacer(string originalValue, string alias)
        {
            OrginalValue = originalValue;
            Alias = alias;
        }

        public string OrginalValue { get; private set; }
        public string Alias { get; private set; }
    }
}
