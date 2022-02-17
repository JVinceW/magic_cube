using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.RubikCube.Const;
using UnityEngine;

namespace Game.Scripts.RubikCube {
    public class RotateCubeCommand : MonoBehaviour, IDisposable {
        [SerializeField] private Transform _originalParent;
        [SerializeField] private float _rotateSpeed = .5f;

        private readonly Stack<CommandData> _commandLst = new Stack<CommandData>();

        public class CommandData {
            public PieceOnFaceDetector Detector;
            public FaceRotationType RotationDirection;
        }

        // public PieceOnFaceDetector[] GetAllPieceOnFaceDetector => _pieceOnFaceDetectors;
        private bool _isRotating;

        public async UniTask ExecuteRotate(PieceOnFaceDetector detector, FaceRotationType rotationDirection,
            CancellationToken ct = default) {
            if (_isRotating) {
                return;
            }

            if (detector == null) {
                _isRotating = false;
                return;
            }

            _isRotating = true;
            var gos = detector.DetectPieceOnFace();
            Debug.Log($"Detector name: {detector.gameObject.name}");
            GatherPieceIntoRotationFace(detector.transform, gos);
            var rotDeg = rotationDirection == FaceRotationType.CLOCKWISE ? 90 : -90;
            var targetRot = GetTargetRotation(detector, rotDeg);
            await detector.transform.DORotateQuaternion(targetRot, _rotateSpeed).ToUniTask(cancellationToken: ct);
            ReturnGameObjectToCube(gos);
            _isRotating = false;
        }

        public async UniTask<bool> Undo(CancellationToken ct = default) {
            if (_commandLst.Count <= 0) {
                Debug.Log("No more Step to undo");
                return false;
            }
            var commandData = _commandLst.Pop();
            await ExecuteRotate(commandData.Detector, commandData.RotationDirection, ct);
            return true;
        }

        public void AddCommand(PieceOnFaceDetector detector, FaceRotationType rotationDirection) {
            var commandData = new CommandData {
                Detector = detector,
                RotationDirection = rotationDirection == FaceRotationType.CLOCKWISE
                    ? FaceRotationType.COUNTER_CLOCKWISE
                    : FaceRotationType.CLOCKWISE
            };
            _commandLst.Push(commandData);
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

        public void Dispose() {
            _commandLst.Clear();
        }
    }
}