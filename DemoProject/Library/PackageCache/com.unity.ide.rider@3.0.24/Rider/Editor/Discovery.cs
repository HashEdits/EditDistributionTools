using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Win32;
using Packages.Rider.Editor.Util;
using Unity.CodeEditor;
using UnityEngine;

namespace Packages.Rider.Editor
{
  internal interface IDiscovery
  {
    CodeEditor.Installation[] PathCallback();
  }

  internal class Discovery : IDiscovery
  {
    public CodeEditor.Installation[] PathCallback()
    {
      var res = RiderPathLocator.GetAllRiderPaths()
        .Select(riderInfo => new CodeEditor.Installation
        {
          Path = riderInfo.Path,
          Name = riderInfo.Presentation
        })
        .ToList();

      var editorPath = RiderScriptEditor.CurrentEditor;
      if (RiderScriptEditor.IsRiderOrFleetInstallation(editorPath) &&
          !res.Any(a => a.Path == editorPath) &&
          FileSystemUtil.EditorPathExists(editorPath))
      {
        // External editor manually set from custom location
        var info = new RiderPathLocator.RiderInfo(editorPath, false);
        var installation = new CodeEditor.Installation
        {
          Path = info.Path,
          Name = info.Presentation
        };
        res.Add(installation);
      }

      return res.ToArray();
    }
  }

  /// <summary>
  /// This code is a modified version of the JetBrains resharper-unity plugin listed here:
  /// https://github.com/JetBrains/resharper-unity/blob/master/unity/JetBrains.Rider.Unity.Editor/EditorPlugin/RiderPathLocator.cs
  /// </summary>
  internal static class RiderPathLocator
  {
#if !(UNITY_4_7 || UNITY_5_5)
    public static RiderInfo[] GetAllRiderPaths()
    {
      try
      {
        switch (SystemInfo.operatingSystemFamily)
        {
          case OperatingSystemFamily.Windows:
          {
            return CollectRiderInfosWindows();
          }

          case OperatingSystemFamily.MacOSX:
          {
            return CollectRiderInfosMac();
          }

          case OperatingSystemFamily.Linux:
          {
            return CollectAllRiderPathsLinux();
          }
        }
      }
      catch (Exception e)
      {
        Debug.LogException(e);
      }

      return Array.Empty<RiderInfo>();
    }
#endif
    private static RiderInfo[] CollectAllRiderPathsLinux()
    {
      var installInfos = new List<RiderInfo>();
      var appsPath = GetAppsRootPathInToolbox();
      
      installInfos.AddRange(CollectToolbox20Linux(appsPath, "*rider*", "bin/rider.sh"));
      installInfos.AddRange(CollectToolbox20Linux(appsPath, "*fleet*", "bin/Fleet"));

      var riderRootPath = Path.Combine(appsPath, "Rider");
      installInfos.AddRange(CollectPathsFromToolbox(riderRootPath, "bin", "rider.sh", false)
        .Select(a => new RiderInfo(a, true)).ToList());
      
      var fleetRootPath = Path.Combine(appsPath, "Fleet");
      installInfos.AddRange(CollectPathsFromToolbox(fleetRootPath, "bin", "Fleet", false)
        .Select(a => new RiderInfo(a, true)).ToList());
      
      var home = Environment.GetEnvironmentVariable("HOME"); 
      if (!string.IsNullOrEmpty(home)) 
      {
        //$Home/.local/share/applications/jetbrains-rider.desktop
        var shortcut = new FileInfo(Path.Combine(home, @".local/share/applications/jetbrains-rider.desktop"));

        if (shortcut.Exists)
        {
          var lines = File.ReadAllLines(shortcut.FullName);
          foreach (var line in lines)
          {
            if (!line.StartsWith("Exec=\""))
              continue;
            var path = line.Split('"').Where((_, index) => index == 1).SingleOrDefault();
            if (string.IsNullOrEmpty(path))
              continue;

            if (installInfos.Any(a => a.Path == path)) // avoid adding similar build as from toolbox
              continue;
            installInfos.Add(new RiderInfo(path, false));
          }
        }
      }

      // snap install
      var snapInstallPath = "/snap/rider/current/bin/rider.sh";
      if (new FileInfo(snapInstallPath).Exists)
        installInfos.Add(new RiderInfo(snapInstallPath, false));
      
      return installInfos.ToArray();
    }

