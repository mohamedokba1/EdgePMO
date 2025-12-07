using AutoMapper;
using EdgePMO.API.Dtos;
using EdgePMO.API.Models;

namespace EdgePMO.API.Settings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserReadDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt ?? DateTime.MinValue));
            CreateMap<CourseVideo, CourseVideoReadDto>();

            CreateMap<Instructor, InstructorReadDto>();

            CreateMap<Course, CourseReadDto>()
                .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
                .ForMember(dest => dest.Instructor, opt => opt.MapFrom(src => src.Instructor))
                .ForMember(dest => dest.CourseVideos, opt => opt.MapFrom(src => src.CourseVideos))
                .ForMember(dest => dest.Students, opt => opt.MapFrom(src => src.CourseUsers.Select(cu => cu.User)));

            CreateMap<UserTemplate, UserTemplateReadDto>()
                    .ForMember(d => d.TemplateId, opt => opt.MapFrom(s => s.Template.Id))
                    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Template.Name))
                    .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.Template.CoverImageUrl))
                    .ForMember(d => d.Price, opt => opt.MapFrom(s => s.Template.Price))
                    .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Template.Category))
                    .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.Template.IsActive))
                    .ForMember(d => d.PurchasedAt, opt => opt.MapFrom(s => s.PurchasedAt))
                    .ForMember(d => d.DownloadedAt, opt => opt.MapFrom(s => s.DownloadedAt))
                    .ForMember(d => d.IsFavorite, opt => opt.MapFrom(s => s.IsFavorite));

            CreateMap<TemplateUpdateDto, Template>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                    srcMember != null && (!(srcMember is string s) || !string.IsNullOrWhiteSpace(s))
                ));

            CreateMap<UserTemplate, UserTemplateReadDto>()
                .ForMember(d => d.TemplateId, opt => opt.MapFrom(s => s.Template.Id))
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Template.Name))
                .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.Template.CoverImageUrl))
                .ForMember(d => d.Price, opt => opt.MapFrom(s => s.Template.Price))
                .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Template.Category))
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.Template.IsActive))
                .ForMember(d => d.PurchasedAt, opt => opt.MapFrom(s => s.PurchasedAt))
                .ForMember(d => d.DownloadedAt, opt => opt.MapFrom(s => s.DownloadedAt))
                .ForMember(d => d.IsFavorite, opt => opt.MapFrom(s => s.IsFavorite));

            CreateMap<Template, TemplateReadDto>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d.CoverImageUrl, opt => opt.MapFrom(s => s.CoverImageUrl))
                .ForMember(d => d.UsersPurchased, opt => opt.MapFrom(s => s.UserTemplates));
       
            CreateMap<Template, TemplateBriefDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.Price))
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Category))
                .ForMember(d => d.CoverImageUrl, o => o.MapFrom(s => s.CoverImageUrl))
                .ForMember(d => d.FilePath, o => o.MapFrom(s => s.FilePath))
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive));

            CreateMap<Course, CourseBriefDto>()
                .ForMember(d => d.CourseId, o => o.MapFrom(s => s.CourseId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.CoursePictureUrl, o => o.MapFrom(s => s.CoursePictureUrl));


            CreateMap<PurchaseRequest, PurchaseRequestResponseDto>()
                .ForMember(d => d.Template, o => o.MapFrom(s => s.Template))
                .ForMember(d => d.Course, o => o.MapFrom(s => s.Course))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
                    srcMember != null));
        }
    }
}

