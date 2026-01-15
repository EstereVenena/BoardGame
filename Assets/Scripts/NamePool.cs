using System.Collections.Generic;
using UnityEngine;

public static class NamePool
{
        public static readonly List<string> Names = new List<string>
    {
        "Kriss", "Roberts", "Artūrs", "Mārtiņš", "Rihards",
        "Jānis", "Edgars", "Andris", "Kristaps", "Toms",
        "Elīna", "Laura", "Anna", "Līga", "Dace",
        "Signe", "Ieva", "Marta", "Paula", "Zane", 
        "Toja", "Ramzess", "Jona", "Kārlis", "Vidmants", "Roberts",
         "Ilze"
    };

    public static List<string> GetShuffled(int seed)
    {
        var list = new List<string>(Names);
        var rng = new System.Random(seed);

        
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }
}
