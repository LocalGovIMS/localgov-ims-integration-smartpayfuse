using Application.Commands;
using Application.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Linq;

namespace Application.Extensions
{
    public static class PaymentExtensions
    {
        public static bool CardDetailsNeedUpdating(this Payment payment)
        {
            return payment.Status == LocalGovIMSResults.Authorised && payment.CardPrefix == null;

        }
    }
}
