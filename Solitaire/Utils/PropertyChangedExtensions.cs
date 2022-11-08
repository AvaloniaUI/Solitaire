using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace Solitaire.Utils;

public static class PropertyChangedExtensions
{
    class PropertyObservable<T> : IObservable<T>
    {
        private readonly INotifyPropertyChanged _target;
        private readonly PropertyInfo _info;

        public PropertyObservable(INotifyPropertyChanged target, PropertyInfo info)
        {
            _target = target;
            _info = info;
        }

        class Subscription : IDisposable
        {
            private readonly INotifyPropertyChanged _target;
            private readonly PropertyInfo _info;
            private readonly IObserver<T> _observer;

            public Subscription(INotifyPropertyChanged target, PropertyInfo info, IObserver<T> observer)
            {
                _target = target;
                _info = info;
                _observer = observer;
                _target.PropertyChanged += OnPropertyChanged!;
                _observer.OnNext(((T)_info.GetValue(_target)!));
            }

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == _info.Name)
                    _observer.OnNext(((T)_info.GetValue(_target)!));
            }

            public void Dispose()
            {
                _target.PropertyChanged -= OnPropertyChanged!;
                _observer.OnCompleted();
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return new Subscription(_target, _info, observer);
        }
    }

    public static IObservable<TRes> WhenAnyValue<TModel, TRes>(this TModel model,
        Expression<Func<TModel, TRes>> expr) where TModel : INotifyPropertyChanged
    {
        var l = (LambdaExpression)expr;
        var ma = (MemberExpression)l.Body;
        var prop = (PropertyInfo)ma.Member;
        return new PropertyObservable<TRes>(model, prop);
    }
}