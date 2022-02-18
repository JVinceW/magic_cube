using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Scripts.Core.SceneManager {
    public abstract class BaseSceneController : MonoBehaviour {
        public abstract UniTask CreateScene(BaseSceneContext sceneContext);
    }
}