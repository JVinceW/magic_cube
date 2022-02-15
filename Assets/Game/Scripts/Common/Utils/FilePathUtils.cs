using System.IO;

namespace Game.Scripts.Common.Utils {
    public static class FilePathUtils {
        public static void CreateIfNotExist(string dir) {
            if (!File.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
        }
    }
}