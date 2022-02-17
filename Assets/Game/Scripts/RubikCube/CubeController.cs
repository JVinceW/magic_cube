using System.Linq;
using Cysharp.Threading.Tasks;
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

        private bool _isRayCastedOnCube;
        private bool _finishedInit;
        private Vector3 _onClickedMousePosition;

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
                    MainGameManager.instance.CanManipulateCamera = true;
                }).AddTo(this);
            this.UpdateAsObservable()
                .Where(x => _isRayCastedOnCube)
                .Subscribe(x => ManipulateCube()).AddTo(this);
        }

        [Button("Shuffle")]
        private void Shuffle() {
            ShuffleCube().Forget();
        }

        private async UniTask ShuffleCube() {
            var faceList = _command.GetAllPieceOnFaceDetector;
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
            var nowPos = Input.mousePosition;
            var direction = nowPos - _onClickedMousePosition;
            Debug.Log($"Direction : {direction}");
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
                _onClickedMousePosition = Input.mousePosition;
                Debug.Log($"Hit piece", nearest.transform);
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