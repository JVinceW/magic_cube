using Cysharp.Threading.Tasks;
using Game.Scripts.Core;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game.Scripts.Common {
    public class OrbitCamera : MonoBehaviour {
        public bool enableRotation = true;

        [Header("Choose target")] public Transform target;

        //Camera fields
        private float _smoothness = 0.5f;
        private Vector3 _cameraOffset;

        //Mouse control fields
        [Space(2)] [Header("Mouse Controls")] public float rotationSpeedMouse = 5;
        public float zoomSpeedMouse = 10;
        private float _zoomAmountMouse;
        private float _maxToClampMouse = 10;

        private void Start() {
            _cameraOffset = transform.position - target.position;
            UniTask.Create(async () => {
                await UniTask.WaitUntil(() => target != null);
                transform.LookAt(target);
            }).AttachExternalCancellation(this.GetCancellationTokenOnDestroy());
            this.LateUpdateAsObservable()
                .SkipWhile(x => !MainGameManager.instance.CanManipulateCamera)
                .Subscribe(x => UpdateCameraManipulation()).AddTo(this);
        }

        private void UpdateCameraManipulation() {
            if (enableRotation && Input.GetMouseButton(0)) {
                var camAngle = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationSpeedMouse, Vector3.up);
                var camAngleY = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * rotationSpeedMouse, Vector3.right);
                camAngle *= camAngleY;

                var newPos = target.position + _cameraOffset;
                _cameraOffset = camAngle * _cameraOffset;

                transform.position = Vector3.Slerp(transform.position, newPos, _smoothness);
                if (target != null) {
                    transform.LookAt(target);
                }
            } else {
                // Translating camera on PC with mouse wheel.
                _zoomAmountMouse += Input.GetAxis("Mouse ScrollWheel");
                _zoomAmountMouse = Mathf.Clamp(_zoomAmountMouse, -_maxToClampMouse, _maxToClampMouse);

                var translate = Mathf.Min(Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")),
                    _maxToClampMouse - Mathf.Abs(_zoomAmountMouse));
                transform.Translate(0, 0, translate * zoomSpeedMouse * Mathf.Sign(Input.GetAxis("Mouse ScrollWheel")));

                _cameraOffset = transform.position - target.position;
            }
        }

    }
}