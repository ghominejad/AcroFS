using Microsoft.Extensions.Internal;

using System;

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
            UtcNow += timeSpan;
        }
    }
}
