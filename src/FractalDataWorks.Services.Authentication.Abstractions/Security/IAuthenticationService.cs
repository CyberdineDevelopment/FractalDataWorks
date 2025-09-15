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
public interface IAuthenticationService : IFractalService
{
    /// <summary>
    /// Authenticates a user based on the provided authentication token.
    /// </summary>
    /// <param name="token">
    /// The authentication token to validate. This can be a JWT bearer token,
    /// API key, or any other token format supported by the implementation.
    /// Cannot be null or empty.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the authentication operation to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous authentication operation.
    /// The task result contains an IFdwResult with the IAuthenticationContext
    /// if authentication succeeds, or error information if authentication fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs comprehensive token validation including:
    /// - Token format verification
    /// - Signature validation (for signed tokens like JWT)
    /// - Expiration checking
    /// - Issuer validation
    /// - Custom claim validation
    /// </para>
    /// <para>
    /// On successful authentication, the returned context will contain
    /// user identity information, roles, permissions, and authentication metadata.
    /// </para>
    /// <para>
    /// Common failure scenarios include:
    /// - Invalid token format
    /// - Expired tokens
    /// - Invalid signatures
    /// - Revoked tokens
    /// - Network connectivity issues (for remote validation)
    /// </para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="token"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="token"/> is empty or whitespace.
    /// </exception>
    Task<IFdwResult<IAuthenticationContext>> AuthenticateAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether the provided token is currently valid without performing full authentication.
    /// </summary>
    /// <param name="token">
    /// The authentication token to validate. This can be a JWT bearer token,
    /// API key, or any other token format supported by the implementation.
    /// Cannot be null or empty.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the validation operation to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous validation operation.
    /// The task result contains an IFdwResult with a boolean value indicating
    /// whether the token is valid (true) or invalid (false).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides lightweight token validation that checks basic
    /// validity without constructing a full authentication context. It's
    /// optimized for scenarios where you only need to verify token validity
    /// without requiring user details or claims.
    /// </para>
    /// <para>
    /// Validation includes:
    /// - Token format verification
    /// - Basic signature validation
    /// - Expiration checking
    /// - Revocation status (if supported)
    /// </para>
    /// <para>
    /// This method is typically faster than AuthenticateAsync as it doesn't
    /// construct the full authentication context or resolve user details.
    /// </para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="token"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="token"/> is empty or whitespace.
    /// </exception>
    Task<IFdwResult<bool>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an authentication token using the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">
    /// The refresh token to use for obtaining a new authentication token.
    /// Cannot be null or empty.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the refresh operation to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous token refresh operation.
    /// The task result contains an IFdwResult with the new authentication token
    /// if the refresh succeeds, or error information if the refresh fails.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Token refresh allows clients to obtain new authentication tokens without
    /// requiring the user to re-authenticate. This is essential for maintaining
    /// long-running sessions while keeping authentication tokens short-lived
    /// for security purposes.
    /// </para>
    /// <para>
    /// The refresh operation validates the refresh token and issues a new
    /// authentication token with the same or updated permissions and claims.
    /// Some implementations may also issue a new refresh token as part of
    /// token rotation security practices.
    /// </para>
    /// <para>
    /// Common failure scenarios include:
    /// - Invalid refresh token format
    /// - Expired refresh tokens
    /// - Revoked refresh tokens
    /// - Network connectivity issues
    /// - Rate limiting on refresh operations
    /// </para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="refreshToken"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="refreshToken"/> is empty or whitespace.
    /// </exception>
    Task<IFdwResult<string>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the specified authentication token, making it invalid for future use.
    /// </summary>
    /// <param name="token">
    /// The authentication token to revoke. This can be a JWT bearer token,
    /// API key, or any other token format supported by the implementation.
    /// Cannot be null or empty.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token to observe while waiting for the revocation operation to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous token revocation operation.
    /// The task result contains an IFdwResult indicating success or failure
    /// of the revocation operation.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Token revocation permanently invalidates an authentication token,
    /// preventing its use in future authentication attempts. This is essential
    /// for security scenarios such as user logout, token compromise, or
    /// administrative token management.
    /// </para>
    /// <para>
    /// The revocation process varies by authentication mechanism:
    /// - For JWT tokens, this may involve adding to a blacklist
    /// - For API keys, this updates the key's active status
    /// - For OAuth2 tokens, this calls the revocation endpoint
    /// </para>
    /// <para>
    /// Once revoked, the token should immediately fail validation in all
    /// subsequent authentication attempts across all services and endpoints.
    /// </para>
    /// <para>
    /// Note: Some token types (like stateless JWT tokens) may require
    /// distributed blacklist mechanisms for effective revocation.
    /// </para>
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="token"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="token"/> is empty or whitespace.
    /// </exception>
    Task<IFdwResult> RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
}
