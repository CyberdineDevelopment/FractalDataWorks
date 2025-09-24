using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Configuration;

/// <summary>
/// Interface for cache strategies.
/// </summary>
public interface ICacheStrategy : IEnumOption<CacheStrategy>
{
}