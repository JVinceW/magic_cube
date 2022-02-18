using System;
using TMPro;
using UniRx;

namespace Game.Scripts.Common {
    public static class TMPUniRxExtension {
        public static IObservable<int> OnValueChangedAsObservable(this TMP_Dropdown dropdown)
        {
            return Observable.CreateWithState<int, TMP_Dropdown>(dropdown, (d, observer) =>
            {
                observer.OnNext(d.value);
                return d.onValueChanged.AsObservable().Subscribe(observer);
            });
        }
        
    }
}