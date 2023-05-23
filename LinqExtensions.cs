using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace BlockyMapGen {
    public static class LinqExtensions {

        [CanBeNull]
        public static T Random<T>(this IEnumerable<T> enumerable) {
            var lenum = enumerable as List<T> ?? enumerable.ToList();
            if (lenum.Count == 0) throw new ArgumentException( "enumerable must have at least one element" );
            return lenum[UnityEngine.Random.Range( 0, lenum.Count )];
        }

        [CanBeNull]
        public static T Random<T>(this IEnumerable<T> enumerable, Func<T, float> chanceValuePredicate) {
            var lenum = enumerable as List<T> ?? enumerable.ToList();
            if (lenum.Count == 0) throw new ArgumentException( "enumerable must have at least one element" );
            var s = UnityEngine.Random.Range(0, lenum.Sum( chanceValuePredicate ));
            foreach (var element in lenum) {
                s -= chanceValuePredicate( element );
                if (s <= 0) return element;
            }
            return lenum[0];
        }

        public static bool Any<T>(this IEnumerable<T> enumerable, Func<T, int, bool> predicate) {
            var i = 0;
            foreach (var item in enumerable) {
                if (predicate( item, i )) return true;
                i++;
            }
            return false;
        }
    }
}