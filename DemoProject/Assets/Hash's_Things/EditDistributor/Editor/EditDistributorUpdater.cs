using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections;

public class EditDistributorUpdater : EditorWindow
{


    private static bool Debug = false;
    private GameObject OGAvatarPrefab = null;
    private GameObject CustomAvatarPrefab = null;
    private string OGfbxPath = "";
    private string CustomfbxPath = "";
    private string patcherToUpdate = "";
    private string PackageName = "";
    private string hdiffz = "";
    private string FBXDiffFile = "";
    private string MetaDiffFile = "";
    string originalStrings;
    string remplacementStrings;
    string[] searchPatterns;
    string[] replacementValues;


    private bool StoreTextTog = false;
    private string DescriptionDir = "";
    string DestinationDir = "";
    private string CreatorName = "";
    private string StorePage = "";


    private byte debugMessage = 0;


    [MenuItem("Tools/Hash/EditDistributor/Updater")]
    private static void ShowWindow()
    {
        EditDistributorUpdater window = GetWindow<EditDistributorUpdater>("Updater");
        //if (!Debug) window.maxSize = new Vector2(442, 160);
        window.minSize = new Vector2(290, 220);
        window.maxSize = new Vector2(400, 450);
        window.Show();
    }

    void OnGUI()
    {
        GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
        boldLabelStyle.fontSize = 14;
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Hash's Distribution Updater", boldLabelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("GitHub Repo", EditorStyles.linkLabel))
            {
                Application.OpenURL("https://github.com/HashEdits/Face-Tracking-Patcher");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    OGAvatarPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab original model   ->", OGAvatarPrefab, typeof(GameObject), true);

                    if (OGAvatarPrefab != null)
                    {
                        OGfbxPath = FindFbxPathInAnimator(OGAvatarPrefab);

                        if (Debug) GUILayout.Label(OGfbxPath);
                        if (string.IsNullOrEmpty(OGfbxPath))
                        {


                        }



                    }
                    else
                    {
                        OGfbxPath = "";

                    }
                }

                EditorGUILayout.Space(10);
                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    CustomAvatarPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab modified model ->", CustomAvatarPrefab, typeof(GameObject), true);

                    if (CustomAvatarPrefab != null)
                    {
                        CustomfbxPath = FindFbxPathInAnimator(CustomAvatarPrefab);

                        if (Debug) GUILayout.Label(CustomfbxPath);
                        if (string.IsNullOrEmpty(CustomfbxPath))
                        {
                            patcherToUpdate = "";
                        }
                        else
                        {
                            patcherToUpdate = Path.Combine(GoUpNDirs(CustomfbxPath, 2), "patcher", "Editor", "Nebbia_FT_Patcher.cs");
                        }
                    }
                    else
                    {
                        CustomfbxPath = "";
                        patcherToUpdate = "";
                    }
                }
            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PackageName = EditorGUILayout.TextField("Name of New package", PackageName);
            }
        }

        //text remplacement
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Generate updated descriptions?");
            StoreTextTog = EditorGUILayout.Toggle("", StoreTextTog);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            if (StoreTextTog)
            {

                AddCenteredLabel("Will copy the selected folder and remplace");

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("More info here", EditorStyles.linkLabel))
                {
                    Application.OpenURL("https://github.com/HashEdits/Face-Tracking-Patcher?tab=readme-ov-file#exemple-");
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.Space(20);
                if (GUILayout.Button("Select custom folder"))
                {
                    DescriptionDir = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                }
                if (GUILayout.Button("Select destination"))
                {
                    DestinationDir = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                }
                EditorGUILayout.LabelField("custom desc source: ", DescriptionDir);
                EditorGUILayout.LabelField("custom desc dest: ", DestinationDir);

                CreatorName = EditorGUILayout.TextField("CreatorName ", CreatorName);

                StorePage = EditorGUILayout.TextField("StorePage ", StorePage);

                //preparing the paterns to feed in the search and remplace algo
                searchPatterns = new string[] { @"/*AVATAR AUTHOR*/", @"/*StoreLink*/", @"/*AVATAR NAME*/", @"/*PACKAGE NAME*/", @"/*DIR PREFAB*/" };
                replacementValues = new string[] { CreatorName, StorePage, Path.GetFileNameWithoutExtension(OGfbxPath), PackageName, "Assets" + GetRelativePathToAssets(Path.Combine(GoUpNDirs(CustomfbxPath, 2), "prefab")) };

            }
        }
        UnityEngine.Debug.Log(StoreTextTog && (string.IsNullOrEmpty(CreatorName) && string.IsNullOrEmpty(StorePage) && string.IsNullOrEmpty(PackageName)));


        if (!File.Exists(OGfbxPath))
        {
            debugMessage = 1;
        }
        else if (!File.Exists(CustomfbxPath))
        {
            debugMessage = 2;
        }
        else if (string.IsNullOrEmpty(PackageName))
        {
            debugMessage = 3;
        }
        else if (!File.Exists(patcherToUpdate))
        {
            debugMessage = 6;
        }
        else if (StoreTextTog && string.IsNullOrEmpty(DescriptionDir))
        {
            debugMessage = 8;
        }
        else if (StoreTextTog && ((string.IsNullOrEmpty(CreatorName) || string.IsNullOrEmpty(StorePage) || string.IsNullOrEmpty(PackageName))))
        {
            debugMessage = 4;
        }
        else if (StoreTextTog && (DescriptionDir == DestinationDir))
        {
            debugMessage = 9;
        }
        else if (StoreTextTog && string.IsNullOrEmpty(DestinationDir))
        {
            debugMessage = 10;
        }
        else
        {
            debugMessage = 0;
        }

        if (debugMessage == 0)
        {
            if (GUILayout.Button("Update"))
            {
                if (StoreTextTog)
                {
                    ReplaceStringsAndCopyFiles(DescriptionDir, DestinationDir, searchPatterns, replacementValues);//remplaces text
                    Updating();
                    debugMessage = 4;
                    AssetDatabase.Refresh();
                }
                else
                {

                    Updating();
                    debugMessage = 4;
                    AssetDatabase.Refresh();
                }
            }
        }
        else
        {
            switch (debugMessage)
            {
                case 0:

                    break;
                case 1:
                    AddBoldCenteredLabel("Please Input a valid original model");
                    break;
                case 2:
                    AddBoldCenteredLabel("Please Input a valid custom model");

                    break;
                case 3:
                    AddBoldCenteredLabel("Please Input a new package name");
                    break;
                case 4:
                    AddBoldCenteredLabel("Fill the text fileds or untick tickbox");
                    break;
                case 6:
                    AddBoldCenteredLabel("A patcher doesn't already exists");
                    AddBoldCenteredLabel("please use the builder tool");
                    break;
                case 8:
                    AddBoldCenteredLabel("Please select a source folder");
                    AddBoldCenteredLabel("or untick tickbox");
                    break;
                case 9:
                    AddBoldCenteredLabel("Destination cannot be the source");
                    break;
                case 10:
                    AddBoldCenteredLabel("Please select a destination folder");
                    AddBoldCenteredLabel("or untick tickbox");
                    break;
                default:
                    AddBoldCenteredLabel("something went wrong with the error code");
                    break;
            }
        }
        if (debugMessage == 4)
        {
            AddBoldCenteredLabel("patcher updated!");
        }
    }

    private void Updating()
    {
        string outputDirectory = Path.Combine(GoUpNDirs(CustomfbxPath, 2), "patcher", "data", "DiffFiles");


        hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz.exe");
        FBXDiffFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + ".hdiff");
        MetaDiffFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + "Meta.hdiff");

        if (File.Exists(hdiffz))
        {
            DeleteAllFilesInDirectory(outputDirectory);
            // Launch the hdiff with the constructed arguments
            string arguments = "\"" + OGfbxPath + "\" \"" + CustomfbxPath + "\" \"" + FBXDiffFile + "\"";
            LaunchProgramWithArguments(hdiffz, arguments);
            //we do the same for the .meta file
            arguments = "\"" + OGfbxPath + ".meta\" \"" + CustomfbxPath + ".meta\" \"" + MetaDiffFile + "\"";
            LaunchProgramWithArguments(hdiffz, arguments);
        }
        else
        {
            UnityEngine.Debug.Log("please make sure you have hdiffz.exe in: " + hdiffz);//need to add to the list of errors                                                                 /!\ todo /!\
        }

        originalStrings = @"    private static string AvatarVersion = " ;
        remplacementStrings =@"    private static string AvatarVersion = " + '\u0022' + PackageName + '\u0022' + ';';
        ReplaceLineInFile(patcherToUpdate, originalStrings, remplacementStrings);
    }

    private static void LaunchProgramWithArguments(string programPath, string arguments)
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

    private string FindFbxPathInAnimator(GameObject prefab)
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

    static void ReplaceStringsAndCopyFiles(string sourceDirectory, string targetDirectory, string[] searchPatterns, string[] replacementValues)
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

    static void ReplaceLineInFile(string filePath, string searchStartsWith, string replacementLine)
    {
        try
        {
            // Check if the file exists
            if (File.Exists(filePath))
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(filePath);

                // Find and replace the line that starts with the specified string
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith(searchStartsWith))
                    {
                        lines[i] = replacementLine;
                        UnityEngine.Debug.Log($"Line replaced: {lines[i]}");
                        break; // Stop searching after the first match (remove if you want to replace all matching lines)
                    }
                }

                // Write the modified lines back to the file
                File.WriteAllLines(filePath, lines);

                UnityEngine.Debug.Log("Replacement process completed successfully.");
            }
            else
            {
                UnityEngine.Debug.Log("File does not exist.");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"An error occurred: {ex.Message}");
        }
    }

    string GoUpNDirs(string MyDir, int n)
    {
        MyDir = Path.GetFullPath(MyDir);
        string[] pathComponents = MyDir.Split('\\');
        if (pathComponents.Length >= n)
        {

            string parentPath = string.Join("\\", pathComponents, 0, pathComponents.Length - n);
            return Path.Combine(parentPath);
        }
        else return string.Empty;

    }

    private string GetFBXPathFromAvatar(Avatar avatar)
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

    private static string GetRelativePathToAssets(string fullPath)
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

    private void AddBoldCenteredLabel(string mymessage)
    {
        GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(mymessage, EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void AddCenteredLabel(string mymessage)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(mymessage);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    static void DeleteAllFilesInDirectory(string directoryPath)
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
}
