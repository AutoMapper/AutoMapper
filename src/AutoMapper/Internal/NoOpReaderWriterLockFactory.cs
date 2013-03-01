namespace AutoMapper.Internal
{
    public class ReaderWriterLockSlimFactory : IReaderWriterLockSlimFactory
    {
        public IReaderWriterLockSlim Create()
        {
            return new NoOpReaderWriterLock();
        }

        public class NoOpReaderWriterLock : IReaderWriterLockSlim
        {
            public void Dispose()
            {
            }

            public void EnterWriteLock()
            {
            }

            public void ExitWriteLock()
            {
            }

            public void EnterUpgradeableReadLock()
            {
            }

            public void ExitUpgradeableReadLock()
            {
            }

            public void EnterReadLock()
            {
            }

            public void ExitReadLock()
            {
            }
        }

    }

}
