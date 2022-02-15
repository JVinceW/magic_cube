namespace Game.Scripts.Common.Singleton {
    public class Singleton<T> where T : class, new() {
        // ReSharper disable once InconsistentNaming
        private static T _instance;

        // ReSharper disable once InconsistentNaming
        public static T instance {
            get {
                if (_instance == null) {
                    _instance = new T();
                }

                return _instance;
            }
        }

        protected Singleton() { }
    }
}