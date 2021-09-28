using UnityEngine;
using System.Runtime.CompilerServices;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * 
 * DESCRIPTION:
 *  This script is useful when creating plugins for unity, it will generate a configuration asset file in the hierarchy.
 *  If editorOnly is set to false, the asset file will be in a Resources folder and the Configuration class will be usable in build.
 *  A top bar button under Tools/{pluginName}/Configuration will allow the user to access the file quickly.
 * 
 * HOW TO USE:
 *  - Change the namespace to your plugin namespace
 *  - Change the different constants according to your plugin organization
 *  - Add new fields for the asset instance and new properties to access them from scripts, for example:
 *  
 *      [SerializeField] bool _booleanOption;
 *      
 *      /// <summary>Access to some boolean option...</summary>
 *      public static bool booleanOption => instance._booleanOption;
 * 
 *  - There should be no 'CHANGE_ME' marker left in the script (try using Ctrl+F to search for it)
 *  
 * SOURCE:
 *  https://github.com/Slyp05/Unity_EasyConfigurationFile
 *  Free to use in any project (even commercially) without having to credit the author.
 * 
 */
namespace CHANGE_ME
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class Configuration : ScriptableObject
    {
        ////////////////
        //            //
        //   CONSTS   //
        //            //
        ////////////////

        // name of the plugin
        const string pluginName = "CHANGE_ME";
        // path of this script relative to the root of the plugin
        const string folderExtraPath = "CHANGE_ME/Configuration.cs";
        // set this to true if you don't want the configuration to be saved in a Resources folder (it will be not be usuable in build)
        const bool editorOnly = false;

        ///////////////////////
        //                   //
        //   EDITOR FIELDS   //
        //                   //
        ///////////////////////

        [SerializeField] bool _booleanOption; // CHANGE_ME

        //////////////////////////////////
        //                              //
        //   PUBLIC STATIC PROPERTIES   //
        //                              //
        //////////////////////////////////

        /// <summary>Access to some boolean option...</summary>
        public static bool booleanOption => instance._booleanOption; // CHANGE_ME

#region Core
        /////////////////////////////////////////////////
        //                                             //
        //   YOU SHOULD NOT NEED TO CHANGE THIS PART   //
        //                                             //
        /////////////////////////////////////////////////

#if UNITY_EDITOR
        // top bar entry
        [MenuItem("Tools/" + pluginName + "/Configuration", priority = 0)]
        static void FocusConfiguration() => Selection.activeObject = asset;
#endif

        // consts
        const string configResourceName = "Configuration " + pluginName;
        const string configFileName = configResourceName + ".asset";

        const string invalidConfigurationPathFormat_folderPath = "Invalid path for configuration file: '{0}', do not change folder hierarchy";
        const string noConfigurationFileFormat_folderPath = "No configuration file found at '{0}', will be created automatically.";

        const string couldntLoadConfigurationStr = "Couldn't load " + pluginName + " configuration.";
        const string invalidFolderExtraPath = "Couldn't find '" + folderExtraPath + "'.";

        // public static properties
        /// <summary>Asset instance of the configuration.</summary>
        public static Object asset => instance;

        /// <summary>Folder of the plugin starting at Assets.</summary>
        public static string folderPath
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                return GetFolderPath();
            }
        }

        // private statics
        static string cachedFolderPath = null;

        static Configuration _instance = null;
        static Configuration instance
        {
#pragma warning disable CS0162
            get
            {
                if (_instance == null)
                {
                    #if UNITY_EDITOR
                    {
                        if (editorOnly)
                            _instance = AssetDatabase.LoadAssetAtPath<Configuration>(Path.Combine(folderPath, configFileName));
                        else
                            _instance = Resources.Load<Configuration>(configResourceName);
                    }
                    #else
                    {
                        _instance = Resources.Load<Configuration>(configResourceName);
                    }
                    #endif
                }

                if (_instance == null)
                {
                    #if UNITY_EDITOR
                    {
                        string targetFolder = (editorOnly ? folderPath : Path.Combine(folderPath, "Resources"));
                        string targetFile = Path.Combine(targetFolder, configFileName);

                        if (File.Exists(targetFile))
                            return CreateInstance(typeof(Configuration)) as Configuration;

                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                            AssetDatabase.Refresh();
                        }

                        Debug.LogWarning(string.Format(noConfigurationFileFormat_folderPath, targetFolder));
                        _instance = CreateInstance(typeof(Configuration)) as Configuration;
                        AssetDatabase.CreateAsset(_instance, targetFile);
                    }
                    #else
                    {
                        Debug.LogError(couldntLoadConfigurationStr);
                        _instance = CreateInstance(typeof(Configuration)) as Configuration;
                    }
                    #endif
                }

                return _instance;
            }
#pragma warning restore CS0162
        }

        static string GetFolderPath([CallerFilePath] string sourceFilePath = "")
        {
            if (cachedFolderPath == null)
            {
                sourceFilePath = sourceFilePath.Replace('\\', '/');

                string folderExtraPath = $"{(Configuration.folderExtraPath.StartsWith("/") ? "" : "/")}{Configuration.folderExtraPath}";
                int index = sourceFilePath.IndexOf(folderExtraPath);
                if (index < 0)
                {
                    Debug.LogError(invalidFolderExtraPath);
                    cachedFolderPath = string.Empty;
                }
                else
                {
                    sourceFilePath = sourceFilePath.Remove(index);
                    sourceFilePath = sourceFilePath.Substring(sourceFilePath.IndexOf("Assets"));
                    cachedFolderPath = sourceFilePath;
                }
            }

            return cachedFolderPath;
        }

#endregion
    }
}
