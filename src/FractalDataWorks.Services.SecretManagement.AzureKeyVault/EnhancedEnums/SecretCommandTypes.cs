using System;
using System.Linq;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault.EnhancedEnums;

/// <summary>
/// Collection class for SecretCommandType with type-based lookup methods.
/// </summary>
[TypeCollection(typeof(SecretCommandTypeBase), typeof(ISecretCommandType), typeof(SecretCommandTypes))]
public class SecretCommandTypes
{
}