using AutoMapper;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mite.Infrastructure.Automapper
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<Payment, OperationModel>()
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src =>
                {
                    switch (src.PaymentType)
                    {
                        case PaymentType.BankCard:
                            return "Банковская карта";
                        case PaymentType.YandexWallet:
                            return "Яндекс.Деньги";
                        case PaymentType.WebMoney:
                            return "WebMoney";
                        default:
                            return src.PaymentType.ToString();
                    }
                }));
        }
    }
}