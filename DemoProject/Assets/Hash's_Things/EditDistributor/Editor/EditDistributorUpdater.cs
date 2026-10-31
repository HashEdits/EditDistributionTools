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
        private List<GameObject> CustomAvatarPrefabs = new List<GameObject>() { null };
        private string OGfbxPath = "";
        private string PackageName = "";
        string[] searchPatterns;

        private string updateSummary = "";

        private bool StoreTextTog = false;
        private string DescriptionDir = "";
        string DestinationDir = "";
        private string CreatorName = "";
        private string StorePage = "";


        private byte debugMessage = 0;

        private class CustomEntry
        {
            public GameObject Prefab;
            public string CustomfbxPath = "";
            public string PatcherToUpdate = "";
            public byte Status = 0; // 0 = empty slot, 1 = ready to update, 2 = fbx file not found, 3 = no patcher found
        }


        [MenuItem("Tools/Hash/EditDistributor/Updater")]
        private static void ShowWindow()
        {
            EditDistributorUpdater window = GetWindow<EditDistributorUpdater>("Updater");
            //if (!Debug) window.maxSize = new Vector2(442, 160);
            window.minSize = new Vector2(290, 220);
            window.maxSize = new Vector2(800, 700);
            window.Show();
        }

        private CustomEntry ComputeEntry(GameObject prefab)
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

            entry.PatcherToUpdate = FindPatcherScript(entry.CustomfbxPath);
            entry.Status = (!string.IsNullOrEmpty(entry.PatcherToUpdate) && File.Exists(entry.PatcherToUpdate)) ? (byte)1 : (byte)3;
            return entry;
        }

        void OnGUI()
        {
            GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            boldLabelStyle.fontSize = 14;

            List<CustomEntry> entries = new List<CustomEntry>();

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
                        }
                        else
                        {
                            OGfbxPath = "";
                        }
                    }

                    EditorGUILayout.Space(10);
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField(new GUIContent("Modified models ->", "Drag in one or more modified prefabs to update each one's existing patcher"));

                        int removeIndex = -1;
                        for (int i = 0; i < CustomAvatarPrefabs.Count; i++)
                        {
                            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                            {
                                CustomAvatarPrefabs[i] = (GameObject)EditorGUILayout.ObjectField(CustomAvatarPrefabs[i], typeof(GameObject), true);

                                CustomEntry entry = ComputeEntry(CustomAvatarPrefabs[i]);
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
                                        GUILayout.Label("no patcher found", GUILayout.Width(100));
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

                    //preparing the paterns to feed in the search and remplace algo (replacement values are computed per custom model)
                    searchPatterns = new string[] { @"/*AVATAR AUTHOR*/", @"/*StoreLink*/", @"/*AVATAR NAME*/", @"/*PACKAGE NAME*/", @"/*DIR PREFAB*/" };

                }
            }

            int readyCount = 0;
            foreach (CustomEntry entry in entries) if (entry.Status == 1) readyCount++;

            if (!File.Exists(OGfbxPath))
            {
                debugMessage = 1;
            }
            else if (string.IsNullOrEmpty(PackageName))
            {
                debugMessage = 3;
            }
            else if (readyCount == 0)
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
                    int updatedCount = 0;
                    int skippedMissingFbx = 0;
                    int skippedNoPatcher = 0;

                    foreach (CustomEntry entry in entries)
                    {
                        if (entry.Prefab == null) continue;

                        if (entry.Status == 2)
                        {
                            skippedMissingFbx++;
                            continue;
                        }
                        if (entry.Status == 3)
                        {
                            skippedNoPatcher++;
                            continue;
                        }

                        if (StoreTextTog)
                        {
                            string[] replacementValues = new string[] { CreatorName, StorePage, Path.GetFileNameWithoutExtension(OGfbxPath), PackageName, "Assets" + CommunFonctions.GetRelativePathToAssets(Path.Combine(CommunFonctions.GoUpNDirs(entry.CustomfbxPath, 2), "prefab")) };
                            string entrySubfolder = Path.GetFileNameWithoutExtension(entry.CustomfbxPath);
                            CommunFonctions.ReplaceStringsAndCopyFiles(DescriptionDir, Path.Combine(DestinationDir, entrySubfolder), searchPatterns, replacementValues);//remplaces text
                        }

                        UpdatingEntry(entry);
                        updatedCount++;
                    }

                    updateSummary = "Updated " + updatedCount + " patcher(s).";
                    if (skippedMissingFbx > 0) updateSummary += " Skipped " + skippedMissingFbx + " (file not found).";
                    if (skippedNoPatcher > 0) updateSummary += " Skipped " + skippedNoPatcher + " (no patcher found).";

                    AssetDatabase.Refresh();
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
                    case 3:
                        CommunFonctions.AddBoldCenteredLabel("Please Input a new package name");
                        break;
                    case 4:
                        CommunFonctions.AddBoldCenteredLabel("Fill the text fileds or untick tickbox");
                        break;
                    case 6:
                        CommunFonctions.AddBoldCenteredLabel("No patchers found to update");
                        CommunFonctions.AddBoldCenteredLabel("add a model, or use the builder tool");
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

            if (!string.IsNullOrEmpty(updateSummary))
            {
                EditorGUILayout.Space(10);
                CommunFonctions.AddBoldCenteredLabel(updateSummary);
            }
        }

        private void UpdatingEntry(CustomEntry entry)
        {
            string outputDirectory = Path.Combine(CommunFonctions.GoUpNDirs(entry.CustomfbxPath, 2), "Patcher", "Data", "DiffFiles");

            string hdiffz = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))    hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Windows", "hdiffz.exe");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))      hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Linux", "hdiffz");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))        hdiffz = Path.Combine(Environment.CurrentDirectory, "Assets", "Hash's_Things", "EditDistributor", "hdiff", "hdiffz", "Mac", "hdiffz");

            string FBXDiffFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + ".hdiff");
            string MetaDiffFile = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + "Meta.hdiff");

            if (File.Exists(hdiffz))
            {

                // Launch the hdiff with the constructed arguments
                // -f forces overwrite: without it, hdiffz refuses to touch an already-existing
                // .hdiff (left over from the Builder) and silently fails on every Update.
                string arguments = "-f \"" + OGfbxPath + "\" \"" + entry.CustomfbxPath + "\" \"" + FBXDiffFile + "\"";
                CommunFonctions.LaunchProgramWithArguments(hdiffz, arguments);
                //we do the same for the .meta file
                arguments = "-f \"" + OGfbxPath + ".meta\" \"" + entry.CustomfbxPath + ".meta\" \"" + MetaDiffFile + "\"";
                CommunFonctions.LaunchProgramWithArguments(hdiffz, arguments);



            }
            else
            {
                UnityEngine.Debug.Log("please make sure you have hdiffz.exe in: " + hdiffz);//need to add to the list of errors                                                                 /!\ todo /!\
            }

            string originalLine = @"    private static string AvatarVersion = ";
            string replacementLine = @"    private static string AvatarVersion = " + '"' + PackageName + '"' + ';';
            ReplaceLineInFile(entry.PatcherToUpdate, originalLine, replacementLine);
        }





        private static string FindPatcherScript(string customfbxPath)
        {
            string patcherEditorDir = Path.Combine(CommunFonctions.GoUpNDirs(customfbxPath, 2), "Patcher", "Editor");

            if (!Directory.Exists(patcherEditorDir))
            {
                return "";
            }

            string[] matches = Directory.GetFiles(patcherEditorDir, "*_Patcher.cs");

            return matches.Length > 0 ? matches[0] : "";
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
