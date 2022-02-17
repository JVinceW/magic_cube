using System;
using System.IO;
using Game.Scripts.Common.Utils;
using Newtonsoft.Json;

namespace Game.Scripts.Common.Singleton {
    public class LocalSaveSingleton<T> where T : LocalSaveSingleton<T>, new() {
        // ReSharper disable once InconsistentNaming
        private static T _instance;

        private const string AES_IV = @"p2s5v8y/A?D(G+Kb"; // iv seed 16 characters
        private const string AES_KEY = @"!z%C*F-JaNdRgUkXn2r5u8x/A?D(G+Kb"; //32 characters

        // ReSharper disable once InconsistentNaming
        private static readonly string _fileName = typeof(T).FullName;

        // ReSharper disable once InconsistentNaming
        public static T instance {
            get {
                AppFilePath.Init();
                if (_instance == null) {
                    Load();
                }

                return _instance;
            }
        }

        public static void Save() {
            _instance.ExeSave();
        }

        public static void Load() {
            try {
                if (File.Exists(GetFilePath())) {
                    var json = File.ReadAllBytes(GetBackupFilePath());
                    var decryption = CryptoUtil.AESDecrypt(json, AES_IV, AES_KEY);
                    var settings = new Newtonsoft.Json.JsonSerializerSettings {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    _instance = JsonConvert.DeserializeObject<T>(decryption, settings);
                    if (!File.Exists(GetBackupFilePath())) {
                        File.WriteAllBytes(GetBackupFilePath(), CryptoUtil.AESEncrypt(decryption, AES_IV, AES_KEY));
                    }
                } else {
                    _instance = new T();
                }
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError(e.Message);
                if (File.Exists(GetBackupFilePath())) {
                    var json = File.ReadAllBytes(GetBackupFilePath());
                    File.ReadAllBytes(GetBackupFilePath());
                    _instance = JsonConvert.DeserializeObject<T>(CryptoUtil.AESDecrypt(json, AES_IV, AES_KEY));
                } else {
                    _instance = new T();
                }
            }
        }

        public static void Reset() {
            if (File.Exists(GetFilePath())) {
                File.Delete(GetFilePath());
            }

            if (File.Exists(GetBackupFilePath())) {
                File.Delete(GetBackupFilePath());
            }


            _instance = null;
        }

        private void ExeSave() {
            var jsonBackupStr = JsonConvert.SerializeObject(instance);
            var backupData = CryptoUtil.AESEncrypt(jsonBackupStr, AES_IV, AES_KEY);
            File.WriteAllBytes(GetBackupFilePath(), backupData);

            var settings = new Newtonsoft.Json.JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var jsonStr = JsonConvert.SerializeObject(instance, settings);
            var data = CryptoUtil.AESEncrypt(jsonStr, AES_IV, AES_KEY);
            File.WriteAllBytes(GetFilePath(), data);
        }

        private static string GetFilePath() {
            var fileName = CryptoUtil.Sha1(_fileName);
            var filePath = $"{AppFilePath.GetLocalSaveDataDir()}/{fileName}";
            // UnityEngine.Debug.Log($"get Encrypted file path: {filePath}");
            return filePath;
        }

        private static string GetBackupFilePath() {
            var fileName = CryptoUtil.Sha1(_fileName);
            var filePath = $"{AppFilePath.GetLocalSaveDataBackupDir()}/{fileName}";
            // UnityEngine.Debug.Log($"get Encrypted backup file path:{filePath}");
            return filePath;
        }
    }
}