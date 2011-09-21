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
using System.Threading;
using System.Diagnostics;

namespace TvdP.Threading
{
    /// <summary>
    /// Tiny spin lock that allows multiple readers simultanously and 1 writer exclusively
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public struct TinyReaderWriterLock
    {
#if !SILVERLIGHT
        [NonSerialized]
#endif
        Int32 _Bits;

        const int ReadersInRegionOffset = 0;
        const int ReadersInRegionsMask = 255;
        const int ReadersWaitingOffset = 8;
        const int ReadersWaitingMask = 255;
        const int WriterInRegionOffset = 16;
        const int WritersWaitingOffset = 17;
        const int WritersWaitingMask = 15;
        const int BiasOffset = 21;
        const int BiasMask = 3;

        enum Bias { None = 0, Readers = 1, Writers = 2 };

        struct Data
        {
            public int _ReadersInRegion;
            public int _ReadersWaiting;
            public bool _WriterInRegion;
            public int _WritersWaiting;
            public Bias _Bias;
            public Int32 _OldBits;
        }

        void GetData(out Data data)
        {
            Int32 bits = _Bits;

            data._ReadersInRegion = bits & ReadersInRegionsMask;
            data._ReadersWaiting = (bits >> ReadersWaitingOffset) & ReadersWaitingMask;
            data._WriterInRegion = ((bits >> WriterInRegionOffset) & 1) != 0;
            data._WritersWaiting = (bits >> WritersWaitingOffset) & WritersWaitingMask;
            data._Bias = (Bias)((bits >> BiasOffset) & BiasMask);
            data._OldBits = bits;
        }

        bool SetData(ref Data data)
        {
            Int32 bits;

            bits =
                data._ReadersInRegion
                | (data._ReadersWaiting << ReadersWaitingOffset)
                | ((data._WriterInRegion ? 1 : 0) << WriterInRegionOffset)
                | (data._WritersWaiting << WritersWaitingOffset)
                | ((int)data._Bias << BiasOffset);

            return Interlocked.CompareExchange(ref _Bits, bits, data._OldBits) == data._OldBits;
        }

        /// <summary>
        /// Release a reader lock
        /// </summary>
        public void ReleaseForReading()
        {
            //try shortcut first.
            if (Interlocked.CompareExchange(ref _Bits, 0, 1) == 1)
                return;

            Data data;

            do
            {
                GetData(out data);

#if DEBUG
                if (data._ReadersInRegion == 0)
                    throw new InvalidOperationException("Mismatching Lock/Release for reading.");

                //if (data._WriterInRegion)
                //    throw new InvalidOperationException("Unexpected writer in region.");
#endif

                --data._ReadersInRegion;

                if (data._ReadersInRegion == 0 && data._ReadersWaiting == 0)
                    data._Bias = data._WritersWaiting != 0 ? Bias.Writers : Bias.None;
            }
            while (!SetData(ref data));
        }

        /// <summary>
        /// Release a writer lock
        /// </summary>
        public void ReleaseForWriting()
        {
            //try shortcut first.
            if (Interlocked.CompareExchange(ref _Bits, 0, 1 << WriterInRegionOffset) == 1 << WriterInRegionOffset)
                return;

            Data data;

            do
            {
                GetData(out data);

#if DEBUG
                if (!data._WriterInRegion)
                    throw new InvalidOperationException("Mismatching Lock/Release for writing.");

                //if (data._ReadersInRegion != 0)
                //    throw new InvalidOperationException("Unexpected reader in region.");
#endif

                data._WriterInRegion = false;

                if (data._WritersWaiting == 0)
                    data._Bias = data._ReadersWaiting != 0 ? Bias.Readers : Bias.None;
            }
            while (!SetData(ref data));
        }

        /// <summary>
        /// Aquire a reader lock. Wait until lock is aquired.
        /// </summary>
        public void LockForReading()
        { LockForReading(true); }

        /// <summary>
        /// Aquire a reader lock.
        /// </summary>
        /// <param name="wait">True if to wait until lock aquired, False to return immediately.</param>
        /// <returns>Boolean indicating if lock was successfuly aquired.</returns>
        public bool LockForReading(bool wait)
        {
            //try shortcut first.
            if (Interlocked.CompareExchange(ref _Bits, 1, 0) == 0)
                return true;

            bool waitingRegistered = false;

            try
            {
                while (true)
                {
                    bool retry = false;
                    Data data;
                    GetData(out data);

                    if (data._Bias != Bias.Writers)
                    {
                        if (data._ReadersInRegion < ReadersInRegionsMask && !data._WriterInRegion)
                        {
                            if (waitingRegistered)
                            {
                                data._Bias = Bias.Readers;
                                --data._ReadersWaiting;
                                ++data._ReadersInRegion;
                                if (SetData(ref data))
                                {
                                    waitingRegistered = false;
                                    return true;
                                }
                                else
                                    retry = true;
                            }
                            else if (data._WritersWaiting == 0)
                            {
                                data._Bias = Bias.Readers;
                                ++data._ReadersInRegion;
                                if (SetData(ref data))
                                    return true;
                                else
                                    retry = true;
                            }
                        }

                        //sleep
                    }
                    else
                    {
                        if (!waitingRegistered && data._ReadersWaiting < ReadersWaitingMask && wait)
                        {
                            ++data._ReadersWaiting;
                            if (SetData(ref data))
                            {
                                waitingRegistered = true;
                                //sleep
                            }
                            else
                                retry = true;
                        }

                        //sleep
                    }

                    if (!retry)
                    {
                        if (!wait)
                            return false;

                        System.Threading.Thread.Sleep(0);
                    }
                }
            }
            finally
            {
                if (waitingRegistered)
                {
                    //Thread aborted?
                    Data data;

                    do
                    {
                        GetData(out data);
                        --data._ReadersWaiting;

                        if (data._ReadersInRegion == 0 && data._ReadersWaiting == 0)
                            data._Bias = data._WritersWaiting != 0 ? Bias.Writers : Bias.None;
                    }
                    while (!SetData(ref data));
                }
            }
        }

        /// <summary>
        /// Aquire a writer lock. Wait until lock is aquired.
        /// </summary>
        public void LockForWriting()
        { LockForWriting(true); }

        /// <summary>
        /// Aquire a writer lock.
        /// </summary>
        /// <param name="wait">True if to wait until lock aquired, False to return immediately.</param>
        /// <returns>Boolean indicating if lock was successfuly aquired.</returns>
        public bool LockForWriting(bool wait)
        {
            //try shortcut first.
            if (Interlocked.CompareExchange(ref _Bits, 1 << WriterInRegionOffset, 0) == 0)
                return true;

            bool waitingRegistered = false;

            try
            {
                while (true)
                {
                    bool retry = false;
                    Data data;
                    GetData(out data);

                    if (data._Bias != Bias.Readers)
                    {
                        if (data._ReadersInRegion == 0 && !data._WriterInRegion)
                        {
                            if (waitingRegistered)
                            {
                                data._Bias = Bias.Writers;
                                --data._WritersWaiting;
                                data._WriterInRegion = true;
                                if (SetData(ref data))
                                {
                                    waitingRegistered = false;
                                    return true;
                                }
                                else
                                    retry = true;
                            }
                            else if (data._ReadersWaiting == 0)
                            {
                                data._Bias = Bias.Writers;
                                data._WriterInRegion = true;
                                if (SetData(ref data))
                                    return true;
                                else
                                    retry = true;
                            }
                        }

                        //sleep
                    }
                    else
                    {
                        if (!waitingRegistered && data._WritersWaiting < WritersWaitingMask && wait)
                        {
                            ++data._WritersWaiting;
                            if (SetData(ref data))
                            {
                                waitingRegistered = true;
                                //sleep
                            }
                            else
                                retry = true;
                        }

                        //sleep
                    }

                    if (!retry)
                    {
                        if (!wait)
                            return false;

                        System.Threading.Thread.Sleep(0);
                    }
                }
            }
            finally
            {
                if (waitingRegistered)
                {
                    //Thread aborted?
                    Data data;

                    do
                    {
                        GetData(out data);
                        --data._WritersWaiting;

                        if (!data._WriterInRegion && data._WritersWaiting == 0)
                            data._Bias = data._ReadersWaiting != 0 ? Bias.Readers : Bias.None;
                    }
                    while (!SetData(ref data));
                }
            }
        }
    }
}
