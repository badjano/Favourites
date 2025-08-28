using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace FavouritesEd
{
    public class FavouritesTreeView : TreeViewWithTreeModel<FavouritesTreeElement>
    {
        private static readonly GUIContent GC_None = new("No Favourites.");
        private static readonly string DragAndDropID = "FavouritesTreeElement";

        private static Func<string> Invoke_folderIconName;

        private FavouritesData data;

        public FavouritesTreeView(TreeViewState treeViewState)
            : base(treeViewState)
        {
            baseIndent = 5f;
        }

        public TreeModel<FavouritesTreeElement> Model { get; private set; }

        public void OnGUI()
        {
            if (Model != null && Model.Data != null && Model.Data.Count > 1)
            {
                base.OnGUI(GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));
            }
            else
            {
                GUILayout.Label(GC_None);
                GUILayout.FlexibleSpace();
            }
        }

        public void LoadAndUpdate(FavouritesData favsData = null)
        {
            if (favsData != null) data = favsData;
            if (data == null) data = FavouritesManager.Instance.Data;

            // add root
            var treeRoot = new FavouritesTreeElement { ID = 0, Depth = -1, Name = "Root" };
            Model = new TreeModel<FavouritesTreeElement>(new List<FavouritesTreeElement> { treeRoot });

            // add categories
            var categories = new List<FavouritesTreeElement>();
            var icon = EditorGUIUtility.IconContent(FolderIconName()).image as Texture2D;
            foreach (var c in data.categories)
            {
                var ele = new FavouritesTreeElement
                {
                    Name = c.name,
                    Icon = icon,
                    ID = Model.GenerateUniqueID(),
                    category = c
                };

                categories.Add(ele);
                Model.QuickAddElement(ele, treeRoot);
            }

            // add favourites from data
            var favs = new List<FavouritesElement>();
            favs.AddRange(data.favs);

            // sort
            favs.Sort((a, b) =>
            {
                var r = a.categoryId.CompareTo(b.categoryId);
                if (r == 0)
                {
                    var objA = FavouritesManager.Instance.GetObjectFromElement(a);
                    var objB = FavouritesManager.Instance.GetObjectFromElement(b);
                    if (objA != null && objB != null) r = objA.name.CompareTo(objB.name);
                }

                return r;
            });

            // and add to tree
            foreach (var ele in favs)
            {
                if (ele == null) continue;

                var obj = FavouritesManager.Instance.GetObjectFromElement(ele);
                if (obj == null) continue;
                foreach (var c in categories)
                    if (c.category.id == ele.categoryId)
                    {
                        var nm = obj.name;
                        var go = obj as GameObject;
                        if (go != null && go.scene.IsValid()) nm = string.Format("{0} ({1})", nm, go.scene.name);

                        icon = AssetPreview.GetMiniTypeThumbnail(obj.GetType());

                        Model.QuickAddElement(new FavouritesTreeElement
                        {
                            Name = nm,
                            Icon = icon,
                            ID = Model.GenerateUniqueID(),
                            fav = ele
                        }, c);

                        break;
                    }
            }

            Model.UpdateDataFromTree();
            Init(Model);
            Reload();
            SetSelection(new List<int>());
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
        }

        protected override void ContextClickedItem(int id)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove"), false, HandleRemoveOption, id);
            
            // Only show Rename for categories
            var ele = Model.Find(id);
            if (ele != null && ele.category != null)
            {
                menu.AddItem(new GUIContent("Rename"), false, HandleRenameOption, id);
            }
            
            menu.ShowAsContext();
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            
            // Auto-locate the selected item if it's a favourite (not a category)
            if (selectedIds.Count > 0)
            {
                var ele = Model.Find(selectedIds[0]);
                if (ele != null && ele.fav != null)
                {
                    var obj = FavouritesManager.Instance.GetObjectFromElement(ele.fav);
                    if (obj != null) EditorGUIUtility.PingObject(obj);
                }
            }
        }

        private void HandleLocateOption(object arg)
        {
            var id = (int)arg;
            var ele = Model.Find(id);
            if (ele != null && ele.fav != null)
            {
                var obj = FavouritesManager.Instance.GetObjectFromElement(ele.fav);
                if (obj != null) EditorGUIUtility.PingObject(obj);
            }
        }

        private void HandleRemoveOption(object arg)
        {
            var id = (int)arg;
            var ele = Model.Find(id);
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

            // Refresh the tree view
            LoadAndUpdate(FavouritesManager.Instance.Data);
        }

        private void HandleRenameOption(object arg)
        {
            var id = (int)arg;
            var ele = Model.Find(id);
            if (ele == null) return;

            if (ele.category != null)
            {
                // Rename category
                TextInputWindow.ShowWindow("Rename Category", "Enter new category name", ele.category.name, 
                    (window) => {
                        var newName = window.Text;
                        window.Close();
                        if (!string.IsNullOrEmpty(newName))
                        {
                            FavouritesManager.Instance.RenameCategory(ele.category.id, newName);
                            LoadAndUpdate(FavouritesManager.Instance.Data);
                        }
                    }, null);
            }
            // Note: Individual favorites don't have names to rename, so we only handle categories
        }

        protected override void DoubleClickedItem(int id)
        {
            var ele = Model.Find(id);
            if (ele != null && ele.fav != null)
            {
                var obj = FavouritesManager.Instance.GetObjectFromElement(ele.fav);
                if (obj != null) AssetDatabase.OpenAsset(obj);
            }
            else
            {
                SetExpanded(id, !IsExpanded(id));
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }


        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            if (data == null || data.categories.Count == 0 ||
                !rootItem.hasChildren || args.draggedItem.parent == rootItem)
                return false;

            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (args.draggedItemIDs.Count == 0) return;

            var item = Model.Find(args.draggedItemIDs[0]);
            if (item == null || item.fav == null) return;

            var obj = FavouritesManager.Instance.GetObjectFromElement(item.fav);
            if (obj == null) return;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(DragAndDropID, item);
            DragAndDrop.objectReferences = new[] { obj };
            DragAndDrop.StartDrag(obj.name);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (data == null || data.categories.Count == 0 || !rootItem.hasChildren)
                return DragAndDropVisualMode.Rejected;

            if (args.performDrop)
            {
                FavouritesTreeElement ele;
                var id = args.parentItem == null ? -1 : args.parentItem.id;
                if (id < 0 || (ele = Model.Find(id)) == null || ele.category == null)
                {
                    var ids = GetSelection();
                    if (ids.Count > 0)
                    {
                        var item = FindItem(ids[0], rootItem);
                        if (item == null) return DragAndDropVisualMode.Rejected;
                        id = item.parent == rootItem ? item.id : item.parent.id;
                    }
                    else
                    {
                        id = rootItem.children[0].id;
                    }

                    ele = Model.Find(id);
                }

                if (ele == null || ele.category == null) return DragAndDropVisualMode.Rejected;

                var categoryId = ele.category.id;

                // first check if it is "internal" drag drop from one category to another
                var draggedEle = DragAndDrop.GetGenericData(DragAndDropID) as FavouritesTreeElement;
                if (draggedEle != null)
                {
                    // Update the category ID for the dragged element
                    draggedEle.fav.categoryId = categoryId;
                    FavouritesManager.Instance.SaveData();
                }

                // else the drag-drop originated somewhere else
                else
                {
                    var objs = DragAndDrop.objectReferences;
                    foreach (var obj in objs)
                    {
                        // make sure it is not a component
                        if (obj as Component != null) continue;

                        // Add to favourites
                        FavouritesManager.Instance.AddFavourite(obj, categoryId);
                    }
                }

                LoadAndUpdate();
            }

            return DragAndDropVisualMode.Generic;
        }

        private static string FolderIconName()
        {
            if (Invoke_folderIconName == null)
                try
                {
                    var asm = Assembly.GetAssembly(typeof(Editor));
                    var editorResourcesType = asm.GetType("UnityEditorInternal.EditorResourcesUtility");
                    if (editorResourcesType != null)
                    {
                        var prop = editorResourcesType.GetProperty("folderIconName",
                            BindingFlags.Static | BindingFlags.Public);
                        if (prop != null)
                        {
                            var method = prop.GetGetMethod(true);
                            if (method != null)
                                Invoke_folderIconName =
                                    (Func<string>)Delegate.CreateDelegate(typeof(Func<string>), method);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Silently fall back to default icon
                }

            // Fallback to a known folder icon name if reflection fails
            if (Invoke_folderIconName == null) return "FolderOpened Icon";

            try
            {
                return Invoke_folderIconName();
            }
            catch (Exception ex)
            {
                return "FolderOpened Icon";
            }
        }
    }
}