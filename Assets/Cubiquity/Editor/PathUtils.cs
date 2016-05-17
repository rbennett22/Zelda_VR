using System;
using System.IO;

namespace Cubiquity
{
    public class PathUtils
    {
        /// Replaces directory seperator charachters with UNIX-style forwards slashes ('/').
        /**
		 * It is usually desirable for editor code to use UNIX-style directory seperators because the paths may
		 * be serialized, and all platdforms recognize the forward slash whereas only Windows recognizes the
		 * backwards slash. This should only matter for editor code, as paths created in play mode shouldn't
		 * be serialized and later used on a different platform.
		 */
        public static void ReplaceDirSeperatorWithFwdSlash(string path)
        {
            path = path.Replace(Path.DirectorySeparatorChar, '/');
            path = path.Replace(Path.AltDirectorySeparatorChar, '/');
        }

        /// Utility method to construct a relative path between its two inputs.
        /**
		 * \param fromPath The path to start from
		 * \param toPath The path to finish at
		 * \return A string containing a relative path between the two inputs.
		 */
        // Implementation taken from here: http://stackoverflow.com/a/340454
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                // Note: We don't use the platforms 'DirectorySeparatorChar' here because we save the result (e.g. path to .vdb)
                // and we need that to work cross-platform. E.g., if we create a VolumeData asset from Windows then we should not
                // use the Windows '\' seperator as this won't work when we try to load the asset on OS X.
                relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/');
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, '/');
            }

            return relativePath;
        }

        /// Test whether 'child' is a subfolder of 'parent'
        // Based on http://stackoverflow.com/a/5617346
        public static bool IsSubfolder(String child, String parent)
        {
            DirectoryInfo parentDir = new DirectoryInfo(parent);
            DirectoryInfo childDir = new DirectoryInfo(child);

            bool isParent = false;
            while (childDir.Parent != null)
            {
                if (childDir.Parent.FullName == parentDir.FullName)
                {
                    isParent = true;
                    break;
                }
                else childDir = childDir.Parent;
            }

            return isParent;
        }

        public static bool IsSameFolderOrSubfolder(String child, String parent)
        {
            DirectoryInfo parentDir = new DirectoryInfo(parent);
            DirectoryInfo childDir = new DirectoryInfo(child);

            if (parentDir.FullName == childDir.FullName)
            {
                return true;
            }
            else
            {
                return IsSubfolder(child, parent);
            }
        }
    }
}