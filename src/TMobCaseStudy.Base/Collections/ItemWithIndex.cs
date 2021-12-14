using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TMobCaseStudy.Base.Collections
{
    public class ItemWithIndex<T>
    {
        public int Index { get; private set; }

        public T Item { get; private set; }

        public ItemWithIndex(int index, T item)
        {
            Index = index;
            Item = item;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;

            var rhs = obj as ItemWithIndex<T>;
            return rhs != null && Index == rhs.Index && Item.Equals(rhs.Item);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Index.GetHashCode() ^ 137 + Item.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Index, Item);
        }
    }
}
