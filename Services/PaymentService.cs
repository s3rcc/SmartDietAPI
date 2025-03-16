using AutoMapper;
using Azure;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.PaymentDTOs;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                var userId = _tokenService.GetUserIdFromToken();

                if (body.price <= 0 || string.IsNullOrEmpty(body.productName))
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Invalid payment details");
                }

                int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));

                ItemData item = new ItemData(body.productName, 1, body.price);
                List<ItemData> items = new List<ItemData>();
                PaymentData paymentData = new PaymentData(orderCode, body.price, body.description, items, body.cancelUrl, body.returnUrl);

                CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);



                var existingUserPayment = await _unitOfWork.Repository<UserPayment>()
                     .GetAllAsync(
                         orderBy: q => q.OrderBy(x => x.CreatedTime),
                         include: q => q.Include(up => up.Subcription) 
                     );
                var subcription = await _unitOfWork.Repository<Subcription>().GetByIdAsync(body.subcriptionId)
                     ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Subscription not found!");

                var existing = existingUserPayment
                    .FirstOrDefault(x => x.SmartDietUserId == userId && x.PaymentStatus.ToLower() == "paid");

                if (existingUserPayment != null && 
                    existing?.CreatedTime.AddMonths(existing.Subcription.MonthOfSubcription) > DateTime.UtcNow) {
                    var subcriptionIsPard = await _unitOfWork.Repository<Subcription>().GetByIdAsync(existing.SubcriptionId);
                    {
                        throw new ErrorException(StatusCodes.Status409Conflict, ErrorCode.CONFLICT,
                            $"{userId} already has an active subscription: {existing.Subcription.Name}");
                    }

                }


                var userPayment = new UserPayment()
                {
                    Id = createPayment.orderCode.ToString(),
                    description = body.description,
                    SubcriptionId = body.subcriptionId,
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
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Failed to create payment link");

            }
        }
        // Get order
        public async Task<PaymentLinkInformation> GetOrder(int orderId)
        {
            try
            {
                PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(orderId);
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
        public async Task<PaymentIsPaidResponse> GetCurrnetSubcription()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var existingUserPayment = await _unitOfWork.Repository<UserPayment>()
                             .GetAllAsync(
                                 orderBy: q => q.OrderBy(x => x.CreatedTime),
                                 include: q => q.Include(up => up.Subcription)
                             );

                var existing = existingUserPayment
                    .FirstOrDefault(x => x.SmartDietUserId == userId && x.PaymentStatus.ToLower() == "paid")
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "UserPayment does not exist!");

                var subscription = await _unitOfWork.Repository<Subcription>().GetByIdAsync(existing.SubcriptionId)
                             ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Subcription does not exist!");
                if(existing.CreatedTime.AddMonths(subscription.MonthOfSubcription) < DateTime.Now)
                {
                    throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, $"Subcription {subscription.Name} had expired!");

                }
                 return new PaymentIsPaidResponse
                {
                    Name = subscription.Name,
                    Description = existing.description,
                    SmartDietUserId = existing.SmartDietUserId,
                    SubscriptionId = subscription.Id,
                    StartDate = existing.CreatedTime,
                    EndDate = existing.CreatedTime.AddMonths(subscription.MonthOfSubcription)
                };
            }
            catch (Exception exception)
            {

                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Get Payment Error");

            }

        }
        public async Task<IEnumerable<PaymentIsPaidResponse>> GetPaymentbyUserId(string id)
        {
            try
            {
                var existingUserPayment = await _unitOfWork.Repository<UserPayment>().FindAsync(x => x.SmartDietUserId == id)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "UserPayment does not exist!");
                var responseList = new List<PaymentIsPaidResponse>();

                foreach (var payment in existingUserPayment)
                {
                    var subscription = await _unitOfWork.Repository<Subcription>().GetByIdAsync(payment.SubcriptionId)
                        ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Subcription does not exist!");

                    var response = new PaymentIsPaidResponse
                    {
                        Name = subscription.Name,
                        Description = payment.description,
                        SmartDietUserId = payment.SmartDietUserId,
                        SubscriptionId = subscription.Id,
                        StartDate = payment.CreatedTime,
                        EndDate = payment.CreatedTime.AddMonths(subscription.MonthOfSubcription)
                    };

                    responseList.Add(response);
                }


                return responseList;
            }
            catch (Exception exception)
            {

                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Get Payment Error");

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
