using System;
using System.Collections.Generic;
using UnityEngine;

namespace FavouritesEd
{
    public class FavouritesContainer : MonoBehaviour
    {
        // This class is kept for backward compatibility but is no longer used
        // All favorites are now stored in the persistent data path via FavouritesManager
        [Obsolete("FavouritesContainer is deprecated. Use FavouritesManager.Instance instead.")]
        public List<FavouritesElement> favs = new();
    }
}