using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Models;

/// <summary>
/// Represents a hierarchical path to a data location (e.g., schema.table or database/collection).
/// </summary>
public sealed class DataPath
{
    private readonly string[] _segments;
    private readonly string _separator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataPath"/> class.
    /// </summary>
    /// <param name="segments">The path segments.</param>
    /// <param name="separator">The separator character (default is ".").</param>
    public DataPath(IEnumerable<string> segments, string separator = ".")
    {
        if (segments == null)
            throw new ArgumentNullException(nameof(segments));

        _segments = segments.ToArray();
        _separator = separator ?? ".";

        if (_segments.Length == 0)
            throw new ArgumentException("Path must have at least one segment.", nameof(segments));
    }

    /// <summary>
    /// Gets the path segments.
    /// </summary>
    public IReadOnlyList<string> Segments => _segments;

    /// <summary>
    /// Gets the separator character.
    /// </summary>
    public string Separator => _separator;

    /// <summary>
    /// Gets the full path as a string.
    /// </summary>
    public string FullPath => string.Join(_separator, _segments);

    /// <summary>
    /// Returns the string representation of the path.
    /// </summary>
    public override string ToString() => FullPath;
}
