using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Payments.VnPay.Models;
using Payments.VnPay.Services;

namespace Management.RealEstate.Controllers.Payment;

[ApiController]
[Route("consumer/api/vnpay")]
public class VnPayPaymentController : ControllerBase
{
    private readonly IVnPayService _vnPayService;
    private readonly ILogger<VnPayPaymentController> _logger;
    private readonly VnPayConfig _vnPayConfig;

    public VnPayPaymentController(
        IVnPayService vnPayService,
        IOptions<VnPayConfig> vnPayConfig,
        ILogger<VnPayPaymentController> logger)
    {
        _vnPayService = vnPayService ?? throw new ArgumentNullException(nameof(vnPayService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _vnPayConfig = vnPayConfig?.Value ?? throw new ArgumentNullException(nameof(vnPayConfig));
    }

    /// <summary>
    /// Create a payment URL for VNPay
    /// </summary>
    /// <param name="request">Payment request details</param>
    /// <returns>VNPay payment URL</returns>
    [HttpPost("payment")]
    public async Task<IActionResult> CreatePayment([FromBody] VnPayPaymentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            return BadRequest(ModelState);

            // Set the client IP address
            request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            
            // Set the return URL if not provided
            if (string.IsNullOrEmpty(request.ReturnUrl))
            {
                request.ReturnUrl = _vnPayConfig.ReturnUrl;
            }

            var result = await _vnPayService.CreatePaymentUrlAsync(request);
            
            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating VNPay payment URL");
            return StatusCode(500, new { Message = "An error occurred while creating payment URL" });
        }
    }

    /// <summary>
    /// Handle VNPay payment return
    /// </summary>
    /// <param name="vnp_Amount">Amount</param>
    /// <param name="vnp_BankCode">Bank code</param>
    /// <param name="vnp_BankTranNo">Bank transaction number</param>
    /// <param name="vnp_CardType">Card type</param>
    /// <param name="vnp_OrderInfo">Order information</param>
    /// <param name="vnp_PayDate">Payment date</param>
    /// <param name="vnp_ResponseCode">Response code</param>
    /// <param name="vnp_TmnCode">Merchant code</param>
    /// <param name="vnp_TransactionNo">Transaction number</param>
    /// <param name="vnp_TransactionStatus">Transaction status</param>
    /// <param name="vnp_TxnRef">Transaction reference</param>
    /// <param name="vnp_SecureHash">Secure hash</param>
    /// <returns>Payment result</returns>
    [HttpGet("payment-return")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymentReturn(
        [FromQuery] string vnp_Amount,
        [FromQuery] string vnp_BankCode,
        [FromQuery] string vnp_BankTranNo,
        [FromQuery] string vnp_CardType,
        [FromQuery] string vnp_OrderInfo,
        [FromQuery] string vnp_PayDate,
        [FromQuery] string vnp_ResponseCode,
        [FromQuery] string vnp_TmnCode,
        [FromQuery] string vnp_TransactionNo,
        [FromQuery] string vnp_TransactionStatus,
        [FromQuery] string vnp_TxnRef,
        [FromQuery] string vnp_SecureHash)
    {
        try
        {
            // Get all query parameters
            var parameters = HttpContext.Request.Query
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

            // Process the payment return
            var result = await _vnPayService.ProcessPaymentReturnAsync(parameters);

            // Log the payment return
            _logger.LogInformation("Payment return processed for order {OrderId}: {Success}", 
                result.OrderId, result.Success);

            // Here you can redirect to a success/failure page in your frontend
            // or return the result as JSON for SPA applications
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment return");
            return StatusCode(500, new { Message = "An error occurred while processing payment return" });
        }
    }

    /// <summary>
    /// Handle VNPay IPN (Instant Payment Notification)
    /// </summary>
    /// <returns>IPN response</returns>
    [HttpPost("ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> IpnHandler()
    {
        try
        {
            // Get all form parameters
            var form = await HttpContext.Request.ReadFormAsync();
            var parameters = form.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());

            // Process the IPN
            var result = await _vnPayService.ProcessIpnAsync(parameters);

            // Log the IPN
            _logger.LogInformation("IPN processed with response code: {RspCode}", result.RspCode);

            // Return the IPN response
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing IPN");
            return StatusCode(500, new VnPayIpnResponse 
            { 
                RspCode = "99", 
                Message = "Error processing IPN" 
            });
        }
    }

    /// <summary>
    /// Refund a transaction
    /// </summary>
    /// <param name="request">Refund request details</param>
    /// <returns>Refund result</returns>
    [HttpPost("refund")]
    public async Task<IActionResult> Refund([FromBody] VnPayRefundRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set the request by to the current user
            request.RequestBy = User.Identity?.Name ?? "system";

            var result = await _vnPayService.ProcessRefundAsync(request);
            
            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for transaction {TransactionId}", request.TransactionId);
            return StatusCode(500, new { Message = "An error occurred while processing refund" });
        }
    }

    /// <summary>
    /// Query transaction status
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <param name="transactionDate">Transaction date</param>
    /// <returns>Transaction status</returns>
    [HttpGet("query-transaction")]
    public async Task<IActionResult> QueryTransaction(
        [Required] string transactionId,
        [Required] DateTime transactionDate)
    {
        try
        {
            var result = await _vnPayService.QueryTransactionAsync(transactionId, transactionDate);
            
            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying transaction {TransactionId}", transactionId);
            return StatusCode(500, new { Message = "An error occurred while querying transaction" });
        }
    }
}
