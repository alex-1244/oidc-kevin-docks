using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication.ExtendedProtection;
using ImageGallery.Client.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }
 
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();

            services.AddAuthentication(opt =>
	            {
		            opt.DefaultScheme = "Cookies";
		            opt.DefaultChallengeScheme = "oidc";
	            })
	            .AddCookie("Cookies")
	            .AddOpenIdConnect("oidc", opt =>
	            {
		            opt.SignInScheme = "Cookies";
		            opt.Authority = "https://localhost:44312/";
		            opt.ClientId = "imagegalleryclient";
		            opt.ResponseType = "code id_token";
					opt.Scope.Add("openid");
					opt.Scope.Add("profile");
					opt.Scope.Add("address");
					opt.SaveTokens = true;
					opt.ClientSecret = "secret";
					opt.GetClaimsFromUserInfoEndpoint = true;
					//opt.CallbackPath = "https://localhost:44344";
					//opt.SignedOutCallbackPath = new PathString();

					opt.ClaimActions.Remove("amr"); // now this claim WILL BE present in claims identity
					opt.ClaimActions.DeleteClaim("sid"); //now this claim WON'T BE present in claims identity (in Cookie)
					opt.ClaimActions.DeleteClaim("idp");
					opt.ClaimActions.DeleteClaim("address");
				});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseAuthentication();

			app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}
