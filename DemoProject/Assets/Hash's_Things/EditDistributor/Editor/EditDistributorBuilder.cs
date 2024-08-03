﻿using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;

namespace EditDistributor { 

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

            if (!Debug) window.maxSize = new Vector2(400, 600);
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
                        OGAvatarPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab original model   ->", "A slot to drag and drop a prefab of the original moddel into (requires the moddel to have an avatar component for now)"), OGAvatarPrefab, typeof(GameObject), true);

                        if (OGAvatarPrefab != null)
                        {
                            OGfbxPath = CommunFonctions.FindFbxPathInAnimator(OGAvatarPrefab);

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
                        CustomAvatarPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab modified model ->", "A slot to drag and drop a prefab of your modified moddel into (requires the moddel to have an avatar component for now)"), CustomAvatarPrefab, typeof(GameObject), true);

                        if (CustomAvatarPrefab != null)
                        {
                            CustomfbxPath = CommunFonctions.FindFbxPathInAnimator(CustomAvatarPrefab);

                            if (Debug) GUILayout.Label(CustomfbxPath);
                            if (string.IsNullOrEmpty(CustomfbxPath))
                            {
                                patcherFolder = "";//
                                //debugMessage = 9;
                            }
                            else
                            {
                                outputDirectory = Path.Combine(CommunFonctions.GoUpNDirs(CustomfbxPath, 2), "Patcher", "Data", "DiffFiles");
                                if (NameOverwriteTickBox)
                                {
                                    if (!string.IsNullOrEmpty(DistributionNameOverwriteText))
                                    {
                                        patcherFolder = Path.Combine(CommunFonctions.GoUpNDirs(CustomfbxPath, 3), DistributionNameOverwriteText, "Patcher");
                                        PatcherScriptDestDir = Path.Combine(CommunFonctions.GoUpNDirs(outputDirectory, 2), "Editor", DistributionNameOverwriteText.Replace(" ", "_") + "_Patcher.cs");
                                    }
                                }
                                else
                                {
                                    patcherFolder = Path.Combine(CommunFonctions.GoUpNDirs(CustomfbxPath, 2), "Patcher");
                                    PatcherScriptDestDir = Path.Combine(CommunFonctions.GoUpNDirs(outputDirectory, 2), "Editor", Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher.cs");
                                }
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

                        //if (!string.IsNullOrEmpty(DescriptionDir)) EditorGUILayout.LabelField(new GUIContent("custom desc source: ", DescriptionDir), DescriptionDir);
                        /*else*/ EditorGUILayout.LabelField("custom desc source: ", DescriptionDir);
                        //if (!string.IsNullOrEmpty(DestinationDir)) EditorGUILayout.LabelField(new GUIContent("custom desc dest: ", DestinationDir), DestinationDir);
                        /*else*/ EditorGUILayout.LabelField("custom desc dest: ", DestinationDir);

                        CreatorName = EditorGUILayout.TextField(new GUIContent("CreatorName ", @"All /*AVATAR AUTHOR*/ tags will be remplaced by what you put in there"), CreatorName);

                        StorePage = EditorGUILayout.TextField(new GUIContent("StorePage ", @"All /*StoreLink*/ tags will be remplaced by what you put in there"), StorePage);

                        //preparing the paterns to feed in the search and remplace algo
                        searchPatterns = new string[] { @"/*AVATAR AUTHOR*/", @"/*StoreLink*/", @"/*AVATAR NAME*/", @"/*PACKAGE NAME*/", @"/*DIR PREFAB*/", @"/*DIR PATCHER*/" };
                        if (NameOverwriteTickBox) {
                            if (!string.IsNullOrEmpty(DistributionNameOverwriteText)) replacementValues = new string[] { CreatorName, StorePage, DistributionNameOverwriteText, PackageName, "Assets" + CommunFonctions.GetRelativePathToAssets(Path.Combine(CommunFonctions.GoUpNDirs(CustomfbxPath, 3), DistributionNameOverwriteText, "prefab")), EditorWindowPath};
                        }
                        else replacementValues = new string[] { CreatorName, StorePage, Path.GetFileNameWithoutExtension(OGfbxPath), PackageName, "Assets" + CommunFonctions.GetRelativePathToAssets(Path.Combine(CommunFonctions.GoUpNDirs(CustomfbxPath, 2), "prefab")), EditorWindowPath};

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
                        if (CommunFonctions.DirectoryContainsExe(patcherFolder))
                        {
                            CommunFonctions.DeleteAllFilesInDirectory(patcherFolder);//clears the directory of any python scripts remaining
                        }

                        if (StoreTextTog)//check if the user wants to searsh and remplace
                        {
                            CommunFonctions.ReplaceStringsAndCopyFiles(DescriptionDir, DestinationDir, searchPatterns, replacementValues);//remplaces text
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
                        default:
                            CommunFonctions.AddBoldCenteredLabel("something went wrong with the error code");
                            CommunFonctions.AddBoldCenteredLabel("it's not supposed to reach this value");
                            break;

                    }
                }



            }


            if (Debug) AddDebugInfo();
        
        }



        private void BuildPatcher()
        {
            //Check if output directory already exists
            if (!Directory.Exists(outputDirectory)){
                Directory.CreateDirectory(outputDirectory);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Windows", "hdiffz.exe");
                hpatchz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hpatchz", "Windows", "hpatchz.exe");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Linux", "hdiffz");
                hpatchz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hpatchz", "Linux", "hpatchz");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Mac", "hdiffz");
                hpatchz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hpatchz", "Mac", "hpatchz");
            }
            string hdiffzFolder = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hpatchz");
            FBXDiffFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + ".hdiff");
            MetaDiffFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + "Meta.hdiff");

            if (File.Exists(hdiffz))
            {

                // Launch the hdiff with the constructed arguments
                string arguments = "\"" + OGfbxPath + "\" \"" + CustomfbxPath + "\" \"" + FBXDiffFile + "\"";

                CommunFonctions.LaunchProgramWithArguments(hdiffz, arguments);
                //we do the same for the .meta file
                arguments = "\"" + OGfbxPath + ".meta\" \"" + CustomfbxPath + ".meta\" \"" + MetaDiffFile + "\"";
                CommunFonctions.LaunchProgramWithArguments(hdiffz, arguments);


                
            }
            else
            {
                UnityEngine.Debug.Log("please make sure you have hdiffz.exe in: " + hdiffz);//need to add to the list of errors                                                                 /!\ todo /!\
            }

            hpatchzDestDir = Path.Combine(CommunFonctions.GoUpNDirs(outputDirectory, 1), "hdiff", "hpatchz");
            Directory.CreateDirectory(hpatchzDestDir);

            PatcherTemplateScript = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "Editor", "PatcherTemplate.cs");

            if (NameOverwriteTickBox)
            {
                if (!string.IsNullOrEmpty(DistributionNameOverwriteText)) remplacementStrings = new string[] { CommunFonctions.GetRelativePathToAssets(OGfbxPath), CommunFonctions.GetRelativePathToAssets(CustomfbxPath), DistributionNameOverwriteText + "_Patcher", DistributionNameOverwriteText + "_Patcher", EditorWindowPath, UserEditorWindowName, PackageName, DistributionNameOverwriteText };
            }
            else PatcherScriptDestDir = Path.Combine(CommunFonctions.GoUpNDirs(outputDirectory, 2), "Editor", Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher.cs");

            //Check if patcher script destination director already exists
            if (!Directory.Exists(CommunFonctions.GoUpNDirs(PatcherScriptDestDir, 1))){
                Directory.CreateDirectory(CommunFonctions.GoUpNDirs(PatcherScriptDestDir, 1));
                CommunFonctions.CopyFolder(hdiffzFolder, hpatchzDestDir);
            }

            CommunFonctions.CheckAndCopyFileIfExists(PatcherTemplateScript, PatcherScriptDestDir);


            string[] originalStrings = new string[] { "\"YourCoolAvatar\"", "\"YourCoolCustomAvatar\"", "NameOfWindow", "PatcherTemplate", "Tools/Hash/EditDistributor/(DO_NOT_USE)", "DistributionCreator", "CurrentPackageVersion", "AvatarName" };
            if (NameOverwriteTickBox)
            {
                if (!string.IsNullOrEmpty(DistributionNameOverwriteText)) remplacementStrings = new string[] { CommunFonctions.MakePathPlatformAgnostic(CommunFonctions.GetRelativePathToAssets(OGfbxPath)), CommunFonctions.MakePathPlatformAgnostic(CommunFonctions.GetRelativePathToAssets(CustomfbxPath)), DistributionNameOverwriteText + "_Patcher", DistributionNameOverwriteText.Replace(" ", "_") + "_Patcher", EditorWindowPath, UserEditorWindowName, PackageName, DistributionNameOverwriteText };
            }
            else remplacementStrings = new string[] { CommunFonctions.MakePathPlatformAgnostic(CommunFonctions.GetRelativePathToAssets(OGfbxPath)), CommunFonctions.MakePathPlatformAgnostic(CommunFonctions.GetRelativePathToAssets(CustomfbxPath)), Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher", Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher", EditorWindowPath, UserEditorWindowName, PackageName, Path.GetFileNameWithoutExtension(OGfbxPath) };
            UnityEngine.Debug.Log(EditorWindowPath);
            CommunFonctions.ReplaceStringsInFile(PatcherScriptDestDir, originalStrings, remplacementStrings);

            UnityEngine.Debug.Log("Patcher created yippie :D");
            AssetDatabase.Refresh();
        }





        private void AddDebugInfo()
        {
            string outputDirectory = Path.Combine(CommunFonctions.GoUpNDirs(CustomfbxPath, 2), "Patcher", "Data", "DiffFiles");
            //some debug things that helped tracing back issues
            GUILayout.Label("outputDirectory: " + outputDirectory);
            GUILayout.Label("hdiff: " + Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "pythonscripts", "hdiff", "hdiffz.exe"));
            GUILayout.Label("hpatchz: " + Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "pythonscripts", "hdiff", "hpatchz.exe"));
            GUILayout.Label("FBXDiffFile: " + Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + ".hdiff"));
            GUILayout.Label("MetaDiffFile: " + Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + "Meta.hdiff"));


            GUILayout.Label("hpatchzDestDir: " + Path.Combine(CommunFonctions.GoUpNDirs(outputDirectory, 1), "hdiff", "hpatchz.exe"));
            GUILayout.Label("PatcherScriptDestDir" + Path.Combine(CommunFonctions.GoUpNDirs(outputDirectory, 2), "Editor", Path.GetFileNameWithoutExtension(CustomfbxPath) + "Patcher.cs"));
            GUILayout.Label("PatcherTemplateScript" + Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "Editor", "PatcherTemplate.cs"));

            GUILayout.Label(CommunFonctions.GetRelativePath(Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "Editor", "PatcherTemplate.cs"), OGfbxPath));

            EditorGUILayout.Space(10);
            GUILayout.Label(CommunFonctions.GetRelativePathToAssets(OGfbxPath));
            GUILayout.Label(CommunFonctions.GetRelativePathToAssets(CustomfbxPath));

            GUILayout.Label(Path.GetFileNameWithoutExtension(CustomfbxPath).Replace(" ", "_") + "_Patcher");
        }

    }
}
