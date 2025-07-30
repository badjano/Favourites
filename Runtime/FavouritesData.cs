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
    public class FavouritesData
    {
        public List<FavouritesElement> favs = new();
        public List<FavouritesCategory> categories = new();
        public int nextCategoryId;

        private static string DataPath => Path.Combine(Application.persistentDataPath, "FavouritesData.json");

        public static FavouritesData Load()
        {
            if (File.Exists(DataPath))
                try
                {
                    var json = File.ReadAllText(DataPath);
                    return JsonUtility.FromJson<FavouritesData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load favourites data: {e.Message}");
                }

            // Return default data if file doesn't exist or loading failed
            return new FavouritesData();
        }

        public void Save()
        {
            try
            {
                var json = JsonUtility.ToJson(this, true);
                File.WriteAllText(DataPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save favourites data: {e.Message}");
            }
        }

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

        public void RemoveCategory(int categoryId)
        {
            categories.RemoveAll(c => c.id == categoryId);
            favs.RemoveAll(f => f.categoryId == categoryId);
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
                Debug.LogWarning($"FavouritesElement has no valid reference data - Category: {element.categoryId}");
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

                    Debug.LogWarning(
                        $"Failed to load asset at path '{path}' for GUID '{element.objGUID}' in category {element.categoryId}");
                }
                else
                {
                    Debug.LogWarning(
                        $"GUID '{element.objGUID}' could not be resolved to a valid asset path in category {element.categoryId}");
                }
            }

            // Fallback to path if GUID doesn't work
            if (!string.IsNullOrEmpty(element.objPath))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(element.objPath);
                if (obj != null) return obj;

                Debug.LogWarning($"Failed to load asset at path '{element.objPath}' in category {element.categoryId}");
            }
#endif

            Debug.LogWarning(
                $"Could not load object for favourite in category {element.categoryId} - GUID: '{element.objGUID}', Path: '{element.objPath}'");
            return null;
        }
    }
}