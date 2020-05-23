using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ActionBuilder.Utils
{
    public static class ReflectiveEnumerator
    {
        public static IEnumerable<Type> GetEnumerableOfType<T>()
        {
            var objects = Assembly.GetAssembly(typeof(T))
                ?.GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)))
                .ToList();
            return objects;
        }
    }
}