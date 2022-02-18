using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI {
    public class ContinueLatestGameUI : MonoBehaviour {
        [SerializeField] private Button _continueGameBtn;
        [SerializeField] private Button _startNewGameBtn;

        public IObservable<Unit> OnClickContinueGameBtn => _continueGameBtn.OnClickAsObservable();
        public IObservable<Unit> OnClickStartNewGameBtn => _startNewGameBtn.OnClickAsObservable();
    }
}