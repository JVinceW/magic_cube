using System;
using Cysharp.Threading.Tasks;
using Game.Scripts.Common;
using Game.Scripts.Core.SceneManager;
using Game.Scripts.RubikCube;
using Game.Scripts.SceneLogic.TitleScene;
using Game.Scripts.UI;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Game.Scripts.SceneLogic.GameScene {
    public class GameSceneController : BaseSceneController {
        [SerializeField] private OrbitCamera _orbitCamera;
        [SerializeField] private GamePlayUI _gamePlayUI;
        private const string CUBE_ASSET_PATH = "Assets/Game/Prefabs/RubikCube_{0}.prefab";

        private GameSceneContext _context;
        private bool _isTimerRunning;
        private readonly ReactiveProperty<float> _playTime = new FloatReactiveProperty();
        private CubeController _playingCube;
        private IDisposable _cubeCheck;

        public override async UniTask CreateScene(BaseSceneContext sceneContext) {
            _context = (GameSceneContext)sceneContext;
            this.UpdateAsObservable().Subscribe(x => { CountPlayTime(); }).AddTo(this);
            await LoadCube();
            SetUpCubeCheck();
            SetUpUi();
        }

        private void SetUpCubeCheck() {
            _cubeCheck?.Dispose();
            _cubeCheck = _playingCube.GameFinished.Subscribe(isFinished => {
                if (isFinished) {
                    _gamePlayUI.ShowUntouchableImg(true);
                    GameScenePlayInfo.instance.CanManipulateCamera = false;
                    StopTimer();
                    PlayWinGameEffect().Forget();
                }
            }).AddTo(this);
        }

        private void SetUpUi() {
            _gamePlayUI.OnClickRestartGameBtn.Subscribe(x => { OnClickRestartGame(); }).AddTo(this);
            _gamePlayUI.OnClickBackToTitleSceneBtn.Subscribe(x => { OnClickBackToTitleScene(); }).AddTo(this);
            _gamePlayUI.OnUndoBtnClick.Subscribe(x => { OnClickUndoBtn(); }).AddTo(this);
            _playTime.Subscribe(x => { _gamePlayUI.ShowPlayTime(x); }).AddTo(this);
        }

        private async UniTask LoadCube() {
            var path = string.Format(CUBE_ASSET_PATH, _context.CubeSize);
            Debug.Log($"Load Cube At Path: {path}");
            var cube = await Addressables.InstantiateAsync(path);
            _playingCube = cube.GetComponent<CubeController>();
            await _playingCube.InitCube();
            StartTimer();
            _orbitCamera.target = _playingCube.CameraPivotTarget.transform;
        }

        private async UniTask PlayWinGameEffect() { }

        private void OnClickUndoBtn() {
            _playingCube.UndoMove();
        }

        private void OnClickRestartGame() {
            PlayerLocalSaveData.Reset();
            Addressables.Release(_playingCube.gameObject);
            UniTask.Create(async () => {
                await LoadCube();
                SetUpCubeCheck();
            }).AttachExternalCancellation(this.GetCancellationTokenOnDestroy());

            _gamePlayUI.RestartGame();
            StopTimer();
            ResetTimer();
        }

        private void OnClickBackToTitleScene() {
            var titleSceneContext = new TitleSceneContext();
            GameSceneManager.instance.LoadScene(GameConfig.SceneName.TITLE_SCENE_ADDRESS, titleSceneContext).Forget();
        }

        private void CountPlayTime() {
            if (!_isTimerRunning) {
                return;
            }

            _playTime.Value += Time.deltaTime;
            Debug.Log($"Time: {_playTime.Value}");
        }

        private void StartTimer() {
            _isTimerRunning = true;
        }

        private void StopTimer() {
            _isTimerRunning = false;
        }

        private void ResetTimer() {
            _playTime.Value = 0;
            _isTimerRunning = false;
        }
    }
}