using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;


namespace EditDistributor
{

    public class CommunFonctions : MonoBehaviour
    {


        public static string GetRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentException("Both paths must be non-empty");
            }

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                // The paths have different schemes (e.g., file and http), so we can't construct a relative path.
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            return Uri.UnescapeDataString(relativeUri.ToString());
        }



        public static void AddCenteredLabel(string mymessage)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(mymessage);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }


        public static void AddBoldCenteredLabel(string mymessage)
        {
            GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(mymessage, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }



        public static void CheckAndCopyFileIfExists(string OGPath, string DestPath)
        {
            if (File.Exists(OGPath))
            {
                File.Copy(OGPath, DestPath);
            }
            else
            {
                PrintMissingFileError(OGPath);
            }
        }



        public static void PrintMissingFileError(string PathToFile)
        {
            UnityEngine.Debug.Log("Please make sure you have " + Path.GetFileName(PathToFile) + " in: " + PathToFile);
        }



        public static void DeleteAllFilesInDirectory(string directoryPath)
        {
            try
            {
                // Check if the directory exists
                if (Directory.Exists(directoryPath))
                {
                    // Get all files in the directory
                    string[] files = Directory.GetFiles(directoryPath);

                    // Delete each file
                    foreach (string filePath in files)
                    {
                        File.Delete(filePath);
                        UnityEngine.Debug.Log($"File deleted: {filePath}");
                    }

                    UnityEngine.Debug.Log("Deletion process completed successfully.");
                }
                else
                {
                    UnityEngine.Debug.Log("Directory does not exist.");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"An error occurred: {ex.Message}");
            }
        }




        public static void ReplaceStringsAndCopyFiles(string sourceDirectory, string targetDirectory, string[] searchPatterns, string[] replacementValues)
        {
            try
            {
                // Check if the source directory exists
                if (Directory.Exists(sourceDirectory))
                {
                    // Create the target directory if it doesn't exist
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }

                    // Iterate through all directories and subdirectories
                    string[] allDirectories = Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories);

                    foreach (string currentDirectory in allDirectories)
                    {
                        // Get all files in the current directory
                        string[] files = Directory.GetFiles(currentDirectory);

                        // Iterate over each file, replace strings, and copy to the target directory
                        foreach (string filePath in files)
                        {
                            // Read the content of the file
                            string content = File.ReadAllText(filePath);

                            // Replace strings in the content
                            for (int i = 0; i < searchPatterns.Length; i++)
                            {
                                content = content.Replace(searchPatterns[i], replacementValues[i]);
                            }

                            // Get the relative path within the source directory
                            string relativePath = filePath.Substring(sourceDirectory.Length + 1);

                            // Create the target path by combining the target directory and the relative path
                            string targetFilePath = Path.Combine(targetDirectory, relativePath);

                            // Create the target directory for the file if it doesn't exist
                            string targetFileDirectory = Path.GetDirectoryName(targetFilePath);
                            if (!Directory.Exists(targetFileDirectory))
                            {
                                Directory.CreateDirectory(targetFileDirectory);
                            }

                            // Write the modified content to the target file
                            File.WriteAllText(targetFilePath, content);

                            // Log using UnityEngine.Debug.Log instead of Console.WriteLine
                            UnityEngine.Debug.Log($"File copied and strings replaced: {targetFilePath}");
                        }
                    }

                    // Copy root directory files as well
                    string[] rootFiles = Directory.GetFiles(sourceDirectory);
                    foreach (string filePath in rootFiles)
                    {
                        // Read the content of the file
                        string content = File.ReadAllText(filePath);

                        // Replace strings in the content
                        for (int i = 0; i < searchPatterns.Length; i++)
                        {
                            content = content.Replace(searchPatterns[i], replacementValues[i]);
                        }

                        // Get the relative path within the source directory
                        string relativePath = filePath.Substring(sourceDirectory.Length + 1);

                        // Create the target path by combining the target directory and the relative path
                        string targetFilePath = Path.Combine(targetDirectory, relativePath);

                        // Create the target directory for the file if it doesn't exist
                        string targetFileDirectory = Path.GetDirectoryName(targetFilePath);
                        if (!Directory.Exists(targetFileDirectory))
                        {
                            Directory.CreateDirectory(targetFileDirectory);
                        }

                        // Write the modified content to the target file
                        File.WriteAllText(targetFilePath, content);

                        // Log using UnityEngine.Debug.Log instead of Console.WriteLine
                        UnityEngine.Debug.Log($"Root file copied and strings replaced: {targetFilePath}");
                    }

                    UnityEngine.Debug.Log("Replacement and copying process completed successfully.");
                }
                else
                {
                    UnityEngine.Debug.Log("Source directory does not exist.");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"An error occurred: {ex.Message}");
            }
        }






        public static void ReplaceStringsInFile(string filePath, string[] searchPatterns, string[] replacementValues)
        {
            if (searchPatterns.Length != replacementValues.Length)
            {
                throw new ArgumentException("Search patterns and replacement values arrays must have the same length.");
            }

            string content = File.ReadAllText(filePath);

            for (int i = 0; i < searchPatterns.Length; i++)
            {
                string escapedPattern = Regex.Escape(searchPatterns[i]);
                content = Regex.Replace(content, escapedPattern, replacementValues[i]);
            }

            File.WriteAllText(filePath, content);
        }



        public static bool DirectoryContainsExe(string directoryPath)
        {
            try
            {
                // Check if the directory exists
                if (Directory.Exists(directoryPath))
                {
                    // Get all files in the directory
                    string[] files = Directory.GetFiles(directoryPath);

                    // Check if any file ends with ".exe"
                    foreach (string filePath in files)
                    {
                        if (Path.GetExtension(filePath).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // Return false if no .exe files are found or an error occurs
            return false;
        }


        public static string FindFbxPathInAnimator(GameObject prefab)
        {

            if (prefab != null)
            {
                Animator animator = prefab.GetComponent<Animator>();

                if (animator != null && animator.avatar != null)
                {
                    Avatar avatar = animator.avatar;

                    return GetFBXPathFromAvatar(avatar);
                }
            }

            return string.Empty;
        }


        private static string GetFBXPathFromAvatar(Avatar avatar)
        {
            string avatarGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(avatar));
            string[] assetPaths = AssetDatabase.GetAllAssetPaths();

            foreach (string assetPath in assetPaths)
            {
                if (AssetDatabase.GUIDToAssetPath(avatarGUID) == assetPath && Path.GetExtension(assetPath).ToLower() == ".fbx")
                {
                    string dataPath = Application.dataPath;
                    string relativePath = assetPath.Substring("Assets/".Length);
                    string fullPath = Path.Combine(dataPath, relativePath);
                    fullPath = Path.GetFullPath(fullPath);
                    return fullPath;
                }
            }

            return string.Empty;
        }


        public static string GoUpNDirs(string MyDir, int n)
        {
            MyDir = MyDir.TrimEnd(Path.PathSeparator);
            for (var i = 0; i < n; i++)
                MyDir = Path.GetDirectoryName(MyDir);
            return MyDir;
        }

        public static string GetRelativePathToAssets(string fullPath)
        {
            string assetsFolder = "Assets";
            int assetsIndex = fullPath.IndexOf(assetsFolder, StringComparison.OrdinalIgnoreCase);

            if (assetsIndex != -1)
            {
                string relativePath = fullPath.Substring(assetsIndex + assetsFolder.Length + 1);

                return relativePath.Replace(Path.DirectorySeparatorChar, '/');
            }

            return fullPath;
        }


        public static void LaunchProgramWithArguments(string programPath, string arguments)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = programPath,
                        Arguments = arguments,
                        WorkingDirectory = Application.dataPath,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    }

                };


                process.Start();
                UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
                process.WaitForExit();

            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
        }



        public static void CopyFolder(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // Copy files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                CheckAndCopyFileIfExists(file, destFile);
            }

            // Recursively copy subdirectories
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyFolder(subDir, destSubDir);
            }
        }

        public static string MakePathPlatformAgnostic(string proprietaryPath)
        {
            // Split the input string by '/'
            string[] parts = proprietaryPath.Split(Path.AltDirectorySeparatorChar);

            // Join the parts with proper formatting
            string platformAgnosticPath = "";
            for (int i = 0; i < parts.Length; i++)
            {
                // Add each part enclosed in @"" and concatenate with + Path.PathSeparator +
                platformAgnosticPath += "@\"" + parts[i] + "\"";

                // Append Path.PathSeparator + if it's not the last part
                if (i < parts.Length - 1)
                    platformAgnosticPath += " + Path.DirectorySeparatorChar + ";
            }

            return platformAgnosticPath;
        }
    }



}
