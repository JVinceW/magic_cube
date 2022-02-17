using System;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Scripts.RubikCube {
    public class CubePiece : MonoBehaviour {
        [SerializeField] private Vector3Int _originalCoord;
        private CubePieceStateData _stateData = new CubePieceStateData();
        private Vector3 _localCoordOnCube;
        
        public CubePieceStateData StateData => _stateData;

        public void SetStateData() {
            var t = transform;
            _stateData.coordinatorX = _originalCoord.x;
            _stateData.coordinatorY = _originalCoord.y;
            _stateData.coordinatorZ = _originalCoord.z;

            var pos = t.position;
            _stateData.positionX = pos.x;
            _stateData.positionY = pos.y;
            _stateData.positionZ = pos.z;

            var rot = t.rotation.eulerAngles;
            _stateData.rotX = rot.x;
            _stateData.rotY = rot.y;
            _stateData.rotZ = rot.z;
            
            // _stateData.coordinator = _originalCoord;
            // _stateData.position = t.position;
            // _stateData.rotation = t.rotation;
        }

#if UNITY_EDITOR
        [Button("Auto Setup coord")]
        private void AutoSetUpCoordOnEditor() {
            var pos = transform.localPosition;
            _originalCoord = new Vector3Int(Convert.ToInt32(pos.x), Convert.ToInt32(pos.y), Convert.ToInt32(pos.z));
        }
#endif
    }
}