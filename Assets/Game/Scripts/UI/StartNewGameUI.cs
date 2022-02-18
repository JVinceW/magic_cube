using System;
using Game.Scripts.Common;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI {
    public class StartNewGameUI : MonoBehaviour{
        [SerializeField] private Button _startGameBtn;
        [SerializeField] private TMP_Dropdown _cubeSizeSelector;
        private int _selectedSize;

        public IObservable<int> OnClickStartNewGame => _startGameBtn.OnClickAsObservable().Select(x => _selectedSize);

        private void Start() {
            _cubeSizeSelector.OnValueChangedAsObservable().Subscribe(x => {
                var option = _cubeSizeSelector.options[x];
                _selectedSize = Convert.ToInt32(option.text);
            }).AddTo(this);
        }
    }
}