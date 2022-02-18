using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.UI {
    public class GamePlayUI : MonoBehaviour {
        [SerializeField] private Button _undoBtn;
        [SerializeField] private Button _menuBtn;
        [SerializeField] private TMP_Text _timerTxt;
        [SerializeField] private Image _untouchableImg;

        [Header("Menu View UI")]
        [SerializeField]
        private GameObject _menuPanel;
        [SerializeField] private Button _backToTitleSceneBtn;
        [SerializeField] private Button _showHideTimerBtn;
        [SerializeField] private Button _restartGameBtn;

        public IObservable<Unit> OnClickBackToTitleSceneBtn => _backToTitleSceneBtn.OnClickAsObservable();
        private IObservable<Unit> OnClickShowHideTimerBtn => _showHideTimerBtn.OnClickAsObservable();
        public IObservable<Unit> OnClickRestartGameBtn => _restartGameBtn.OnClickAsObservable();

        public IObservable<Unit> OnUndoBtnClick => _undoBtn.OnClickAsObservable();
        private IObservable<Unit> OnMenuBtnClick => _menuBtn.OnClickAsObservable();

        public void ShowPlayTime(long seconds) {
            _timerTxt.SetText($"{seconds} s");
        }

        public void ShowUntouchableImg(bool isShow) {
            _untouchableImg.gameObject.SetActive(isShow);
        }

        private void Start() {
            OnMenuBtnClick.Subscribe(x => {
                var isShow = !_menuPanel.activeSelf;
                _menuPanel.SetActive(isShow);
            }).AddTo(this);

            OnClickShowHideTimerBtn.Subscribe(x => {
                var isShow = !_timerTxt.gameObject.activeSelf;
                _timerTxt.gameObject.SetActive(isShow);
            }).AddTo(this);
        }

        public void RestartGame() {
            _timerTxt.SetText("0s");
            _timerTxt.gameObject.SetActive(true);
            ShowUntouchableImg(false);
        }
    }
}