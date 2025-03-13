using BusinessObjects.Base;
using DataAccessObjects.Migrations;
using DTOs.AuthDTOs;
using DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Services.Interfaces;
using System.Collections.Generic;

namespace SmartDietAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private IPaymentService _service;
        public PaymentController(IPaymentService service)
        {
            _service = service;
        }
        [HttpPost("/create-payment-link")]
        public async Task<IActionResult> Checkout(List<CheckOutRequest> input)
        {
            CreatePaymentResult result = await _service.Checkout(input);
            return Ok(ApiResponse<object>.Success(result));
        }

        [HttpPost("create")]

        public async Task<IActionResult> CreatePaymentLink(CreatePaymentLinkRequest body)
        {
            CreatePaymentResult result = await _service.CreatePaymentLink(body);
            return Ok(ApiResponse<object>.Success(result));

        }
        [HttpGet("get-order/{orderId}")]
        public async Task<IActionResult> GetOrder([FromRoute] int orderId)
        {
            PaymentLinkInformation result =  await _service.GetOrder(orderId);
            return Ok(ApiResponse<object>.Success(result));
        }
        [HttpGet("get-order-by/{userId}")]
        public async Task<IActionResult> GetPaymentbyUserId([FromRoute] string userId)
        {
            var result = await _service.GetPaymentbyUserId(userId);
            return Ok(ApiResponse<object>.Success(result));
        }
        [HttpGet("get-current-subscription")]
        public async Task<IActionResult> GetCurrnetSubcription()
        {
            var result = await _service.GetCurrnetSubcription();
            return Ok(ApiResponse<object>.Success(result));
        }
        [HttpPut("cancel-order/{orderId}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int orderId)
        {
            PaymentLinkInformation result =  await _service.CancelOrder(orderId);
            return Ok(ApiResponse<object>.Success(result));
        }
        [HttpPost("payos_transfer_handler")]
        public IActionResult PayOSTransferHandler(WebhookType body)
        {
            var isTransferSuccessful = _service.payOSTransferHandler(body);
            if (isTransferSuccessful)
            {
                return Ok(ApiResponse<object>.Success(null, "Transfer successful"));
            }
            else
            {
                return Ok(ApiResponse<object>.Error(null, "Transfer failed"));

            }
        }
        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook(ConfirmWebhookRequest body)
        {
           await _service.ConfirmWebhook(body);
            return Ok(ApiResponse<object>.Success(null, "Confirm-wedhook"));

        }
    }
}
