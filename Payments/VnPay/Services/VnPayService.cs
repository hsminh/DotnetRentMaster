using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Payments.VnPay.Models;

namespace Payments.VnPay.Services;

public class VnPayService : IVnPayService
{
    private readonly VnPayConfig _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<VnPayService> _logger;
    private readonly HttpClient _httpClient;

    public VnPayService(
        IOptions<VnPayConfig> config,
        IHttpContextAccessor httpContextAccessor,
        ILogger<VnPayService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Configure HttpClient
        _httpClient = httpClientFactory.CreateClient("VnPay");
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<VnPayPaymentResponse> CreatePaymentUrlAsync(VnPayPaymentRequest request)
    {
        try
        {
            _logger.LogInformation("Creating payment URL for order: {OrderId}", request.OrderId);

            // Set default values
            var tmnCode = !string.IsNullOrEmpty(request.TmnCode) ? request.TmnCode : _config.TmnCode;
            var returnUrl = !string.IsNullOrEmpty(request.ReturnUrl) ? request.ReturnUrl : _config.ReturnUrl;
            var ipAddress = !string.IsNullOrEmpty(request.IpAddress) ? request.IpAddress : GetClientIpAddress();
            
            // Build payment data
            var paymentData = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_Version"] = _config.Version,
                ["vnp_Command"] = _config.Command,
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = ((long)(request.Amount * 100)).ToString(), // Convert to VND
                ["vnp_CreateDate"] = (request.CreateDate ?? DateTime.Now).ToString("yyyyMMddHHmmss"),
                ["vnp_CurrCode"] = _config.CurrencyCode,
                ["vnp_IpAddr"] = ipAddress,
                ["vnp_Locale"] = request.Language ?? _config.Locale,
                ["vnp_OrderInfo"] = request.OrderInfo,
                ["vnp_OrderType"] = request.OrderType ?? _config.OrderType,
                ["vnp_ReturnUrl"] = GetFullUrl(returnUrl),
                ["vnp_TxnRef"] = request.OrderId,
            };

            // Add bank code if specified
            if (!string.IsNullOrEmpty(request.BankCode))
            {
                paymentData.Add("vnp_BankCode", request.BankCode);
            }

            // Add expire date if specified
            if (request.ExpireDate.HasValue)
            {
                paymentData.Add("vnp_ExpireDate", request.ExpireDate.Value.ToString("yyyyMMddHHmmss"));
            }

            // Generate signature
            var rawData = BuildRawHashData(paymentData);
            var signature = GenerateSignature(rawData);

            // Build payment URL
            var paymentUrl = BuildPaymentUrl(paymentData, signature);
            _logger.LogInformation("Generated VNPay URL for order {OrderId}", request.OrderId);

            return new VnPayPaymentResponse
            {
                Success = true,
                Message = "Payment URL created successfully",
                PaymentUrl = paymentUrl,
                TransactionId = paymentData["vnp_TxnRef"],
                OrderId = request.OrderId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment URL for order {OrderId}", request.OrderId);
            return new VnPayPaymentResponse
            {
                Success = false,
                Message = $"Error creating payment URL: {ex.Message}"
            };
        }
    }

    public async Task<VnPayPaymentResult> ProcessPaymentReturnAsync(Dictionary<string, string> parameters)
    {
        try
        {
            _logger.LogInformation("Processing payment return with parameters: {Parameters}", 
                string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}")));

            // Verify the signature first
            if (!parameters.TryGetValue("vnp_SecureHash", out var secureHash) || 
                !VerifySignature(parameters, secureHash))
            {
                _logger.LogWarning("Invalid signature in payment return");
                return new VnPayPaymentResult
                {
                    Success = false,
                    Message = "Invalid signature"
                };
            }

            // Parse the response
            var result = new VnPayPaymentResult
            {
                TransactionId = parameters.TryGetValue("vnp_TransactionNo", out var transNo) ? transNo : string.Empty,
                OrderId = parameters.TryGetValue("vnp_TxnRef", out var orderId) ? orderId : string.Empty,
                Amount = parameters.TryGetValue("vnp_Amount", out var amountStr) && long.TryParse(amountStr, out var amount) ? amount / 100 : 0,
                BankCode = parameters.TryGetValue("vnp_BankCode", out var bankCode) ? bankCode : string.Empty,
                BankTranNo = parameters.TryGetValue("vnp_BankTranNo", out var bankTranNo) ? bankTranNo : string.Empty,
                CardType = parameters.TryGetValue("vnp_CardType", out var cardType) ? cardType : string.Empty,
                PayDate = parameters.TryGetValue("vnp_PayDate", out var payDateStr) && 
                         DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var payDate) 
                         ? payDate : (DateTime?)null,
                ResponseCode = parameters.TryGetValue("vnp_ResponseCode", out var responseCode) ? responseCode : string.Empty,
                TransactionStatus = parameters.TryGetValue("vnp_TransactionStatus", out var status) ? status : string.Empty
            };

            // Check if payment was successful
            result.Success = result.ResponseCode == "00" && result.TransactionStatus == "00";
            result.Message = result.Success ? "Payment successful" : "Payment failed";

            _logger.LogInformation("Processed payment return for order {OrderId}: {Success}", result.OrderId, result.Success);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment return");
            return new VnPayPaymentResult
            {
                Success = false,
                Message = $"Error processing payment return: {ex.Message}"
            };
        }
    }

