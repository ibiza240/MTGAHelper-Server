#nullable enable
using System;
using System.Collections.Generic;

namespace MTGAHelper.Lib.CardProviders;

public static class SortedArrayHelper
{
    // Binary search copied and adapted from (internal) System.Collections.Generic.ArraySortHelper<T>.InternalBinarySearch
    public static TArr[] BinarySearchContiguousEquals<TArr, TCmp>(
        TArr[] array,
        Func<TArr, TCmp> selector,
        TCmp value,
        IComparer<TCmp>? comparer = null)
    {
        comparer ??= Comparer<TCmp>.Default;
        // invariant: indices below `lo` are too small, indices above `hi` are too big 
        int lo = 0;
        int hi = array.Length - 1;
        while (lo <= hi)
        {
            // take a pivot, halfway between the low and high points
            int pivot = lo + (hi - lo >> 1);
            int res = Compare(pivot);
            switch (res)
            {
                case 0:
                    // we found one match, now find all contiguous items
                    return GetContiguous(array, pivot, lo, hi, Compare);
                case < 0:
                    // pivot value lower than objective: confine search above pivot
                    lo = pivot + 1;
                    break;
                case > 0:
                    // pivot value higher than objective: confine search below pivot
                    hi = pivot - 1;
                    break;
            }
        }

        return Array.Empty<TArr>();

        int Compare(int index) => comparer.Compare(selector(array[index]), value);
    }

    private static TArr[] GetContiguous<TArr>(TArr[] array, int middle, int lo, int hi, Func<int, int> compare)
    {
        int from = FindFirstEqual(lo, middle, compare);
        int upto = FindFirstBigger(middle, hi, compare);

        return array[from..upto];
    }

    private static int FindFirstEqual(int lo, int hi, Func<int, int> compare)
    {
        while (lo <= hi)
        {
            var pivot = lo + (hi - lo >> 1);
            int res = compare(pivot);
            switch (res)
            {
                case < 0:
                    lo = pivot + 1;
                    break;
                case >= 0:
                    // note: we expect (only) equality here

                    // this will set `hi` to the index of the last unequal element eventually,
                    // in that case `lo` will be set to the index of the first equal element
                    hi = pivot - 1;
                    break;
            }
        }

        return lo;
    }

    private static int FindFirstBigger(int lo, int hi, Func<int, int> compare)
    {
        while (lo <= hi)
        {
            var pivot = lo + (hi - lo >> 1);
            int res = compare(pivot);
            switch (res)
            {
                case <= 0:
                    // note: we expect (only) equality here

                    // in the last iteration this will set `lo` to the index of the first bigger element
                    lo = pivot + 1;
                    break;
                case > 0:
                    hi = pivot - 1;
                    break;
            }
        }

        return lo;
    }
}