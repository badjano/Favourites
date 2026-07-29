using System;

namespace FavouritesEd
{
    [Serializable]
    public class FavouritesCategory
    {
        public int id;
        public string name;
        public int parentCategoryId = -1;
    }
}