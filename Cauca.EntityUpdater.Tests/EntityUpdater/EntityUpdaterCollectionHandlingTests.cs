using Cause.CustomerPortal.ServiceLayer.Base;
using Cause.CustomerPortal.ServiceLayer.Tests.Mocks.EntityUpdaterTestMocks;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Cause.CustomerPortal.ServiceLayer.Tests.EntityUpdater
{
    [TestFixture]
    public class EntityUpdaterCollectionHandlingTests : EntityUpdater<Child>
    {

        [Test]
        public void UpdateAllItemsInCollectionIsValid()
        {
            var friend1 = new Child { Id = Guid.NewGuid().ToString() };
            var friend2 = new Child { Id = Guid.NewGuid().ToString() };
            var childrenSource = new List<Child> { friend1, friend2 };
            var childrenDestination = new List<Child> { friend2, friend1 };

            UpdateCollection(childrenDestination, childrenSource);

            Assert.That(childrenDestination, Is.EquivalentTo(childrenSource));
        }

        [Test]
        public void DeletedItemAreCorrectlyDeactivated()
        {
            var id1 = Guid.NewGuid().ToString();

            var friend1 = new Child { Id = id1 };

            var oldFriend1 = new Child { Id = id1 };
            var oldFriend2  = new Child { Id = Guid.NewGuid().ToString() };

            var childrenSource = new List<Child> { friend1 };
            var childrenDestination = new List<Child> { oldFriend1, oldFriend2 };

            UpdateCollection(childrenDestination, childrenSource);

            Assert.That(oldFriend1.IsActive, Is.True);
            Assert.That(oldFriend2.IsActive, Is.False);
        }

        [Test]
        public void DeleteChildAreCorrectlyDeactivated()
        {
            var id1 = Guid.NewGuid().ToString();

            var friend1 = new Child { Id = id1, Friends = new List<Friend> { } };
            var oldFriend1 = new Child { Id = id1, Friends = new List<Friend> { new Friend() } };

            var childrenSource = new List<Child> { friend1 };
            var childrenDestination = new List<Child> { oldFriend1 };

            UpdateCollection(childrenDestination, childrenSource);

            Assert.True(oldFriend1.Friends.TrueForAll(t => !t.IsActive));
        }
    }
}
