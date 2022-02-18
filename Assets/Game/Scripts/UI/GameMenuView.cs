using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Scripts.UI {
    public class GameMenuView : MonoBehaviour {
        [SerializeField] private ContinueLatestGameUI _continueLatestGameUI;
        [SerializeField] private StartNewGameUI _startNewGameUI;

        private CompositeDisposable _continueLatestGameUiDisposable = new CompositeDisposable();
        private CompositeDisposable _startGameUiDisposable = new CompositeDisposable();

        private void Start() {
        }

        public void ShowContinueLatestGameUI(UnityAction onClickContinue, UnityAction<int> onClickStartNewGame) {
            _continueLatestGameUI.gameObject.SetActive(true);
            _startNewGameUI.gameObject.SetActive(false);
            _continueLatestGameUiDisposable?.Dispose();
            _continueLatestGameUiDisposable.AddTo(this);
            var continueBtn = _continueLatestGameUI.OnClickContinueGameBtn.Subscribe(x => {
                onClickContinue?.Invoke();
            }).AddTo(this);
            var startNewGameBtn = _continueLatestGameUI.OnClickStartNewGameBtn.Subscribe(x => {
                ShowStartNewGameUI(onClickStartNewGame);
            }).AddTo(this);
            _continueLatestGameUiDisposable = new CompositeDisposable();
            _continueLatestGameUiDisposable.Add(continueBtn);
            _continueLatestGameUiDisposable.Add(startNewGameBtn);
        }

        public void ShowStartNewGameUI(UnityAction<int> onClickStartNewGame) {
            _startNewGameUI.gameObject.SetActive(true);
            _continueLatestGameUI.gameObject.SetActive(false);
            _startGameUiDisposable?.Dispose();
            _startGameUiDisposable = new CompositeDisposable();
            _startGameUiDisposable.AddTo(this);
            var startGameBtn = _startNewGameUI.OnClickStartNewGame.Subscribe(x => {
                onClickStartNewGame?.Invoke(x);
            }).AddTo(this);
            _startGameUiDisposable.Add(startGameBtn);
        }
    }
}