using AutoMapper;
using EdgePMO.API.Dtos;
using EdgePMO.API.Dtos.Courses;
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
                .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.CourseOutline))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Students, opt => opt.MapFrom(src => src.Students))
                .ForMember(dest => dest.Subtitle, opt => opt.MapFrom(src => src.Subtitle))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Sessions, opt => opt.MapFrom(src => src.Sessions))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.Level, opt => opt.MapFrom(src => src.Level))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                .ForMember(dest => dest.CoursePictureUrl, opt => opt.MapFrom(src => src.CoursePictureUrl))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Certification, opt => opt.MapFrom(src => src.Certification))
                .ForMember(dest => dest.SoftwareUsed, opt => opt.MapFrom(src => src.SoftwareUsed ?? new List<string>()))
                .ForMember(dest => dest.MainObjective, opt => opt.MapFrom(src => src.MainObjective ?? src.Description))
                .ForMember(dest => dest.WhatStudentsLearn, opt => opt.MapFrom(src => src.WhatStudentsLearn ?? new List<string>()))
                .ForMember(dest => dest.WhoShouldAttend, opt => opt.MapFrom(src => src.WhoShouldAttend ?? new List<string>()))
                .ForMember(dest => dest.Requirements, opt => opt.MapFrom(src => src.Requirements ?? new List<string>()))
                .ForMember(dest => dest.StudentsList, opt => opt.MapFrom(src => src.CourseUsers.Select(cu => cu.User).ToList()));

            CreateMap<CourseOutline, CourseContentReadDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.Order))
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => src.Videos ?? new List<CourseVideo>()))
                .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.Documents ?? new List<CourseDocument>()));

            CreateMap<CourseVideo, CourseVideoReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.DurationSeconds, opt => opt.MapFrom(src => src.DurationMinutes))
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src.Order));

            CreateMap<CourseDocument, CourseDocumentReadDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CourseDocumentId))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.DocumentUrl));
            
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

            CreateMap<CourseReview, CourseReviewReadDto>()
               .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
               .ForMember(d => d.Header, opt => opt.MapFrom(s => s.Header))
               .ForMember(d => d.Rating, opt => opt.MapFrom(s => s.Rating))
               .ForMember(d => d.Content, opt => opt.MapFrom(s => s.Content))
               .ForMember(d => d.Username, opt => opt.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}" ))
               .ForMember(d => d.Email, opt => opt.MapFrom(s => s.User.Email ));
        }
    }
}

