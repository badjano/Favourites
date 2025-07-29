using System;

namespace FavouritesEd
{
    [Serializable]
    public class FavouritesElement
    {
        public int categoryId; // id of the Favourites category this is in
        public string objGUID; // the GUID of the object this is pointing to
        public string objPath; // the path of the object for fallback
    }
}