    private static IEnumerable<RiderInfo> CollectToolbox20Linux(string appsPath, string pattern, string relPath)
    {
      var result = new List<RiderInfo>();
      if (string.IsNullOrEmpty(appsPath) || !Directory.Exists(appsPath))
        return result;
      
      CollectToolbox20(appsPath, pattern, relPath, result);
      
      return result;
    }

    private static RiderInfo[] CollectRiderInfosMac()
    {
      var installInfos = new List<RiderInfo>();

      installInfos.AddRange(CollectFromApplications("*Rider*.app"));
      installInfos.AddRange(CollectFromApplications("*Fleet*.app"));

      var appsPath = GetAppsRootPathInToolbox();
      var riderRootPath = Path.Combine(appsPath, "Rider");
      installInfos.AddRange(CollectPathsFromToolbox(riderRootPath, "", "Rider*.app", true)
        .Select(a => new RiderInfo(a, true)));
      
      var fleetRootPath = Path.Combine(appsPath, "Fleet");
      installInfos.AddRange(CollectPathsFromToolbox(fleetRootPath, "", "Fleet*.app", true)
        .Select(a => new RiderInfo(a, true)));

      return installInfos.ToArray();
    }

    private static RiderInfo[] CollectFromApplications(string productMask)
    {
      var result = new List<RiderInfo>();
      var folder = new DirectoryInfo("/Applications");
      if (folder.Exists)
      {
        result.AddRange(folder.GetDirectories(productMask)
          .Select(a => new RiderInfo(a.FullName, false))
          .ToList());
      }

      var home = Environment.GetEnvironmentVariable("HOME");
      if (!string.IsNullOrEmpty(home))
      {
        var userFolder = new DirectoryInfo(Path.Combine(home, "Applications"));
        if (userFolder.Exists)
        {
          result.AddRange(userFolder.GetDirectories(productMask)
            .Select(a => new RiderInfo(a.FullName, false))
            .ToList());
        }
      }
      
      return result.ToArray();
    }

    private static RiderInfo[] CollectRiderInfosWindows()
    {
      var installInfos = new List<RiderInfo>();

      installInfos.AddRange(CollectToolbox20Windows("*Rider*", "bin/rider64.exe"));
      installInfos.AddRange(CollectToolbox20Windows("*Fleet*", "Fleet.exe"));

      var appsPath = GetAppsRootPathInToolbox();
      var riderRootPath = Path.Combine(appsPath, "Rider");
      installInfos.AddRange(CollectPathsFromToolbox(riderRootPath, "bin", "rider64.exe", false).ToList()
        .Select(a => new RiderInfo(a, true)).ToList());
      
      var fleetRootPath = Path.Combine(appsPath, "Fleet");
      installInfos.AddRange(CollectPathsFromToolbox(fleetRootPath, string.Empty, "Fleet.exe", false).ToList()
        .Select(a => new RiderInfo(a, true)).ToList());
      
      var installPaths = new List<string>();
      const string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
      CollectPathsFromRegistry(registryKey, installPaths);
      const string wowRegistryKey = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
      CollectPathsFromRegistry(wowRegistryKey, installPaths);
      
      installInfos.AddRange(installPaths.Select(a => new RiderInfo(a, false)).ToList());

      return installInfos.ToArray();
    }

    private static IEnumerable<RiderInfo> CollectToolbox20Windows(string pattern, string relPath)
    {
      var result = new List<RiderInfo>();
      var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      if (!string.IsNullOrEmpty(localAppData))
      {
        CollectToolbox20(Path.Combine(localAppData, "Programs"), pattern, relPath, result);
      }

      return result;
    }

    private static void CollectToolbox20(string dir, string pattern, string relPath, List<RiderInfo> result)
    {
      var directoryInfo = new DirectoryInfo(dir);
      if (!directoryInfo.Exists)
        return;
      
      foreach (var riderDirectory in directoryInfo.GetDirectories(pattern))
      {
        var executable = Path.Combine(riderDirectory.FullName, relPath);
       
        if (File.Exists(executable))
        {
          result.Add(new RiderInfo(executable, false)); // false, because we can't check if it is Toolbox or not anyway
        }
      }
    }

