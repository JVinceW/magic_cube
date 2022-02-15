using System;
using Game.Scripts.Common.Utils;
using Newtonsoft.Json;

namespace Game.Scripts.Common.Singleton {
    public class LocalSaveSingleton<T> where T : LocalSaveSingleton<T>, new() {
         // ReSharper disable once InconsistentNaming
        private static T _instance;

        private const string AES_IV = @"WnZr4u7x!A%D*F-J"; // iv seed 16 characters
        private const string AES_KEY = @"Zq4t7w!z%C*F-JaNdRgUjXn2r5u8x/A?"; //32 characters
        // ReSharper disable once InconsistentNaming
        private static readonly string _fileName = typeof(T).FullName;

        // ReSharper disable once InconsistentNaming
        public static T instance
        {
            get
            {
                AppFilePath.Init();
                if (_instance == null)
                {
                    Load();
                }

                return _instance;
            }
        }

        public static void Save()
        {
            _instance.ExeSave();
        }

        public static void Load()
        {
            try
            {
                if (
                    // File.Exists(GetFilePath())
                    UnityEngine.PlayerPrefs.HasKey(GetFilePath())
                    )
                {
                    var json =
                       // File.ReadAllBytes(GetBackupFilePath());
                       Convert.FromBase64String(UnityEngine.PlayerPrefs.GetString(GetBackupFilePath()));
                    var decryption = CryptoUtil.AESDecrypt(json, AES_IV, AES_KEY);
                    _instance = JsonConvert.DeserializeObject<T>(decryption);
                    if (
                        // !File.Exists(GetBackupFilePath())
                        !UnityEngine.PlayerPrefs.HasKey(GetBackupFilePath())
                        )
                    {
                        // File.WriteAllBytes(GetBackupFilePath(), CryptoUtil.AESEncrypt(decryption, AES_IV, AES_KEY));

                        UnityEngine.PlayerPrefs.SetString(GetBackupFilePath(), Convert.ToBase64String(CryptoUtil.AESEncrypt(decryption, AES_IV, AES_KEY)));
                        UnityEngine.PlayerPrefs.Save();

                    }
                }
                else
                {
                    _instance = new T();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
                if (
                    // File.Exists(GetBackupFilePath())
                    UnityEngine.PlayerPrefs.HasKey(GetBackupFilePath())
                    )
                {
                    // byte[] json = File.ReadAllBytes(GetBackupFilePath());
                    var json =
                      // File.ReadAllBytes(GetBackupFilePath());
                      Convert.FromBase64String(UnityEngine.PlayerPrefs.GetString(GetBackupFilePath()));
                    _instance = JsonConvert.DeserializeObject<T>(CryptoUtil.AESDecrypt(json, AES_IV, AES_KEY));
                }
                else
                {
                    _instance = new T();
                }
            }
        }

        public static void Reset()
        {
            /*
            if (File.Exists(GetFilePath())) {
                File.Delete(GetFilePath());
            }

            if (File.Exists(GetBackupFilePath())) {
                File.Delete(GetBackupFilePath());        
            }
            */

            UnityEngine.PlayerPrefs.DeleteKey(GetFilePath());
            UnityEngine.PlayerPrefs.DeleteKey(GetBackupFilePath());

            _instance = null;
        }

        private void ExeSave()
        {
            var jsonBackupStr = JsonConvert.SerializeObject(instance);
            var backupData = CryptoUtil.AESEncrypt(jsonBackupStr, AES_IV, AES_KEY);
            // File.WriteAllBytes(GetBackupFilePath(), backupData);
            UnityEngine.PlayerPrefs.SetString(GetBackupFilePath(), Convert.ToBase64String(backupData));
            UnityEngine.PlayerPrefs.Save();

            var jsonStr = JsonConvert.SerializeObject(instance);
            var data = CryptoUtil.AESEncrypt(jsonStr, AES_IV, AES_KEY);
            UnityEngine.PlayerPrefs.SetString(GetFilePath(), Convert.ToBase64String(data));
            UnityEngine.PlayerPrefs.Save();
            // File.WriteAllBytes(GetFilePath(), data);
        }

        private static string GetFilePath()
        {
            var fileName = CryptoUtil.Sha1(_fileName);
            var filePath = $"{AppFilePath.GetLocalSaveDataDir()}/{fileName}";
            // UnityEngine.Debug.Log($"get Encrypted file path: {filePath}");
            return filePath;
        }

        private static string GetBackupFilePath()
        {
            var fileName = CryptoUtil.Sha1(_fileName);
            var filePath = $"{AppFilePath.GetLocalSaveDataBackupDir()}/{fileName}";
            // UnityEngine.Debug.Log($"get Encrypted backup file path:{filePath}");
            return filePath;
        }
    }
}