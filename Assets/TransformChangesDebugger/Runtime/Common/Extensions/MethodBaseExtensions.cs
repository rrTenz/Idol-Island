using System.Linq;
using System.Reflection;

namespace ImmersiveVRTools.Runtime.Common.Extensions
{
    public static class MethodBaseExtensions
    {
        // private static readonly Dictionary<MethodBase, string> MethodBaseToFullNameCache = new Dictionary<MethodBase, string>();
        // private static readonly PropertyInfo FullNameProperty = typeof(MethodBase).GetProperty("FullName", BindingFlags.NonPublic | BindingFlags.Instance);

        public static string ResolveFullName(this MethodBase method)
        {
            if (method == null) return string.Empty;
            
            //PERF: potentially could reverse patch FullName instead?
            //manual resolution - if not working that well could revert to reflection call, or perhaps reverse patch?
            return $"{method.ReflectedType.FullName}.{method.Name}({string.Join(",", method.GetParameters().Select(o => $"{o.ParameterType}").ToArray())})";
            
            // if (!MethodBaseToFullNameCache.ContainsKey(method))
            // {
            //     var fullMethodName = (string) FullNameProperty.GetMethod.Invoke(method, null);
            //     MethodBaseToFullNameCache[method] = fullMethodName;
            // }
            //
            // return MethodBaseToFullNameCache[method];
        }
    }
}