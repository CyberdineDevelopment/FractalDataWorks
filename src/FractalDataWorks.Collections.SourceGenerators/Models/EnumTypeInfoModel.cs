using System;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.Collections.Models;

/// <summary>
/// Type alias for CollectionTypeInfoModel used in Collections context.
/// </summary>
public class EnumTypeInfoModel : CollectionTypeInfoModel, IEquatable<EnumTypeInfoModel>
{
    /// <summary>
    /// Determines whether the specified <see cref="EnumTypeInfoModel"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The EnumTypeInfoModel to compare with the current instance.</param>
    /// <returns>true if the specified EnumTypeInfoModel is equal to the current instance; otherwise, false.</returns>
    public bool Equals(EnumTypeInfoModel? other)
    {
        return base.Equals(other);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is EnumTypeInfoModel other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code for this instance.</returns>
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}