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


        }

    }
}
