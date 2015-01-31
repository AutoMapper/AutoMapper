using System.ComponentModel;

namespace AutoMapper.Impl {
    public abstract class ProxyBase {
		protected void NotifyPropertyChanged(PropertyChangedEventHandler handler, string method)
		{
		    handler?.Invoke(this, new PropertyChangedEventArgs(method));
		}
    }
}
