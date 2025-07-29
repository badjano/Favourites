using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace FavouritesEd
{
    public class FavouritesEdWindow : EditorWindow
    {
        private static readonly GUIContent GC_Add = new("+", "Add category");
        private static readonly GUIContent GC_Remove = new("-", "Remove selected");

        [SerializeField] private TreeViewState treeViewState;
        private SearchField searchField;
        [SerializeField] private FavouritesTreeView treeView;

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
                if (GUILayout.Button(GC_Add, EditorStyles.toolbarButton, GUILayout.Width(25)))
                    TextInputWindow.ShowWindow("Favourites", "Enter category name", "", AddCategory, null);
                GUI.enabled = treeView.Model.Data.Count > 0;
                if (GUILayout.Button(GC_Remove, EditorStyles.toolbarButton, GUILayout.Width(25))) RemoveSelected();
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

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
    }
}