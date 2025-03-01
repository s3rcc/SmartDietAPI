using Azure;
using BusinessObjects.Exceptions;
using DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PayOS _payOS;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PaymentService(PayOS payOS, IHttpContextAccessor httpContextAccessor)
        {
            _payOS = payOS;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<CreatePaymentResult> Checkout()
        {
            try
            {
                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
                ItemData item = new ItemData("Mì tôm hảo hảo ly", 1, 1000);
                List<ItemData> items = new List<ItemData> { item };

                // Get the current request's base URL
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                PaymentData paymentData = new PaymentData(
                    orderCode,
                    2000,
                    "Thanh toan don hang",
                    items,
                    $"{baseUrl}/cancel",
                    $"{baseUrl}/success"
                );

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                return createPayment;
            }
            catch (System.Exception exception)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Checkout Error");
            }
        }

        public bool payOSTransferHandler(WebhookType body)
        {
            try
            {
                WebhookData data = _payOS.verifyPaymentWebhookData(body);

                if (data.description == "Ma giao dich thu nghiem" || data.description == "VQRIO123")
                {
                    return true;
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "tranfer handle error");
            }

        }
        // Create payment link
        public async Task<CreatePaymentResult> CreatePaymentLink(CreatePaymentLinkRequest body)
        {
            try
            {
                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
                ItemData item = new ItemData(body.productName, 1, body.price);
                List<ItemData> items = new List<ItemData>();
                items.Add(item);
                PaymentData paymentData = new PaymentData(orderCode, body.price, body.description, items, body.cancelUrl, body.returnUrl);

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);

                return createPayment;
            }
            catch (Exception exception)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Create paymentlink error");

            }
        }
        // Get order
        public async Task<PaymentLinkInformation> GetOrder(int orderId)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderId);
                return paymentLinkInformation;
            }
            catch (System.Exception exception)
            {

                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Get order error");

            }

        }
        // CancleOrder
        public async Task<PaymentLinkInformation> CancelOrder(int orderId)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.cancelPaymentLink(orderId);
                return paymentLinkInformation;
            }
            catch (Exception exception)
            {

                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Cancel order error");

            }

        }
        // ConfirmWedHook
        public async Task ConfirmWebhook(ConfirmWebhookRequest body)
        {
            try
            {
                await _payOS.confirmWebhook(body.webhook_url);
            }
            catch (System.Exception exception)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Confirm Wedhook error");
            }

        }
    }
}
