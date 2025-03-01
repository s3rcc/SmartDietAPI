using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.Linq;

public static class RegionTypeExtensions
{
    public static RegionType CombineRegionTypes(this IEnumerable<RegionType> regionTypes)
    {
        RegionType combined = RegionType.None;
        foreach (var type in regionTypes)
        {
            combined |= type;
        }
        return combined;
    }

    public static List<RegionType> SplitRegionTypes(this RegionType combinedType)
    {
        return Enum.GetValues(typeof(RegionType))
            .Cast<RegionType>()
            .Where(r => r != RegionType.None && combinedType.HasFlag(r))
            .ToList();
    }
} 