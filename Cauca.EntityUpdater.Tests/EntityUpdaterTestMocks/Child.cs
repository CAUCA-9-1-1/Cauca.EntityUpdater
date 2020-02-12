using Cause.CustomerPortal.Models.Base;
using System.Collections.Generic;

namespace Cause.CustomerPortal.ServiceLayer.Tests.Mocks.EntityUpdaterTestMocks
{
    public class Child : BaseModelWithActiveFields
    {
        public List<Friend> Friends { get; set; }
    }
}
