﻿namespace GenericUnityObjects
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Marks a GenericScriptableObject-derived type to be automatically listed in the Assets/Create submenu, so that
    /// instances of the type can be easily created and stored in the project as ".asset" files.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [BaseTypeRequired(typeof(GenericScriptableObject))]
    public class CreateGenericAssetMenuAttribute : Attribute
    {
        /// <summary>The default file name used by newly created instances of this type.</summary>
        [PublicAPI, NotNull] public string FileName = string.Empty;

        /// <summary>The display name for this type shown in the Assets/Create menu.</summary>
        [PublicAPI, NotNull] public string MenuName = string.Empty;

        /// <summary>The position of the menu item within the Assets/Create menu.</summary>
        [PublicAPI] public int Order = 0;
    }
}