using Repositorys;

namespace InSync_Api.DependencyInjectService
{
    public static class DependencyInjectService
    {
        public static void InjectService(this IServiceCollection services)
        {
            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<ICustomerReviewRepository,  CustomerReviewRepository>(); 
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IScenarioRepository, ScenarioRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<IUserRepository,  UserRepository>();
            services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();
            services.AddScoped<IPageRepository, PageRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<ICategoryDocumentRepository, CategoryDocumentRepository>();
        }
    }
}
