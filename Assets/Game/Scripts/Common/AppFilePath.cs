using Game.Scripts.Common.Utils;
using UnityEngine;

namespace Game.Scripts.Common {
    public static class AppFilePath {
        private static string _localSaveDataDir;
        private static string _localSaveDataBackupDir;
        public static bool _isInit;

        public static void Init(bool force = false) {
            if (force) {
                _isInit = false;
            }
            if (_isInit) {
                return;
            }
#if UNITY_STANDALONE
            InitForStandalone();
            FilePathUtils.CreateIfNotExist(_localSaveDataDir);
            FilePathUtils.CreateIfNotExist(_localSaveDataBackupDir);
#endif
            _isInit = true;
        }

        private static void InitForStandalone() {
            _localSaveDataDir = $"{Application.persistentDataPath}/Database";
            _localSaveDataBackupDir = $"{Application.persistentDataPath}/DatabaseBackup";
        }

        public static string GetLocalSaveDataDir() {
            return _localSaveDataDir;
        }

        public static string GetLocalSaveDataBackupDir() {
            return _localSaveDataBackupDir;
        } 
    }
}