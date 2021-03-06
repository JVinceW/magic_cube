using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.Common;
using Game.Scripts.RubikCube.Const;
using Game.Scripts.SceneLogic.GameScene;
using NaughtyAttributes;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Game.Scripts.RubikCube {
    public class CubeController : MonoBehaviour {
        [SerializeField] private PieceOnFaceDetector[] _pieceOnFaceDetectors;
        [SerializeField] private RotateCubeCommand _command;
        [SerializeField] private LayerMask _cubeCastLayerMask;
        [SerializeField] private int _shuffleMinStep = 10;
        [SerializeField] private int _shuffleMaxStep = 20;
        [SerializeField] private int _cubeSize;
        [SerializeField] private GameObject _cameraPivotTarget;
        private bool _isRayCastedOnCube;
        private bool _finishedInit;
        private Vector3 _onClickedMousePosition;
        private Transform _rayCastHitCubePiece;
        private bool _isRotating;
        private const float ROTATE_DIRECTION_DETECTION_THRESHOLD = 10;
        private List<CubePiece> _cubePieces = new List<CubePiece>();
        private readonly BoolReactiveProperty _gameFinished = new BoolReactiveProperty();

        public BoolReactiveProperty GameFinished => _gameFinished;
        public GameObject CameraPivotTarget => _cameraPivotTarget;

        public async UniTask InitCube() {
            this.UpdateAsObservable()
                .Where(x => Input.GetMouseButtonDown(0))
                .Subscribe(x => {
                    if (!_finishedInit) {
                        return;
                    }

                    if (EventSystem.current.IsPointerOverGameObject()) {
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
                    GameScenePlayInfo.instance.CanManipulateCamera = true;
                }).AddTo(this);
            this.UpdateAsObservable()
                .Where(x => _isRayCastedOnCube)
                .Subscribe(x => ManipulateCube()).AddTo(this);
            _cubePieces = GetComponentsInChildren<CubePiece>().ToList();
            await ConstructCubeData();
            _command.Dispose();
        }

        [Button("Test Init Cube On Editor")]
        private void TestCube() {
            InitCube().Forget();
        }

        private async UniTask PlayAnimationWhenNewGame() {
            var cubeOriginal = Quaternion.identity;
            // rotate horizontal 2 round and vertical 2 round
            var yTargetAngle = Quaternion.AngleAxis(180, Vector3.up);
            var xTargetAngle = Quaternion.AngleAxis(180, Vector3.right);
            await _cameraPivotTarget.transform.DORotateQuaternion(yTargetAngle, 1f);
            await _cameraPivotTarget.transform.DORotateQuaternion(xTargetAngle, 1f);
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            await _cameraPivotTarget.transform.DORotateQuaternion(cubeOriginal, 1f);
        }

        private async UniTask ConstructCubeData() {
            if (PlayerLocalSaveData.instance.LastPlayedRubickSize == 0) {
                PlayerLocalSaveData.instance.LastPlayedRubickSize = _cubeSize;
                await PlayAnimationWhenNewGame();
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
                var pieceCoord = new Vector3(pieceState.coordinatorX, pieceState.coordinatorY,
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

        private void UpdateLocalStateAfterMove() {
            var piecesData = new List<CubePieceStateData>();
            foreach (var piece in _cubePieces) {
                piece.SetStateData();
                piecesData.Add(piece.StateData);
            }

            PlayerLocalSaveData.instance.LastPlayedCubePieceStateDatas = piecesData;
            PlayerLocalSaveData.Save();
        }

        [Button("Shuffle")]
        private void Shuffle() {
            ShuffleCube().Forget();
        }

        [Button("Reset Save Data")]
        private void ResetSaveData() {
            PlayerLocalSaveData.Reset();
        }

        public void UndoMove() {
            _command.Undo().Forget();
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
                    _command.AddCommand(selectedDetector, rotationType);
                    await _command.ExecuteRotate(selectedDetector, rotationType, this.GetCancellationTokenOnDestroy());
                    UpdateLocalStateAfterMove();
                    CheckPuzzleDone();
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
            GameScenePlayInfo.instance.CanManipulateCamera = false;
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

        private void CheckPuzzleDone() {
            var isAllAtRightPos = _cubePieces.All(x => x.IsAtRightPosition());
            _gameFinished.Value = isAllAtRightPos;
        }
        

#if UNITY_EDITOR
        private void Reset() {
            _pieceOnFaceDetectors = GetComponentsInChildren<PieceOnFaceDetector>();
            _command = GetComponent<RotateCubeCommand>();
        }
#endif
    }
}