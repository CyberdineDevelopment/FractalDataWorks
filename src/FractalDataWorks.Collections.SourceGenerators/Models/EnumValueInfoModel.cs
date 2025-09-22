using System;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.Collections.Models;

/// <summary>
/// Type alias for FdwValueInfoModel used in Collections context.
/// Maintains compatibility while using the generalized model.
/// </summary>
public class EnumValueInfoModel : FdwValueInfoModel, IEquatable<EnumValueInfoModel>
{
    /// <summary>
    /// Determines whether the specified <see cref="EnumValueInfoModel"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The EnumValueInfoModel to compare with the current instance.</param>
    /// <returns>true if the specified EnumValueInfoModel is equal to the current instance; otherwise, false.</returns>
    public bool Equals(EnumValueInfoModel? other)
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
        return obj is EnumValueInfoModel other && Equals(other);
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