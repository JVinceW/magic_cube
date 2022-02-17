using System;
using NaughtyAttributes;
using UnityEngine;

namespace Game.Scripts.RubikCube {
    public class CubePiece : MonoBehaviour {
        [SerializeField] private Vector3Int _originalCoord;
        private Vector3 _localCoordOnCube;

        /// <summary>
        /// Face of cube min = 1, max = 3
        /// </summary>
        private int _faces;

#if UNITY_EDITOR
        [Button("Auto Setup coord")]
        private void AutoSetUpCoordOnEditor() {
            var pos = transform.localPosition;
            _originalCoord = new Vector3Int(Convert.ToInt32(pos.x), Convert.ToInt32(pos.y), Convert.ToInt32(pos.z));
        }
#endif
    }
}