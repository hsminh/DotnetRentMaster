using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payments.MoMo.Models;

namespace Payments.MoMo.Services;

public class MoMoPaymentService : IMoMoPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly MoMoConfig _config;
    private readonly ILogger<MoMoPaymentService> _logger;

    public MoMoPaymentService(
        HttpClient httpClient, 
        IOptions<MoMoConfig> config,
        ILogger<MoMoPaymentService> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;
    }

public async Task<MoMoPaymentResponse> CreatePaymentAsync(MoMoPaymentRequest request)
{
    try
    {
        // Set default values
        request.PartnerCode = _config.PartnerCode;
        request.RequestType = "captureWallet";
        request.IpnUrl = _config.NotifyUrl;
        request.RedirectUrl = _config.ReturnUrl;
        request.Lang = "vi";
        
        // Ensure required fields
        request.ExtraData ??= "";
        request.OrderInfo = string.IsNullOrEmpty(request.OrderInfo) 
            ? $"Payment for order {request.OrderId}" 
            : request.OrderInfo;

        // 1. URL encode values that need encoding for both signature and request
        var encodedOrderInfo = Uri.EscapeDataString(request.OrderInfo);
        
        // Create the raw hash string with exact parameter order as per MoMo's documentation
        // Note: The signature MUST be generated from the same URL-encoded values sent in the request
        var rawHash = new StringBuilder();
        
        // Add parameters in the exact order required by MoMo using URL-encoded values
        rawHash.Append($"accessKey={_config.AccessKey}&");
        rawHash.Append($"amount={request.Amount}&");
        rawHash.Append($"extraData={request.ExtraData}&");
        rawHash.Append($"ipnUrl={_config.NotifyUrl}&");
        rawHash.Append($"orderId={request.OrderId}&");
        rawHash.Append($"orderInfo={encodedOrderInfo}&");  // Use URL-encoded value for signature
        rawHash.Append($"partnerCode={_config.PartnerCode}&");
        rawHash.Append($"redirectUrl={_config.ReturnUrl}&");
        rawHash.Append($"requestId={request.RequestId}&");
        rawHash.Append($"requestType={request.RequestType}");
        
        request.IpnUrl = _config.NotifyUrl;
        request.RedirectUrl = _config.ReturnUrl;

        var rawHashString = rawHash.ToString();
        _logger.LogInformation("Raw hash for signature: {RawHash}", rawHashString);
        
        // 2. Generate signature from the raw hash
        request.Signature = GenerateSignature(rawHashString);
        _logger.LogInformation("Generated signature: {Signature}", request.Signature);
        
        // Log the exact string being hashed (for debugging)
        _logger.LogDebug("String being hashed: {RawHashString}", rawHashString);
        _logger.LogDebug("Secret Key: {SecretKey}", _config.SecretKey);

        // 3. Create request object with properties in camelCase and proper URL encoding
        var requestObj = new
        {
            partnerCode = request.PartnerCode,
            requestId = request.RequestId,
            amount = request.Amount,
            orderId = request.OrderId,
            orderInfo = encodedOrderInfo,  // Use the same URL-encoded value used in signature
            redirectUrl = request.RedirectUrl,
            ipnUrl = request.IpnUrl,
            requestType = request.RequestType,
            extraData = request.ExtraData,
            lang = request.Lang,
            signature = request.Signature
        };

        // 4. Serialize with camelCase
        var json = JsonSerializer.Serialize(requestObj, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        
        _logger.LogInformation("Request JSON: {Json}", json);
        
        // 5. Send request
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_config.ApiEndpoint, content);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        _logger.LogInformation("MoMo API Response: {StatusCode} - {Response}", 
            response.StatusCode, responseContent);
            
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"MoMo API request failed with status {response.StatusCode}");
        }

        var paymentResponse = JsonSerializer.Deserialize<MoMoPaymentResponse>(responseContent);
        if (paymentResponse == null)
        {
            throw new Exception("Failed to deserialize MoMo response");
        }

        return paymentResponse;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in CreatePaymentAsync");
        throw;
    }
}
    public string GenerateSignature(string text)
    {
        try
        {
            _logger.LogInformation("Generating signature for text: {Text}", text);
            
            if (string.IsNullOrEmpty(_config.SecretKey))
            {
                _logger.LogError("Secret key is null or empty");
                throw new ArgumentNullException(nameof(_config.SecretKey), "Secret key cannot be null or empty");
            }
            
            // Convert secret key to bytes using UTF8 encoding
            var secretKeyBytes = Encoding.UTF8.GetBytes(_config.SecretKey);
            
            // Convert message to bytes using UTF8 encoding
            var messageBytes = Encoding.UTF8.GetBytes(text);
            
            // Create HMAC-SHA256 hasher with the secret key
            using var hmac = new HMACSHA256(secretKeyBytes);
            
            // Compute hash
            var hashBytes = hmac.ComputeHash(messageBytes);
            
            // Convert the byte array to a hexadecimal string (lowercase, no dashes)
            var signature = new StringBuilder();
            foreach (var b in hashBytes)
            {
                signature.Append(b.ToString("x2")); // x2 formats as lowercase hex
            }
            
            var result = signature.ToString();
            _logger.LogInformation("Successfully generated signature");
            _logger.LogDebug("Signature details - Input: {Input}, Output: {Signature}", text, result);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating signature. Secret Key Length: {KeyLength}, Input Length: {InputLength}", 
                _config.SecretKey?.Length ?? 0, text?.Length ?? 0);
            throw new Exception("Failed to generate signature. Please check the logs for more details.", ex);
        }
    }

    public bool VerifySignature(string text, string signature)
    {
        try
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Cannot verify signature: text or signature is empty");
                return false;
            }

            var generatedSignature = GenerateSignature(text);
            var isValid = generatedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
            
            _logger.LogInformation("Signature verification: {IsValid} - Generated: {Generated}, Provided: {Provided}",
                isValid, generatedSignature, signature);
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying signature");
            return false;
        }
    }

    public string GenerateRequestId()
    {
        // Use a more unique and reliable format
        // MoMo expects: MM + timestamp + random number
        return "MM" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Random.Shared.Next(1000, 9999);
    }
    
    public string GetIpnUrl()
    {
        return _config.NotifyUrl;
    }
    
    public string GetReturnUrl()
    {
        return _config.ReturnUrl;
    }
    
    public string GetPartnerCode()
    {
        return _config.PartnerCode;
    }
    
    public string GetAccessKey()
    {
        return _config.AccessKey;
    }
}
