using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Scripts.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            var partition = new List<T>(size);

            foreach (var item in source)
            {
                partition.Add(item);

                if (partition.Count == size)
                {
                    yield return partition.AsReadOnly();
                    partition = new List<T>(size);
                }
            }

            if (partition.Count > 0)
                yield return partition.AsReadOnly();
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this ICollection<T> source, int numberOfSplits)
        {
            if (numberOfSplits <= 0 || numberOfSplits > source.Count)
                throw new ArgumentOutOfRangeException(nameof(numberOfSplits));

            var size = Mathf.CeilToInt((float)source.Count / numberOfSplits);
            var split = new List<T>(size);

            foreach (var item in source)
            {
                split.Add(item);

                if (split.Count == size)
                {
                    yield return split.AsReadOnly();
                    split = new List<T>(size);
                }
            }

            if (split.Count != 0)
                yield return split.AsReadOnly();
        }
    }
}