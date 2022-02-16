using Game.Scripts.Core;
using UnityEngine;

namespace Game.Scripts.RubikCube {
    public class Cube : MonoBehaviour {
        // each cube will have 6 faces
        [SerializeField] private Transform[] _cubeFaces;
        [SerializeField] private GameObject _cubePiecePrefab;

        private int _cubeSize;

        private void Start() {
            ConstructCube();
        }

        private void ConstructCube() {
            _cubeSize = MainGameManager.instance.CubePlaySize;
            
        }
    }
}