    public async Task<VnPayIpnResponse> ProcessIpnAsync(Dictionary<string, string> parameters)
    {
        try
        {
            _logger.LogInformation("Processing IPN with parameters: {Parameters}", 
                string.Join(", ", parameters.Select(p => $"{p.Key}={p.Value}")));

            // Verify the signature first
            if (!parameters.TryGetValue("vnp_SecureHash", out var secureHash) || 
                !VerifySignature(parameters, secureHash))
            {
                _logger.LogWarning("Invalid signature in IPN");
                return new VnPayIpnResponse
                {
                    RspCode = "97",
                    Message = "Invalid signature"
                };
            }

            // Get required parameters
            if (!parameters.TryGetValue("vnp_TxnRef", out var orderId) || 
                !parameters.TryGetValue("vnp_Amount", out var amountStr) ||
                !long.TryParse(amountStr, out var amount) ||
                !parameters.TryGetValue("vnp_ResponseCode", out var responseCode) ||
                !parameters.TryGetValue("vnp_TransactionNo", out var vnpTransactionNo) ||
                !parameters.TryGetValue("vnp_TransactionStatus", out var transactionStatus))
            {
                _logger.LogWarning("Missing required parameters in IPN");
                return new VnPayIpnResponse
                {
                    RspCode = "99",
                    Message = "Missing required parameters"
                };
            }

            // Here you would typically update your database with the payment status
            // For example:
            // await _orderService.UpdateOrderStatusAsync(orderId, transactionStatus, vnpTransactionNo);

            _logger.LogInformation("Successfully processed IPN for order {OrderId}", orderId);
            return new VnPayIpnResponse
            {
                RspCode = "00",
                Message = "Confirm success"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing IPN");
            return new VnPayIpnResponse
            {
                RspCode = "99",
                Message = $"Error processing IPN: {ex.Message}"
            };
        }
    }

    public async Task<VnPayRefundResponse> ProcessRefundAsync(VnPayRefundRequest request)
    {
        try
        {
            _logger.LogInformation("Processing refund for transaction: {TransactionId}", request.TransactionId);

            // Prepare parameters
            var requestData = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_RequestId"] = Guid.NewGuid().ToString("N"),
                ["vnp_Version"] = _config.Version,
                ["vnp_Command"] = "refund",
                ["vnp_TmnCode"] = _config.TmnCode,
                ["vnp_TxnRef"] = request.TransactionId,
                ["vnp_OrderInfo"] = request.Description ?? $"Refund for transaction {request.TransactionId}",
                ["vnp_TransactionNo"] = request.VnpTransactionNo,
                ["vnp_TransactionDate"] = request.TransactionDate.ToString("yyyyMMddHHmmss"),
                ["vnp_CreateBy"] = request.RequestBy,
                ["vnp_CreateDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
                ["vnp_Amount"] = ((long)(request.Amount * 100)).ToString(), // Convert to VND
                ["vnp_IpAddr"] = GetClientIpAddress()
            };

            // Generate signature
            var rawData = BuildRawHashData(requestData);
            var signature = GenerateSignature(rawData);
            
            // Add signature to request
            requestData.Add("vnp_SecureHashType", "SHA256");
            requestData.Add("vnp_SecureHash", signature);

            // Send request to VNPay
            var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync("/api/v1/refund", content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("VNPay API returned error: {StatusCode}", response.StatusCode);
                return new VnPayRefundResponse
                {
                    Success = false,
                    Message = $"VNPay API error: {response.StatusCode}"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = ParseQueryString(responseContent);

            // Process response
            var result = new VnPayRefundResponse
            {
                TransactionId = responseData.TryGetValue("vnp_TransactionNo", out var txnNo) ? txnNo : string.Empty,
                OrderId = responseData.TryGetValue("vnp_TxnRef", out var orderId) ? orderId : string.Empty,
                Amount = responseData.TryGetValue("vnp_Amount", out var amountStr) && long.TryParse(amountStr, out var amount) ? amount / 100 : 0,
                ResponseCode = responseData.TryGetValue("vnp_ResponseCode", out var responseCode) ? responseCode : string.Empty,
                Message = responseData.TryGetValue("vnp_Message", out var message) ? message : string.Empty,
                Success = responseCode == "00"
            };

            _logger.LogInformation("Processed refund for transaction {TransactionId}: {Success}", request.TransactionId, result.Success);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for transaction {TransactionId}", request.TransactionId);
            return new VnPayRefundResponse
            {
                Success = false,
                Message = $"Error processing refund: {ex.Message}"
            };
        }
    }

    public async Task<VnPayTransactionResponse> QueryTransactionAsync(string transactionId, DateTime transactionDate)
    {
        try
        {
            _logger.LogInformation("Querying transaction: {TransactionId}", transactionId);

            // Prepare parameters
            var requestData = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                ["vnp_RequestId"] = Guid.NewGuid().ToString("N"),
                ["vnp_Version"] = _config.Version,
                ["vnp_Command"] = "querydr",
                ["vnp_TmnCode"] = _config.TmnCode,
                ["vnp_TxnRef"] = transactionId,
                ["vnp_OrderInfo"] = $"Query transaction {transactionId}",
                ["vnp_TransDate"] = transactionDate.ToString("yyyyMMddHHmmss"),
                ["vnp_IpAddr"] = GetClientIpAddress()
            };

            // Generate signature
            var rawData = BuildRawHashData(requestData);
            var signature = GenerateSignature(rawData);
            
            // Add signature to request
            requestData.Add("vnp_SecureHashType", "SHA256");
            requestData.Add("vnp_SecureHash", signature);

            // Send request to VNPay
            var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync("/api/v1/query", content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("VNPay API returned error: {StatusCode}", response.StatusCode);
                return new VnPayTransactionResponse
                {
                    Success = false,
                    Message = $"VNPay API error: {response.StatusCode}"
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = ParseQueryString(responseContent);

            // Process response
            var result = new VnPayTransactionResponse
            {
                TransactionId = responseData.TryGetValue("vnp_TransactionNo", out var txnNo) ? txnNo : string.Empty,
                OrderId = responseData.TryGetValue("vnp_TxnRef", out var orderId) ? orderId : string.Empty,
                Amount = responseData.TryGetValue("vnp_Amount", out var amountStr) && long.TryParse(amountStr, out var amount) ? amount / 100 : 0,
                BankCode = responseData.TryGetValue("vnp_BankCode", out var bankCode) ? bankCode : string.Empty,
                BankTranNo = responseData.TryGetValue("vnp_BankTranNo", out var bankTranNo) ? bankTranNo : string.Empty,
                ResponseCode = responseData.TryGetValue("vnp_ResponseCode", out var responseCode) ? responseCode : string.Empty,
                TransactionStatus = responseData.TryGetValue("vnp_TransactionStatus", out var status) ? status : string.Empty,
                PayDate = responseData.TryGetValue("vnp_PayDate", out var payDateStr) ? payDateStr : null,
                Message = responseData.TryGetValue("vnp_Message", out var message) ? message : string.Empty,
                Success = responseCode == "00"
            };

            _logger.LogInformation("Queried transaction {TransactionId}: {Status}", transactionId, result.TransactionStatus);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying transaction {TransactionId}", transactionId);
            return new VnPayTransactionResponse
            {
                Success = false,
                Message = $"Error querying transaction: {ex.Message}"
            };
        }
    }

    #region Private Methods

    private string BuildRawHashData(SortedDictionary<string, string> data)
    {
        var sb = new StringBuilder();
        foreach (var kvp in data)
        {
            if (!string.IsNullOrEmpty(kvp.Value))
            {
                sb.Append(Uri.EscapeDataString(kvp.Key));
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(kvp.Value));
                sb.Append("&");
            }
        }

        // Remove last ampersand
        if (sb.Length > 0)
        {
            sb.Length--;
        }

        return sb.ToString();
    }

    private string GenerateSignature(string data)
    {
        try
        {
            var keyBytes = Encoding.UTF8.GetBytes(_config.HashSecret);
            var inputBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA512(keyBytes);
            var hashValue = hmac.ComputeHash(inputBytes);

            var hash = new StringBuilder();
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating VNPay signature");
            throw;
        }
    }

    private string BuildPaymentUrl(SortedDictionary<string, string> data, string signature)
    {
        var sb = new StringBuilder();
        sb.Append(_config.PaymentUrl);
        sb.Append("?");

        foreach (var kvp in data)
        {
            sb.Append(Uri.EscapeDataString(kvp.Key));
            sb.Append("=");
            sb.Append(Uri.EscapeDataString(kvp.Value));
            sb.Append("&");
        }

        sb.Append("vnp_SecureHash=");
        sb.Append(signature);
        sb.Append("&vnp_SecureHashType=SHA512");

        return sb.ToString();
    }

    private bool VerifySignature(Dictionary<string, string> data, string inputHash)
    {
        try
        {
            // Remove the signature from the data
            if (!data.Remove("vnp_SecureHash", out _) && 
                !data.Remove("vnp_SecureHashType", out _))
            {
                return false;
            }

            // Sort the parameters
            var sortedData = new SortedDictionary<string, string>(data, StringComparer.Ordinal);
            var rawData = BuildRawHashData(sortedData);
            var signature = GenerateSignature(rawData);

            return string.Equals(signature, inputHash, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying VNPay signature");
            return false;
        }
    }

    private string GetClientIpAddress()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return "127.0.0.1";

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress))
                return ipAddress;

            // Try to get the IP from X-Forwarded-For header
            if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var addresses = forwardedFor.ToString().Split(',');
                if (addresses.Length > 0 && !string.IsNullOrEmpty(addresses[0]))
                    return addresses[0].Trim();
            }

            // Try to get the IP from X-Real-IP header
            if (httpContext.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
            {
                if (!string.IsNullOrEmpty(realIp))
                    return realIp.ToString().Trim();
            }

            return "127.0.0.1";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting client IP address");
            return "127.0.0.1";
        }
    }

    private string GetFullUrl(string relativeUrl)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
            return relativeUrl;

        var scheme = request.Scheme;
        var host = request.Host.Value;
        var pathBase = request.PathBase.Value ?? string.Empty;
        
        if (relativeUrl.StartsWith("/"))
            relativeUrl = relativeUrl.Substring(1);
            
        return $"{scheme}://{host}{pathBase}/{relativeUrl}".Replace("///", "//").Replace("//", "/");
    }

    private Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        if (string.IsNullOrEmpty(queryString))
            return result;

        var queryParts = queryString.Split('&');
        foreach (var part in queryParts)
        {
            var keyValue = part.Split('=');
            if (keyValue.Length == 2)
            {
                var key = WebUtility.UrlDecode(keyValue[0]);
                var value = WebUtility.UrlDecode(keyValue[1]);
                result[key] = value;
            }
        }

        return result;
    }

    #endregion
}
