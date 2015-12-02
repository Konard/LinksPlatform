using System;

namespace Platform.Helpers
{
    public static class RandomHelpers
    {
        public static readonly Random DefaultFactory = new Random((int)DateTime.UtcNow.Ticks);
    }
}
