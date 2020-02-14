using System.Collections.Generic;

namespace Cauca.EntityUpdater.Tests.EntityUpdaterTestMocks
{
    public class Parent : BaseModel
    {
        public string ReadOnlyValue { get; } = "8";
        public string WriteOnlyValue { private get; set; }
        public FamilyMember FamilyMember { get; set; }
        public List<Friend> Friends { get; set; }
        public List<Child> Children { get; set; }
        public SomeEnum Enum { get; set; }
    }
}
