﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Runtime.InteropServices;

public class PatcherTemplate : EditorWindow
{
    private static bool Debug = false;
    private static string OGfbxPath = "YourCoolAvatar";
    private static string CustomfbxPath = "YourCoolCustomAvatar";
    private byte debugMessage = 0;
    private static string AvatarVersion = @"CurrentPackageVersion";
    private bool PatchButtonHasBeenPressed = false;

    [MenuItem(@"Tools/Hash/EditDistributor/(DO_NOT_USE)")]
    public static void ShowWindow()
    {
        PatcherTemplate window = GetWindow<PatcherTemplate>(@"NameOfWindow");
        if (!Debug) window.maxSize = new Vector2(442, 223);
        if (Debug) window.maxSize = new Vector2(1000, 700);
        window.Show();
    }

    void OnGUI()
    {

        GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            
            EditorGUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(@"DistributionCreator" + "'s "+ @"AvatarName", boldLabelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            string currentOGfbxPath = Path.Combine(Application.dataPath, OGfbxPath);

            string currentCustomfbxPath = Path.Combine(Application.dataPath, CustomfbxPath);

            string hpatchz = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) hpatchz = Path.Combine(GoUpNDirs(currentCustomfbxPath, 2), "patcher", "data", "hdiff", "hpatchz", "Windows", "hpatchz.exe");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) hpatchz = Path.Combine(GoUpNDirs(currentCustomfbxPath, 2), "patcher", "data", "hdiff", "hpatchz", "Linux", "hpatchz");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) hpatchz = Path.Combine(GoUpNDirs(currentCustomfbxPath, 2), "patcher", "data", "hdiff", "hpatchz", "Mac", "hpatchz");

            string FBXDiffFile = Path.Combine(GoUpNDirs(currentCustomfbxPath, 2), "patcher", "data", "DiffFiles", Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + ".hdiff");
            string MetaDiffFile = Path.Combine(GoUpNDirs(currentCustomfbxPath, 2), "patcher", "data", "DiffFiles", Path.GetFileNameWithoutExtension(OGfbxPath).Replace(" ", "_") + "Meta.hdiff");
            string fbxfolder = GoUpNDirs(currentCustomfbxPath, 1);

            if (Debug) ShowDebugMessages(currentOGfbxPath, FBXDiffFile, MetaDiffFile, currentCustomfbxPath);



            EditorGUILayout.Space(20);
            if (!File.Exists(currentOGfbxPath))
            {
                AddBoldCenteredLabel("Please make sure your have the original FBX in: " + GoUpNDirs(OGfbxPath, 1));
                debugMessage = 1;
            }
            else if (File.Exists(currentCustomfbxPath))
            {
                AddBoldCenteredLabel("Edited model is already in the folder Enjoy :)");
                debugMessage = 2;
            }
            else if (!(File.Exists(FBXDiffFile) && File.Exists(MetaDiffFile)))
            {
                AddBoldCenteredLabel("Please make sure your have diff files in: " + GoUpNDirs(FBXDiffFile, 1));
                debugMessage = 3;
            }
            else if (PatchButtonHasBeenPressed)
            {
                if (debugMessage == 4)
                {

                }
                else
                {
                    debugMessage = 0;
                    PatchButtonHasBeenPressed = false;
                }

            }
            else
            {
                debugMessage = 0;
            }


            if (debugMessage == 0 || debugMessage == 4)
            {
                if (GUILayout.Button("Patch"))
                {
                    PatchButtonHasBeenPressed = true;
                    if (Directory.Exists(fbxfolder))
                    {
                        if (Debug) UnityEngine.Debug.Log("fbx exists");
                    }
                    else
                    {
                        Directory.CreateDirectory(fbxfolder);
                    }

                    string arguments = "\"" + currentOGfbxPath + ".meta\" \"" + MetaDiffFile + "\" \"" + currentCustomfbxPath + ".meta\"";
                    if (Debug) UnityEngine.Debug.Log("meta Arguments: " + arguments);
                    if (LaunchProgramWithArguments(hpatchz, arguments, currentCustomfbxPath + ".meta") == true)
                    {
                        debugMessage = 5;
                        //communicate to the user that something went wrong during the patching process of the meta file

                    }
                    else
                    {
                        debugMessage = 6;//it workey
                        DeleteAllFilesInDirectory(Path.Combine(GoUpNDirs(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 1), "LocalLow", "VRChat", "VRChat", "OSC"));

                    }

                    arguments = "\"" + currentOGfbxPath + "\" \"" + FBXDiffFile + "\" \"" + currentCustomfbxPath + "\"";
                    if (Debug) UnityEngine.Debug.Log("FBX Arguments: " + arguments);
                    if (LaunchProgramWithArguments(hpatchz, arguments, currentCustomfbxPath) == true)
                    {
                        debugMessage = 4;
                        //communicate to the user that something went wrong during the patching process of the FBX
                    }
                    else
                    {
                        debugMessage = 6;//it workey
                        DeleteAllFilesInDirectory(Path.Combine(GoUpNDirs(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 1), "LocalLow", "VRChat", "VRChat", "OSC"));
                    }



                    AssetDatabase.Refresh();


                }
            }



            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {

                switch (debugMessage)
                {
                    case 0:
                        //please click the button
                        AddBoldCenteredLabel("Please click the button above to patch your model :)");
                        EditorGUILayout.Space(10);
                        AddBoldCenteredLabel("Please double check that you used " + AvatarVersion);
                        AddBoldCenteredLabel("to import the original avatar in your project");
                        break;
                    case 4:
                        //fbx fucked up
                        AddBoldCenteredLabel("/!\\ Something went wrong during the patching of the FBX /!\\ ");
                        EditorGUILayout.Space(10);
                        AddBoldCenteredLabel("Please double check that you used " + AvatarVersion);
                        AddBoldCenteredLabel("to import the original avatar in your project");
                        break;
                    case 5:
                        //meta fucked up
                        AddBoldCenteredLabel("/!\\ Something went wrong during the patching of the import file /!\\ ");
                        EditorGUILayout.Space(10);
                        AddBoldCenteredLabel("Please double check that you used " + AvatarVersion + "to import the original avatar in your project");
                        break;
                    case 6:
                        AddBoldCenteredLabel("FBX patched, get to the prefab folder to enjoy your product");
                        break;
                    default:
                        break;


                }

            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("This package is using Hash's Edit Distributor", EditorStyles.linkLabel))
                {
                    Application.OpenURL("https://github.com/HashEdits/Face-Tracking-Patcher");
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

    }


    private static string GoUpNDirs(string MyDir, int n)
    {
        MyDir = MyDir.TrimEnd(Path.PathSeparator);
        for (var i = 0; i < n; i++)
            MyDir = Path.GetDirectoryName(MyDir);
        return MyDir;
    }


    private static bool LaunchProgramWithArguments(string programPath, string arguments, string destinationFile)
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
            string output = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log(output);
            if (!File.Exists(destinationFile))
            {
                return true;
            }
            process.WaitForExit();

            return false;

        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e.Message);
            //SaveStringToFile(e.Message, " ", Application.dataPath + "/Logs", "Error crash " + Path.GetFileNameWithoutExtension(CustomfbxPath) + " Patcher  " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log");
            return true;
        }
    }

    private static bool SaveStringToFile(string inputString, string searchString, string saveDirectory, string fileName)
    {
        if (inputString.Contains(searchString))
        {
            try
            {
                // Ensure the directory exists
                Directory.CreateDirectory(saveDirectory);

                // Combine the directory and filename to get the full path
                string filePath = Path.Combine(saveDirectory, fileName);

                // Write the input string to the file
                File.WriteAllText(filePath, inputString);

                if (Debug) UnityEngine.Debug.Log("Something went wrong, please get in contact and provide: " + filePath + "/" + fileName);
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log($"Error saving string to file: {e.Message}");
                return true;
            }
        }
        else
        {
            return false;
        }

    }

    private void AddBoldCenteredLabel(string mymessage)
    {
        EditorGUILayout.LabelField(mymessage, EditorStyles.boldLabel);
    }

    private void ShowDebugMessages(string currentOGfbxPath, string FBXDiffFile, string MetaDiffFile, string currentCustomfbxPath)
    {
        GUILayout.Label("OGfbxPath: " + currentOGfbxPath);
        GUILayout.Label("OGfbxPath exists: " + File.Exists(currentOGfbxPath));

        GUILayout.Label("FBXDiffFile: " + FBXDiffFile);
        GUILayout.Label("MetaDiffFile: " + MetaDiffFile);

        GUILayout.Label("FBXDiffFile exists: " + File.Exists(FBXDiffFile));
        GUILayout.Label("MetaDiffFile exists: " + File.Exists(MetaDiffFile));
        EditorGUILayout.Space(10);
        GUILayout.Label("arguments fed to hpatchz: " + currentOGfbxPath + "\" \"" + FBXDiffFile + "\" \"" + currentCustomfbxPath + "\"");
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
}
