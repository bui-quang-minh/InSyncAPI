﻿using Repositorys;

namespace InSync_Api.DependencyInjectService
{
    public static class DependencyInjectService
    {
        public static void InjectService(this IServiceCollection services)
        {
            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<ICustomerReviewRepository,  CustomerReviewRepository>(); 
            services.AddScoped<IPrivacyPolicyRepository, PrivacyPolicyRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IScenarioRepository, ScenarioRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<ITermRepository, TermRepository>();
            services.AddScoped<ITutorialRepository, TutorialRepository>();
            services.AddScoped<IUserRepository,  UserRepository>();
            services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();	
		}
    }
}
