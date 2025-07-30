using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace FavouritesEd
{
    public class FavouritesEdWindow : EditorWindow
    {
        private static readonly GUIContent GC_Add = new("+", "Add category");
        private static readonly GUIContent GC_Remove = new("-", "Remove selected");
        private static readonly GUIContent GC_SaveSearch = new("💾", "Save current search");
        private static readonly GUIContent GC_RemoveSearch = new("🗑", "Remove saved search");

        [SerializeField] private TreeViewState treeViewState;
        private SearchField searchField;
        [SerializeField] private FavouritesTreeView treeView;
        private Vector2 savedSearchesScrollPosition;

        private void OnEnable()
        {
            // Ensure data is fresh when window opens
            FavouritesManager.Instance.RefreshData();
            UpdateTreeview();
        }

        private void OnGUI()
        {
            if (treeView == null) UpdateTreeview();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                treeView.searchString = searchField.OnToolbarGUI(treeView.searchString, GUILayout.ExpandWidth(true));
                GUILayout.Space(5);
                
                // Only show save button if there's text in the search box
                if (!string.IsNullOrEmpty(treeView.searchString))
                {
                    if (GUILayout.Button(GC_SaveSearch, EditorStyles.toolbarButton, GUILayout.Width(25)))
                        SaveCurrentSearch();
                }
                
                if (GUILayout.Button(GC_Add, EditorStyles.toolbarButton, GUILayout.Width(25)))
                    TextInputWindow.ShowWindow("Favourites", "Enter category name", "", AddCategory, null);
                GUI.enabled = treeView.Model.Data.Count > 0;
                if (GUILayout.Button(GC_Remove, EditorStyles.toolbarButton, GUILayout.Width(25))) RemoveSelected();
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            // Draw saved search buttons
            DrawSavedSearchButtons();

            treeView.OnGUI();
        }

        private void OnHierarchyChange()
        {
            UpdateTreeview();
        }

        private void OnProjectChange()
        {
            UpdateTreeview();
        }

        [MenuItem("Tools/Favourites/Show Favourites Window", false, 0)]
        private static void ShowWindow()
        {
            GetWindow<FavouritesEdWindow>("Favourites").UpdateTreeview();
        }

        public void UpdateTreeview()
        {
            if (treeViewState == null)
                treeViewState = new TreeViewState();

            if (treeView == null)
            {
                searchField = null;
                treeView = new FavouritesTreeView(treeViewState);
            }

            if (searchField == null)
            {
                searchField = new SearchField();
                searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
            }

            // Ensure we have fresh data
            FavouritesManager.Instance.RefreshData();
            treeView.LoadAndUpdate(FavouritesManager.Instance.Data);
            Repaint();
        }

        private void AddCategory(TextInputWindow wiz)
        {
            var s = wiz.Text;
            wiz.Close();
            if (string.IsNullOrEmpty(s)) return;

            FavouritesManager.Instance.AddCategory(s);

            UpdateTreeview();
            Repaint();
        }

        private void RemoveSelected()
        {
            var ids = treeView.GetSelection();
            if (ids.Count == 0) return;

            var ele = treeView.Model.Find(ids[0]);
            if (ele == null) return;

            if (ele.category != null)
            {
                // Remove category and all its favourites
                FavouritesManager.Instance.RemoveCategory(ele.category.id);
            }
            else if (ele.fav != null)
            {
                // Remove specific favourite
                var obj = FavouritesManager.Instance.GetObjectFromElement(ele.fav);
                if (obj != null) FavouritesManager.Instance.RemoveFavourite(obj, ele.fav.categoryId);
            }

            UpdateTreeview();
            Repaint();
        }

        private void SaveCurrentSearch()
        {
            var currentSearch = treeView.searchString;
            if (string.IsNullOrEmpty(currentSearch))
            {
                EditorUtility.DisplayDialog("Save Search", "Please enter a search query first.", "OK");
                return;
            }

            // Use the search query itself as the name
            FavouritesManager.Instance.AddSavedSearch(currentSearch, currentSearch);
            
            // Clear the search box after saving
            treeView.searchString = "";
            
            // Force the search field to update its display by recreating it
            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
            
            Repaint();
        }

        private void DrawSavedSearchButtons()
        {
            var savedSearches = FavouritesManager.Instance.Data.savedSearches;
            if (savedSearches.Count == 0) return;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                savedSearchesScrollPosition = EditorGUILayout.BeginScrollView(savedSearchesScrollPosition, 
                    GUIStyle.none, GUIStyle.none, GUILayout.Height(20));
                
                EditorGUILayout.BeginHorizontal();
                {
                    // Create a copy of the list to avoid modification during enumeration
                    var searchesToDisplay = new List<SavedSearch>(savedSearches);
                    
                    foreach (var search in searchesToDisplay)
                    {
                        var buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
                        if (treeView.searchString == search.query)
                        {
                            buttonStyle.normal.background = buttonStyle.active.background;
                            buttonStyle.fontStyle = FontStyle.Bold;
                        }

                        if (GUILayout.Button(search.name, buttonStyle, GUILayout.Width(EditorStyles.toolbarButton.CalcSize(new GUIContent(search.name)).x + 10)))
                        {
                            treeView.searchString = search.query;
                            treeView.SetFocusAndEnsureSelectedItem();
                        }

                        if (GUILayout.Button(GC_RemoveSearch, EditorStyles.toolbarButton, GUILayout.Width(20)))
                        {
                            if (EditorUtility.DisplayDialog("Remove Saved Search", 
                                $"Are you sure you want to remove '{search.name}'?", "Yes", "No"))
                            {
                                FavouritesManager.Instance.RemoveSavedSearch(search.id);
                                Repaint();
                            }
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}