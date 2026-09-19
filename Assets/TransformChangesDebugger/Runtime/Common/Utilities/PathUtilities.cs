using System;
using System.IO;
using System.Text;

namespace ImmersiveVRTools.Runtime.Common.Utilities
{
    public static class PathUtilities
    {
        public static bool TryMakeRelative(
            string absoluteParentPath,
            string absolutePath,
            out string relativePath)
        {
            if (CanMakeRelative(absoluteParentPath, absolutePath))
            {
                relativePath = MakeRelative(absoluteParentPath, absolutePath);
                return true;
            }

            relativePath = null;
            return false;
        }

        public static bool CanMakeRelative(string absoluteParentPath, string absolutePath)
        {
            if (absoluteParentPath == null)
                throw new ArgumentNullException(nameof(absoluteParentPath));
            if (absolutePath == null)
                throw new ArgumentNullException(nameof(absoluteParentPath));
            absoluteParentPath = absoluteParentPath.Replace('\\', '/').Trim('/');
            absolutePath = absolutePath.Replace('\\', '/').Trim('/');
            return Path.GetPathRoot(absoluteParentPath)
                .Equals(Path.GetPathRoot(absolutePath), StringComparison.CurrentCultureIgnoreCase);
        }

        public static string MakeRelative(string absoluteParentPath, string absolutePath)
        {
            absoluteParentPath = absoluteParentPath.TrimEnd('\\', '/');
            absolutePath = absolutePath.TrimEnd('\\', '/');
            var strArray1 = absoluteParentPath.Split('/', '\\');
            var strArray2 = absolutePath.Split('/', '\\');
            var num = -1;
            for (var index = 0;
                index < strArray1.Length && index < strArray2.Length && strArray1[index]
                    .Equals(strArray2[index], StringComparison.CurrentCultureIgnoreCase);
                ++index)
                num = index;
            if (num == -1)
                throw new InvalidOperationException("No common directory found.");
            var stringBuilder = new StringBuilder();
            if (num + 1 < strArray1.Length)
                for (var index = num + 1; index < strArray1.Length; ++index)
                {
                    if (stringBuilder.Length > 0)
                        stringBuilder.Append('/');
                    stringBuilder.Append("..");
                }

            for (var index = num + 1; index < strArray2.Length; ++index)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append('/');
                stringBuilder.Append(strArray2[index]);
            }

            return stringBuilder.ToString();
        }

        public static string RemoveLatestPathPart(string path, string pathDelimiter = "/")
        {
            var index = path.LastIndexOf(pathDelimiter);
            if (index >= 0) {
                return path.Substring(0, index);
            }
            else {
                return path;
            }
        }
    }
}