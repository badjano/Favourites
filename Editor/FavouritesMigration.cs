using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FavouritesEd
{
    public static class FavouritesMigration
    {
        private static string GetCategoryName(FavouritesAsset asset, int categoryId)
        {
            foreach (var category in asset.categories)
                if (category.id == categoryId)
                    return category.name;

            return "Unknown";
        }

        [MenuItem("Tools/Favourites/Migrate from ScriptableObject")]
        public static void MigrateFromScriptableObject()
        {
            // Find existing ScriptableObject assets
            var guids = AssetDatabase.FindAssets("t:FavouritesAsset");

            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Migration", "No FavouritesAsset found to migrate.", "OK");
                return;
            }

            var migrated = false;
            var totalCategories = 0;
            var totalFavourites = 0;
            var successfulFavourites = 0;
            var skippedInvalidEntries = 0;
            var invalidEntries = new List<string>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var oldAsset = AssetDatabase.LoadAssetAtPath<FavouritesAsset>(path);

                if (oldAsset != null)
                {
                    // Migrate categories
                    foreach (var category in oldAsset.categories)
                    {
                        FavouritesManager.Instance.AddCategory(category.name);
                        totalCategories++;
                    }

                    // Migrate favourites - use the old data structure directly
                    foreach (var element in oldAsset.favs)
                    {
                        totalFavourites++;

                        // Validate the element before attempting to load
                        // Invalid entries can occur when:
                        // 1. The asset was deleted from the project
                        // 2. The asset was moved/renamed and the references weren't updated
                        // 3. There was a data corruption issue
                        if (string.IsNullOrEmpty(element.objGUID) && string.IsNullOrEmpty(element.objPath))
                        {
                            skippedInvalidEntries++;

                            var categoryName = GetCategoryName(oldAsset, element.categoryId);
                            var invalidEntry =
                                $"Category '{categoryName}' (ID: {element.categoryId}): Missing both GUID and Path";
                            invalidEntries.Add(invalidEntry);

                            continue;
                        }

                        // Try to load the object using the old element's GUID and path
                        Object obj = null;

                        if (!string.IsNullOrEmpty(element.objGUID))
                        {
                            var assetPath = AssetDatabase.GUIDToAssetPath(element.objGUID);
                            if (!string.IsNullOrEmpty(assetPath))
                            {
                                obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                            }
                        }

                        // Fallback to path if GUID doesn't work
                        if (obj == null && !string.IsNullOrEmpty(element.objPath))
                        {
                            obj = AssetDatabase.LoadAssetAtPath<Object>(element.objPath);
                        }

                        if (obj != null)
                        {
                            FavouritesManager.Instance.AddFavourite(obj, element.categoryId);
                            successfulFavourites++;
                        }
                        else
                        {
                            skippedInvalidEntries++;

                            var categoryName = GetCategoryName(oldAsset, element.categoryId);
                            var invalidEntry =
                                $"Category '{categoryName}' (ID: {element.categoryId}): GUID '{element.objGUID}', Path '{element.objPath}' - Asset not found";
                            invalidEntries.Add(invalidEntry);

                        }
                    }

                    migrated = true;
                }
            }

            if (migrated)
            {
                var message = "Migration Complete!\n\n" +
                              $"Categories migrated: {totalCategories}\n" +
                              $"Favourites found: {totalFavourites}\n" +
                              $"Favourites successfully migrated: {successfulFavourites}\n" +
                              $"Failed to migrate: {skippedInvalidEntries}\n\n";

                if (skippedInvalidEntries > 0)
                {
                    message += "Invalid entries were skipped (likely deleted/moved assets):\n";
                    foreach (var invalidEntry in invalidEntries) message += $"• {invalidEntry}\n";
                    message += "\n";
                    message += "These invalid entries typically occur when:\n";
                    message += "• Assets were deleted from the project\n";
                    message += "• Assets were moved/renamed and references weren't updated\n";
                    message += "• There was a data corruption issue\n\n";
                    message +=
                        "You can use 'Tools > Favourites > Validate ScriptableObject Data' to get more details.\n\n";
                }

                message += "The old ScriptableObject assets can now be safely deleted.";

                EditorUtility.DisplayDialog("Migration Complete", message, "OK");

                // Force a refresh of the favourites window if it's open
                var window = EditorWindow.GetWindow<FavouritesEdWindow>();
                if (window != null) window.UpdateTreeview();
            }
        }

        [MenuItem("Tools/Favourites/Validate ScriptableObject Data")]
        public static void ValidateScriptableObjectData()
        {
            // Find existing ScriptableObject assets
            var guids = AssetDatabase.FindAssets("t:FavouritesAsset");

            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Validation", "No FavouritesAsset found to validate.", "OK");
                return;
            }

            var validationResults = new List<string>();
            var totalAssets = 0;
            var totalCategories = 0;
            var totalFavourites = 0;
            var validFavourites = 0;
            var invalidFavourites = 0;
            var missingBothGUIDAndPath = 0;
            var invalidGUIDs = 0;
            var invalidPaths = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var oldAsset = AssetDatabase.LoadAssetAtPath<FavouritesAsset>(path);

                if (oldAsset != null)
                {
                    totalAssets++;
                    totalCategories += oldAsset.categories.Count;
                    totalFavourites += oldAsset.favs.Count;

                    validationResults.Add($"Asset: {path}");
                    validationResults.Add($"  Categories: {oldAsset.categories.Count}");
                    validationResults.Add($"  Favourites: {oldAsset.favs.Count}");

                    foreach (var category in oldAsset.categories)
                        validationResults.Add($"    Category: {category.name} (ID: {category.id})");

                    foreach (var element in oldAsset.favs)
                    {
                        var hasGUID = !string.IsNullOrEmpty(element.objGUID);
                        var hasPath = !string.IsNullOrEmpty(element.objPath);
                        var guidValid = false;
                        var pathValid = false;

                        if (hasGUID)
                        {
                            var assetPath = AssetDatabase.GUIDToAssetPath(element.objGUID);
                            guidValid = !string.IsNullOrEmpty(assetPath);
                            if (!guidValid) invalidGUIDs++;
                        }

                        if (hasPath)
                        {
                            pathValid = AssetDatabase.LoadAssetAtPath<Object>(element.objPath) != null;
                            if (!pathValid) invalidPaths++;
                        }

                        var categoryName = GetCategoryName(oldAsset, element.categoryId);

                        if (!hasGUID && !hasPath)
                        {
                            missingBothGUIDAndPath++;
                            validationResults.Add(
                                $"    ❌ Invalid: Category '{categoryName}' (ID: {element.categoryId}) - Missing both GUID and Path");
                        }
                        else if (hasGUID && !guidValid && hasPath && !pathValid)
                        {
                            invalidFavourites++;
                            validationResults.Add(
                                $"    ❌ Invalid: Category '{categoryName}' (ID: {element.categoryId}) - GUID '{element.objGUID}' and Path '{element.objPath}' both invalid");
                        }
                        else if (hasGUID && !guidValid && !hasPath)
                        {
                            invalidFavourites++;
                            validationResults.Add(
                                $"    ❌ Invalid: Category '{categoryName}' (ID: {element.categoryId}) - Invalid GUID '{element.objGUID}', no path fallback");
                        }
                        else if (!hasGUID && hasPath && !pathValid)
                        {
                            invalidFavourites++;
                            validationResults.Add(
                                $"    ❌ Invalid: Category '{categoryName}' (ID: {element.categoryId}) - Invalid Path '{element.objPath}', no GUID");
                        }
                        else
                        {
                            validFavourites++;
                            var status = guidValid ? "GUID" : "Path";
                            validationResults.Add(
                                $"    ✅ Valid: Category '{categoryName}' (ID: {element.categoryId}) - {status} valid");
                        }
                    }

                    validationResults.Add("");
                }
            }

            var summary = "Validation Summary:\n\n" +
                          $"Assets found: {totalAssets}\n" +
                          $"Categories: {totalCategories}\n" +
                          $"Total favourites: {totalFavourites}\n" +
                          $"Valid favourites: {validFavourites}\n" +
                          $"Invalid favourites: {invalidFavourites}\n" +
                          $"  - Missing both GUID and Path: {missingBothGUIDAndPath}\n" +
                          $"  - Invalid GUIDs: {invalidGUIDs}\n" +
                          $"  - Invalid Paths: {invalidPaths}\n\n" +
                          $"Migration success rate: {(totalFavourites > 0 ? (validFavourites * 100.0 / totalFavourites).ToString("F1") : "0")}%";

            var fullMessage = summary + "\n\nDetailed Results:\n" + string.Join("\n", validationResults);

            EditorUtility.DisplayDialog("Validation Complete", fullMessage, "OK");
        }

        [MenuItem("Tools/Favourites/Clear All Favourites")]
        public static void ClearAllFavourites()
        {
            if (EditorUtility.DisplayDialog("Clear Favourites",
                    "Are you sure you want to clear all favourites? This action cannot be undone.",
                    "Yes, Clear All", "Cancel"))
            {
                FavouritesManager.Instance.Data.favs.Clear();
                FavouritesManager.Instance.Data.categories.Clear();
                FavouritesManager.Instance.Data.nextCategoryId = 0;
                FavouritesManager.Instance.SaveData();

                EditorUtility.DisplayDialog("Clear Complete", "All favourites have been cleared.", "OK");
            }
        }

        [MenuItem("Tools/Favourites/Show Data Location")]
        public static void ShowDataLocation()
        {
            var dataPath = Path.Combine(Application.persistentDataPath, "FavouritesData.json");
            EditorUtility.DisplayDialog("Favourites Data Location",
                $"Favourites data is stored at:\n{dataPath}", "OK");
        }

        [MenuItem("Tools/Favourites/Debug Current Data")]
        public static void DebugCurrentData()
        {
            var data = FavouritesManager.Instance.Data;
            var message = $"Categories: {data.categories.Count}\nFavourites: {data.favs.Count}";
            EditorUtility.DisplayDialog("Favourites Data Summary", message, "OK");
        }

        [MenuItem("Tools/Favourites/Clean Invalid Entries")]
        public static void CleanInvalidEntries()
        {
            var data = FavouritesManager.Instance.Data;
            var totalEntries = data.favs.Count;
            var removedEntries = 0;
            var removedDetails = new List<string>();

            // Create a list of entries to remove (we can't modify the collection while iterating)
            var entriesToRemove = new List<FavouritesElement>();

            foreach (var element in data.favs)
            {
                var shouldRemove = false;
                var reason = "";

                // Check if element has no reference data
                if (string.IsNullOrEmpty(element.objGUID) && string.IsNullOrEmpty(element.objPath))
                {
                    shouldRemove = true;
                    reason = "Missing both GUID and Path";
                }
                else
                {
                    // Check if the referenced object still exists
                    var obj = FavouritesManager.Instance.GetObjectFromElement(element);
                    if (obj == null)
                    {
                        shouldRemove = true;
                        reason = "Referenced asset no longer exists";
                    }
                }

                if (shouldRemove)
                {
                    entriesToRemove.Add(element);
                    removedDetails.Add(
                        $"Category {element.categoryId}: {reason} (GUID: '{element.objGUID}', Path: '{element.objPath}')");
                }
            }

            // Remove the invalid entries
            foreach (var element in entriesToRemove)
            {
                data.favs.Remove(element);
                removedEntries++;
            }

            if (removedEntries > 0)
            {
                FavouritesManager.Instance.SaveData();

                var message = "Cleanup Complete!\n\n" +
                              $"Total entries: {totalEntries}\n" +
                              $"Removed entries: {removedEntries}\n" +
                              $"Remaining entries: {totalEntries - removedEntries}\n\n" +
                              "Removed entries:\n";

                foreach (var detail in removedDetails) message += $"• {detail}\n";

                EditorUtility.DisplayDialog("Cleanup Complete", message, "OK");

                // Force a refresh of the favourites window if it's open
                var window = EditorWindow.GetWindow<FavouritesEdWindow>();
                if (window != null) window.UpdateTreeview();
            }
            else
            {
                EditorUtility.DisplayDialog("Cleanup Complete", "No invalid entries found. All favourites are valid.",
                    "OK");
            }
        }
    }
}