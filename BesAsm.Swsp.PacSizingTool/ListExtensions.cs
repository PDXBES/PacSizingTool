using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BesAsm.Swsp.PacSizingTool
{
  internal static class ListExtensions
  {
    internal static IList<double> Accumulate(this IList<double> incremental)
    {
      IList<double> accumulated = new List<double>().Initialize(incremental.Count);

      for (int i = 1; i < incremental.Count() - 1; i++)
        accumulated[i] = accumulated[i - 1] + incremental[i];

      return accumulated;
    }

    internal static IList<double> Deaccumulate(this IList<double> accumulated)
    {
      IList<double> incremental = new List<double>().Initialize(accumulated.Count);

      for (int i = 1; i < accumulated.Count() - 1; i++)
        incremental[i] = accumulated[i] - accumulated[i - 1];

      return incremental;
    }

    internal static IList<double> Initialize(this IList<double> list, int count)
    {
      list.Clear();
      for (int i = 0; i < count; i++)
        list.Add(0);

      return list;
    }

    internal static IList<double> PairwiseAddition(this IList<double> list1, IList<double> list2)
    {
      List<double> result = new List<double>();
      for (int i = 0; i < list1.Count; i++)
      {
        result[i] = list1[i] + list2[i];
      }
      return result;

      //return list1.Zip(list2, (p, q) => p + q).ToList();
    }

  }
}
