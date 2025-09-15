using FluentValidation;

namespace FractalDataWorks.Services.SecretManagement.AzureKeyVault.Commands;

/// <summary>
/// Validates Azure Key Vault secret management commands.
/// </summary>
public sealed class AzureKeyVaultCommandValidator : AbstractValidator<AzureKeyVaultCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultCommandValidator"/> class.
    /// </summary>
    public AzureKeyVaultCommandValidator()
    {
        RuleFor(x => x.CommandType)
            .NotEmpty()
            .WithMessage("Command type is required.");

        RuleFor(x => x.SecretKey)
            .NotEmpty()
            .When(x => x.CommandType is not "ListSecrets")
            .WithMessage("Secret key is required for this operation.");

        RuleFor(x => x.Parameters)
            .NotNull()
            .WithMessage("Command parameters must be initialized.");
    }
}