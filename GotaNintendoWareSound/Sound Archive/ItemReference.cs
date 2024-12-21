using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotaNintendoWare.SoundArchive {
    
    /// <summary>
    /// An item reference that updates its index automatically.
    /// </summary>
    public class ItemReference<T> {

        /// <summary>
        /// Collection of items.
        /// </summary>
        public List<T> Collection;

        /// <summary>
        /// Item.
        /// </summary>
        public T Item {
            get => m_Item;
            set {
                m_Item = value;
                if (m_Item != null) { 
                    if (Collection != null) {
                        if (Collection.Contains(m_Item)) {
                            m_Index = Collection.IndexOf(m_Item);
                        }
                    }
                }
            }
        }
        private T m_Item;

        /// <summary>
        /// Item index.
        /// </summary>
        public int Index {
            get {
                if (Collection != null && Item != null) {
                    m_Index = Collection.IndexOf(Item);
                }
                return m_Index;
            }
            set {
                m_Index = value;
                if (Collection != null) {
                    if (m_Index >= 0 && m_Index < Collection.Count) {
                        m_Item = Collection[m_Index];
                    }
                }
            }
        }
        private int m_Index;

    }

}
