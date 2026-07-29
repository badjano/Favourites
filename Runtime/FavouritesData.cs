using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FavouritesEd
{
    [Serializable]
    public class SavedSearch
    {
        public string name;
        public string query;
        public int id;

        public SavedSearch()
        {
            // Required for JsonUtility serialization
        }

        public SavedSearch(string name, string query, int id)
        {
            this.name = name;
            this.query = query;
            this.id = id;
        }
    }

    [Serializable]
    public class RecentAsset
    {
        public string objGUID;
        public string objPath;
        public long lastAccessTime;
        public int accessCount;

        public RecentAsset()
        {
            // Required for JsonUtility serialization
        }

        public RecentAsset(string objGUID, string objPath)
        {
            this.objGUID = objGUID;
            this.objPath = objPath;
            this.lastAccessTime = DateTime.Now.Ticks;
            this.accessCount = 1;
        }

        public void UpdateAccess()
        {
            lastAccessTime = DateTime.Now.Ticks;
            accessCount++;
        }
    }

    [Serializable]
    public class FavouritesData
    {
        private const int CurrentVersion = 1;

        public List<FavouritesElement> favs = new();
        public List<FavouritesCategory> categories = new();
        public List<SavedSearch> savedSearches = new();
        public List<RecentAsset> recentAssets = new();
        public List<int> expandedCategoryIds = new();
        public int nextCategoryId;
        public int nextSearchId;
        public int version = CurrentVersion;

        private static string DataPath => Path.Combine(Application.persistentDataPath, "FavouritesData.json");

        public static FavouritesData Load()
        {
            if (File.Exists(DataPath))
                try
                {
                    var json = File.ReadAllText(DataPath);
                    var data = JsonUtility.FromJson<FavouritesData>(json) ?? new FavouritesData();
                    data.EnsureValidData();
                    return data;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load favourites data: {e.Message}");
                }

            return new FavouritesData();
        }

        public void Save()
        {
            try
            {
                EnsureValidData();
                var json = JsonUtility.ToJson(this, true);
                File.WriteAllText(DataPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save favourites data: {e.Message}");
            }
        }

        public FavouritesCategory AddCategory(string name, int parentCategoryId = -1)
        {
            var c = new FavouritesCategory
            {
                id = nextCategoryId,
                name = name,
                parentCategoryId = GetCategory(parentCategoryId) == null ? -1 : parentCategoryId
            };

            nextCategoryId++;
            categories.Add(c);

            return c;
        }

        public void RemoveCategory(int categoryId)
        {
            var categoryIdsToRemove = new HashSet<int> { categoryId };
            var foundChild = true;

            while (foundChild)
            {
                foundChild = false;
                foreach (var category in categories)
                {
                    if (!categoryIdsToRemove.Contains(category.parentCategoryId) ||
                        categoryIdsToRemove.Contains(category.id))
                        continue;

                    categoryIdsToRemove.Add(category.id);
                    foundChild = true;
                }
            }

            categories.RemoveAll(c => categoryIdsToRemove.Contains(c.id));
            favs.RemoveAll(f => categoryIdsToRemove.Contains(f.categoryId));
            expandedCategoryIds.RemoveAll(categoryIdsToRemove.Contains);
        }

        public void RenameCategory(int categoryId, string newName)
        {
            var category = categories.Find(c => c.id == categoryId);
            if (category != null)
            {
                category.name = newName;
            }
        }

        public FavouritesCategory GetCategory(int categoryId)
        {
            return categories.Find(c => c.id == categoryId);
        }

        public FavouritesCategory FindCategory(string name, int parentCategoryId)
        {
            return categories.Find(c =>
                c.parentCategoryId == parentCategoryId &&
                string.Equals(c.name, name, StringComparison.OrdinalIgnoreCase));
        }

        public void MoveCategory(int categoryId, int parentCategoryId)
        {
            var category = GetCategory(categoryId);
            if (category == null || categoryId == parentCategoryId) return;
            if (parentCategoryId >= 0 && GetCategory(parentCategoryId) == null) return;

            var ancestorId = parentCategoryId;
            while (ancestorId >= 0)
            {
                if (ancestorId == categoryId) return;
                var ancestor = GetCategory(ancestorId);
                if (ancestor == null) break;
                ancestorId = ancestor.parentCategoryId;
            }

            category.parentCategoryId = parentCategoryId;
        }

        public SavedSearch AddSavedSearch(string name, string query)
        {
            var search = new SavedSearch(name, query, nextSearchId);
            nextSearchId++;
            savedSearches.Add(search);
            return search;
        }

        public void RemoveSavedSearch(int searchId)
        {
            savedSearches.RemoveAll(s => s.id == searchId);
        }

        public SavedSearch GetSavedSearch(int searchId)
        {
            return savedSearches.Find(s => s.id == searchId);
        }

        public void AddRecentAsset(Object obj)
        {
            if (obj == null) return;

            var guid = "";
            var path = "";

#if UNITY_EDITOR
            guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            path = AssetDatabase.GetAssetPath(obj);
#endif

            if (string.IsNullOrEmpty(guid) && string.IsNullOrEmpty(path)) return;

            // Check if already exists
            var existingAsset = recentAssets.Find(ra => ra.objGUID == guid || ra.objPath == path);
            if (existingAsset != null)
            {
                existingAsset.UpdateAccess();
            }
            else
            {
                var recentAsset = new RecentAsset(guid, path);
                recentAssets.Add(recentAsset);
            }

            // Keep only the 5 most recently accessed assets
            if (recentAssets.Count > 5)
            {
                recentAssets.Sort((a, b) => b.lastAccessTime.CompareTo(a.lastAccessTime));
                recentAssets.RemoveRange(5, recentAssets.Count - 5);
            }
        }

        public List<RecentAsset> GetRecentAssets(int maxCount = 5)
        {
            // Sort by last access time (most recent first) and return up to maxCount
            recentAssets.Sort((a, b) => b.lastAccessTime.CompareTo(a.lastAccessTime));
            return recentAssets.GetRange(0, Math.Min(maxCount, recentAssets.Count));
        }

        public void AddFavourite(Object obj, int categoryId)
        {
            if (obj == null) return;

            var guid = "";
            var path = "";

#if UNITY_EDITOR
            guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            path = AssetDatabase.GetAssetPath(obj);
#endif

            // Check if already exists
            if (favs.Exists(f => f.objGUID == guid && f.categoryId == categoryId))
                return;

            var element = new FavouritesElement
            {
                categoryId = categoryId,
                objGUID = guid,
                objPath = path
            };

            favs.Add(element);
        }

        public void RemoveFavourite(Object obj, int categoryId)
        {
            var guid = "";
#if UNITY_EDITOR
            guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
#endif
            favs.RemoveAll(f => f.objGUID == guid && f.categoryId == categoryId);
        }

        public List<FavouritesElement> GetFavouritesInCategory(int categoryId)
        {
            return favs.FindAll(f => f.categoryId == categoryId);
        }

        public Object GetObjectFromElement(FavouritesElement element)
        {
            if (element == null)
                return null;

            // Check if we have any reference data
            if (string.IsNullOrEmpty(element.objGUID) && string.IsNullOrEmpty(element.objPath))
            {
                return null;
            }

#if UNITY_EDITOR
            // Try GUID first
            if (!string.IsNullOrEmpty(element.objGUID))
            {
                var path = AssetDatabase.GUIDToAssetPath(element.objGUID);
                if (!string.IsNullOrEmpty(path))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                    if (obj != null) return obj;
                }
            }

            // Fallback to path if GUID doesn't work
            if (!string.IsNullOrEmpty(element.objPath))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(element.objPath);
                if (obj != null) return obj;
            }
#endif

            return null;
        }

        private void EnsureValidData()
        {
            favs ??= new List<FavouritesElement>();
            categories ??= new List<FavouritesCategory>();
            savedSearches ??= new List<SavedSearch>();
            recentAssets ??= new List<RecentAsset>();
            expandedCategoryIds ??= new List<int>();

            if (version < CurrentVersion)
            {
                foreach (var category in categories)
                    category.parentCategoryId = -1;
                version = CurrentVersion;
            }

            var highestCategoryId = -1;
            foreach (var category in categories)
            {
                highestCategoryId = Math.Max(highestCategoryId, category.id);
                if (category.parentCategoryId == category.id || GetCategory(category.parentCategoryId) == null)
                    category.parentCategoryId = -1;
            }

            foreach (var category in categories)
            {
                var visitedCategoryIds = new HashSet<int> { category.id };
                var ancestorId = category.parentCategoryId;

                while (ancestorId >= 0)
                {
                    if (!visitedCategoryIds.Add(ancestorId))
                    {
                        category.parentCategoryId = -1;
                        break;
                    }

                    var ancestor = GetCategory(ancestorId);
                    if (ancestor == null) break;
                    ancestorId = ancestor.parentCategoryId;
                }
            }

            nextCategoryId = Math.Max(nextCategoryId, highestCategoryId + 1);
            expandedCategoryIds.RemoveAll(id => GetCategory(id) == null);
        }
    }
}