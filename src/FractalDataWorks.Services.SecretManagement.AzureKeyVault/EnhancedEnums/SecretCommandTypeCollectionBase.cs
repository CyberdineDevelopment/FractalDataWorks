using System;
using System.Linq;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault.EnhancedEnums;

/// <summary>
/// Base collection class for SecretCommandType with type-based lookup methods.
/// The source generator will create SecretCommandTypes that inherits from this.
/// </summary>
[StaticEnumCollection(CollectionName = "SecretCommandTypes", DefaultGenericReturnType = typeof(ISecretCommandType))]
public abstract class SecretCommandTypeCollectionBase : EnumCollectionBase<SecretCommandTypeBase>
{
    /// <summary>
    /// Gets a command type by the Type it handles.
    /// </summary>
    /// <param name="commandType">The command type to find a handler for.</param>
    /// <returns>The SecretCommandTypeBase that handles the specified command type, or null if not found.</returns>
    public static SecretCommandTypeBase? ByType(Type commandType)
    {
        return All().FirstOrDefault(ct => ct.CommandType == commandType);
    }
    
    /// <summary>
    /// Gets a command type by the generic Type it handles.
    /// </summary>
    /// <typeparam name="T">The command type to find a handler for.</typeparam>
    /// <returns>The SecretCommandTypeBase that handles the specified command type, or null if not found.</returns>
    public static SecretCommandTypeBase? ByType<T>()
    {
        return ByType(typeof(T));
    }
}