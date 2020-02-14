using Cauca.EntityUpdater.Tests.EntityUpdaterTestMocks;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Cauca.EntityUpdater.Tests.EntityUpdater
{
    [TestFixture]
    public class EntityUpdaterCollectionHandlingTests : EntityUpdater<Child>
    {

        [Test]
        public void UpdateAllItemsInCollectionIsValid()
        {
            var friend1 = new Child { Id = Guid.NewGuid() };
            var friend2 = new Child { Id = Guid.NewGuid() };
            var childrenSource = new List<Child> { friend1, friend2 };
            var childrenDestination = new List<Child> { friend2, friend1 };

            UpdateCollection(childrenDestination, childrenSource);

            Assert.That(childrenDestination, Is.EquivalentTo(childrenSource));
        }
    }
}
