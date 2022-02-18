using Cysharp.Threading.Tasks;
using Game.Scripts.Common;
using Game.Scripts.Core.SceneManager;
using Game.Scripts.RubikCube;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Scripts.SceneLogic.GameScene {
    public class GameSceneController : BaseSceneController {
        [SerializeField] private OrbitCamera _orbitCamera;
        private const string CUBE_ASSET_PATH = "Assets/Game/Prefabs/RubikCube_{0}.prefab";
        
        private GameSceneContext _context;
        public override async UniTask CreateScene(BaseSceneContext sceneContext) {
            _context = (GameSceneContext)sceneContext;
            await LoadCube();
        }

        private async UniTask LoadCube() {
            var path = string.Format(CUBE_ASSET_PATH, _context.CubeSize);
            Debug.Log($"Load Cube At Path: {path}");
            var cube = await Addressables.InstantiateAsync(path);
            var comp = cube.GetComponent<CubeController>();
            comp.InitCube();
            _orbitCamera.target = comp.CameraPivotTarget.transform;
        }
    }
}