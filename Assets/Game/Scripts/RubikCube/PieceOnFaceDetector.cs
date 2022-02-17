using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scripts.RubikCube {
    [RequireComponent(typeof(BoxCollider))]
    public class PieceOnFaceDetector : MonoBehaviour {
        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private string _rotationAxis;
        [SerializeField] private LayerMask _checkLayerMask;

        public string RotationAxis => _rotationAxis;

        /// <summary>
        /// Piece per face, max size of playable cube is 6x6x6 so the max piece will be 6x6 = 36
        /// </summary>
        [Range(4, 36)]
        [SerializeField]
        private int _maxCollision = 9;

        public List<GameObject> DetectPieceOnFace() {
            var colObj = new Collider[9];
            Physics.OverlapBoxNonAlloc(transform.position, _boxCollider.size / 2f, colObj, Quaternion.identity, _checkLayerMask);
            return colObj.Where(x => x != null).Select(x => x.gameObject).ToList();
        }

        private void Reset() {
            _boxCollider = GetComponent<BoxCollider>();
        }
    }
}