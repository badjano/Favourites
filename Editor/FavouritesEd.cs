using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace FavouritesEd
{
    public static class FavouritesEd
    {
        // This class is kept for backward compatibility but is no longer used
        // All favorites are now stored in the persistent data path via FavouritesManager

        [Obsolete("FavouritesEd.Containers is deprecated. Use FavouritesManager.Instance.Data instead.")]
        public static IEnumerable<FavouritesContainer> Containers => new List<FavouritesContainer>();

        [Obsolete("FavouritesEd.GetContainer is deprecated. Use FavouritesManager.Instance instead.")]
        public static FavouritesContainer GetContainer(Scene scene)
        {
            return null;
        }
    }
}