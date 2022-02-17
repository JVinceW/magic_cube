using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Scripts.Common;
using Game.Scripts.Core;
using Game.Scripts.RubikCube.Const;
using NaughtyAttributes;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts.RubikCube {
    public class CubeController : MonoBehaviour {
        [SerializeField] private PieceOnFaceDetector[] _pieceOnFaceDetectors;
        [SerializeField] private RotateCubeCommand _command;
        [SerializeField] private LayerMask _cubeCastLayerMask;
        [SerializeField] private int _shuffleMinStep = 10;
        [SerializeField] private int _shuffleMaxStep = 20;
        [SerializeField] private int _cubeSize;

        private bool _isRayCastedOnCube;
        [SerializeField] private bool _finishedInit;
        private Vector3 _onClickedMousePosition;
        private Transform _rayCastHitCubePiece;
        private bool _isRotating;
        private const float ROTATE_DIRECTION_DETECTION_THRESHOLD = 10;
        private List<CubePiece> _cubePieces = new List<CubePiece>();

        private void Start() {
            this.UpdateAsObservable()
                .Where(x => Input.GetMouseButtonDown(0))
                .Subscribe(x => {
                    if (!_finishedInit) {
                        return;
                    }

                    CheckClickOnCubePiece();
                }).AddTo(this);
            this.UpdateAsObservable()
                .Where(x => Input.GetMouseButtonUp(0))
                .Subscribe(x => {
                    _isRayCastedOnCube = false;
                    _onClickedMousePosition = Vector3.zero;
                    _rayCastHitCubePiece = null;
                    MainGameManager.instance.CanManipulateCamera = true;
                }).AddTo(this);
            this.UpdateAsObservable()
                .Where(x => _isRayCastedOnCube)
                .Subscribe(x => ManipulateCube()).AddTo(this);
            _cubePieces = GetComponentsInChildren<CubePiece>().ToList();
            ConstructCubeData().Forget();
        }

        private async UniTask ConstructCubeData() {
            if (PlayerLocalSaveData.instance.LastPlayedRubickSize == 0) {
                PlayerLocalSaveData.instance.LastPlayedRubickSize = _cubeSize;
                await ShuffleCube();
                PlayerLocalSaveData.instance.LastPlayedCubePieceStateDatas = _cubePieces.Select(x => x.StateData).ToList();
                PlayerLocalSaveData.Save();
            } else {
                ConstructCubeStateFromSaveData();
            }
        }

        private void ConstructCubeStateFromSaveData() {
            Debug.Log("Start construct rubick cube");
            var playerData = PlayerLocalSaveData.instance.LastPlayedCubePieceStateDatas;
            var cubeLst = new List<CubePiece>(_cubePieces);
            foreach (var pieceState in playerData) {
                var pieceCoord = new Vector3Int(pieceState.coordinatorX, pieceState.coordinatorY,
                    pieceState.coordinatorZ);
                var p = cubeLst.FirstOrDefault(x => x.OriginalCoord == pieceCoord);
                if (p != null) {
                    Debug.Log($"Setup piece: {pieceState}");
                    p.UpdatePieceWithStateData(pieceState);
                    cubeLst.Remove(p);
                }
            }

            _finishedInit = true;
        }

        [Button("Shuffle")]
        private void Shuffle() {
            ShuffleCube().Forget();
        }

        [Button("Reset Save Data")]
        private void ResetSaveData() {
            PlayerLocalSaveData.Reset();
        }

        private async UniTask ShuffleCube() {
            var faceList = _pieceOnFaceDetectors;
            var rotateDirection = FaceRotationType.CLOCKWISE;
            var shuffleStep = Random.Range(_shuffleMinStep, _shuffleMaxStep);
            Debug.Log($"Shuffle Step: {shuffleStep}");
            for (var i = 0; i < shuffleStep; i++) {
                var randomShuffleFace = Random.Range(0, faceList.Length - 1);
                var face = faceList[randomShuffleFace];
                await _command.ExecuteRotate(face, rotateDirection, this.GetCancellationTokenOnDestroy());
            }
            _finishedInit = true;
        }

        private void ManipulateCube() {
            if (_isRotating) {
                return;
            }

            var nowPos = Input.mousePosition;
            var direction = nowPos - _onClickedMousePosition;

            // check if rotate horizontal or rotate vertically
            var xDir = direction.x;
            var yDir = direction.y;

            if (Mathf.Abs(xDir - yDir) < ROTATE_DIRECTION_DETECTION_THRESHOLD) {
                return;
            }

            // x > y: rotate horizontally
            // x < y: rotate vertically
            var faceDetectors = _pieceOnFaceDetectors
                .Where(x => x.DetectPieceOnFace().Contains(_rayCastHitCubePiece.gameObject))
                .ToList();
            PieceOnFaceDetector selectedDetector;
            FaceRotationType rotationType;
            // Rotate vertically relate to the camera position
            if (Mathf.Abs(xDir) > Mathf.Abs(yDir)) {
                selectedDetector = faceDetectors.FirstOrDefault(x => x.RotationAxis == "y");
                rotationType = xDir > 0 ? FaceRotationType.COUNTER_CLOCKWISE : FaceRotationType.CLOCKWISE;
            } else {
                selectedDetector = faceDetectors.FirstOrDefault(x => x.RotationAxis == "x");
                rotationType = yDir > 0 ? FaceRotationType.CLOCKWISE : FaceRotationType.COUNTER_CLOCKWISE;
            }
            if (selectedDetector != null) {
                _isRotating = true;
                UniTask.Create(async () => {
                    await _command.ExecuteRotate(selectedDetector, rotationType, this.GetCancellationTokenOnDestroy());
                    _isRotating = false;
                }).AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
            }
        }

        private void CheckClickOnCubePiece() {
            if (Camera.main == null) return;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hits = new RaycastHit[10];
            var rayCastCnt = Physics.RaycastNonAlloc(ray, hits, Mathf.Infinity, _cubeCastLayerMask);
            if (rayCastCnt <= 0) {
                _isRayCastedOnCube = false;
                return;
            }

            hits = hits.Where(x => x.transform != null).ToArray();
            MainGameManager.instance.CanManipulateCamera = false;
            var min = hits.Min(x => x.distance);
            var nearest = hits.FirstOrDefault(x => x.distance <= min);
            if (nearest.transform != null) {
                _isRayCastedOnCube = true;
                _rayCastHitCubePiece = nearest.transform;
                _onClickedMousePosition = Input.mousePosition;
            } else {
                _isRayCastedOnCube = false;
            }
        }
        

#if UNITY_EDITOR
        private void Reset() {
            _pieceOnFaceDetectors = GetComponentsInChildren<PieceOnFaceDetector>();
            _command = GetComponent<RotateCubeCommand>();
        }
#endif
    }
}