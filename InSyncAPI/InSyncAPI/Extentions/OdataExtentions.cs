using BusinessObjects.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using System.Globalization;
using System.Security.Permissions;

namespace WebNewsAPIs.Extentions
{
    public static class OdataExtentions
    {
        public static void ConfigOdata(this IServiceCollection services)
        {
            services.AddControllers()
                    .AddOData(builderOdata => builderOdata.Select().Filter().Count().OrderBy().Expand().SetMaxTop(100)
                    .AddRouteComponents("odata", getEdmModel()));
        }
        public static IEdmModel getEdmModel()
        {
            ODataConventionModelBuilder conventionModelBuilder = new ODataConventionModelBuilder();
            conventionModelBuilder.EntitySet<Asset>("Assets");
                
            conventionModelBuilder.EntitySet<CustomerReview>("CustomerReviews");
            conventionModelBuilder.EntitySet<PrivacyPolicy>("PrivacyPolicys");
                
            conventionModelBuilder.EntitySet<Project>("Projects");
            conventionModelBuilder.EntitySet<Scenario>("Scenarios");
            conventionModelBuilder.EntitySet<SubscriptionPlan>("SubscriptionPlans");
            conventionModelBuilder.EntitySet<Term>("Terms");
            conventionModelBuilder.EntitySet<Tutorial>("Tutorials");
            conventionModelBuilder.EntitySet<User>("Users");
            conventionModelBuilder.EntitySet<UserSubscription>("UserSubscription");


            return conventionModelBuilder.GetEdmModel();
        }
    }
}
