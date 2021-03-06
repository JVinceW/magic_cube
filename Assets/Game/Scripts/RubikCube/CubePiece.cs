using System;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Scripts.RubikCube {
    public class CubePiece : MonoBehaviour {
        [SerializeField] private Vector3 _originalCoord;
        private readonly CubePieceStateData _stateData = new CubePieceStateData();
        private Vector3 _originalPos;
        
        public bool IsAtRightPosition() {
            return transform.position == _originalPos;
        }

        private void Start() {
            _originalPos = transform.position;
        }

        public CubePieceStateData StateData {
            get {
                SetStateData();
                return _stateData;
            }
        }

        public Vector3 OriginalCoord => _originalCoord;

        public void UpdatePieceWithStateData(CubePieceStateData data) {
            var pos = new Vector3(data.positionX, data.positionY, data.positionZ);
            var rot = Quaternion.Euler(data.rotX, data.rotY, data.rotZ);
            var t = transform;
            t.position = pos;
            t.rotation = rot;
        }

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
        }

#if UNITY_EDITOR
        [Button("Auto Setup coord")]
        private void AutoSetUpCoordOnEditor() {
            var pos = transform.localPosition;
            _originalCoord = new Vector3(Convert.ToInt32(pos.x), Convert.ToInt32(pos.y), Convert.ToInt32(pos.z));
        }
#endif
    }
}