using barangay_crime_compliant_api.Hubs;
using jep_construction_api.Models;
using jep_construction_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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
            services.AddSignalR();
            services.AddMemoryCache();
            services.AddControllers()
            .AddNewtonsoftJson(opt =>
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });


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
                    .WithOrigins(Cors.GetSection("Origins").Value ?? "")
                    .WithHeaders(Cors.GetSection("Headers").Value ?? "")
                    .WithMethods(Cors.GetSection("Methods").Value ?? "")
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"] ?? ""))
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
            services.AddTransient<IClientRequestService, ClientRequestService>();
            services.AddTransient<IInventoryService, InventoryService>();
            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<IAttendanceService, AttendanceService>();
            services.AddTransient<IMessageService, MessageService>();

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10_000_000; // 10 MB limit per file
                options.ValueLengthLimit = int.MaxValue;      // unlimited input length
                options.MultipartHeadersLengthLimit = int.MaxValue; // unlimited header length
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            //app.UseCors("allow-policy");

            app.UseRouting();

            app.UseCors(option =>
            {
                option.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
            });

            // Local
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"uploads")),
                RequestPath = new PathString("/uploads")
            });

            // IIS
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


            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<MessageHub>("/messagehub");
                endpoints.MapControllers();

            });
        }
    }
}
