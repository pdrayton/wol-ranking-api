using System.Collections.Generic;

namespace WoLRankingApi {
    public static class Extensions {
        public static IEnumerable<T> AsNotNull<T>(this IEnumerable<T> original) {
            return original ?? new T[0];
        } 
    }
}