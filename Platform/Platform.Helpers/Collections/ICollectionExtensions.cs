﻿using System.Collections.Generic;
using System.Linq;

namespace Platform.Helpers.Collections
{
    public static class ICollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection) => collection == null || collection.Count == 0;

        public static bool AllEqualToDefault<T>(this ICollection<T> collection) => collection.All(item => Equals(item, default(T)));
    }
}
