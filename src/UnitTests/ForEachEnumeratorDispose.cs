namespace AutoMapper.UnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Shouldly;
    using Xunit;

    public class ForEachEnumeratorDispose : SpecBase
    {
        public IMapper Mapper => new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<SourceDest, SourceDest>()));

        [Fact]
        public void ForEach_for_unknown_nondisposable_class_should_not_call_Dispose()
        {
            Collection.IsDisposable = false;
            Collection.ShouldThrowException = false;
            Collection.UseStruct = false;
            SourceDest source = new SourceDest { Numbers = new Collection() };
            Mapper.Map<SourceDest>(source).ShouldNotBeNull();
        }

        [Fact]
        public void ForEach_for_unknown_disposable_class_should_call_Dispose()
        {
            Collection.IsDisposable = true;
            Collection.ShouldThrowException = false;
            Collection.UseStruct = false;
            SourceDest source = new SourceDest { Numbers = new Collection() };
            Mapper.Map<SourceDest>(source).ShouldNotBeNull();
            source.Numbers.IsDisposed.ShouldBe(true);
        }

        [Fact]
        public void ForEach_for_unknown_disposable_class_should_call_Dispose_despite_of_exception()
        {
            Collection.IsDisposable = true;
            Collection.ShouldThrowException = true;
            Collection.UseStruct = false;
            SourceDest source = new SourceDest { Numbers = new Collection() };
            Should.Throw<Exception>(() => Mapper.Map<SourceDest>(source).ShouldNotBeNull());
            source.Numbers.IsDisposed.ShouldBe(true);
        }

        [Fact]
        public void ForEach_for_unknown_nondisposable_struct_should_not_call_Dispose()
        {
            Collection.IsDisposable = false;
            Collection.ShouldThrowException = false;
            Collection.UseStruct = true;
            SourceDest source = new SourceDest { Numbers = new Collection() };
            Mapper.Map<SourceDest>(source).ShouldNotBeNull();
        }

        [Fact]
        public void ForEach_for_unknown_disposable_struct_should_call_Dispose()
        {
            Collection.IsDisposable = true;
            Collection.ShouldThrowException = false;
            Collection.UseStruct = true;
            SourceDest source = new SourceDest { Numbers = new Collection() };
            Mapper.Map<SourceDest>(source).ShouldNotBeNull();
            source.Numbers.IsDisposed.ShouldBe(true);
        }

        [Fact]
        public void ForEach_for_unknown_disposable_struct_should_call_Dispose_despite_of_exception()
        {
            Collection.IsDisposable = true;
            Collection.ShouldThrowException = true;
            Collection.UseStruct = true;
            SourceDest source = new SourceDest { Numbers = new Collection() };
            Should.Throw<Exception>(() => Mapper.Map<SourceDest>(source).ShouldNotBeNull());
            source.Numbers.IsDisposed.ShouldBe(true);
        }

        [Fact]
        public void ForEach_for_known_disposable_class_should_call_Dispose()
        {
            GenericCollection.UseStruct = false;
            GenericCollection.ShouldThrowException = false;
            SourceDest source = new SourceDest { GenericNumbers = new GenericCollection() };
            Mapper.Map<SourceDest>(source).ShouldNotBeNull();
            source.GenericNumbers.IsDisposed.ShouldBe(true);
        }

        [Fact]
        public void ForEach_for_known_disposable_class_should_call_Dispose_despite_of_exception()
        {
            GenericCollection.UseStruct = false;
            GenericCollection.ShouldThrowException = true;
            SourceDest source = new SourceDest { GenericNumbers = new GenericCollection() };
            Should.Throw<Exception>(() => Mapper.Map<SourceDest>(source).ShouldNotBeNull());
            source.GenericNumbers.IsDisposed.ShouldBe(true);
        }

        [Fact]
        public void ForEach_for_known_disposable_struct_should_call_Dispose()
        {
            GenericCollection.UseStruct = true;
            GenericCollection.ShouldThrowException = false;
            SourceDest source = new SourceDest { GenericNumbers = new GenericCollection() };
            Mapper.Map<SourceDest>(source).ShouldNotBeNull();
            source.GenericNumbers.IsDisposed.ShouldBe(true);
        }

        [Fact]
        public void ForEach_for_known_disposable_struct_should_call_Dispose_despite_of_exception()
        {
            GenericCollection.UseStruct = true;
            GenericCollection.ShouldThrowException = true;
            SourceDest source = new SourceDest { GenericNumbers = new GenericCollection() };
            Should.Throw<Exception>(() => Mapper.Map<SourceDest>(source).ShouldNotBeNull());
            source.GenericNumbers.IsDisposed.ShouldBe(true);
        }
    }

    public class SourceDest
    {
        public Collection Numbers { get; set; }

        public GenericCollection GenericNumbers { get; set; }
    }

    public class Collection : IList
    {
        public static bool IsDisposable { get; set; }

        public static bool UseStruct { get; set; }

        public static bool ShouldThrowException { get; set; }

        public bool IsDisposed { get; private set; }

        private class NonDisposableEnumerator : IEnumerator
        {
            public bool MoveNext() => false;

            public void Reset() { }

            object IEnumerator.Current => null;
        }

        private struct NonDisposableEnumeratorStruct : IEnumerator
        {
            public bool MoveNext() => false;

            public void Reset() { }

            object IEnumerator.Current => null;
        }

        private class DisposableEnumerator : IEnumerator, IDisposable
        {
            private readonly Collection _collection;

            public DisposableEnumerator(Collection collection)
            {
                _collection = collection;
            }

            public bool MoveNext()
            {
                if (ShouldThrowException)
                {
                    throw new InvalidOperationException();
                }
                return false;
            }

            public void Reset() { }

            object IEnumerator.Current => null;

            public void Dispose()
            {
                _collection.IsDisposed = true;
            }
        }

        private struct DisposableEnumeratorStruct : IEnumerator, IDisposable
        {
            private readonly Collection _collection;

            public DisposableEnumeratorStruct(Collection collection)
            {
                _collection = collection;
            }

            public bool MoveNext()
            {
                if (ShouldThrowException)
                {
                    throw new InvalidOperationException();
                }
                return false;
            }

            public void Reset() { }

            object IEnumerator.Current => null;

            public void Dispose()
            {
                _collection.IsDisposed = true;
            }
        }

        public IEnumerator GetEnumerator()
        {
            if (IsDisposable)
            {
                if (UseStruct)
                {
                    return new DisposableEnumeratorStruct(this);
                }
                return new DisposableEnumerator(this);
            }
            if (UseStruct)
            {
                return new NonDisposableEnumeratorStruct();
            }
            return new NonDisposableEnumerator();
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

    public class GenericCollection : IList<int>
    {
        public static bool UseStruct { get; set; }

        public static bool ShouldThrowException { get; set; }

        public bool IsDisposed { get; private set; }

        private class GenericDisposableEnumerator : IEnumerator<int>
        {
            private readonly GenericCollection _collection;

            public GenericDisposableEnumerator(GenericCollection collection)
            {
                _collection = collection;
            }

            public void Dispose()
            {
                _collection.IsDisposed = true;
            }

            public bool MoveNext()
            {
                if (ShouldThrowException)
                {
                    throw new InvalidOperationException();
                }
                return false;
            }

            public void Reset() { }

            object IEnumerator.Current => Current;

            public int Current => 0;
        }

        private struct GenericDisposableEnumeratorStruct : IEnumerator<int>
        {
            private readonly GenericCollection _collection;

            public GenericDisposableEnumeratorStruct(GenericCollection collection)
            {
                _collection = collection;
            }

            public void Dispose()
            {
                _collection.IsDisposed = true;
            }

            public bool MoveNext()
            {
                if (ShouldThrowException)
                {
                    throw new InvalidOperationException();
                }
                return false;
            }

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
            if (UseStruct)
            {
                return new GenericDisposableEnumeratorStruct(this);
            }
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