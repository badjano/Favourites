using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FavouritesEd
{
    public class FavouritesManager
    {
        private static FavouritesManager _instance;

        private FavouritesManager()
        {
            LoadData();
        }

        public static FavouritesManager Instance
        {
            get
            {
                if (_instance == null) _instance = new FavouritesManager();
                return _instance;
            }
        }

        public FavouritesData Data { get; private set; }

        public void LoadData()
        {
            Data = FavouritesData.Load();
        }

        public void RefreshData()
        {
            LoadData();
        }

        public void SaveData()
        {
            Data?.Save();
        }

        public FavouritesCategory AddCategory(string name)
        {
            var category = Data.AddCategory(name);
            SaveData();
            return category;
        }

        public void RemoveCategory(int categoryId)
        {
            Data.RemoveCategory(categoryId);
            SaveData();
        }

        public void RenameCategory(int categoryId, string newName)
        {
            Data.RenameCategory(categoryId, newName);
            SaveData();
        }

        public void AddFavourite(Object obj, int categoryId)
        {
            Data.AddFavourite(obj, categoryId);
            SaveData();
        }

        public void RemoveFavourite(Object obj, int categoryId)
        {
            Data.RemoveFavourite(obj, categoryId);
            SaveData();
        }

        public bool IsFavourite(Object obj, int categoryId)
        {
            var guid = "";
#if UNITY_EDITOR
            guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
#endif
            return Data.favs.Exists(f => f.objGUID == guid && f.categoryId == categoryId);
        }

        public Object GetObjectFromElement(FavouritesElement element)
        {
            return Data.GetObjectFromElement(element);
        }
    }
}