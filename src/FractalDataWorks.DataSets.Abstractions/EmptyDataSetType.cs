using System;
using System.Collections.Generic;

namespace FractalDataWorks.DataSets.Abstractions;

/// <summary>
/// Empty placeholder dataset type used by source generators when no dataset types are defined.
/// This type should never be instantiated directly - it exists only for source generation purposes.
/// </summary>
internal sealed class EmptyDataSetType : DataSetTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyDataSetType"/> class.
    /// This constructor should never be called - this type exists only as a placeholder.
    /// </summary>
    public EmptyDataSetType() : base(
        id: -1,
        name: "Empty",
        description: "Empty placeholder dataset type",
        recordType: typeof(object),
        fields: new List<IDataField> { new EmptyDataField() },
        category: "System")
    {
        throw new InvalidOperationException("EmptyDataSetType should never be instantiated.");
    }

    /// <summary>
    /// Empty data field implementation for the empty dataset type.
    /// </summary>
    private class EmptyDataField : IDataField
    {
        public string Name => "EmptyField";
        public Type FieldType => typeof(object);
        public bool IsRequired => false;
        public bool IsKey => false;
        public object? DefaultValue => null;
        public string? Description => "Empty placeholder field";
        public int? MaxLength => null;
    }
}