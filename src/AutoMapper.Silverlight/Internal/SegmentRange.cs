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
    internal class Segmentrange<TStored, TSearch>
    {
        protected Segmentrange()
        { }

        public static Segmentrange<TStored, TSearch> Create(int segmentCount, int initialSegmentSize)
        {
            var instance = new Segmentrange<TStored, TSearch>();
            instance.Initialize(segmentCount, initialSegmentSize);
            return instance;
        }

        protected virtual void Initialize(int segmentCount, int initialSegmentSize)
        {
            _Segments = new Segment<TStored, TSearch>[segmentCount];

            for (int i = 0, end = _Segments.Length; i != end; ++i)
                _Segments[i] = CreateSegment(initialSegmentSize);

            for (int w = segmentCount; w != 0; w <<= 1)
                ++_Shift;
        }

        protected virtual Segment<TStored, TSearch> CreateSegment(int initialSegmentSize)
        { return Segment<TStored, TSearch>.Create(initialSegmentSize); }

        Segment<TStored, TSearch>[] _Segments;
        Int32 _Shift;

        public Segment<TStored, TSearch> GetSegment(UInt32 hash)
        { return _Segments[hash >> _Shift]; }

        public Segment<TStored, TSearch> GetSegmentByIndex(Int32 index)
        { return _Segments[index]; }

        public Int32 Count { get { return _Segments.Length; } }

        public Int32 Shift { get { return _Shift; } }
    }
}
