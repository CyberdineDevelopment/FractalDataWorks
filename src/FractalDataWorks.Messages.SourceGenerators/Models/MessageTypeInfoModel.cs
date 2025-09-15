using System;
using FractalDataWorks.SourceGenerators.Models;

namespace FractalDataWorks.Messages.SourceGenerators.Models;

/// <summary>
/// Type alias for CollectionTypeInfoModel used in Messages context.
/// </summary>
public class MessageTypeInfoModel : CollectionTypeInfoModel, IEquatable<MessageTypeInfoModel>
{
    /// <summary>
    /// Gets the list of properties that should be used for lookup methods.
    /// This shadows the base class property to use MessagePropertyLookupInfoModel instead of PropertyLookupInfoModel.
    /// </summary>
    public new EquatableArray<MessagePropertyLookupInfoModel> LookupProperties { get; set; } = EquatableArray.Empty<MessagePropertyLookupInfoModel>();

    /// <summary>
    /// Determines whether the specified <see cref="MessageTypeInfoModel"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The MessageTypeInfoModel to compare with the current instance.</param>
    /// <returns>true if the specified MessageTypeInfoModel is equal to the current instance; otherwise, false.</returns>
    public bool Equals(MessageTypeInfoModel? other)
    {
        return base.Equals(other);
    }
    
    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>true if the specified object is equal to the current instance; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as MessageTypeInfoModel);
    
    /// <summary>
    /// Returns a hash code for this message type info model.
    /// </summary>
    /// <returns>A hash code for the current message type info model.</returns>
    public override int GetHashCode() => base.GetHashCode();
}