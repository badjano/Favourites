using System.Collections.Generic;
using UnityEngine;

namespace FavouritesEd
{
    public class FavouritesAsset : ScriptableObject
    {
        public List<FavouritesElement> favs = new();
        public List<FavouritesCategory> categories = new();
        [SerializeField] private int nextCategoryId;

        public FavouritesCategory AddCategory(string name)
        {
            var c = new FavouritesCategory
            {
                id = nextCategoryId,
                name = name
            };

            nextCategoryId++;
            categories.Add(c);

            return c;
        }
    }
}