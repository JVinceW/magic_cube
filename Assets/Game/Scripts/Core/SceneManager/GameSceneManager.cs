using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Scripts.Common.Singleton;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.Core.SceneManager {
    public class GameSceneManager : SingletonMonoBehaviour<GameSceneManager> {
        public async UniTask LoadScene(string sceneAddress, BaseSceneContext sceneContext) {
            var sceneInstance = await Addressables.LoadSceneAsync(sceneAddress);
            var rootGos = sceneInstance.Scene.GetRootGameObjects();
            var controller = rootGos.First(x => x.GetComponent<BaseSceneController>())
                .GetComponent<BaseSceneController>();
            await controller.CreateScene(sceneContext);
        }
        
    }
}