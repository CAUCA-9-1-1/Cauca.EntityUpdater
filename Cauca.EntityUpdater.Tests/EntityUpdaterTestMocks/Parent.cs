using Cause.CustomerPortal.Models.Base;
using System.Collections.Generic;

namespace Cause.CustomerPortal.ServiceLayer.Tests.Mocks.EntityUpdaterTestMocks
{
    public enum SomeEnum
    {
        Value1,
        Value2
    }

    public class Parent : BaseModelWithActiveFields
    {
        public string ReadOnlyValue { get; } = "8";
        public string WriteOnlyValue { private get; set; }
        public FamilyMember FamilyMember { get; set; }
        public List<Friend> Friends { get; set; }
        public List<Child> Children { get; set; }
        public SomeEnum Enum { get; set; }
    }
}
