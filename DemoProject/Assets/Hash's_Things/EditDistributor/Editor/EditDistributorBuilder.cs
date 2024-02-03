using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;

public class EditDistributorBuilder : EditorWindow
{
    private static bool Debug = false;
    private GameObject OGAvatarPrefab = null;
    private GameObject CustomAvatarPrefab = null;
    private string OGfbxPath = "";
    private string CustomfbxPath = "";
    private string UserEditorWindowName = "";

    private bool NameOverwriteTickBox = false;
    private string DistributionNameOverwriteText = "";

    private bool StoreTextTog = false;
    private string DescriptionDir = "";
    private string CreatorName = "";
    private string StorePage = "";
    private string PackageName = "";
    string DestinationDir = "";
    string[] searchPatterns;
    string[] replacementValues;


    private string hdiffz = "";
    private string hpatchz = "";
    private string FBXDiffFile = "";
    private string MetaDiffFile = "";
    private string hpatchzDestDir = "";
    private string PatcherScriptDestDir = "";
    private string PatcherTemplateScript = "";
    private string EditorWindowPath = "";
    private string outputDirectory = "";
    string[] remplacementStrings;

    private byte debugMessage = 0;

    private string patcherFolder = "";




    [MenuItem("Tools/Hash/EditDistributor/Builder")]
    private static void ShowWindow()
    {
        EditDistributorBuilder window = GetWindow<EditDistributorBuilder>("Builder");
        if (!Debug) window.minSize = new Vector2(290, 320);
        if (Debug) window.minSize = new Vector2(900, 530);

        if (!Debug) window.maxSize = new Vector2(290, 500);
        if (Debug) window.maxSize = new Vector2(1300, 700);
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
            GUILayout.Label("Hash's Distribution Builder", boldLabelStyle);
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

                            //debugMessage = 8;
                        }
                        //else debugMessage = 0;


                    }
                    else
                    {
                        OGfbxPath = "";
                        //debugMessage = 1;
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
                            patcherFolder = "";//
                            //debugMessage = 9;
                        }
                        else
                        {
                            outputDirectory = Path.Combine(GoUpNDirs(CustomfbxPath, 2), "patcher", "data", "DiffFiles");
                            PatcherScriptDestDir = Path.Combine(GoUpNDirs(outputDirectory, 2), "Editor", Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher.cs");
                            if (NameOverwriteTickBox)
                            {
                                if (string.IsNullOrEmpty(DistributionNameOverwriteText))
                                {
                                    //string is empty, don't even try, just wait for the error
                                }
                                else
                                {
                                    patcherFolder = Path.Combine(GoUpNDirs(CustomfbxPath, 3), DistributionNameOverwriteText, "patcher");
                                }
                            }
                            else patcherFolder = Path.Combine(GoUpNDirs(CustomfbxPath, 2), "patcher");
                        }
                    }
                    else
                    {
                        CustomfbxPath = "";
                        patcherFolder = "";
                    }
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Change distribution name?");
                NameOverwriteTickBox = EditorGUILayout.Toggle("", NameOverwriteTickBox);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                if (NameOverwriteTickBox) DistributionNameOverwriteText = EditorGUILayout.TextField("OverwriteName ", DistributionNameOverwriteText);

            }

            

            EditorGUILayout.Space(10);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {

                UserEditorWindowName = EditorGUILayout.TextField("Your Name: ", UserEditorWindowName);
                if (NameOverwriteTickBox)
                {
                    if (string.IsNullOrEmpty(DistributionNameOverwriteText))
                    {
                        //string is empty, don't even try, just wait for the error
                    }
                    else EditorWindowPath = "Tools/" + UserEditorWindowName + "/" + DistributionNameOverwriteText + "/" + DistributionNameOverwriteText + "Patcher";
                }
                else EditorWindowPath = "Tools/" + UserEditorWindowName + "/" + Path.GetFileNameWithoutExtension(OGfbxPath) + "/" + Path.GetFileNameWithoutExtension(OGfbxPath) + "Patcher";
                if (string.IsNullOrEmpty(UserEditorWindowName))
                {

                    //no custom creator name

                }
                else
                {
                    AddCenteredLabel(EditorWindowPath);

                }

                PackageName = EditorGUILayout.TextField("Original package name", PackageName);
            }

            EditorGUILayout.Space(10);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Generate descriptions?");
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
                    if (NameOverwriteTickBox) {
                        if (string.IsNullOrEmpty(DistributionNameOverwriteText))
                        {
                            //string is empty, don't even try, just wait for the error
                        }
                        else replacementValues = new string[] { CreatorName, StorePage, DistributionNameOverwriteText, PackageName, "Assets" + GetRelativePathToAssets(Path.Combine(GoUpNDirs(CustomfbxPath, 3), DistributionNameOverwriteText, "prefab")) };
                    }
                    else replacementValues = new string[] { CreatorName, StorePage, Path.GetFileNameWithoutExtension(OGfbxPath), PackageName, "Assets" + GetRelativePathToAssets(Path.Combine(GoUpNDirs(CustomfbxPath, 2), "prefab")) };

                }
            }


            EditorGUILayout.Space(10);





            //checking for errors
            if (!File.Exists(OGfbxPath))
            {
                debugMessage = 1;
            }
            else if (!File.Exists(CustomfbxPath))
            {
                debugMessage = 2;
            }
            else if (string.IsNullOrEmpty(UserEditorWindowName))
            {
                debugMessage = 7;
            }
            else if  (string.IsNullOrEmpty(PackageName))
            {
                debugMessage = 3;
            }
            else if (File.Exists(PatcherScriptDestDir))
            {
                debugMessage = 6;
            }
            else if (StoreTextTog && string.IsNullOrEmpty(DescriptionDir))
            {
                debugMessage = 8;
            }
            else if (StoreTextTog &&( (string.IsNullOrEmpty(CreatorName) || string.IsNullOrEmpty(StorePage) || string.IsNullOrEmpty(PackageName))))
            {
                debugMessage = 4;
            }
            else if(StoreTextTog && (DescriptionDir == DestinationDir))
            {
                debugMessage = 9;
            }
            else if(StoreTextTog && string.IsNullOrEmpty(DestinationDir))
            {
                debugMessage = 10;
            }
            else if (NameOverwriteTickBox && string.IsNullOrEmpty(DistributionNameOverwriteText))
            {
                debugMessage = 11;
            }
            
            else
            {
                debugMessage = 0;
            }


            //main patching logic
            if (debugMessage == 0)
            {
                if (GUILayout.Button("Build"))
                {   
                    if (DirectoryContainsExe(patcherFolder))
                    {
                        DeleteAllFilesInDirectory(patcherFolder);//clears the directory of any python scripts remaining
                    }

                    if (StoreTextTog)//check if the user wants to searsh and remplace
                    {
                        ReplaceStringsAndCopyFiles(DescriptionDir, DestinationDir, searchPatterns, replacementValues);//remplaces text
                        BuildPatcher();
                        debugMessage = 5;//indicate to the user that everything went smoothly
                    }
                    else
                    {
                        BuildPatcher();
                        debugMessage = 5;//indicate to the user that everything went smoothly
                    }
                }
            }
            else
            {
                //errors let's check
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
                        AddBoldCenteredLabel("Please Input a original package name");
                        break;

                    case 4:
                        AddBoldCenteredLabel("Fill the text fileds or untick tickbox");
                        break;

                    case 5:
                        AddBoldCenteredLabel("patcher created     Yippie :D");
                        break;
                    case 6:
                        AddBoldCenteredLabel("A patcher already exists");
                        AddBoldCenteredLabel("please use the updater tool");
                        break;
                    case 7:
                        AddBoldCenteredLabel("Please input your name");
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
                    case 11:
                        AddBoldCenteredLabel("add a name overwrite");
                        AddBoldCenteredLabel("or untick tickbox");
                        break;
                    default:
                        AddBoldCenteredLabel("something went wrong with the error code");
                        AddBoldCenteredLabel("it's not supposed to reach this value");
                        break;

                }
            }



        }


        if (Debug) AddDebugInfo();
        
    }



    private void BuildPatcher()
    {
        
        Directory.CreateDirectory(outputDirectory);

        hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz.exe");
        hpatchz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hpatchz.exe");
        string hpatchzlicence = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "License.txt");
        FBXDiffFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + ".hdiff");
        MetaDiffFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + "Meta.hdiff");

        if (File.Exists(hdiffz))
        {
            // Launch the hdiff with the constructed arguments
            string arguments = "\"" + OGfbxPath + "\" \"" + CustomfbxPath+ "\" \"" +  FBXDiffFile+ "\"";
            LaunchProgramWithArguments(hdiffz, arguments);
            //we do the same for the .meta file
            arguments = "\"" + OGfbxPath + ".meta\" \"" + CustomfbxPath + ".meta\" \"" + MetaDiffFile + "\"";
            LaunchProgramWithArguments(hdiffz, arguments);
        }
        else
        {
            UnityEngine.Debug.Log("please make sure you have hdiffz.exe in: " + hdiffz);//need to add to the list of errors                                                                 /!\ todo /!\
        }

        hpatchzDestDir = Path.Combine(GoUpNDirs(outputDirectory, 1), "hdiff", "hpatchz.exe");
        string hpatchzLicenceDestDir = Path.Combine(GoUpNDirs(outputDirectory, 1), "hdiff", "License.txt");
        Directory.CreateDirectory(GoUpNDirs(hpatchzDestDir, 1));

        PatcherTemplateScript = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "Editor", "PatcherTemplate.cs");

        PatcherScriptDestDir = Path.Combine(GoUpNDirs(outputDirectory, 2), "Editor", Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher.cs");
        Directory.CreateDirectory(GoUpNDirs(PatcherScriptDestDir, 1));
        CheckAndCopyFileIfExists(hpatchz, hpatchzDestDir);
        CheckAndCopyFileIfExists(hpatchzlicence, hpatchzLicenceDestDir);
        CheckAndCopyFileIfExists(PatcherTemplateScript, PatcherScriptDestDir);


        string[] originalStrings = new string[] { "YourCoolAvatar", "YourCoolCustomAvatar", "NameOfWindow", "PatcherTemplate", "Tools/Hash/EditDistributor/(DO_NOT_USE)", "Creator", "CurrentPackageVersion", "AvatarName" };
        if (NameOverwriteTickBox)
        {
            if (string.IsNullOrEmpty(DistributionNameOverwriteText))
            {
                //string is empty, don't even try, just wait for the error
            }
            else remplacementStrings = new string[] { GetRelativePathToAssets(OGfbxPath), GetRelativePathToAssets(CustomfbxPath), Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher", Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher", EditorWindowPath, UserEditorWindowName, PackageName, DistributionNameOverwriteText };
        }
        else remplacementStrings = new string[] { GetRelativePathToAssets(OGfbxPath), GetRelativePathToAssets(CustomfbxPath), Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher", Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher", EditorWindowPath, UserEditorWindowName, PackageName, Path.GetFileNameWithoutExtension(OGfbxPath) };
        UnityEngine.Debug.Log(EditorWindowPath);
        ReplaceStringsInFile(PatcherScriptDestDir, originalStrings, remplacementStrings);

        UnityEngine.Debug.Log("patcher created yippie :D");
        AssetDatabase.Refresh();
    }

    private static string GetRelativePath(string fromPath, string toPath)
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

    void CheckAndCopyFileIfExists(string OGPath, string DestPath)
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

    void PrintMissingFileError(string PathToFile)
    {
        UnityEngine.Debug.Log("please make sure you have " + Path.GetFileName(PathToFile) + " in: " + PathToFile);
    }

    private static void ReplaceStringsInFile(string filePath, string[] searchPatterns, string[] replacementValues)
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

    static bool DirectoryContainsExe(string directoryPath)
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

    private void AddCenteredLabel(string mymessage)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(mymessage);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
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

    private void AddDebugInfo()
    {
        string outputDirectory = Path.Combine(GoUpNDirs(CustomfbxPath, 2), "patcher", "data", "DiffFiles");
        //some debug things that helped tracing back issues
        GUILayout.Label("outputDirectory: " + outputDirectory);
        GUILayout.Label("hdiff: " + Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "pythonscripts", "hdiff", "hdiffz.exe"));
        GUILayout.Label("hpatchz: " + Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "pythonscripts", "hdiff", "hpatchz.exe"));
        GUILayout.Label("FBXDiffFile: " + Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + ".hdiff"));
        GUILayout.Label("MetaDiffFile: " + Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + "Meta.hdiff"));


        GUILayout.Label("hpatchzDestDir: " + Path.Combine(GoUpNDirs(outputDirectory, 1), "hdiff", "hpatchz.exe"));
        GUILayout.Label("PatcherScriptDestDir" + Path.Combine(GoUpNDirs(outputDirectory, 2), "Editor", Path.GetFileNameWithoutExtension(CustomfbxPath) + "Patcher.cs"));
        GUILayout.Label("PatcherTemplateScript" + Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "Editor", "PatcherTemplate.cs"));

        GUILayout.Label(GetRelativePath(Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "Editor", "PatcherTemplate.cs"), OGfbxPath));

        EditorGUILayout.Space(10);
        GUILayout.Label(GetRelativePathToAssets(OGfbxPath));
        GUILayout.Label(GetRelativePathToAssets(CustomfbxPath));

        GUILayout.Label(Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher");
    }

}
