using System.ComponentModel;
using Castle.DynamicProxy;

namespace AutoMapper.Internal
{
    internal class NotifyPropertyBehaviorInterceptor : PropertyBehaviorInterceptor
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public override void Intercept(IInvocation info)
        {
            if (info.Method.Name == "add_PropertyChanged")
            {
                PropertyChanged += (PropertyChangedEventHandler)info.Arguments[0];
                return;
            }
            if (info.Method.Name == "remove_PropertyChanged")
            {
                PropertyChanged -= (PropertyChangedEventHandler)info.Arguments[0];
                return;
            }

            base.Intercept(info);

            if (IsSetterCall(info))
            {
                OnPropertyChanged(info.Proxy, new PropertyChangedEventArgs(GetPropertyName(info)));
            }
        }

        protected virtual void OnPropertyChanged(object target, PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(target, e);
        }
    }
}