using System;

namespace FractalDataWorks.ServiceTypes.Attributes;

/// <summary>
/// Marks a property on a ServiceType for automatic lookup method generation.
/// The ServiceTypeCollectionGenerator will create a static lookup method based on this property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ServiceTypeLookupAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeLookupAttribute"/> class.
    /// </summary>
    /// <param name="methodName">Custom method name for the lookup (e.g., "Name", "Id", "ServiceType").</param>
    public ServiceTypeLookupAttribute(string methodName)
    {
        MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
    }

    /// <summary>
    /// Gets the custom method name for the lookup.
    /// </summary>
    public string MethodName { get; }
}
