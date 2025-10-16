using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;

namespace FractalDataWorks.SourceGenerators.Models;

/// <summary>
/// Represents constructor information for an enum option type.
/// </summary>
/// <remarks>
/// This code is excluded from code coverage because source generators run at compile-time and cannot be unit tested via runtime tests.
/// </remarks>

public sealed class ConstructorInfo : IEquatable<ConstructorInfo>
{
    /// <summary>
    /// Gets the list of parameters for this constructor.
    /// </summary>
    public IList<ParameterInfo> Parameters { get; set; } = new List<ParameterInfo>();
    
    /// <summary>
    /// Gets or sets the accessibility level of this constructor.
    /// </summary>
    public Accessibility Accessibility { get; set; }
    
    /// <summary>
    /// Gets or sets whether this is a primary constructor (C# 12+).
    /// </summary>
    public bool IsPrimary { get; set; }
    
    /// <summary>
    /// Writes this constructor information to a hash for change detection.
    /// </summary>
    public void WriteToHash(SHA256 sha256)
    {
        var bytes = Encoding.UTF8.GetBytes($"Constructor:{Accessibility}:{IsPrimary}:{Parameters.Count}");
        sha256.TransformBlock(bytes, 0, bytes.Length, null, 0);
        
        foreach (var param in Parameters)
        {
            param.WriteToHash(sha256);
        }
    }
    
    /// <inheritdoc/>
    public bool Equals(ConstructorInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Accessibility == other.Accessibility &&
               IsPrimary == other.IsPrimary &&
               Parameters.SequenceEqual(other.Parameters);
    }
    
    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ConstructorInfo);
    
    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Accessibility);
        hash.Add(IsPrimary);
        foreach (var param in Parameters)
        {
            hash.Add(param);
        }
        return hash.ToHashCode();
    }
}