    private static string GetAppsRootPathInToolbox()
    {
      string localAppData = string.Empty;
      switch (SystemInfo.operatingSystemFamily)
      {
        case OperatingSystemFamily.Windows:
        {
          localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
          break;
        }

        case OperatingSystemFamily.MacOSX:
        {
          var home = Environment.GetEnvironmentVariable("HOME");
          if (!string.IsNullOrEmpty(home))
          {
            localAppData = Path.Combine(home, @"Library/Application Support");
          }
          break;
        }

        case OperatingSystemFamily.Linux:
        {
          var home = Environment.GetEnvironmentVariable("HOME");
          if (!string.IsNullOrEmpty(home))
          {
            localAppData = Path.Combine(home, @".local/share");
          }
          break;
        }
        default:
          throw new Exception("Unknown OS");
      }
      
      var toolboxPath = Path.Combine(localAppData, @"JetBrains/Toolbox");
      var settingsJson = Path.Combine(toolboxPath, ".settings.json");

      if (File.Exists(settingsJson))
      {
        var path = SettingsJson.GetInstallLocationFromJson(File.ReadAllText(settingsJson));
        if (!string.IsNullOrEmpty(path))
          toolboxPath = path;
      }

      return Path.Combine(toolboxPath, "apps");
    }

    internal static ProductInfo GetBuildVersion(string path)
    {
      var buildTxtFileInfo = new FileInfo(Path.Combine(path, GetRelativePathToBuildTxt()));
      var dir = buildTxtFileInfo.DirectoryName;
      if (!Directory.Exists(dir))
        return null;
      var buildVersionFile = new FileInfo(Path.Combine(dir, "product-info.json"));
      if (!buildVersionFile.Exists) 
        return null;
      var json = File.ReadAllText(buildVersionFile.FullName);
      return ProductInfo.GetProductInfo(json);
    }
    
    internal static Version GetBuildNumber(string path)
    {
      var buildTxtFileInfo = new FileInfo(Path.Combine(path, GetRelativePathToBuildTxt()));
      return GetBuildNumberWithBuildTxt(buildTxtFileInfo) ?? GetBuildNumberFromPath(path);
    }
    
    private static Version GetBuildNumberWithBuildTxt(FileInfo file)
    {
      if (!file.Exists) 
        return null;
      var text = File.ReadAllText(file.FullName);
      var index = text.IndexOf("-", StringComparison.Ordinal) + 1; // RD-191.7141.355
      if (index <= 0) 
        return null;
      
      var versionText = text.Substring(index);
      return Version.TryParse(versionText, out var v) ? v : null;
    }

    [CanBeNull]
    private static Version GetBuildNumberFromPath(string input)
    {
      if (string.IsNullOrEmpty(input))
        return null;

      var match = Regex.Match(input, @"(?<major>\d+)\.(?<minor>\d+)\.(?<build>\d+)");
      var groups = match.Groups;
      Version version = null;
      if (match.Success)
      {
        version = Version.Parse($"{groups["major"].Value}.{groups["minor"].Value}.{groups["build"].Value}");
      }

      return version;
    }

    public static bool GetIsToolbox(string path)
    {
      return Path.GetFullPath(path).StartsWith(Path.GetFullPath(GetAppsRootPathInToolbox()));
    }

    private static string GetRelativePathToBuildTxt()
    {
      switch (SystemInfo.operatingSystemFamily)
      {
        case OperatingSystemFamily.Windows: 
        case OperatingSystemFamily.Linux:
          return "../../build.txt";
        case OperatingSystemFamily.MacOSX:
          return "Contents/Resources/build.txt";
      }
      throw new Exception("Unknown OS");
    }
    private static void CollectPathsFromRegistry(string registryKey, List<string> installPaths)
    {
      using (var key = Registry.CurrentUser.OpenSubKey(registryKey))
      {
        CollectPathsFromRegistry(installPaths, key);
      }
      using (var key = Registry.LocalMachine.OpenSubKey(registryKey))
      {
        CollectPathsFromRegistry(installPaths, key);
      }
    }

