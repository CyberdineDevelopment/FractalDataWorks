using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Collection of authentication methods.
/// </summary>
[EnumCollection(typeof(AuthenticationMethodBase), typeof(AuthenticationMethodBase), typeof(AuthenticationMethod))]
public abstract class AuthenticationMethod : EnumCollectionBase<AuthenticationMethodBase>
{
    // DO NOT IMPLEMENT BY HAND!
    // Source generator automatically creates static AuthenticationMethod class with:
    // - AuthenticationMethod.OAuth2 (returns AuthenticationMethodBase)
    // - AuthenticationMethod.FormBased (returns AuthenticationMethodBase)
    // - AuthenticationMethod.ApiKey (returns AuthenticationMethodBase)
    // - AuthenticationMethod.All (collection of AuthenticationMethodBase)
    // - AuthenticationMethod.GetById(int id) (returns AuthenticationMethodBase)
    // - AuthenticationMethod.GetByName(string name) (returns AuthenticationMethodBase)
}