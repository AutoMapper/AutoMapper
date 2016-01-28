namespace AutoMapper.Internal
{
    using System.ComponentModel;

    public abstract class ProxyBase
    {
        public ProxyBase()
        {
            
        }

        protected void NotifyPropertyChanged(PropertyChangedEventHandler handler, string method)
        {
            handler?.Invoke(this, new PropertyChangedEventArgs(method));
        }
    }
}