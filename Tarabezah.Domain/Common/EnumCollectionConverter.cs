using System;
using System.Collections.Generic;
using System.Linq;

namespace Tarabezah.Domain.Common;

/// <summary>
/// Utility class for converting between collections of enum values and lists of integers
/// </summary>
public static class EnumCollectionConverter
{
    /// <summary>
    /// Converts a list of enum values to a list of integers
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="enumValues">The enum values to convert</param>
    /// <returns>A list of integer values representing the enum values</returns>
    public static List<int> ToIntList<T>(IEnumerable<T> enumValues) where T : Enum
    {
        return enumValues.Select(x => Convert.ToInt32(x)).ToList();
    }
    
    /// <summary>
    /// Converts a list of integers to a list of enum values
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="intValues">The integer values to convert</param>
    /// <returns>A list of enum values</returns>
    public static List<T> ToEnumList<T>(IEnumerable<int> intValues) where T : Enum
    {
        return intValues.Select(x => (T)Enum.ToObject(typeof(T), x)).ToList();
    }
} 