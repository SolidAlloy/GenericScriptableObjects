﻿namespace GenericUnityObjects.EditorTests
{
    using System.Linq;
    using Editor.GeneratedTypesDatabase;
    using NUnit.Framework;

    internal partial class GenerationDatabaseTests
    {
        public class UpdateGenericTypeGUID : GenerationDatabaseTests
        {
            private const string NewGUID = "newGUID";
            private static GenericTypeInfo _expectedBehaviour;

            [SetUp]
            public override void BeforeEachTest()
            {
                base.BeforeEachTest();
                AddEntries();
                _expectedBehaviour = new GenericTypeInfo(_genericType.TypeNameAndAssembly, NewGUID, _genericType.ArgNames);
            }

            [Test]
            public void Updates_behaviour_GUID_in_behaviours_list()
            {
                _database.UpdateGenericTypeGUIDImpl(_genericType, NewGUID);

                Assert.IsTrue(_database.InstanceGenericTypes.Length == 1);
                Assert.Contains(_expectedBehaviour, _database.InstanceGenericTypes);
            }

            [Test]
            public void Updates_behaviour_GUID_in_referenced_behaviours()
            {
                _database.UpdateGenericTypeGUIDImpl(_genericType, NewGUID);

                var behaviours = _database.GetReferencedGenericTypesImpl(_firstArg);

                Assert.IsTrue(behaviours.Any(behaviour => behaviour.GUID == NewGUID));
            }

            [Test]
            public void Concrete_classes_can_be_found_by_new_behaviour()
            {
                _database.UpdateGenericTypeGUIDImpl(_genericType, NewGUID);

                var concreteClasses = _database.GetConcreteClassesImpl(_genericType);

                Assert.IsTrue(concreteClasses.Length != 0);
            }

            [Test]
            public void Updates_passed_argument_GUID()
            {
                _database.UpdateGenericTypeGUIDImpl(_genericType, NewGUID);
                Assert.IsTrue(_genericType.GUID == NewGUID);
            }
        }
    }
}