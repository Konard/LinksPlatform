using Platform.Helpers.Collections;

namespace Platform.Helpers
{
    public static class HashHelpers 
    {
        public static int Generate<T>(params T[] hashCodes) => hashCodes.GenerateHashCode();
    }
}