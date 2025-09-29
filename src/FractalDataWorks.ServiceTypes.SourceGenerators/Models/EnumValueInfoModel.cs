using System;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.ServiceTypes.SourceGenerators.Models;

/// <summary>
/// Type alias for GenericValueInfoModel used in ServiceTypes context.
/// Maintains compatibility while using the generalized model.
/// </summary>
public class EnumValueInfoModel : GenericValueInfoModel, IEquatable<EnumValueInfoModel>
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
    public override bool Equals(object? obj) => Equals(obj as EnumValueInfoModel);
    
    /// <summary>
    /// Returns a hash code for this enum value info model.
    /// </summary>
    /// <returns>A hash code for the current enum value info model.</returns>
    public override int GetHashCode() => base.GetHashCode();
}