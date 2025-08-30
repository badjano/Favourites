using UnityEngine;
using System.Collections.Generic;
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

        public SavedSearch AddSavedSearch(string name, string query)
        {
            var search = Data.AddSavedSearch(name, query);
            SaveData();
            return search;
        }

        public void RemoveSavedSearch(int searchId)
        {
            Data.RemoveSavedSearch(searchId);
            SaveData();
        }

        public void TrackAssetAccess(Object obj)
        {
            if (obj != null)
            {
                Data.AddRecentAsset(obj);
                SaveData();
            }
        }

        public void TrackAssetAccessByPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;

#if UNITY_EDITOR
            var obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (obj != null)
            {
                TrackAssetAccess(obj);
            }
#endif
        }

        public List<RecentAsset> GetRecentAssets(int maxCount = 5)
        {
            return Data.GetRecentAssets(maxCount);
        }

        public void ClearRecentAssets()
        {
            Data.recentAssets.Clear();
            SaveData();
        }

        public Object GetObjectFromRecentAsset(RecentAsset asset)
        {
            if (asset == null) return null;

            // Check if we have any reference data
            if (string.IsNullOrEmpty(asset.objGUID) && string.IsNullOrEmpty(asset.objPath))
            {
                return null;
            }

#if UNITY_EDITOR
            // Try GUID first
            if (!string.IsNullOrEmpty(asset.objGUID))
            {
                var path = AssetDatabase.GUIDToAssetPath(asset.objGUID);
                if (!string.IsNullOrEmpty(path))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (obj != null) return obj;
                }
            }

            // Fallback to path if GUID doesn't work
            if (!string.IsNullOrEmpty(asset.objPath))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(asset.objPath);
                if (obj != null) return obj;
            }
#endif

            return null;
        }

        public SavedSearch GetSavedSearch(int searchId)
        {
            return Data.GetSavedSearch(searchId);
        }
    }
}