using Cysharp.Threading.Tasks;
using Game.Scripts.RubikCube.Const;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game.Scripts.RubikCube {
    public class CubeController : MonoBehaviour {
        [SerializeField] private RotateCubeCommand _command;

        [SerializeField] private CubeFaceType _testCubeFaceType;
        private void Start() {
            this.UpdateAsObservable().Subscribe(x => {
                Manipulation();
            }).AddTo(this);
        }

        private void Manipulation() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                _command.ExecuteRotate(_testCubeFaceType, FaceRotationType.CLOCKWISE,
                    this.GetCancellationTokenOnDestroy()).Forget();
            }
        }
    }
}