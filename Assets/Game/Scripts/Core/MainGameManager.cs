using Cysharp.Threading.Tasks;
using Game.Scripts.Common;
using Game.Scripts.Common.Singleton;
using Game.Scripts.Core.SceneManager;
using Game.Scripts.SceneLogic.TitleScene;

namespace Game.Scripts.Core {
    public class MainGameManager : SingletonMonoBehaviour<MainGameManager> {
        private GameState _gameState;

        private void Start() {
            InitGame().Forget();
        }

        private static async UniTask InitGame() {
            var titleSceneContext = new TitleSceneContext();
            await GameSceneManager.instance.LoadScene(GameConfig.SceneName.TITLE_SCENE_ADDRESS, titleSceneContext);
        }
    }
}