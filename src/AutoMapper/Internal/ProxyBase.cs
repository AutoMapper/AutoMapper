using System.ComponentModel;

namespace AutoMapper.Internal {
    public abstract class ProxyBase {
		protected void NotifyPropertyChanged(PropertyChangedEventHandler handler, string method) {
			if (handler != null) {
				handler(this, new PropertyChangedEventArgs(method));
			}
		}
	}
}