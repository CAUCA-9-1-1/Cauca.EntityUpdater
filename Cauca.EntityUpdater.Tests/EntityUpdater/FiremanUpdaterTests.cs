using Cause.CustomerPortal.Models.ComputerAidedDispatch.Firemen;
using Cause.CustomerPortal.Models.ComputerAidedDispatch.FireStations;
using Cause.CustomerPortal.ServiceLayer.Base;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cause.CustomerPortal.ServiceLayer.Tests.EntityUpdater
{
    [TestFixture]
    public class FiremanUpdaterTests : EntityUpdater<Fireman>
    {
        [Test]
        public void UpdateFiremanFireStations()
        {
            var id = Guid.NewGuid().ToString();
            var currentFireman = new Fireman { Id = id };
            currentFireman.FireStationFiremen.Add(new FireStationFireman());
            currentFireman.FireStationFiremen.Add(new FireStationFireman());
            currentFireman.FireStationFiremen.Add(new FireStationFireman());

            var newFireman = new Fireman { Id = id };

            var newList = new List<Fireman>() { newFireman } as IList;
            var oldList = new List<Fireman>() { currentFireman } as IList;

            UpdateFromList(newList, oldList);
            Assert.True(oldList.OfType<Fireman>().All(fireman => fireman.FireStationFiremen.All(c => !c.IsActive)));
        }
    }
}
