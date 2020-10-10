﻿namespace GenericScriptableObjects.Editor
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using GenericScriptableObjects;
    using SolidUtilities.Editor.EditorWindows;
    using SolidUtilities.Editor.Helpers;
    using TypeReferences;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;
    using UnityEngine.Assertions;
    using Assembly = System.Reflection.Assembly;

    [TypeDescriptionProvider(typeof(GenericSODescriptionProvider))]
    public class GenericSOCreator : SingletonScriptableObject<GenericSOCreator>
    {
        private const string GenericSOTypesPath = "Scripts/GenericScriptableObjectTypes";
        private const string NamespaceName = "GenericScriptableObjectsTypes";

        [SerializeField] private TypeReference _pendingCreationType; // TODO: hide in inspector
        [SerializeField] private TypeReference _genericSOType;

        [DidReloadScripts]
        private static void OnScriptsReload()
        {
            if (Instance._pendingCreationType.Type == null)
                return;

            try
            {
                string classSafeGenericTypeName = GetClassSafeTypeName(Instance._genericSOType.Type.Name);
                string typeName = GetClassSafeTypeName(GetTypeNameWithoutAssembly(Instance._pendingCreationType.Type.FullName));
                CreateAssetFromExistingType(Instance._genericSOType, Instance._pendingCreationType, classSafeGenericTypeName, typeName);
            }
            finally
            {
                Instance._pendingCreationType.Type = null;
                Instance._genericSOType = null;
            }
        }

        protected static void CreateAsset(Type genericSOType)
        {
            TypeSelectionWindow.Create(selectedParamType =>
            {
                string classSafeGenericTypeName = GetClassSafeTypeName(genericSOType.Name);
                string genericTypeNameWithoutParam = genericSOType.Name.Split('`')[0];
                string fullParamTypeName = GetTypeNameWithoutAssembly(selectedParamType.FullName);
                string classSafeParamTypeName = GetClassSafeTypeName(fullParamTypeName);

                if (GenericSODatabase.ContainsKey(genericSOType, selectedParamType))
                {
                    CreateAssetInteractively(genericSOType, selectedParamType, classSafeGenericTypeName, classSafeParamTypeName);
                    return;
                }

                string fullAssetPath = $"{Application.dataPath}/{GenericSOTypesPath}/Generic_{classSafeParamTypeName}.cs";

                string scriptContent = GetScriptContent(NamespaceName, classSafeGenericTypeName, classSafeParamTypeName,
                    genericSOType.Namespace, genericTypeNameWithoutParam, fullParamTypeName);

                if (FileContentMatches(fullAssetPath, scriptContent))
                {
                    CreateAssetFromExistingType(genericSOType, selectedParamType, classSafeGenericTypeName, classSafeParamTypeName);
                    return;
                }

                Instance._pendingCreationType = selectedParamType;
                Instance._genericSOType = genericSOType;
                AssetDatabaseHelper.MakeSureFolderExists(GenericSOTypesPath);
                File.WriteAllText(fullAssetPath, scriptContent);
                AssetDatabase.Refresh();
            });
        }

        private static bool FileContentMatches(string filePath, string contentToCompareTo)
        {
            if (File.Exists(filePath))
            {
                string oldFileContent = File.ReadAllText(filePath);
                if (oldFileContent == contentToCompareTo)
                    return true;
            }

            return false;
        }

        private static string GetScriptContent(
            string namespaceName,
            string classSafeGenericTypeName,
            string classSafeTypeName,
            string genericNamespace,
            string genericTypeNameWithoutParam,
            string type)
        {
            return $"namespace {namespaceName} {{ " +
                   $"public class {classSafeGenericTypeName}_{classSafeTypeName} : " +
                   $"{genericNamespace}.{genericTypeNameWithoutParam}<{type}> {{ }} }}";
        }

        private static void CreateAssetFromExistingType(Type genericSOType, Type selectedType, string classSafeGenericTypeName, string classSafeTypeName)
        {
            var csharpAssembly = Assembly.Load("Assembly-CSharp");
            Type assetType = csharpAssembly.GetType($"GenericScriptableObjectsTypes.Generic_{classSafeTypeName}");
            Assert.IsNotNull(assetType);
            GenericSODatabase.Add(genericSOType, selectedType, assetType);
            CreateAssetInteractively(genericSOType, selectedType, classSafeGenericTypeName, classSafeTypeName);
        }

        private static void CreateAssetInteractively(Type genericSOType, Type selectedType, string classSafeGenericTypeName, string classSafeTypeName)
        {
            var asset = GenericScriptableObject.Create(genericSOType, selectedType);
            Assert.IsNotNull(asset);
            AssetCreator.Create(asset, $"New {classSafeGenericTypeName}_{classSafeTypeName}.asset");
        }

        private static string GetClassSafeTypeName(string rawTypeName)
        {
            return rawTypeName
                .Replace('.', '_')
                .Replace('`', '_');
        }

        private static string GetTypeNameWithoutAssembly(string fullTypeName)
        {
            return fullTypeName.Split('[')[0];
        }
    }

    public class CustomSOCreator : GenericSOCreator
    {
        [CreateCustomAssetMenu("Custom Generic SO")]
        private static void CreateAsset()
        {
            CreateAsset(typeof(CustomGeneric<>));
        }
    }
}