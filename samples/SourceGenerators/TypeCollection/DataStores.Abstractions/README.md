# DataStores.Abstractions

DataStore types abstractions with embedded TypeCollection source generator.

This package provides the base abstractions for DataStore types and includes an embedded source generator that automatically discovers and creates collections of DataStore implementations.

## Usage

The package automatically generates `DataStoreTypes` collection when you reference DataStore implementation packages that contain types decorated with `[TypeOption]` attributes.

## Generated Code

The source generator creates:
- Static collections with O(1) lookup performance using FrozenDictionary
- Strongly-typed access to all registered DataStore types
- Automatic discovery of types from referenced packages