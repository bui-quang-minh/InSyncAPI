using AutoMapper;
using BusinessObjects.Models;
using InSyncAPI.Dtos;

namespace InSync_Api.MapperProfile
{
    public class InSyncMapperProfile: Profile
    {
       
        public InSyncMapperProfile() 
        {
            // CustomerReview 
            CreateMap<AddCustomerReviewDto, CustomerReview>().ReverseMap();
            CreateMap<UpdateCustomerReviewDto, CustomerReview>().ReverseMap();
            CreateMap<CustomerReview,ViewCustomerReviewDto>().ReverseMap();
            //PrivacyPolicy
            CreateMap<AddPrivacyPolicyDto, PrivacyPolicy>().ReverseMap();
            CreateMap<UpdatePrivacyPolicyDto, PrivacyPolicy>().ReverseMap();
            CreateMap<PrivacyPolicy, ViewPrivacyPolicyDto>().ReverseMap();
            //Term Dto 
            CreateMap<AddTermsDto, Term>().ReverseMap();
            CreateMap<UpdateTermsDto, Term>().ReverseMap();
            CreateMap<Term, ViewTermDto>().ReverseMap();
            //Tutorial Dto 
            CreateMap<AddTutorialDto, Tutorial>().ReverseMap();
            CreateMap<UpdateTutorialDto, Tutorial>().ReverseMap();
            CreateMap<Tutorial, ViewTutorialDto>().ReverseMap();
            // Subscription Plan
            CreateMap<AddSubscriptionPlanDto, SubscriptionPlan>().ReverseMap();          
            CreateMap<UpdateSubscriptionPlanDto, SubscriptionPlan>().ReverseMap();
            CreateMap<SubscriptionPlan, ViewSubscriptionPlanDto>()
                .ForMember(c => c.UserName, a => a.MapFrom(r => r.User.UserName))
                .ReverseMap();
            // Scenario Dto
           
            CreateMap<AddScenarioDto, Scenario>().ReverseMap();
            CreateMap<UpdateScenarioDto, Scenario>().ReverseMap();
            CreateMap<Scenario, ViewScenarioDto>()
                .ForMember(c => c.ProjectName, a => a.MapFrom(r => r.Project.ProjectName))
                .ForMember(c => c.UserName, a => a.MapFrom(r => r.CreatedByNavigation.UserName))
                .ReverseMap();
            // Project Dto
            CreateMap<AddProjectDto, Project>().ReverseMap();
            CreateMap<UpdateProjectDto, Project>().ReverseMap();
            CreateMap<Project, ViewProjectDto>()
                .ForMember(c => c.UserName, a => a.MapFrom(r => r.User.UserName))
                .ReverseMap();
            //Clerk Mapper
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Data.Email_Addresses.FirstOrDefault().Email_Address))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Data.Email_Addresses.FirstOrDefault().Email_Address))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Data.Profile_Image_Url))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Data.Phone_Numbers.FirstOrDefault()))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => (DateTime)DateTimeOffset.FromUnixTimeMilliseconds(src.Data.Created_At).UtcDateTime))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.Data.Updated_At == 0 ? (DateTime?)null : (DateTime)DateTimeOffset.FromUnixTimeMilliseconds(src.Data.Updated_At).UtcDateTime))
                .ReverseMap();
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Data.First_Name + " " + src.Data.Last_Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Data.Email_Addresses.FirstOrDefault().Email_Address))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Data.Profile_Image_Url))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Data.Phone_Numbers.FirstOrDefault()))
                .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(src => (DateTime)DateTimeOffset.FromUnixTimeMilliseconds(src.Data.Created_At).UtcDateTime))
                .ForMember(dest => dest.DateUpdated, opt => opt.MapFrom(src => src.Data.Updated_At == 0 ? (DateTime?)null : (DateTime)DateTimeOffset.FromUnixTimeMilliseconds(src.Data.Updated_At).UtcDateTime))
                .ReverseMap();


        }

    }
}
