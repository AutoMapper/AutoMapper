namespace AutoMapper.UnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Shouldly;
    using Xunit;

    public class ForEachEnumeratorDispose : NonValidatingSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg => cfg.CreateMap<SourceDest, SourceDest>());

        [Fact]
        public void ForEach_for_nondisposable_should_not_call_Dispose()
        {
            Configuration.CompileMappings();

            SourceDest source = new SourceDest
            {
                NondisposableNumbers = new NondisposableCollection()
            };
            Mapper.Map<SourceDest>(source).ShouldNotBeNull();
        }

        [Fact]
        public void ForEach_for_disposable_runtimetype_should_call_Dispose()
        {
            Configuration.CompileMappings();

            SourceDest source = new SourceDest
            {
                DisposableNumbers = new DisposableCollection()
            };
            Mapper.Map<SourceDest>(source).ShouldNotBeNull();

            source.DisposableNumbers.IsDisposed.ShouldBe(true);
        }

        [Fact]
        public void ForEach_for_disposable_type_should_call_Dispose_directly()
        {
            Configuration.CompileMappings();

            SourceDest source = new SourceDest
            {
                GenericDisposableNumbers = new GenericDisposableCollection()
            };
            Mapper.Map<SourceDest>(source).ShouldNotBeNull();

            source.GenericDisposableNumbers.IsDisposed.ShouldBe(true);
        }
    }

    public class SourceDest
    {
        public NondisposableCollection NondisposableNumbers { get; set; }

        public DisposableCollection DisposableNumbers { get; set; }

        public GenericDisposableCollection GenericDisposableNumbers { get; set; }
    }

    public class NondisposableCollection : IList
    {
        private class NondisposableEnumerator : IEnumerator
        {
            public bool MoveNext() => false;

            public void Reset() { }

            object IEnumerator.Current => null;
        }

        public IEnumerator GetEnumerator()
        {
            return new NondisposableEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public object this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class DisposableCollection : IList
    {
        public bool IsDisposed { get; private set; }

        private class DisposableEnumerator : IEnumerator, IDisposable
        {
            private readonly DisposableCollection _collection;

            public DisposableEnumerator(DisposableCollection collection)
            {
                _collection = collection;
            }

            public bool MoveNext() => false;

            public void Reset() { }

            object IEnumerator.Current => null;

            public void Dispose()
            {
                _collection.IsDisposed = true;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new DisposableEnumerator(this);
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public object this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class GenericDisposableCollection : IList<int>
    {
        public bool IsDisposed { get; private set; }

        private class GenericDisposableEnumerator : IEnumerator<int>
        {
            private readonly GenericDisposableCollection _collection;

            public GenericDisposableEnumerator(GenericDisposableCollection collection)
            {
                _collection = collection;
            }

            public void Dispose()
            {
                _collection.IsDisposed = true;
            }

            public bool MoveNext() => false;

            public void Reset() { }

            object IEnumerator.Current => Current;

            public int Current => 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<int> GetEnumerator()
        {
            return new GenericDisposableEnumerator(this);
        }

        public void Add(int item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
        }

        public bool Contains(int item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(int[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(int item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public int IndexOf(int item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, int item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public int this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}