using API.DTO;
using API.Entities;
using API.Extentions;
using AutoMapper;
using System.ComponentModel;
using System.Linq;

namespace API.Heplers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>().ForMember(dest => dest.PhotoUrl, 
                                                        opt => opt.MapFrom(src=>src.Photos.FirstOrDefault(x=>x.IsMain).Url))
                                            .ForMember(dest=>dest.Age, opt=>opt.MapFrom(src=>src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<Message, MessageDto>()
                .ForMember(x=>x.SenderPhotoUrl, y=>y.MapFrom(s=>s.Sender.Photos.FirstOrDefault(p=>p.IsMain).Url))
                .ForMember(x=>x.RecipientPhotoUrl, y=>y.MapFrom(s=>s.Recipient.Photos.FirstOrDefault(p=>p.IsMain).Url));
        }
    }
}
