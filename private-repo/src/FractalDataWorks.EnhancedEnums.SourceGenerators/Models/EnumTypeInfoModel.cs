using System;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.EnhancedEnums.Models;

/// <summary>
/// Type alias for CollectionTypeInfoModel used in Enhanced Enums context.
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
    public override bool Equals(object? obj) => Equals(obj as EnumTypeInfoModel);
    
    /// <summary>
    /// Returns a hash code for this enum type info model.
    /// </summary>
    /// <returns>A hash code for the current enum type info model.</returns>
    public override int GetHashCode() => base.GetHashCode();
}