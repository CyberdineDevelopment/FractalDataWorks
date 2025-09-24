# DataStoreTypes.File

File system DataStore type implementations for the TypeCollection demo.

This package provides concrete file-based DataStore types that are automatically discovered by the TypeCollection source generator.

## Types Included

- **FileSystemDataStoreType** - Local file system access
- **SftpDataStoreType** - SFTP file transfer protocol

## Usage

Reference this package alongside DataStores.Abstractions to automatically include these file system types in the generated DataStoreTypes collection.