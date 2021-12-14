namespace TMobCaseStudy.Base.Collections;

public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            return items == null || items.IsEmpty();
        }

        public static bool IsEmpty<T>(this IEnumerable<T> items)
        {
            return !items.Any();
        }

        public static T MinItem<T>(this IEnumerable<T> items) where T : IComparable<T>
        {
            return items.MinItem(x => x);
        }

        public static T MinItem<T, TV>(this IEnumerable<T> items, Func<T, TV> selector)
            where TV : IComparable<TV>
        {
            return items.MinOrMaxItem(selector, isMin: true);
        }

        public static T MaxItem<T>(this IEnumerable<T> items) where T : IComparable<T>
        {
            return items.MaxItem(x => x);
        }

        public static T MaxItem<T, TV>(this IEnumerable<T> items, Func<T, TV> selector)
            where TV : IComparable<TV>
        {
            return items.MinOrMaxItem(selector, isMin: false);
        }

        public static int MinItemIndex<T>(this IEnumerable<T> items) where T : IComparable<T>
        {
            return items.MinItemIndex(x => x);
        }

        public static int MinItemIndex<T, TV>(this IEnumerable<T> items, Func<T, TV> selector)
            where TV : IComparable<TV>
        {
            return items.MinOrMaxItemIndex(selector, isMin: true);
        }

        public static int MaxItemIndex<T>(this IEnumerable<T> items) where T : IComparable<T>
        {
            return items.MaxItemIndex(x => x);
        }

        public static int MaxItemIndex<T, TV>(this IEnumerable<T> items, Func<T, TV> selector)
            where TV : IComparable<TV>
        {
            return items.MinOrMaxItemIndex(selector, isMin: false);
        }

        public static ItemWithIndex<T> MinItemWithIndex<T>(this IEnumerable<T> items)
            where T : IComparable<T>
        {
            return items.MinItemWithIndex(x => x);
        }

        public static ItemWithIndex<T> MinItemWithIndex<T, TV>(this IEnumerable<T> items,
            Func<T, TV> selector) where TV : IComparable<TV>
        {
            return items.MinOrMaxItemWithIndex(selector, isMin: true);
        }

        public static ItemWithIndex<T> MaxItemWithIndex<T>(this IEnumerable<T> items)
            where T : IComparable<T>
        {
            return items.MaxItemWithIndex(x => x);
        }

        public static ItemWithIndex<T> MaxItemWithIndex<T, TV>(this IEnumerable<T> items,
            Func<T, TV> selector) where TV : IComparable<TV>
        {
            return items.MinOrMaxItemWithIndex(selector, isMin: false);
        }

        private static T MinOrMaxItem<T, TV>(this IEnumerable<T> items, 
            Func<T, TV> selector, bool isMin) where TV : IComparable<TV>
        {
            var minOrMaxItemWithIndex = items.MinOrMaxItemWithIndex(selector, isMin);
            return minOrMaxItemWithIndex == null ? default(T) : minOrMaxItemWithIndex.Item;
        }

        private static int MinOrMaxItemIndex<T, TV>(this IEnumerable<T> items,
            Func<T, TV> selector, bool isMin) where TV : IComparable<TV>
        {
            var minOrMaxItemWithIndex = items.MinOrMaxItemWithIndex(selector, isMin);
            return minOrMaxItemWithIndex == null ? -1 : minOrMaxItemWithIndex.Index;
        }

        private static ItemWithIndex<T> MinOrMaxItemWithIndex<T, TV>(
            this IEnumerable<T> items, Func<T, TV> selector, bool isMin) where TV : IComparable<TV>
        {
            var enumerator = items.GetEnumerator();
            if (!enumerator.MoveNext()) return null;

            var minMaxIndex = 0;
            var minMaxItem = enumerator.Current;
            var minMaxValue = selector(enumerator.Current);
            for (var i = 1; enumerator.MoveNext(); ++i)
            {
                var currentValue = selector(enumerator.Current);
                if ((isMin && currentValue.CompareTo(minMaxValue) < 0) ||
                    (!isMin && currentValue.CompareTo(minMaxValue) > 0))
                {
                    minMaxIndex = i;
                    minMaxItem = enumerator.Current;
                    minMaxValue = currentValue;
                }
            }
            return new ItemWithIndex<T>(minMaxIndex, minMaxItem);
        }

        public static ItemWithIndex<T> FindItemWithIndex<T>(
            this IEnumerable<T> items, Predicate<T> predicate)
        {
            var enumerator = items.GetEnumerator();
            
            for (var i = 0; enumerator.MoveNext(); ++i)
            {
                var matches = predicate(enumerator.Current);
                if (matches)
                {
                    return new ItemWithIndex<T>(i, enumerator.Current);
                }
            }
            return null;
        }
        
        public static T MedianItem<T>(this IEnumerable<T> items, Func<T, decimal> selector)
        {
            var orderedItems = items.OrderBy(selector).ToList();
            if (orderedItems.Count == 0) return default(T);

            int middle = orderedItems.Count / 2;
            return orderedItems[middle];
        }

        public static decimal? Median<T>(this IEnumerable<T> items, Func<T, decimal> selector)
        {
            var orderedItems = items.OrderBy(selector).ToList();
            if (orderedItems.Count == 0) return null;
            if (orderedItems.Count % 2 == 0)
            {
                var firstValue = selector(orderedItems[orderedItems.Count / 2 - 1]);
                var secondValue = selector(orderedItems[orderedItems.Count / 2]);
                return (firstValue + secondValue) / 2m;
            }
            else
            {
                int middle = orderedItems.Count / 2;
                return selector(orderedItems[middle]);
            }
        }

        public static decimal? Mean<T>(this IEnumerable<T> items, Func<T, decimal> selector)
        {
            var length = items.Count();
            if (length == 0) return null;

            var sum = items.Sum(selector);
            return sum / length;
        }

        public static bool IsOrderedEqual<T>(this IEnumerable<T> items1, IEnumerable<T> items2)
        {
            if (items1 == null && items2 == null) return true;
            if (items1 == null || items2 == null) return false;
            if (ReferenceEquals(items1, items2)) return true;

            return items1.SequenceEqual(items2);
        }

        public static bool IsUnorderedEqual<T>(this IEnumerable<T> items1, IEnumerable<T> items2)
        {
            if (items1 == null && items2 == null) return true;
            if (items1 == null || items2 == null) return false;
            if (ReferenceEquals(items1, items2)) return true;

            return items1.All(x => items2.Contains(x)) && items2.All(x => items1.Contains(x));
        }
    }