using DTOs.PaymentDTOs;
using Net.payOS.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IPaymentService
    {
        Task<CreatePaymentResult> Checkout(List<CheckOutRequest> input);
        bool payOSTransferHandler(WebhookType body);
         Task<CreatePaymentResult> CreatePaymentLink(CreatePaymentLinkRequest body);
          Task<PaymentLinkInformation> GetOrder(int orderId);
         Task<PaymentLinkInformation> CancelOrder(int orderId);
         Task ConfirmWebhook(ConfirmWebhookRequest body);
        Task<IEnumerable<PaymentIsPaidResponse>> GetPaymentbyUserId(string id);
        Task<PaymentIsPaidResponse> GetCurrnetSubcription();


    }
}
