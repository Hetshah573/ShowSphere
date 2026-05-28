using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ShowSphere.Domain.Interfaces;

namespace ShowSphere.Infrastructure.PaymentGateways;

/// <summary>
/// Razorpay payment gateway implementation using REST API.
/// Implements IPaymentGateway (Strategy Pattern).
/// Switch to a different gateway by creating a new implementation and registering it in DI.
/// </summary>
public class RazorpayPaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly string _keyId;
    private readonly string _keySecret;

    public string ProviderName => "Razorpay";

    public RazorpayPaymentGateway(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _keyId = configuration["Payment:Razorpay:KeyId"]!;
        _keySecret = configuration["Payment:Razorpay:KeySecret"]!;

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_keyId}:{_keySecret}"));
        _httpClient.BaseAddress = new Uri("https://api.razorpay.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    public async Task<PaymentOrderResult> CreateOrderAsync(CreatePaymentOrderRequest request)
    {
        try
        {
            var payload = new
            {
                amount = (int)(request.Amount * 100), // Razorpay expects amount in paise
                currency = request.Currency,
                receipt = request.BookingNumber,
                notes = request.Metadata ?? new Dictionary<string, string>()
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("orders", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentOrderResult(false, null, $"Razorpay error: {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var orderId = doc.RootElement.GetProperty("id").GetString();

            return new PaymentOrderResult(true, orderId, GatewayKey: _keyId);
        }
        catch (Exception ex)
        {
            return new PaymentOrderResult(false, null, ex.Message);
        }
    }

    public Task<PaymentVerificationResult> VerifyPaymentAsync(VerifyPaymentRequest request)
    {
        try
        {
            // Razorpay signature verification:
            // generated_signature = HMAC-SHA256(order_id + "|" + payment_id, secret)
            var payload = $"{request.OrderId}|{request.PaymentId}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_keySecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = BitConverter.ToString(computedHash).Replace("-", "").ToLowerInvariant();

            if (computedSignature == request.Signature)
            {
                return Task.FromResult(new PaymentVerificationResult(true, request.PaymentId));
            }

            return Task.FromResult(new PaymentVerificationResult(false, Error: "Invalid payment signature"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new PaymentVerificationResult(false, Error: ex.Message));
        }
    }

    public async Task<RefundResult> RefundAsync(RefundRequest request)
    {
        try
        {
            var payload = new
            {
                amount = (int)(request.Amount * 100),
                notes = new { reason = request.Reason }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"payments/{request.PaymentId}/refund", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new RefundResult(false, Error: $"Refund failed: {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var refundId = doc.RootElement.GetProperty("id").GetString();

            return new RefundResult(true, refundId);
        }
        catch (Exception ex)
        {
            return new RefundResult(false, Error: ex.Message);
        }
    }
}
