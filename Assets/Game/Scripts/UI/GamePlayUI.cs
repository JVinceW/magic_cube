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
        public IObservable<Unit> OnClickShowHideTimerBtn => _showHideTimerBtn.OnClickAsObservable();
        public IObservable<Unit> OnClickRestartGameBtn => _restartGameBtn.OnClickAsObservable();

        public IObservable<Unit> OnUndoBtnClick => _undoBtn.OnClickAsObservable();
        public IObservable<Unit> OnMenuBtnClick => _menuBtn.OnClickAsObservable();

        public void ShowPlayTime(float seconds) {
            _timerTxt.SetText($"{seconds} s");
        }

        public void ShowUntouchableImg(bool isShow) {
            _untouchableImg.gameObject.SetActive(isShow);
        }

        private void Start() {
            OnMenuBtnClick.Subscribe(x => {
                var isShow = !_menuPanel.activeSelf;
                ShowHideMenuPanel(isShow);
            }).AddTo(this);

            OnClickShowHideTimerBtn.Subscribe(x => {
                var isShow = !_timerTxt.gameObject.activeSelf;
                ShowHideTimer(isShow);
            }).AddTo(this);
        }

        public void ShowHideTimer(bool isShow) {
            _timerTxt.gameObject.SetActive(isShow);   
        }

        public void ShowHideMenuPanel(bool isShow) {
            _menuPanel.SetActive(isShow);
        }

        public void RestartGame() {
            _timerTxt.SetText("0s");
            ShowUntouchableImg(false);
            ShowHideTimer(true);
            ShowHideMenuPanel(false);
        }
    }
}