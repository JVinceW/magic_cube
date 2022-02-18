using Cysharp.Threading.Tasks;
using Game.Scripts.Common;
using Game.Scripts.Core.SceneManager;
using Game.Scripts.SceneLogic.GameScene;
using Game.Scripts.UI;
using UnityEngine;

namespace Game.Scripts.SceneLogic {
    public class TitleSceneController : BaseSceneController {
        [SerializeField] private GameMenuView _menuView;

        public override UniTask CreateScene(BaseSceneContext sceneContext) {
            ShowGameStartUIFlow();
            return UniTask.CompletedTask;
        }

        private void ShowGameStartUIFlow() {
            var hasSaved = PlayerLocalSaveData.instance.LastPlayedRubickSize > 0;
            if (hasSaved) {
                _menuView.ShowContinueLatestGameUI(ContinueLastSessionGame, StartNewGame);
            } else {
                _menuView.ShowStartNewGameUI(StartNewGame);
            }
        }

        private static void ContinueLastSessionGame() {
            var gameSceneContext = new GameSceneContext {
                IsContinueGame = true,
                CubeSize = PlayerLocalSaveData.instance.LastPlayedRubickSize
            };
            GameSceneManager.instance.LoadScene(GameConfig.SceneName.GAME_SCENE_ADDRESS, gameSceneContext).Forget();
        }

        private static void StartNewGame(int cubeSize) {
            var gameSceneContext = new GameSceneContext {
                IsContinueGame = false,
                CubeSize = cubeSize
            };
            GameSceneManager.instance.LoadScene(GameConfig.SceneName.GAME_SCENE_ADDRESS, gameSceneContext).Forget();
        }
    }
}