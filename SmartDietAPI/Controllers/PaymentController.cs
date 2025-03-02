using DTOs.AuthDTOs;
using DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Services.Interfaces;

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
        public async Task<IActionResult> Checkout()
        {
            CreatePaymentResult result = await _service.Checkout();
            return Ok(result);
        }

        [HttpPost("create")]

        public async Task<IActionResult> CreatePaymentLink(CreatePaymentLinkRequest body)
        {
            CreatePaymentResult result = await _service.CreatePaymentLink(body);
            return Ok(result);

        }
        [HttpGet("get-order/{orderId}")]
        public async Task<IActionResult> GetOrder([FromRoute] int orderId)
        {
            PaymentLinkInformation result =  await _service.GetOrder(orderId);
            return Ok(result);
        }
        [HttpPut("cancel-order/{orderId}")]
        public async Task<IActionResult> CancelOrder([FromRoute] int orderId)
        {
            PaymentLinkInformation result =  await _service.CancelOrder(orderId);
            return Ok(result);
        }
        [HttpPost("payos_transfer_handler")]
        public IActionResult PayOSTransferHandler(WebhookType body)
        {
            var isTransferSuccessful = _service.payOSTransferHandler(body);
            if (isTransferSuccessful)
            {
                return Ok("Transfer successful");
            }
            else
            {
                return BadRequest("Transfer failed"); // Hoặc bạn có thể xử lý theo cách khác nếu không thành công
            }
        }
        [HttpPost("confirm-webhook")]
        public async Task<IActionResult> ConfirmWebhook(ConfirmWebhookRequest body)
        {
           await _service.ConfirmWebhook(body);
            return Ok("Confirm-wedhook");
        }
    }
}
