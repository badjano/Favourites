using System;
using System.Collections.Generic;
using UnityEngine;

namespace FavouritesEd
{
    [Serializable]
    public class TreeElement
    {
        [SerializeField] private int m_ID;
        [SerializeField] private string m_Name;
        [SerializeField] private int m_Depth;
        [NonSerialized] private List<TreeElement> m_Children;

        [NonSerialized] private TreeElement m_Parent;

        public TreeElement()
        {
        }

        public TreeElement(string name, int depth, int id)
        {
            m_Name = name;
            m_ID = id;
            m_Depth = depth;
        }

        public TreeElement(string name, int depth, int id, Texture2D icon)
        {
            m_Name = name;
            m_ID = id;
            m_Depth = depth;
            Icon = icon;
        }

        public int Depth
        {
            get => m_Depth;
            set => m_Depth = value;
        }

        public TreeElement Parent
        {
            get => m_Parent;
            set => m_Parent = value;
        }

        public List<TreeElement> Children
        {
            get => m_Children;
            set => m_Children = value;
        }

        public bool HasChildren => Children != null && Children.Count > 0;

        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        public virtual string SearchHelper => m_Name;

        public int ID
        {
            get => m_ID;
            set => m_ID = value;
        }

        public Texture2D Icon { get; set; }
    }
}