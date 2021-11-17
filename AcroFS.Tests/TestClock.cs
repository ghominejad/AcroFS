using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcroFS.Tests
{
    public class TestClock : ISystemClock
    {
        public TestClock()
        {
            UtcNow = new DateTime(2013, 6, 15, 12, 34, 56, 789);
        }

        public DateTimeOffset UtcNow { get; set; }

        public void Add(TimeSpan timeSpan)
        {
            UtcNow = UtcNow + timeSpan;
        }
    }
}
