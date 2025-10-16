using System;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.Messages.SourceGenerators.Models;

/// <summary>
/// Type alias for CollectionValueInfoModel used in Messages context.
/// </summary>
public class MessageValueInfoModel : CollectionValueInfoModel, IEquatable<MessageValueInfoModel>
{
    /// <summary>
    /// Determines whether the specified <see cref="MessageValueInfoModel"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The MessageValueInfoModel to compare with the current instance.</param>
    /// <returns>true if the specified MessageValueInfoModel is equal to the current instance; otherwise, false.</returns>
    public bool Equals(MessageValueInfoModel? other)
    {
        return base.Equals(other);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MessageValueInfoModel);
    
    /// <summary>
    /// Returns a hash code for this message value info model.
    /// </summary>
    /// <returns>A hash code for the current message value info model.</returns>
    public override int GetHashCode() => base.GetHashCode();
}