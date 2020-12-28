namespace GenericUnityObjects.Editor
{
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using MonoBehaviour;
    using NUnit.Framework;
    using ScriptableObject;
    using UnityEditor;
    using UnityEngine;
    using Util;

    [InitializeOnLoad]
    internal static class LogListener
    {
        private static readonly Regex PathRegex = new Regex(@"^.*?(?=\()", RegexOptions.Compiled);

        private static readonly Regex NamespaceRegex = new Regex(@"(?<=namespace\s').*?(?=')", RegexOptions.Compiled);

        private static readonly Regex ParentTypeRegex = new Regex(@"(?<=namespace name ').*?(?=')", RegexOptions.Compiled);

        static LogListener() => Application.logMessageReceived += OnLogMessageReceived;

        private static void OnLogMessageReceived(string message, string _, LogType logType)
        {
            if ( ! MissingTypeError(logType, message))
                return;

            string failedScriptPath = PathRegex.Match(message).Value;
            if (failedScriptPath.Length == 0)
                return;

            string classSafeParentTypeName = GetClassSafeParentTypeName(message);
            if (classSafeParentTypeName == null)
                return;

            bool isMenuItemsFile = IsMenuItemsFile(failedScriptPath);
            bool isAutoGeneratedClass = IsAutoGeneratedClass(failedScriptPath);

            if ( ! isMenuItemsFile && ! isAutoGeneratedClass)
                return;

            if (isAutoGeneratedClass)
                DeleteAsset(failedScriptPath);

            RemoveMenuItemMethod(message, classSafeParentTypeName);

            // RemoveMenuItemMethod refreshes the asset database if the method is found, but even if it is not found,
            // the asset is deleted, so the asset database needs to be refreshed.
            AssetDatabase.Refresh();
        }

        private static bool MissingTypeError(LogType logType, string message) =>
            logType == LogType.Error && message.Contains("error CS0234");

        private static string GetClassSafeParentTypeName(string message)
        {
            string parentTypeName = ParentTypeRegex.Match(message).Value;
            Assert.IsNotEmpty(parentTypeName);

            int genericArgsCount = parentTypeName.IndexOf('>') - parentTypeName.IndexOf('<');

            if (genericArgsCount == 0)
                return null;

            string parentTypeWithoutGenericArgs = parentTypeName.Split('<')[0];

            return $"{parentTypeWithoutGenericArgs}_{genericArgsCount}";
        }

        private static bool IsMenuItemsFile(string path)
        {
            return path == Config.MenuItemsPath;
        }

        private static bool IsAutoGeneratedClass(string path) =>
            path.Contains(Config.GeneratedTypesPath);

        private static void DeleteAsset(string path)
        {
            File.Delete(path);
            File.Delete($"{path}.meta");
        }

        private static void RemoveMenuItemMethod(string message, string parentTypeName)
        {
            string namespaceName = NamespaceRegex.Match(message).Value;
            Assert.IsNotEmpty(namespaceName);

            string fullTypeName = $"{namespaceName.Replace('.', '_')}_{parentTypeName}";
            MenuItemsGenerator.RemoveMethod(fullTypeName);
        }

        private static void ClearConsoleOnConcreteClass(string message, LogType logType)
        {
            if (logType != LogType.Error)
                return;

            if (message ==
                $"'{AssemblyCreator.ConcreteClassName}' is missing the class attribute 'ExtensionOfNativeClass'!")
            {
                RemoveLogsByMode(2); // Removes "'{AssemblyCreator.ConcreteClassName}' is missing the class attribute 'ExtensionOfNativeClass'!"
                RemoveLogsByMode(512); // Removes "GameObject (named 'Test Object') references runtime script in scene file. Fixing!"
            }
        }

        private static void RemoveLogsByMode(int mode)
        {
            var assembly = Assembly.GetAssembly(typeof(Editor));
            var type = assembly.GetType("UnityEditor.LogEntry");
            var method = type.GetMethod("RemoveLogEntriesByMode", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method);
            method.Invoke(new object(), new object[] { mode });
        }
    }
}
