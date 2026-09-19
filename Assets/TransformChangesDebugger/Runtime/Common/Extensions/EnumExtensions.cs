using System;
using System.Collections.Generic;
using System.Linq;

namespace ImmersiveVRTools.Runtime.Common.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<Enum> GetFlags(this Enum value)
        {
            return GetFlags(value, Enum.GetValues(value.GetType()).Cast<Enum>().ToArray());
        }

        private static IEnumerable<Enum> GetFlags(Enum value, Enum[] values)
        {
            ulong bits = Convert.ToUInt64(value);
            List<Enum> results = new List<Enum>();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                ulong mask = Convert.ToUInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask)
                {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }
            if (bits != 0L)
                return Enumerable.Empty<Enum>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<Enum>();
            if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<Enum>();
        }

        private static IEnumerable<Enum> GetFlagValues(Type enumType)
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }

        public static Dictionary<TEnum, TMappedValue> AsAllElementsRequiredMap<TEnum, TMappedValue>(this Dictionary<TEnum, TMappedValue> enumMap)
            => AsAllElementsRequiredMap(enumMap, new List<TEnum>());

        public static Dictionary<TEnum, TMappedValue> AsAllElementsRequiredMap<TEnum, TMappedValue>(this Dictionary<TEnum, TMappedValue> enumMap, List<TEnum> ignoreValues)
        {
            var enumType = typeof(TEnum);

            var enumMapValues = enumMap.Select(em => em.Key).ToList();
            var missingEnumValues = Enum.GetValues(enumType).Cast<TEnum>().Where(ev => !enumMapValues.Contains(ev)).ToList();
            var missingNotExcludedEnumValues = missingEnumValues.Except(ignoreValues).ToList();
            if (missingNotExcludedEnumValues.Any())
                throw new Exception($"Invalid map setup, missing values: {string.Join(", ", missingNotExcludedEnumValues.Select(ev => ev).ToList())}");

            return enumMap;
        }
    }
}