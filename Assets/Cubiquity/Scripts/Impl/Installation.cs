using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace Cubiquity
{
    namespace Impl
    {
        public class Installation
        {
            public static void ValidateAndFix()
            {
                // Get the architecture (32 or 64 bit). Is there a better way?
                string archName = "";
                switch (IntPtr.Size)
                {
                    case 4:
                        archName = "x86";
                        break;
                    case 8:
                        archName = "x86-64";
                        break;
                    default:
                        throw new CubiquityInstallationException("We're sorry, but Cubiquity for Unity3D is not currently supported on your architecture (IntPtr.Size = " + IntPtr.Size + ").");
                }

                // Get the name and the path of the library we will copy (different per platform).
                string fileName = "";
                string sourcePath = Paths.SDK;
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                        fileName = "CubiquityC.dll";
                        sourcePath = System.IO.Path.Combine(sourcePath, "Windows");
                        sourcePath = System.IO.Path.Combine(sourcePath, archName);
                        break;
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                        sourcePath = System.IO.Path.Combine(sourcePath, "OSX");
                        // No need to append archName as OSX uses universal binaries.
                        fileName = "libCubiquityC.dylib";
                        break;
                    case RuntimePlatform.LinuxPlayer:
                        fileName = "libCubiquityC.so";
                        sourcePath = System.IO.Path.Combine(sourcePath, "Linux");
                        sourcePath = System.IO.Path.Combine(sourcePath, archName);
                        break;
                    default:
                        throw new CubiquityInstallationException("We're sorry, but Cubiquity for Unity3D is not currently supported on your platform");
                }

                // Destination path is always the current directory.
                string destPath = System.IO.Directory.GetCurrentDirectory();

                // Use Path class to manipulate file and directory paths.
                string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                string destFile = System.IO.Path.Combine(destPath, fileName);

                // If required, copy the native code library from the SDK to the working directory.
                if (System.IO.File.Exists(destFile))
                {
                    byte[] sourceChecksum = GetChecksum(sourceFile);
                    byte[] destChecksum = GetChecksum(destFile);

                    bool checksumsMatch = true;
                    for (int i = 0; i < sourceChecksum.Length; i++)
                    {
                        if (sourceChecksum[i] != destChecksum[i])
                        {
                            checksumsMatch = false;
                            break;
                        }
                    }

                    if (!checksumsMatch)
                    {
                        Debug.Log("Updating " + fileName + " in the project root folder as it doesn't match the version in the Cubiquity SDK.");

                        try
                        {
                            // The target file exists (it's just the wrong version) so we set the flag to overwrite.
                            System.IO.File.Copy(sourceFile, destFile, true);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            throw new CubiquityInstallationException("Failed to copy '" + fileName + "'", e);
                        }
                    }
                }
                else
                {
                    Debug.Log("Setting up Cubiquity for Unity3D by copying " + fileName + " to the project root folder.");

                    try
                    {
                        // The target file doesn't exist so we don't need to set the flag to overwrite.
                        System.IO.File.Copy(sourceFile, destFile, false);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        throw new CubiquityInstallationException("Failed to copy '" + fileName + "'", e);
                    }
                }

                if (System.IO.File.Exists(destFile) == false)
                {
                    throw new CubiquityInstallationException("The Cubiquity DLL was not found in the project root folder, and this problem was not resolved.");
                }
            }

            // From http://stackoverflow.com/q/1177607
            private static byte[] GetChecksum(string file)
            {
                using (FileStream stream = File.OpenRead(file))
                {
                    SHA256Managed sha = new SHA256Managed();
                    return sha.ComputeHash(stream);
                }
            }
        }
    }
}