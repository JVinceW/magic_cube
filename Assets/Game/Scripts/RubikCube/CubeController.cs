using System.Linq;
using Cysharp.Threading.Tasks;
using Game.Scripts.Core;
using Game.Scripts.RubikCube.Const;
using NaughtyAttributes;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game.Scripts.RubikCube {
    public class CubeController : MonoBehaviour {
        [SerializeField] private RotateCubeCommand _command;

        [SerializeField] private CubeFaceType _testCubeFaceType;
        [SerializeField] private LayerMask _cubeCastLayerMask;
        [SerializeField] private float _rayCastMaxRange;

        private void Start() {
            this.UpdateAsObservable().Subscribe(x => { Manipulation(); }).AddTo(this);
            this.UpdateAsObservable()
                .Where(x => Input.GetMouseButtonDown(0))
                .Subscribe(x => { CheckClickOnCube(); }).AddTo(this);
            this.UpdateAsObservable()
                .Where(x => Input.GetMouseButtonUp(0))
                .Subscribe(x => {
                    MainGameManager.instance.CanManipulateCamera = true;
                }).AddTo(this);
        }

        private void Manipulation() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                _command.ExecuteRotate(_testCubeFaceType, FaceRotationType.CLOCKWISE,
                    this.GetCancellationTokenOnDestroy()).Forget();
            }
        }

        [Button("Can Manipulate")]
        private void CanManipulate() {
            MainGameManager.instance.CanManipulateCamera = true;
        }

        private void CheckClickOnCube() {
            if (Camera.main == null) return;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var hits = new RaycastHit[10];
            var rayCastCnt = Physics.RaycastNonAlloc(ray, hits, _rayCastMaxRange, _cubeCastLayerMask);
            if (rayCastCnt <= 0) return;
            hits = hits.Where(x => x.transform != null).ToArray();
            MainGameManager.instance.CanManipulateCamera = false;
            var min = hits.Min(x => x.distance);
            var nearest = hits.FirstOrDefault(x => x.distance <= min);
            if (nearest.transform != null) {
                Debug.Log($"Hit piece", nearest.transform);
            }
        }
    }
}