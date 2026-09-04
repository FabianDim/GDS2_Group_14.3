using System.Collections.Generic;

namespace _Project.Utility_Scripts
{
    public static class ListUtility
    {
        public static List<T> Shuffle<T>(List<T> ls) 
        {
            var list = new List<T>(ls);
            var count = list.Count;
            var last = count - 1;
            
            for (var i = 0; i < last; ++i) {
                var r = UnityEngine.Random.Range(i, count);
                (list[i], list[r]) = (list[r], list[i]);
            }
            return list;
        }
    }
}
