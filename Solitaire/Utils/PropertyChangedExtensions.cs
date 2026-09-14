using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Solitaire.Utils;

public static class PropertyChangedExtensions
{
    public static IObservable<TValue> ObserveProperty<TModel, TValue>(this TModel model,
        string propertyName, Func<TModel, TValue> read) where TModel : INotifyPropertyChanged =>
        Observable.Create<TValue>(observer =>
        {
            void Changed(object? sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName == propertyName)
                    observer.OnNext(read(model));
            }
            model.PropertyChanged += Changed;
            observer.OnNext(read(model));
            return Disposable.Create(() =>
            {
                model.PropertyChanged -= Changed;
                observer.OnCompleted();
            });
        });
}
