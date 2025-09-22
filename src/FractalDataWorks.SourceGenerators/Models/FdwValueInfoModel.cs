using System;

namespace FractalDataWorks.SourceGenerators.Models;

/// <summary>
/// Generalized type alias for CollectionValueInfoModel used across source generators.
/// Renamed from EnumValueInfoModel to FdwValueInfoModel for consistency.
/// </summary>
public class FdwValueInfoModel : CollectionValueInfoModel, IEquatable<FdwValueInfoModel>
{
    /// <summary>
    /// Determines whether the specified <see cref="FdwValueInfoModel"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The FdwValueInfoModel to compare with the current instance.</param>
    /// <returns>true if the specified FdwValueInfoModel is equal to the current instance; otherwise, false.</returns>
    public bool Equals(FdwValueInfoModel? other)
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
        return obj is FdwValueInfoModel other && Equals(other);
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