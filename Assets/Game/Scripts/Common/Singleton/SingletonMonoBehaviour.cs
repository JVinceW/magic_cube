using UnityEngine;

namespace Game.Scripts.Common.Singleton {
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {
        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private static object syncObj = new object();

        // ReSharper disable once InconsistentNaming
        private static T _instance;

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public static T instance {
            get {
                if (_instance == null) {
                    var t = typeof(T);
                    _instance = (T)FindObjectOfType(t);
                    if (_instance == null) {
                        // 同時にインスタンスが生成されないようにする
                        lock (syncObj) {
                            var obj = new GameObject(t.Name);
                            _instance = obj.AddComponent<T>();
                        }
                    }
                }

                return _instance;
            }
        }

        // ReSharper disable once InconsistentNaming
        public static bool exists => _instance != null;

        /// <summary>
        /// インスタンスの削除
        /// </summary>
        public static void DestroyInstance() {
            if (_instance != null) {
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        protected virtual void AwakeSingleton() { }

        /// <summary>
        /// Will be call once, right after the singleton awake
        /// </summary>
        protected virtual void OnCreateSingleton() { }

        private void Awake() {
            if (this != instance) {
                Destroy(gameObject);
                var o = instance.gameObject;
                Debug.LogError(
                    $"[Singleton {typeof(T)}] existed in other gameObject. The new instance will not be created. Attached Object: {o.name}",
                    o);
                return;
            }

            DontDestroyOnLoad(gameObject);

            AwakeSingleton();
            OnCreateSingleton();
        }
    }
}