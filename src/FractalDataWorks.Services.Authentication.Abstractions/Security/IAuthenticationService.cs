using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Authentication.Abstractions.Security;

/// <summary>
/// Provides authentication services for validating and managing user credentials and tokens.
/// This service handles the core authentication operations including token validation,
/// refresh operations, and credential verification for REST endpoints.
/// </summary>
/// <remarks>
/// <para>
/// The authentication service is designed to be implementation-agnostic, supporting
/// various authentication mechanisms including JWT, API keys, OAuth2, and custom
/// authentication schemes through the SecurityMethod enhanced enum system.
/// </para>
/// <para>
/// All operations return IFdwResult to provide consistent error handling and
/// success/failure semantics across the FractalDataWorks framework.
/// </para>
/// <para>
/// This interface is intended for dependency injection and should be registered
/// as a singleton or scoped service depending on the authentication provider's
/// requirements and thread-safety characteristics.
/// </para>
/// </remarks>
public interface IAuthenticationService : IFdwService
{
    /// <summary>
    /// Authenticates a user based on the provided authentication token.
    /// </summary>
    /// <param name="token">The authentication token to validate.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the authentication operation to complete.</param>
    /// <returns>A task that represents the asynchronous authentication operation.</returns>
    Task<IFdwResult<IAuthenticationContext>> Authenticate(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the provided token is currently valid without performing full authentication.
    /// </summary>
    /// <param name="token">The authentication token to validate.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the validation operation to complete.</param>
    /// <returns>A task that represents the asynchronous validation operation.</returns>
    Task<IFdwResult<bool>> ValidateToken(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an authentication token using the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to use for obtaining a new authentication token.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the refresh operation to complete.</param>
    /// <returns>A task that represents the asynchronous token refresh operation.</returns>
    Task<IFdwResult<string>> RefreshToken(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the specified authentication token, making it invalid for future use.
    /// </summary>
    /// <param name="token">The authentication token to revoke.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the revocation operation to complete.</param>
    /// <returns>A task that represents the asynchronous token revocation operation.</returns>
    Task<IFdwResult> RevokeToken(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an authentication command and returns a typed result.
    /// Integrates with the command pattern for unified service execution.
    /// </summary>
    /// <typeparam name="TResult">The type of result expected from the command execution.</typeparam>
    /// <param name="command">The authentication command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command execution to complete.</param>
    /// <returns>A task that represents the asynchronous command execution operation.</returns>
    Task<IFdwResult<TResult>> Execute<TResult>(IAuthenticationCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an authentication command without returning a specific result type.
    /// Integrates with the command pattern for unified service execution.
    /// </summary>
    /// <param name="command">The authentication command to execute.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the command execution to complete.</param>
    /// <returns>A task that represents the asynchronous command execution operation.</returns>
    Task<IFdwResult> Execute(IAuthenticationCommand command, CancellationToken cancellationToken = default);
}
