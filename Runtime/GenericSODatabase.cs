﻿namespace GenericScriptableObjects
{
    using System;
    using System.Collections.Generic;
    using TypeReferences;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    public class GenericSODatabase :
        SingletonScriptableObject<GenericSODatabase>,
        ISerializationCallbackReceiver
    {
        private readonly Dictionary<TypeReference, TypeDictionary> _dict =
            new Dictionary<TypeReference, TypeDictionary>(new TypeReferenceComparer());

        [SerializeField] [HideInInspector] private TypeReference[] _keys;
        [SerializeField] [HideInInspector] private TypeDictionary[] _values;

        public static void Add(Type genericType, Type[] key, Type value)
        {
            TypeDictionary assetDict = GetAssetDict(genericType);
            assetDict.Add(key, value);
            EditorUtility.SetDirty(Instance);
        }

        public static bool ContainsKey(Type genericType, Type[] key)
        {
            TypeDictionary assetDict = GetAssetDict(genericType);
            return assetDict.ContainsKey(key);
        }

        public static bool TryGetValue(Type genericType, out Type value)
        {
            var paramTypes = genericType.GetGenericArguments();
            Assert.IsFalse(paramTypes.Length == 0);
            Type genericTypeWithoutParams = genericType.GetGenericTypeDefinition();
            TypeDictionary assetDict = GetAssetDict(genericTypeWithoutParams);
            bool result = assetDict.TryGetValue(paramTypes, out value);
            return result;
        }

        public void OnAfterDeserialize()
        {
            if (_keys == null || _values == null || _keys.Length != _values.Length)
                return;

            _dict.Clear();
            int keysLength = _keys.Length;

            for (int i = 0; i < keysLength; ++i)
                _dict[_keys[i]] = _values[i];

            _keys = null;
            _values = null;
        }

        public void OnBeforeSerialize()
        {
            int dictLength = _dict.Count;
            _keys = new TypeReference[dictLength];
            _values = new TypeDictionary[dictLength];

            int keysIndex = 0;
            foreach (var pair in _dict)
            {
                _keys[keysIndex] = pair.Key;
                _values[keysIndex] = pair.Value;
                ++keysIndex;
            }
        }

        private static TypeDictionary GetAssetDict(Type genericType)
        {
            if (Instance._dict.TryGetValue(genericType, out TypeDictionary assetDict))
                return assetDict;

            assetDict = new TypeDictionary();
            Instance._dict.Add(genericType, assetDict);
            EditorUtility.SetDirty(Instance);
            return assetDict;
        }
    }
}