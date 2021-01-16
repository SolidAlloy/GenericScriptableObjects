﻿namespace GenericUnityObjects.Editor.Util
{
    using System;
    using System.Collections.Generic;
    using GenericUnityObjects.Util;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// A class used to hold serialized values that need to survive assemblies reload. It is mainly used for asset
    /// creation, but also for MenuItem methods creation and Usage Example installation.
    /// </summary>
    internal class PersistentStorage : EditorOnlySingletonSO<PersistentStorage>, ICanBeInitialized
    {
        [SerializeField] private TypeReference _genericSOType;
        [SerializeField] private string _fileName;

        [SerializeField] private GameObject _gameObject;
        [SerializeField] private TypeReference _genericBehaviourType;

        [SerializeField] private MenuItemMethod[] _menuItemMethods = { };

        public static MenuItemMethod[] MenuItemMethods
        {
            get => Instance._menuItemMethods;
            set
            {
                Instance._menuItemMethods = value;
                EditorUtility.SetDirty(Instance);
            }
        }

        [SerializeField] private bool _usageExampleTypesAreAdded;

        public static bool UsageExampleTypesAreAdded
        {
            get => Instance._usageExampleTypesAreAdded;
            set
            {
                Instance._usageExampleTypesAreAdded = value;
                EditorUtility.SetDirty(Instance);
            }
        }

        [SerializeField] private List<string> _behaviourIconGUIDs = new List<string>();
        [SerializeField] private List<string> _scriptableObjectIconGUIDs = new List<string>();

        public static IEnumerable<string> BehaviourIconGUIDs => Instance._behaviourIconGUIDs;

        public static IEnumerable<string> ScriptableObjectIconGUIDs => Instance._scriptableObjectIconGUIDs;

        public static void AddAssemblyForIconChange<TObject>(string guid) where TObject : Object
        {
            Type objectType = typeof(TObject);

            if (objectType == typeof(MonoBehaviour))
            {
                Instance._behaviourIconGUIDs.Add(guid);
            }
            else if (objectType == typeof(GenericScriptableObject))
            {
                Instance._scriptableObjectIconGUIDs.Add(guid);
            }
            else
            {
                throw new ArgumentException(
                    $"Expected type parameter {nameof(MonoBehaviour)} or {nameof(GenericScriptableObject)}. Got {objectType} instead.");
            }

            EditorUtility.SetDirty(Instance);
        }

        public static void ClearIconChangeGUIDs()
        {
            Instance._behaviourIconGUIDs.Clear();
            Instance._scriptableObjectIconGUIDs.Clear();
            EditorUtility.SetDirty(Instance);
        }

        public static bool NeedsSOCreation => Instance._genericSOType?.Type != null;

        public static bool NeedsBehaviourCreation => Instance._genericBehaviourType?.Type != null;

        public static void SaveForAssemblyReload(Type genericTypeToCreate, string fileName)
        {
            Instance._genericSOType = genericTypeToCreate;
            Instance._fileName = fileName;
        }

        public static void SaveForAssemblyReload(GameObject gameObject, Type genericType)
        {
            Instance._gameObject = gameObject;
            Instance._genericBehaviourType = genericType;
        }

        public static (Type genericSOType, string fileName) GetGenericSODetails()
        {
            return (Instance._genericSOType, Instance._fileName);
        }

        public static (GameObject gameObject, Type genericBehaviourType) GetGenericBehaviourDetails()
        {
            return (Instance._gameObject, Instance._genericBehaviourType);
        }

        public static void Clear()
        {
            Instance._genericSOType = null;
            Instance._fileName = null;
            Instance._gameObject = null;
            Instance._genericBehaviourType = null;
        }

        public void Initialize() { }
    }
}