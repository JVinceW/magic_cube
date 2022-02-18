using Game.Scripts.Common.Singleton;
using UnityEngine;

namespace Game.Scripts.SceneLogic.GameScene {
    /// <summary>
    /// Cube and some state info will be use across game object inside game scene
    /// We use in scene singleton that mean when scene destroyed, this info will automatic cleared too
    /// </summary>
    public class GameScenePlayInfo : InSceneSingletonMonoBehaviour<GameScenePlayInfo> {
        private int _cubePlaySize;
        public int CubePlaySize => Mathf.Clamp(_cubePlaySize, 2, 6);

        public bool CanManipulateCamera { get; set; } = true;
    }
}