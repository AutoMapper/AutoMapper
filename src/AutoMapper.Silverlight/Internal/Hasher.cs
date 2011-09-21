/*  
 Copyright 2008 The 'A Concurrent Hashtable' development team  
 (http://www.codeplex.com/CH/People/ProjectPeople.aspx)

 This library is licensed under the GNU Library General Public License (LGPL).  You should 
 have received a copy of the license along with the source code.  If not, an online copy
 of the license can be found at http://www.codeplex.com/CH/license.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvdP.Collections
{
    static class Hasher
    {
        public static UInt32 Rehash(Int32 hash)
        {
            unchecked
            {
                Int64 prod = ((Int64)hash ^ 0x00000000691ac2e9L) * 0x00000000a931b975L;
                return (UInt32)(prod ^ (prod >> 32));
            }
        }
    }
}
