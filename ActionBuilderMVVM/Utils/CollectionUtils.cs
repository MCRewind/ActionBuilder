using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionBuilderMVVM.Utils
{
    public static class CollectionUtils
    {
        public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] sequences)
        {
            return sequences.SelectMany(x => x);
        }
    }
}
