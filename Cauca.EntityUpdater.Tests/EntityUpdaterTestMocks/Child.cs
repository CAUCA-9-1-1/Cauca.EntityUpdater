using System.Collections.Generic;

namespace Cauca.EntityUpdater.Tests.EntityUpdaterTestMocks
{
    public class Child : BaseModel
    {
        public List<Friend> Friends { get; set; }
    }
}
