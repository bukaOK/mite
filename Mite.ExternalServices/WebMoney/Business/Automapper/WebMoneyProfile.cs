using AutoMapper;
using Mite.ExternalServices.WebMoney.Params;
using Mite.ExternalServices.WebMoney.Requests;

namespace Mite.ExternalServices.WebMoney.Business.Automapper
{
    public class WebMoneyProfile : Profile
    {
        public WebMoneyProfile()
        {
            CreateMap<ExpressPaymentParams, ExpressPaymentRequest>()
                .ForMember(dest => dest.ClientPhone, opt => opt.MapFrom(src => src.ClientPhone.ToString()))
                .ForMember(dest => dest.LoginType, opt => opt.MapFrom(src => (byte)src.LoginType))
                .ForMember(dest => dest.ConfirmType, opt => opt.MapFrom(src => (byte)src.ConfirmType))
                .ForMember(dest => dest.EmulatedFlag, opt => opt.MapFrom(src => src.EmulatedFlag ? 1 : 0));
            CreateMap<ExpressPaymentConfirmParams, ExpressPaymentConfirmRequest>();
        }
    }
}
