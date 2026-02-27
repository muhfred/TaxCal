namespace TaxCal.Application.Interfaces;

/// <summary>
/// Extension point for future tax credits. Implementations provide credit amounts to subtract from total tax (e.g. from external service by employee id).
/// Not used in MVP; application layer may call this after calculation when the feature is implemented.
/// </summary>
public interface ITaxCreditProvider
{
    /// <summary>
    /// Get the total credit amount to subtract from total taxes for the given context.
    /// </summary>
    /// <param name="countryCode">Country code (e.g. DE, ES).</param>
    /// <param name="employeeId">Optional employee identifier for lookups.</param>
    /// <param name="totalTaxes">Calculated total taxes before credits.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Credit amount to subtract from total taxes (e.g. 0 if no credits).</returns>
    Task<decimal> GetCreditsAsync(string countryCode, string? employeeId, decimal totalTaxes, CancellationToken cancellationToken = default);
}
