using System.Collections.Generic;

namespace _Project.Utility_Scripts
{
    public static class ListUtility
    {
        public static List<T> Shuffle<T>(List<T> ts) 
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i) {
                var r = UnityEngine.Random.Range(i, count);
                (ts[i], ts[r]) = (ts[r], ts[i]);
            }
            return ts;
        }
    }
}
