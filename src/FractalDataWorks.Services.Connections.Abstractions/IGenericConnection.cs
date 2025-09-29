using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Interface for FractalDataWorks framework connections.
/// Provides a framework-specific interface for connection implementations.
/// </summary>
public interface IGenericConnection : IDisposable, IGenericService
{
}