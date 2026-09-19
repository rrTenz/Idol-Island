using System;
using System.Collections.Generic;
using System.Linq;

namespace ImmersiveVRTools.Runtime.Common.Utilities
{
    public class ReflectionHelper
    {
        public static List<Type> GetAllInstantiableTypesDerivedFrom(Type type, List<Type> except = null)
        {
            var all = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(t => t.IsSubclassOf(type) && t != type && !t.IsAbstract)
                .Distinct()
                .ToList();

            return except != null
                ? all.Except(except).ToList()
                : all;
        }
    }
}