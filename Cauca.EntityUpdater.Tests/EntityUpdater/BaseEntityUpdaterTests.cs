using Cause.CustomerPortal.ServiceLayer.Base;
using Cause.CustomerPortal.ServiceLayer.Tests.Mocks.EntityUpdaterTestMocks;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Cauca.EntityUpdater.Tests.EntityUpdater
{
    [TestFixture]
    public class BaseEntityUpdaterTests : EntityUpdater<Parent>
    {
        [Test]
        public void CollectionIsCorrectlyDetectedAsNavigationProperty()
        {
            var property = typeof(Parent).GetProperty(nameof(Parent.Friends));
            Assert.That(PropertiesUpdater.IsNavigationProperty(typeof(Parent), property), Is.True);
        }

        [Test]
        public void OneToOneRelationIsCorrectlyDetectedAsNavigationProperty()
        {
            var property = typeof(Parent).GetProperty(nameof(Parent.FamilyMember));
            Assert.That(PropertiesUpdater.IsNavigationProperty(typeof(Parent), property), Is.True);
        }

        [Test]
        public void EnumTypeIsNotDetectedAsNavigationProperty()
        {
            var property = typeof(Parent).GetProperty(nameof(Parent.Enum));
            Assert.That(PropertiesUpdater.IsNavigationProperty(typeof(Parent), property), Is.False);
        }

        [Test]
        public void GetNonNavigationWritablePropertiesDoesNotReturnsReadOnlyProperty()
        {
            var properties = PropertiesUpdater.GetNonNavigationWritableProperties(typeof(Parent));
            Assert.That(properties, Does.Not.Contains(nameof(Parent.ReadOnlyValue)));
        }

        [Test]
        public void GetNonNavigationWritablePropertiesDoesNotReturnsCollections()
        {
            var properties = PropertiesUpdater.GetNonNavigationWritableProperties(typeof(Parent));
            Assert.That(properties, Does.Not.Contains(nameof(Parent.Children)).And.Not.Contains(nameof(Parent.Friends)));
        }

        [Test]
        public void GetNonNavigationWritablePropertiesDoesContainsOneToOneRelation()
        {
            var properties = PropertiesUpdater.GetNonNavigationWritableProperties(typeof(Parent));
            Assert.That(properties, Does.Not.Contains(nameof(Parent.FamilyMember)));
        }

        [Test]
        public void UpdateValuesCorrectlyUpdateAllFieldThatAreNotReadOnlyOrNavigation()
        {
            var familyMemberId = Guid.NewGuid().ToString();
            var id1 = Guid.NewGuid().ToString();
            var parent1 = new Parent { Id = id1, Enum = SomeEnum.Value1, IsActive = false, FamilyMember = new FamilyMember() { Id = familyMemberId } };
            var parent2 = new Parent { Id = Guid.NewGuid().ToString(), Enum = SomeEnum.Value2, IsActive = true, FamilyMember = new FamilyMember() };

            PropertiesUpdater.UpdateValues(parent1, parent2);

            bool shouldBeEqual = parent1.Id == parent2.Id && parent1.Enum == parent2.Enum && parent1.IsActive == parent2.IsActive;
            bool shouldNotBeEqual = parent1.FamilyMember.Id != parent2.FamilyMember.Id;

            Assert.That(shouldBeEqual && shouldNotBeEqual, Is.True);
        }

        [Test]
        public void GetCollectionByPropertyNameCorrectlyReturnsAnICollectionForSpecificValueWhenItExists()
        {
            var friend1 = new Child { Id = Guid.NewGuid().ToString() };
            var friend2 = new Child { Id = Guid.NewGuid().ToString() };
            var children = new List<Child> { friend1, friend2 };
            var parent = new Parent { Id = Guid.NewGuid().ToString(), Children = children };

            Assert.That(GetCollectionByPropertyName(parent, nameof(Parent.Children)), Is.EquivalentTo(children));
        }

        [Test]
        public void EntityIsNotInListCorrectlyReturnsTrueWhenEntityIsNotInList()
        {
            var friend1 = new Child { Id = Guid.NewGuid().ToString() };
            var friend2 = new Child { Id = Guid.NewGuid().ToString() };
            var children = new List<Child> { friend2 };

            Assert.That(EntityIsNotInList(children, friend1), Is.True);
        }

        [Test]
        public void EntityIsNotInListCorrectlyReturnsFalseWhenEntityIsInList()
        {
            var friend1 = new Child { Id = Guid.NewGuid().ToString() };
            var children = new List<Child> { friend1 };

            Assert.That(EntityIsNotInList(children, friend1), Is.False);
        }
    }
}
