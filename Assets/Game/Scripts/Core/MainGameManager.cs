using Game.Scripts.Common.Singleton;
using UnityEngine;

namespace Game.Scripts.Core {
    public class MainGameManager : SingletonMonoBehaviour<MainGameManager> {
        private int _cubePlaySize;
        private bool _canZoomCamera = true;
        public int CubePlaySize => Mathf.Clamp(_cubePlaySize, 2, 6);
        public bool CanZoomCamera => _canZoomCamera;
    }
}