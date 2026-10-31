using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EditDistributor {

    public class EditDistributorBuilder : EditorWindow
    {
        private static bool Debug = false;
        private GameObject OGAvatarPrefab = null;
        private List<GameObject> CustomAvatarPrefabs = new List<GameObject>() { null };
        private string OGfbxPath = "";
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

        private string EditorWindowPath = "";

        private byte debugMessage = 0;
        private string buildSummary = "";

        private class CustomEntry
        {
            public GameObject Prefab;
            public string CustomfbxPath = "";
            public string OutputDirectory = "";
            public string PatcherScriptDestDir = "";
            public byte Status = 0; // 0 = empty slot, 1 = ready to build, 2 = fbx file not found, 3 = patcher already exists
        }




        [MenuItem("Tools/Hash/EditDistributor/Builder")]
        private static void ShowWindow()
        {
            EditDistributorBuilder window = GetWindow<EditDistributorBuilder>("Builder");
            if (!Debug) window.minSize = new Vector2(290, 320);
            if (Debug) window.minSize = new Vector2(900, 530);

            if (!Debug) window.maxSize = new Vector2(800, 900);
            if (Debug) window.maxSize = new Vector2(1300, 700);
            window.Show();

        }

        private CustomEntry ComputeEntry(GameObject prefab, bool useOverride)
        {
            CustomEntry entry = new CustomEntry { Prefab = prefab };

            if (prefab == null)
            {
                entry.Status = 0;
                return entry;
            }

            entry.CustomfbxPath = CommunFonctions.GetGameObjectPath(prefab);

            if (string.IsNullOrEmpty(entry.CustomfbxPath) || !File.Exists(entry.CustomfbxPath))
            {
                entry.Status = 2;
                return entry;
            }

            entry.OutputDirectory = Path.Combine(CommunFonctions.GoUpNDirs(entry.CustomfbxPath, 2), "Patcher", "Data", "DiffFiles");

            if (useOverride)
            {
                entry.PatcherScriptDestDir = Path.Combine(CommunFonctions.GoUpNDirs(entry.OutputDirectory, 2), "Editor", DistributionNameOverwriteText.Replace(" ", "_") + "_Patcher.cs");
            }
            else
            {
                entry.PatcherScriptDestDir = Path.Combine(CommunFonctions.GoUpNDirs(entry.OutputDirectory, 2), "Editor", Path.GetFileNameWithoutExtension(entry.CustomfbxPath).Replace(" ", "_") + "_Patcher.cs");
            }

            entry.Status = File.Exists(entry.PatcherScriptDestDir) ? (byte)3 : (byte)1;
            return entry;
        }

        void OnGUI()
        {

            GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            boldLabelStyle.fontSize = 14;

            int nonNullCount = 0;
            foreach (GameObject p in CustomAvatarPrefabs) if (p != null) nonNullCount++;
            bool useOverride = NameOverwriteTickBox && !string.IsNullOrEmpty(DistributionNameOverwriteText) && nonNullCount <= 1;

            List<CustomEntry> entries = new List<CustomEntry>();

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
                        OGAvatarPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab original model   ->", "A slot to drag and drop a prefab of the original moddel into (requires the moddel to have an avatar component for now)"), OGAvatarPrefab, typeof(GameObject), true);

                        if (OGAvatarPrefab != null)
                        {
                            OGfbxPath = CommunFonctions.GetGameObjectPath(OGAvatarPrefab);

                            if (Debug) GUILayout.Label(OGfbxPath);
                        }
                        else
                        {
                            OGfbxPath = "";
                        }
                    }

                    EditorGUILayout.Space(10);
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField(new GUIContent("Modified models ->", "Drag in one or more modified prefabs to build a patcher for each against the same original model"));

                        int removeIndex = -1;
                        for (int i = 0; i < CustomAvatarPrefabs.Count; i++)
                        {
                            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                            {
                                CustomAvatarPrefabs[i] = (GameObject)EditorGUILayout.ObjectField(CustomAvatarPrefabs[i], typeof(GameObject), true);

                                CustomEntry entry = ComputeEntry(CustomAvatarPrefabs[i], useOverride);
                                entries.Add(entry);

                                switch (entry.Status)
                                {
                                    case 1:
                                        GUILayout.Label("ready", GUILayout.Width(80));
                                        break;
                                    case 2:
                                        GUILayout.Label("file not found", GUILayout.Width(80));
                                        break;
                                    case 3:
                                        GUILayout.Label("exists, skip", GUILayout.Width(80));
                                        break;
                                }

                                if (GUILayout.Button("-", GUILayout.Width(20)))
                                {
                                    removeIndex = i;
                                }
                            }
                        }

                        if (removeIndex >= 0)
                        {
                            CustomAvatarPrefabs.RemoveAt(removeIndex);
                            entries.RemoveAt(removeIndex);
                        }

                        if (GUILayout.Button("+ Add model"))
                        {
                            CustomAvatarPrefabs.Add(null);
                        }

                        if (NameOverwriteTickBox && nonNullCount > 1)
                        {
                            EditorGUILayout.HelpBox("Name override is ignored with multiple models. Each patcher's menu entry will use its own model's name instead.", MessageType.Info);
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(new GUIContent("Change distribution name?", "Do you want to change the name of the script and menu bar?"));
                    NameOverwriteTickBox = EditorGUILayout.Toggle("", NameOverwriteTickBox);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    if (NameOverwriteTickBox) DistributionNameOverwriteText = EditorGUILayout.TextField( new GUIContent("OverwriteName ", "Name that will be used for the patcher script and hotbar menu"), DistributionNameOverwriteText);

                }



                EditorGUILayout.Space(10);

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {

                    UserEditorWindowName = EditorGUILayout.TextField(new GUIContent("Your Name: ", "Will be used to organize your patchers in the menu bar"), UserEditorWindowName);
                    if (NameOverwriteTickBox)
                    {
                        if (! string.IsNullOrEmpty(DistributionNameOverwriteText)) EditorWindowPath = "Tools/" + UserEditorWindowName + "/" + DistributionNameOverwriteText + " Patcher";
                    }
                    else EditorWindowPath = "Tools/" + UserEditorWindowName + "/" + Path.GetFileNameWithoutExtension(OGfbxPath) + " Patcher";
                    if (string.IsNullOrEmpty(UserEditorWindowName))
                    {

                        //no custom creator name

                    }
                    else
                    {
                        CommunFonctions.AddCenteredLabel(EditorWindowPath);
                        if (!NameOverwriteTickBox && nonNullCount > 1)
                        {
                            EditorGUILayout.HelpBox("Each model's menu entry will be named after that model instead, to avoid duplicate menu entries.", MessageType.Info);
                        }

                    }

                    PackageName = EditorGUILayout.TextField(new GUIContent("Original package name", "package that the user is expected to have in their project"), PackageName);
                }

                EditorGUILayout.Space(10);
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(new GUIContent("Generate descriptions?", "Do you want to generate descriptions for your store pages?"));
                    StoreTextTog = EditorGUILayout.Toggle("", StoreTextTog);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(20);

                    if (StoreTextTog)
                    {

                        CommunFonctions.AddCenteredLabel("Will copy contents of folder and remplace");

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(new GUIContent("More info here", "A link to the GutHub repo"), EditorStyles.linkLabel))
                        {
                            Application.OpenURL("https://github.com/HashEdits/Face-Tracking-Patcher?tab=readme-ov-file#exemple-");
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.Space(20);
                        if (GUILayout.Button(new GUIContent("Select folder with formated descriptions", "folder that will be copied and have all of it's .txt files searched for the specific terms and remplaced")))
                        {
                            DescriptionDir = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                        }
                        if (GUILayout.Button(new GUIContent("Select destination folder", "folder where the remplaced files will be placed")))
                        {
                            DestinationDir = EditorUtility.OpenFolderPanel("Select Folder", "", "");
                        }

                        EditorGUILayout.LabelField("custom desc source: ", DescriptionDir);
                        EditorGUILayout.LabelField("custom desc dest: ", DestinationDir);

                        CreatorName = EditorGUILayout.TextField(new GUIContent("CreatorName ", @"All /*AVATAR AUTHOR*/ tags will be remplaced by what you put in there"), CreatorName);

                        StorePage = EditorGUILayout.TextField(new GUIContent("StorePage ", @"All /*StoreLink*/ tags will be remplaced by what you put in there"), StorePage);

                        //preparing the paterns to feed in the search and remplace algo (replacement values are computed per custom model, see BuildReplacementValues)
                        searchPatterns = new string[] { @"/*AVATAR AUTHOR*/", @"/*StoreLink*/", @"/*AVATAR NAME*/", @"/*PACKAGE NAME*/", @"/*DIR PREFAB*/", @"/*DIR PATCHER*/" };

                    }
                }


                EditorGUILayout.Space(10);

                int readyCount = 0;
                foreach (CustomEntry entry in entries) if (entry.Status == 1) readyCount++;

                //checking for errors
                if (!File.Exists(OGfbxPath))
                {
                    debugMessage = 1;
                }
                else if (string.IsNullOrEmpty(UserEditorWindowName))
                {
                    debugMessage = 7;
                }
                else if  (string.IsNullOrEmpty(PackageName))
                {
                    debugMessage = 3;
                }
                else if (readyCount == 0)
                {
                    debugMessage = 12;
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
                        int builtCount = 0;
                        int skippedExisting = 0;
                        int skippedMissing = 0;

                        foreach (CustomEntry entry in entries)
                        {
                            if (entry.Prefab == null) continue;

                            if (entry.Status == 3)
                            {
                                skippedExisting++;
                                continue;
                            }
                            if (entry.Status != 1)
                            {
                                skippedMissing++;
                                continue;
                            }

                            if (StoreTextTog)//check if the user wants to searsh and remplace
                            {
                                string[] replacementValues = BuildReplacementValues(entry, useOverride);
                                string entrySubfolder = useOverride ? DistributionNameOverwriteText : Path.GetFileNameWithoutExtension(entry.CustomfbxPath);
                                CommunFonctions.ReplaceStringsAndCopyFiles(DescriptionDir, Path.Combine(DestinationDir, entrySubfolder), searchPatterns, replacementValues);//remplaces text
                            }

                            BuildPatcherForEntry(entry, useOverride);
                            builtCount++;
                        }

                        buildSummary = "Built " + builtCount + " patcher(s).";
                        if (skippedExisting > 0) buildSummary += " Skipped " + skippedExisting + " (already exists).";
                        if (skippedMissing > 0) buildSummary += " Skipped " + skippedMissing + " (file not found).";

                        debugMessage = 5;
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
                            CommunFonctions.AddBoldCenteredLabel("Please Input a valid original model");
                            break;
                        case 2:
                            CommunFonctions.AddBoldCenteredLabel("Please Input a valid custom model");

                            break;
                        case 3:
                            CommunFonctions.AddBoldCenteredLabel("Please Input a original package name");
                            break;

                        case 4:
                            CommunFonctions.AddBoldCenteredLabel("Fill the text fileds or untick tickbox");
                            break;

                        case 5:
                            CommunFonctions.AddBoldCenteredLabel("patcher created     Yippie :D");
                            break;
                        case 6:
                            CommunFonctions.AddBoldCenteredLabel("A patcher already exists");
                            CommunFonctions.AddBoldCenteredLabel("please use the updater tool");
                            break;
                        case 7:
                            CommunFonctions.AddBoldCenteredLabel("Please input your name");
                            break;
                        case 8:
                            CommunFonctions.AddBoldCenteredLabel("Please select a source folder");
                            CommunFonctions.AddBoldCenteredLabel("or untick tickbox");
                            break;
                        case 9:
                            CommunFonctions.AddBoldCenteredLabel("Destination cannot be the source");
                            break;
                        case 10:
                            CommunFonctions.AddBoldCenteredLabel("Please select a destination folder");
                            CommunFonctions.AddBoldCenteredLabel("or untick tickbox");
                            break;
                        case 11:
                            CommunFonctions.AddBoldCenteredLabel("add a name overwrite");
                            CommunFonctions.AddBoldCenteredLabel("or untick tickbox");
                            break;
                        case 12:
                            CommunFonctions.AddBoldCenteredLabel("No new models to build");
                            CommunFonctions.AddBoldCenteredLabel("add a model, or use the updater for existing ones");
                            break;
                        default:
                            CommunFonctions.AddBoldCenteredLabel("something went wrong with the error code");
                            CommunFonctions.AddBoldCenteredLabel("it's not supposed to reach this value");
                            break;

                    }
                }

                if (!string.IsNullOrEmpty(buildSummary))
                {
                    EditorGUILayout.Space(10);
                    CommunFonctions.AddBoldCenteredLabel(buildSummary);
                }



            }


            if (Debug) AddDebugInfo(entries);

        }

        private string GetEditorWindowPathForEntry(CustomEntry entry, bool useOverride)
        {
            if (useOverride) return "Tools/" + UserEditorWindowName + "/" + DistributionNameOverwriteText + " Patcher";
            return "Tools/" + UserEditorWindowName + "/" + Path.GetFileNameWithoutExtension(entry.CustomfbxPath) + " Patcher";
        }

        private string[] BuildReplacementValues(CustomEntry entry, bool useOverride)
        {
            string avatarName = useOverride ? DistributionNameOverwriteText : Path.GetFileNameWithoutExtension(OGfbxPath);
            string dirPrefab = useOverride
                ? "Assets" + CommunFonctions.GetRelativePathToAssets(Path.Combine(CommunFonctions.GoUpNDirs(entry.CustomfbxPath, 3), DistributionNameOverwriteText, "prefab"))
                : "Assets" + CommunFonctions.GetRelativePathToAssets(Path.Combine(CommunFonctions.GoUpNDirs(entry.CustomfbxPath, 2), "prefab"));

            return new string[] { CreatorName, StorePage, avatarName, PackageName, dirPrefab, GetEditorWindowPathForEntry(entry, useOverride) };
        }




        private void BuildPatcherForEntry(CustomEntry entry, bool useOverride)
        {
            string patcherFolder = useOverride
                ? Path.Combine(CommunFonctions.GoUpNDirs(entry.CustomfbxPath, 3), DistributionNameOverwriteText, "Patcher")
                : Path.Combine(CommunFonctions.GoUpNDirs(entry.CustomfbxPath, 2), "Patcher");

            if (CommunFonctions.DirectoryContainsExe(patcherFolder))
            {
                CommunFonctions.DeleteAllFilesInDirectory(patcherFolder);//clears the directory of any python scripts remaining
            }

            string outputDirectory = entry.OutputDirectory;
            //Check if output directory already exists
            if (!Directory.Exists(outputDirectory)){
                Directory.CreateDirectory(outputDirectory);
            }

            string hdiffz = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Windows", "hdiffz.exe");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Linux", "hdiffz");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Mac", "hdiffz");
            }
            string hdiffzFolder = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hpatchz");
            string FBXDiffFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + ".hdiff");
            string MetaDiffFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + "Meta.hdiff");

            if (File.Exists(hdiffz))
            {

                // Launch the hdiff with the constructed arguments
                string arguments = "\"" + OGfbxPath + "\" \"" + entry.CustomfbxPath + "\" \"" + FBXDiffFile + "\"";

                CommunFonctions.LaunchProgramWithArguments(hdiffz, arguments);
                //we do the same for the .meta file
                arguments = "\"" + OGfbxPath + ".meta\" \"" + entry.CustomfbxPath + ".meta\" \"" + MetaDiffFile + "\"";
                CommunFonctions.LaunchProgramWithArguments(hdiffz, arguments);



            }
            else
            {
                UnityEngine.Debug.Log("please make sure you have hdiffz.exe in: " + hdiffz);//need to add to the list of errors                                                                 /!\ todo /!\
            }

            string hpatchzDestDir = Path.Combine(CommunFonctions.GoUpNDirs(outputDirectory, 1), "hdiff", "hpatchz");
            Directory.CreateDirectory(hpatchzDestDir);

            string PatcherTemplateScript = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "Editor", "PatcherTemplate.cs");
            string PatcherScriptDestDir = entry.PatcherScriptDestDir;

            //Check if patcher script destination director already exists
            if (!Directory.Exists(CommunFonctions.GoUpNDirs(PatcherScriptDestDir, 1))){
                Directory.CreateDirectory(CommunFonctions.GoUpNDirs(PatcherScriptDestDir, 1));
                CommunFonctions.CopyFolder(hdiffzFolder, hpatchzDestDir);
            }

            CommunFonctions.CheckAndCopyFileIfExists(PatcherTemplateScript, PatcherScriptDestDir);

            string windowTitle;
            string className;
            if (useOverride)
            {
                windowTitle = DistributionNameOverwriteText + "_Patcher";
                className = DistributionNameOverwriteText.Replace(" ", "_") + "_Patcher";
            }
            else
            {
                windowTitle = Path.GetFileNameWithoutExtension(entry.CustomfbxPath).Replace(" ", "_") + "_Patcher";
                className = windowTitle;
            }

            string[] originalStrings = new string[] { "\"YourCoolAvatar\"", "\"YourCoolCustomAvatar\"", "NameOfWindow", "PatcherTemplate", "Tools/Hash/EditDistributor/(DO_NOT_USE)", "DistributionCreator", "CurrentPackageVersion", "AvatarName" };
            string avatarNamePlaceholder = useOverride ? DistributionNameOverwriteText : Path.GetFileNameWithoutExtension(OGfbxPath);
            string entryEditorWindowPath = GetEditorWindowPathForEntry(entry, useOverride);
            string[] remplacementStrings = new string[] { CommunFonctions.MakePathPlatformAgnostic(CommunFonctions.GetRelativePathToAssets(OGfbxPath)), CommunFonctions.MakePathPlatformAgnostic(CommunFonctions.GetRelativePathToAssets(entry.CustomfbxPath)), windowTitle, className, entryEditorWindowPath, UserEditorWindowName, PackageName, avatarNamePlaceholder };
            UnityEngine.Debug.Log(entryEditorWindowPath);
            CommunFonctions.ReplaceStringsInFile(PatcherScriptDestDir, originalStrings, remplacementStrings);

            UnityEngine.Debug.Log("Patcher created yippie :D for " + entry.CustomfbxPath);
            AssetDatabase.Refresh();
        }





        private void AddDebugInfo(List<CustomEntry> entries)
        {
            //some debug things that helped tracing back issues
            GUILayout.Label("hdiff: " + Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "pythonscripts", "hdiff", "hdiffz.exe"));
            GUILayout.Label("hpatchz: " + Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "pythonscripts", "hdiff", "hpatchz.exe"));

            foreach (CustomEntry entry in entries)
            {
                if (entry.Prefab == null) continue;
                EditorGUILayout.Space(10);
                GUILayout.Label("CustomfbxPath: " + entry.CustomfbxPath);
                GUILayout.Label("outputDirectory: " + entry.OutputDirectory);
                GUILayout.Label("PatcherScriptDestDir: " + entry.PatcherScriptDestDir);
                GUILayout.Label("Status: " + entry.Status);
            }
        }

    }
}
