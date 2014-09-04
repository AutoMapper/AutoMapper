using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoMapper.Internal
{
    public class MemberNameReplacer
    {
        public MemberNameReplacer(string originalValue, string newValue)
        {
            OrginalValue = originalValue;
            NewValue = newValue;
        }

        public string OrginalValue { get; private set; }
        public string NewValue { get; private set; }
    }
}