    private static void CollectPathsFromRegistry(List<string> installPaths, RegistryKey key)
    {
      if (key == null) return;
      foreach (var subkeyName in key.GetSubKeyNames())
      {
        using (var subkey = key.OpenSubKey(subkeyName))
        {
          var folderObject = subkey?.GetValue("InstallLocation");
          if (folderObject == null) continue;
          var folder = folderObject.ToString();
          if (folder.Length == 0) continue;
          var displayName = subkey.GetValue("DisplayName");
          if (displayName == null) continue;
          if (displayName.ToString().Contains("Rider"))
          {
            try // possible "illegal characters in path"
            {
              var possiblePath = Path.Combine(folder, @"bin\rider64.exe"); 
              if (File.Exists(possiblePath))
                installPaths.Add(possiblePath);
            }
            catch (ArgumentException) { }  
          }
          else if (displayName.ToString().Contains("Fleet"))
          {
            try // possible "illegal characters in path"
            {
              var possiblePath = Path.Combine(folder, @"Fleet.exe"); 
              if (File.Exists(possiblePath))
                installPaths.Add(possiblePath);
            }
            catch (ArgumentException) { }  
          }
        }
      }
    }

    private static string[] CollectPathsFromToolbox(string productRootPathInToolbox, string dirName, string searchPattern,
      bool isMac)
    {
      if (!Directory.Exists(productRootPathInToolbox))
        return new string[0];

      var channelDirs = Directory.GetDirectories(productRootPathInToolbox);
      var paths = channelDirs.SelectMany(channelDir =>
        {
          try
          {
            // use history.json - last entry stands for the active build https://jetbrains.slack.com/archives/C07KNP99D/p1547807024066500?thread_ts=1547731708.057700&cid=C07KNP99D
            var historyFile = Path.Combine(channelDir, ".history.json");
            if (File.Exists(historyFile))
            {
              var json = File.ReadAllText(historyFile);
              var build = ToolboxHistory.GetLatestBuildFromJson(json);
              if (build != null)
              {
                var buildDir = Path.Combine(channelDir, build);
                var executablePaths = GetExecutablePaths(dirName, searchPattern, isMac, buildDir);
                if (executablePaths.Any())
                  return executablePaths;
              }
            }

            var channelFile = Path.Combine(channelDir, ".channel.settings.json");
            if (File.Exists(channelFile))
            {
              var json = File.ReadAllText(channelFile).Replace("active-application", "active_application");
              var build = ToolboxInstallData.GetLatestBuildFromJson(json);
              if (build != null)
              {
                var buildDir = Path.Combine(channelDir, build);
                var executablePaths = GetExecutablePaths(dirName, searchPattern, isMac, buildDir);
                if (executablePaths.Any())
                  return executablePaths;
              }
            }

            // changes in toolbox json files format may brake the logic above, so return all found installations
            return Directory.GetDirectories(channelDir)
              .SelectMany(buildDir => GetExecutablePaths(dirName, searchPattern, isMac, buildDir));
          }
          catch (Exception e)
          {
            // do not write to Debug.Log, just log it.
            Logger.Warn($"Failed to get path from {channelDir}", e);
          }

          return new string[0];
        })
        .Where(c => !string.IsNullOrEmpty(c))
        .ToArray();
      return paths;
    }

    private static string[] GetExecutablePaths(string dirName, string searchPattern, bool isMac, string buildDir)
    {
      var folder = new DirectoryInfo(Path.Combine(buildDir, dirName));
      if (!folder.Exists)
        return new string[0];

      if (!isMac)
        return new[] {Path.Combine(folder.FullName, searchPattern)}.Where(File.Exists).ToArray();
      return folder.GetDirectories(searchPattern).Select(f => f.FullName)
        .Where(Directory.Exists).ToArray();
    }

    // Disable the "field is never assigned" compiler warning. We never assign it, but Unity does.
    // Note that Unity disable this warning in the generated C# projects
#pragma warning disable 0649
    
    [Serializable]
    class SettingsJson
    {
      // ReSharper disable once InconsistentNaming
      public string install_location;
      
