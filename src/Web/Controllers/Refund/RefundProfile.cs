using Application.Models;
using AutoMapper;

namespace Web.Controllers
{
    public class RefundProfile : Profile
    {
        public RefundProfile()
        {
            CreateMap<RefundModel, Refund>();
        }
    }
}
