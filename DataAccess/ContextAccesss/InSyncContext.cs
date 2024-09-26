using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using BusinessObjects.Models;
using Microsoft.Extensions.Configuration;

namespace DataAccess.ContextAccesss
{
    public partial class InSyncContext : DbContext
    {
        public InSyncContext()
        {
        }

        public InSyncContext(DbContextOptions<InSyncContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Asset> Assets { get; set; } = null!;
        public virtual DbSet<CustomerReview> CustomerReviews { get; set; } = null!;
        public virtual DbSet<PrivacyPolicy> PrivacyPolicys { get; set; } = null!;
        public virtual DbSet<Project> Projects { get; set; } = null!;
        public virtual DbSet<Scenario> Scenarios { get; set; } = null!;
        public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
        public virtual DbSet<Term> Terms { get; set; } = null!;
        public virtual DbSet<Tutorial> Tutorials { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("server =103.9.77.22; database = InSync;uid=sa;pwd=InSync123!;MultipleActiveResultSets=True;Connection Timeout=30;Application Name=InSyncApi;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AssestName)
                    .HasMaxLength(255)
                    .HasColumnName("assest_name");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created");

                entity.Property(e => e.DateUdpated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_udpated");

                entity.Property(e => e.FilePath)
                    .HasColumnType("text")
                    .HasColumnName("file_path");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.Type)
                    .HasMaxLength(255)
                    .HasColumnName("type");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Assets)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("assets_project_id_foreign");
            });

            modelBuilder.Entity<CustomerReview>(entity =>
            {
                entity.ToTable("Customer_Reviews");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created");

                entity.Property(e => e.ImageUrl)
                    .HasColumnType("text")
                    .HasColumnName("image_url");

                entity.Property(e => e.IsShow).HasColumnName("is_show");

                entity.Property(e => e.JobTitle)
                    .HasMaxLength(150)
                    .HasColumnName("job_title");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Review)
                    .HasMaxLength(200)
                    .HasColumnName("review");
            });

            modelBuilder.Entity<PrivacyPolicy>(entity =>
            {
                entity.ToTable("Privacy_Policys");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created");

                entity.Property(e => e.DateUpdated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_updated");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.Title)
                    .HasMaxLength(300)
                    .HasColumnName("title");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created");

                entity.Property(e => e.DateUpdated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_updated");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.IsPublish).HasColumnName("is_publish");

                entity.Property(e => e.ProjectName)
                    .HasMaxLength(255)
                    .HasColumnName("project_name");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Projects)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("projects_user_id_foreign");
            });

            modelBuilder.Entity<Scenario>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created");

                entity.Property(e => e.DateUpdated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_updated");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.ImageUrl)
                    .HasColumnType("text")
                    .HasColumnName("image_url");

                entity.Property(e => e.IsFavorites)
                    .HasColumnName("is_favorites")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.ScenarioName)
                    .HasMaxLength(255)
                    .HasColumnName("scenario_name");

                entity.Property(e => e.StepsAndroid)
                    .HasColumnType("text")
                    .HasColumnName("steps_android");

                entity.Property(e => e.StepsWeb)
                    .HasColumnType("text")
                    .HasColumnName("steps_web");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.Scenarios)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("scenario_user_id_foreign");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Scenarios)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("scenarios_project_id_foreign");
            });

            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.ToTable("Subscription_Plans");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.CustomFeaturesDescription)
                    .HasColumnType("text")
                    .HasColumnName("custom_features_description");

                entity.Property(e => e.DataRetentionPeriod).HasColumnName("data_retention_period");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created");

                entity.Property(e => e.DateUpdated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_updated");

                entity.Property(e => e.MaxAssets).HasColumnName("max_assets");

                entity.Property(e => e.MaxProjects).HasColumnName("max_projects");

                entity.Property(e => e.MaxScenarios).HasColumnName("max_scenarios");

                entity.Property(e => e.MaxUsersAccess).HasColumnName("max_users_access");

                entity.Property(e => e.MonthlyReporting).HasColumnName("monthly_reporting");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(8, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.PrioritySupport).HasColumnName("priority_support");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.StorageLimit).HasColumnName("storage_limit");

                entity.Property(e => e.SubscriptionsName)
                    .HasMaxLength(255)
                    .HasColumnName("subscriptions_name");

                entity.Property(e => e.SupportLevel)
                    .HasMaxLength(255)
                    .HasColumnName("support_level");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SubscriptionPlans)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("subscription_plans_user_id_foreign");
            });

            modelBuilder.Entity<Term>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Answer)
                    .HasColumnType("text")
                    .HasColumnName("answer");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created");

                entity.Property(e => e.DateUpdated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_updated");

                entity.Property(e => e.Question)
                    .HasColumnType("text")
                    .HasColumnName("question");
            });

            modelBuilder.Entity<Tutorial>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created");

                entity.Property(e => e.DateUpdated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_updated");

                entity.Property(e => e.IsShow).HasColumnName("is_show");

                entity.Property(e => e.Order).HasColumnName("order");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.UserIdClerk, "UQ__Users__EACE3C398BC32165")
                    .IsUnique();

                entity.HasIndex(e => e.UserName, "users_user_name_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.AccessFailCount)
                    .HasColumnName("access_fail_count")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.Address)
                    .HasColumnType("text")
                    .HasColumnName("address");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created");

                entity.Property(e => e.DateUpdated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_updated");

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(255)
                    .HasColumnName("display_name");

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("dob");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .HasColumnName("email");

                entity.Property(e => e.EmailConfirmed).HasColumnName("email_confirmed");

                entity.Property(e => e.ImageUrl)
                    .HasColumnType("text")
                    .HasColumnName("image_url");

                entity.Property(e => e.LockoutEnable)
                    .IsRequired()
                    .HasColumnName("lockout_enable")
                    .HasDefaultValueSql("('0')");

                entity.Property(e => e.LockoutEnd)
                    .HasColumnType("datetime")
                    .HasColumnName("lockout_end");

                entity.Property(e => e.PasswordHash)
                    .HasColumnType("text")
                    .HasColumnName("password_hash");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("phone_number");

                entity.Property(e => e.Role)
                    .HasMaxLength(200)
                    .HasColumnName("role");

                entity.Property(e => e.StatusUser)
                    .HasColumnName("status_user")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.UserIdClerk)
                    .HasMaxLength(500)
                    .HasColumnName("user_id_clerk");

                entity.Property(e => e.UserName)
                    .HasMaxLength(255)
                    .HasColumnName("user_name");
            });

            modelBuilder.Entity<UserSubscription>(entity =>
            {
                entity.ToTable("User_Subscriptions");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("date_created");

                entity.Property(e => e.StripeCurrentPeriodEnd)
                    .HasColumnType("datetime")
                    .HasColumnName("stripe_current_period_end");

                entity.Property(e => e.StripeCustomerId)
                    .HasColumnType("text")
                    .HasColumnName("stripe_customer_id");

                entity.Property(e => e.StripePriceId)
                    .HasColumnType("text")
                    .HasColumnName("stripe_price_id");

                entity.Property(e => e.StripeSubscriptionId)
                    .HasColumnType("text")
                    .HasColumnName("stripe_subscription_id");

                entity.Property(e => e.SubscriptionPlanId).HasColumnName("subscription_plan_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.SubscriptionPlan)
                    .WithMany(p => p.UserSubscriptions)
                    .HasForeignKey(d => d.SubscriptionPlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_subscriptions_subscription_plan_id_foreign");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserSubscriptions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_subscriptions_user_id_foreign");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
