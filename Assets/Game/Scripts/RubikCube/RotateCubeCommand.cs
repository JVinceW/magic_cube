using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.RubikCube.Const;
using UnityEngine;

namespace Game.Scripts.RubikCube {
    public class RotateCubeCommand : MonoBehaviour {
        [SerializeField] private Transform _originalParent;
        [SerializeField] private PieceOnFaceDetector[] _pieceOnFaceDetectors;
        [SerializeField] private float _rotateSpeed = .5f; 
        private bool _isRotating;

        public async UniTask ExecuteRotate(CubeFaceType faceType, FaceRotationType rotationDirection, CancellationToken ct = default) {
            if (_isRotating) {
                return;
            }

            var detector = _pieceOnFaceDetectors.FirstOrDefault(x => x.CubeFaceType == faceType);
            if (detector == null) {
                Debug.LogError($"Can not detect face detector: {faceType}");
                _isRotating = false;
                return;
            }

            _isRotating = true;
            var gos = detector.DetectPieceOnFace();
            Debug.Log($"Detector name: {detector.gameObject.name}");
            GatherPieceIntoRotationFace(detector.transform, gos);
            var rotDeg = rotationDirection == FaceRotationType.CLOCKWISE ? 90 : -90;
            var targetRot = GetTargetRotation(detector, rotDeg);
            await detector.transform.DORotateQuaternion(targetRot, _rotateSpeed).ToUniTask(cancellationToken:ct);
            ReturnGameObjectToCube(gos);
            _isRotating = false;
        }
        

        private static Quaternion GetTargetRotation(PieceOnFaceDetector detector, float rotateDeg) {
            var localRot = detector.transform.localRotation;
            var axis = detector.RotationAxis;
            
            var rotateDegQuaternion = Quaternion.identity;
            switch (axis.ToLower()) {
                case "x":
                    rotateDegQuaternion = Quaternion.AngleAxis(rotateDeg, detector.transform.right);
                    break;
                case "y":
                    rotateDegQuaternion = Quaternion.AngleAxis(rotateDeg, detector.transform.up);
                    break;
                case "z":
                    rotateDegQuaternion = Quaternion.AngleAxis(rotateDeg, detector.transform.forward);
                    break;
            }

            var targetRot = localRot * rotateDegQuaternion;
            Debug.Log($"Rotate From: {localRot.eulerAngles} - to : {targetRot.eulerAngles}");
            return targetRot;
        }

        private void GatherPieceIntoRotationFace(Transform faceTransform, List<GameObject> faceGos) {
            foreach (var go in faceGos) {
                go.transform.SetParent(faceTransform);
            }
        }

        private void ReturnGameObjectToCube(List<GameObject> faceGos) {
            foreach (var go in faceGos) {
                go.transform.SetParent(_originalParent);
            }
        }

#if UNITY_EDITOR
        private void Reset() {
            _pieceOnFaceDetectors = GetComponentsInChildren<PieceOnFaceDetector>();
        }
#endif
    }
}