      [CanBeNull]
      public static string GetInstallLocationFromJson(string json)
      {
        try
        {
#if UNITY_4_7 || UNITY_5_5
          return JsonConvert.DeserializeObject<SettingsJson>(json).install_location;
#else
          return JsonUtility.FromJson<SettingsJson>(json).install_location;
#endif
        }
        catch (Exception)
        {
          Logger.Warn($"Failed to get install_location from json {json}");
        }

        return null;
      }
    }

    [Serializable]
    class ToolboxHistory
    {
      public List<ItemNode> history;

      [CanBeNull]
      public static string GetLatestBuildFromJson(string json)
      {
        try
        {
#if UNITY_4_7 || UNITY_5_5
          return JsonConvert.DeserializeObject<ToolboxHistory>(json).history.LastOrDefault()?.item.build;
#else
          return JsonUtility.FromJson<ToolboxHistory>(json).history.LastOrDefault()?.item.build;
#endif
        }
        catch (Exception)
        {
          Logger.Warn($"Failed to get latest build from json {json}");
        }

        return null;
      }
    }

    [Serializable]
    class ItemNode
    {
      public BuildNode item;
    }

    [Serializable]
    class BuildNode
    {
      public string build;
    }

    [Serializable]
    internal class ProductInfo
    {
      public string version;
      public string versionSuffix;

      [CanBeNull]
      internal static ProductInfo GetProductInfo(string json)
      {
        try
        {
          var productInfo = JsonUtility.FromJson<ProductInfo>(json);
          return productInfo;
        }
        catch (Exception)
        {
          Logger.Warn($"Failed to get version from json {json}");
        }

        return null;
      }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    [Serializable]
    class ToolboxInstallData
    {
      // ReSharper disable once InconsistentNaming
      public ActiveApplication active_application;

      [CanBeNull]
      public static string GetLatestBuildFromJson(string json)
      {
        try
        {
#if UNITY_4_7 || UNITY_5_5
          var toolbox = JsonConvert.DeserializeObject<ToolboxInstallData>(json);
#else
          var toolbox = JsonUtility.FromJson<ToolboxInstallData>(json);
#endif
          var builds = toolbox.active_application.builds;
          if (builds != null && builds.Any())
            return builds.First();
        }
        catch (Exception)
        {
          Logger.Warn($"Failed to get latest build from json {json}");
        }

        return null;
      }
    }

    [Serializable]
    class ActiveApplication
    {
      // ReSharper disable once InconsistentNaming
      public List<string> builds;
    }

#pragma warning restore 0649

    internal struct RiderInfo
    {
      public bool IsToolbox;
      public string Presentation;
      public Version BuildNumber;
      public ProductInfo ProductInfo;
      public string Path;

      public RiderInfo(string path, bool isToolbox)
      {
        if (path == RiderScriptEditor.CurrentEditor)
        {
          RiderScriptEditorData.instance.Init();
          BuildNumber = RiderScriptEditorData.instance.editorBuildNumber.ToVersion();
          ProductInfo = RiderScriptEditorData.instance.productInfo;
        }
        else
        {
          BuildNumber = GetBuildNumber(path);
          ProductInfo = GetBuildVersion(path);
        }

        var fileInfo = new FileInfo(path);
        var productName = RiderScriptEditor.GetProductNameForPresentation(fileInfo);  
        Path = fileInfo.FullName; // normalize separators
        var presentation = $"{productName} {BuildNumber}";

        if (ProductInfo != null && !string.IsNullOrEmpty(ProductInfo.version))
        {
          var suffix = string.IsNullOrEmpty(ProductInfo.versionSuffix) ? "" : $" {ProductInfo.versionSuffix}";
          presentation = $"{productName} {ProductInfo.version}{suffix}";
        }

        if (isToolbox)
          presentation += " (JetBrains Toolbox)";

        Presentation = presentation;
        IsToolbox = isToolbox;
      }
    }

    private static class Logger
    {
      internal static void Warn(string message, Exception e = null)
      {
#if RIDER_EDITOR_PLUGIN // can't be used in com.unity.ide.rider
        Log.GetLog(typeof(RiderPathLocator).Name).Warn(message);
        if (e != null) 
          Log.GetLog(typeof(RiderPathLocator).Name).Warn(e);
#else
        Debug.LogError(message);
        if (e != null)
          Debug.LogException(e);
#endif
      }
    }
  }
}