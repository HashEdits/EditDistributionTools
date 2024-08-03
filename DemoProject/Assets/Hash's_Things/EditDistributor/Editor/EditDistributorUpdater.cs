using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;

namespace EditDistributor
{

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
                            OGfbxPath = CommunFonctions.GetGameObjectPath(OGAvatarPrefab);

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
                            CustomfbxPath = CommunFonctions.GetGameObjectPath(CustomAvatarPrefab);

                            if (Debug) GUILayout.Label(CustomfbxPath);
                            if (string.IsNullOrEmpty(CustomfbxPath))
                            {
                                patcherToUpdate = "";
                            }
                            else
                            {
                                patcherToUpdate = Path.Combine(CommunFonctions.GoUpNDirs(CustomfbxPath, 2), "Patcher", "Editor", Path.GetFileNameWithoutExtension(CustomfbxPath) + "_Patcher.cs");
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

                    CommunFonctions.AddCenteredLabel("Will copy the selected folder and remplace");

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
                    EditorGUILayout.LabelField("Custom desc source: ", DescriptionDir);
                    EditorGUILayout.LabelField("Custom desc dest: ", DestinationDir);

                    CreatorName = EditorGUILayout.TextField("CreatorName ", CreatorName);

                    StorePage = EditorGUILayout.TextField("StorePage ", StorePage);

                    //preparing the paterns to feed in the search and remplace algo
                    searchPatterns = new string[] { @"/*AVATAR AUTHOR*/", @"/*StoreLink*/", @"/*AVATAR NAME*/", @"/*PACKAGE NAME*/", @"/*DIR PREFAB*/" };
                    replacementValues = new string[] { CreatorName, StorePage, Path.GetFileNameWithoutExtension(OGfbxPath), PackageName, "Assets" + CommunFonctions.GetRelativePathToAssets(Path.Combine(CommunFonctions.GoUpNDirs(CustomfbxPath, 2), "prefab")) };

                }
            }
            UnityEngine.Debug.Log("Patcher to update: " + patcherToUpdate);
            UnityEngine.Debug.Log("Does the patcher to update exists" + File.Exists(patcherToUpdate));


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
                        CommunFonctions.ReplaceStringsAndCopyFiles(DescriptionDir, DestinationDir, searchPatterns, replacementValues);//remplaces text
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
                        CommunFonctions.AddBoldCenteredLabel("Please Input a valid original model");
                        break;
                    case 2:
                        CommunFonctions.AddBoldCenteredLabel("Please Input a valid custom model");

                        break;
                    case 3:
                        CommunFonctions.AddBoldCenteredLabel("Please Input a new package name");
                        break;
                    case 4:
                        CommunFonctions.AddBoldCenteredLabel("Fill the text fileds or untick tickbox");
                        break;
                    case 6:
                        CommunFonctions.AddBoldCenteredLabel("A patcher doesn't already exists");
                        CommunFonctions.AddBoldCenteredLabel("please use the builder tool");
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
                    default:
                        CommunFonctions.AddBoldCenteredLabel("something went wrong with the error code");
                        break;
                }
            }
            if (debugMessage == 4)
            {
                CommunFonctions.AddBoldCenteredLabel("Patcher updated!");
            }
        }

        private void Updating()
        {
            string outputDirectory = Path.Combine(CommunFonctions.GoUpNDirs(CustomfbxPath, 2), "Patcher", "Data", "DiffFiles");


            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))    hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Windows", "hdiffz.exe");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))      hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Linux", "hdiffz");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))        hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Mac", "hdiffz");
            
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

            originalStrings = @"    private static string AvatarVersion = ";
            remplacementStrings = @"    private static string AvatarVersion = " + '\u0022' + PackageName + '\u0022' + ';';
            ReplaceLineInFile(patcherToUpdate, originalStrings, remplacementStrings);
        }



        

        private static void ReplaceLineInFile(string filePath, string searchStartsWith, string replacementLine)
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

        
    }
}
