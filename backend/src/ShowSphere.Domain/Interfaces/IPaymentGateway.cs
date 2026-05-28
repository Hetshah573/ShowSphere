namespace ShowSphere.Domain.Interfaces;

/// <summary>
/// Payment gateway abstraction (Strategy Pattern).
/// Implement this interface for any payment provider (Razorpay, Stripe, PayPal, etc.)
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Creates an order/payment intent on the gateway side.
    /// Returns a gateway-specific order ID that the frontend uses to initiate checkout.
    /// </summary>
    Task<PaymentOrderResult> CreateOrderAsync(CreatePaymentOrderRequest request);

    /// <summary>
    /// Verifies a payment after the user completes checkout on the frontend.
    /// Validates signature/webhook to ensure payment is authentic.
    /// </summary>
    Task<PaymentVerificationResult> VerifyPaymentAsync(VerifyPaymentRequest request);

    /// <summary>
    /// Initiates a refund for a confirmed payment.
    /// </summary>
    Task<RefundResult> RefundAsync(RefundRequest request);

    /// <summary>
    /// Returns the provider name (e.g., "Razorpay", "Stripe").
    /// </summary>
    string ProviderName { get; }
}

public record CreatePaymentOrderRequest(
    string BookingNumber,
    decimal Amount,
    string Currency,
    string CustomerEmail,
    string CustomerName,
    Dictionary<string, string>? Metadata = null);

public record PaymentOrderResult(
    bool Success,
    string? OrderId,
    string? Error = null,
    string? GatewayKey = null);

public record VerifyPaymentRequest(
    string OrderId,
    string PaymentId,
    string Signature);

public record PaymentVerificationResult(
    bool IsValid,
    string? TransactionId = null,
    string? Error = null);

public record RefundRequest(
    string PaymentId,
    decimal Amount,
    string Reason);

public record RefundResult(
    bool Success,
    string? RefundId = null,
    string? Error = null);
