using System.Text;
using jep_construction_api.Models;
using jep_construction_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace jep_construction_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var Cors = Configuration.GetSection("Cors");
            services.AddMemoryCache();
            services.AddControllers()
            .AddNewtonsoftJson(opt =>
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            //SQL SERVER CONNECTION HERE

            services.AddDbContext<Jep_ConstructionContext>(option =>
            {
                option.UseSqlServer(Configuration.GetConnectionString("Jep_Construction"));
            });

            // services.AddDbContext<axpcmContext>(option =>
            // {
            //     option.UseSqlServer(Configuration.GetConnectionString("axpcm"));
            // });

            services.AddCors(options =>
            {
                options.AddPolicy("allow-policy", policy =>
                {
                    policy
                    .WithOrigins(Cors.GetSection("Origins").Value)
                    .WithHeaders(Cors.GetSection("Headers").Value)
                    .WithMethods(Cors.GetSection("Methods").Value)
                    .AllowCredentials();
                });
            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                // jwt setup
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };

                // append header when jwt expired
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            services.AddCors();


            //SERVICES
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IEmployeeListService, EmployeeListService>();
            services.AddTransient<IProjectManagementService, ProjectManagementService>();
            services.AddTransient<IReviewService, ReviewService>();
            // services.AddTransient<IEmailService, EmailService>();
            // services.AddTransient<IEmailTemplateService, EmailTemplateService>();
            // services.AddTransient<IRoleService, RoleService>();
            // services.AddTransient<ISecurityAccessService, SecurityAccessService>();
            // services.AddTransient<ISecurityRoleAccessService, SecurityRoleAccessService>();
            // services.AddTransient<ISmsService, SmsService>();
            // services.AddTransient<IInvoiceReceiptService, InvoiceReceiptService>();


            // Local
            // app.UseStaticFiles(new StaticFileOptions()
            // {
            //     FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"uploads")),
            //     RequestPath = new PathString("/uploads")
            // });

            // IIS FOR FILE UPLOADING
            // app.UseStaticFiles(new StaticFileOptions()
            // {
            //     FileProvider = new PhysicalFileProvider(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"uploads")),
            //     RequestPath = new PathString("/uploads")
            // }); 

            // for single host of web and api start
            // app.Use(async (context, next) =>

            // {

            //     await next();

            //     if (context.Response.StatusCode == 404 && !System.IO.Path.HasExtension(context.Request.Path.Value))

            //     {

            //         context.Request.Path = "/index.html";

            //         await next();

            //     }

            // });

            // app.UseDefaultFiles();
            // app.UseStaticFiles();
            // for single host of web and api end
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(option =>
            {
                option.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
            });

            //app.UseCors("allow-policy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
