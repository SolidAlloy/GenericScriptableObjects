﻿namespace GenericUnityObjects.Editor.Util
{
    using System;
    using GenericUnityObjects.Util;
    using TypeReferences;

    /// <summary>
    /// This attribute is applied to <see cref="TypeReferenceWithBaseTypes"/> so that they show only the types that
    /// are relevant to user. It filters out generic types, as well as setting a number of properties to preset values.
    /// </summary>
    internal class NonGenericAttribute : InheritsAttribute
    {
        private static readonly string[] _additionalAssemblies = { "Assembly-CSharp" };

        public NonGenericAttribute(Type[] baseTypes)
            : base(baseTypes)
        {
            ExcludeNone = true;
            IncludeAdditionalAssemblies = _additionalAssemblies;
            ShortName = true;

            // When type has inheritance constraints, there's usually only a few types available, so expanding all
            // folders will look nice.
            if (baseTypes != null && baseTypes.Length != 0)
                ExpandAllFolders = true;
        }

        internal override bool MatchesRequirements(Type type)
        {
            return base.MatchesRequirements(type) && ! type.IsGenericTypeDefinition;
        }
    }
}