using AutoMapper;
using Azure;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using Repositories.Interfaces;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        public PaymentService(PayOS payOS, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _payOS = payOS;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }
        public async Task<CreatePaymentResult> Checkout(List<CheckOutRequest> input)
        {
            try
            {
                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));
                List<ItemData> items = new List<ItemData>();

                foreach (var data in input) {
                    items.Add(new ItemData (data.Name, data.Quantity, data.Price));                           
                }
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
                var userId = _tokenService.GetUserIdFromToken();

                var userPayment = new UserPayment()
                {
                    Id = createPayment.orderCode.ToString(),
                    description = body.description,
                    Amount = body.price,
                    PaymentMethod = "QR",
                    PaymentDate = DateTime.Now,
                    PaymentStatus = "Pending",
                    SmartDietUserId = userId,
                    CreatedBy = userId,
                    CreatedTime = DateTime.Now,
                };


                await _unitOfWork.Repository<UserPayment>().AddAsync(userPayment);
                await _unitOfWork.SaveChangeAsync();
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
                var userId = _tokenService.GetUserIdFromToken();

                var existingUserPayment = await _unitOfWork.Repository<UserPayment>().GetByIdAsync(orderId.ToString())
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "UserPayment does not exist!");

                existingUserPayment.PaymentStatus = paymentLinkInformation.status.ToString();
                existingUserPayment.LastUpdatedTime = DateTime.UtcNow;
                existingUserPayment.LastUpdatedBy = userId;

                await _unitOfWork.Repository<UserPayment>().UpdateAsync(existingUserPayment);
                await _unitOfWork.SaveChangeAsync();
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
