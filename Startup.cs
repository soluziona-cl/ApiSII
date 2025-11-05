using ApiSII.Filters;
using ApiSII.Interfaces;
using ApiSII.Middleware;
using ApiSII.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Text;

namespace ApiSII
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            // Configurar JWT Authentication para la API
            var jwtSettings = Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no está configurada.");

            services.AddAuthentication(options =>
            {
                // Esquema por defecto para la API (JWT)
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            })
            // Esquema de cookies para proteger Swagger y Docs
            .AddCookie("DocsCookieAuth", options =>
            {
                options.Cookie.Name = "ApiSII.Docs.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.LoginPath = "/docs-login";
                options.AccessDeniedPath = "/docs-login?denied=true";
            });
            
            // Configurar caché distribuido en memoria (requerido para sesiones)
            services.AddDistributedMemoryCache();
            
            // Configurar sesiones para mantener el estado de autenticación
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            // Configurar HttpClientFactory
            services.AddHttpClient();

            // Registrar servicios
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IWhatsAppService, WhatsAppService>();

            // Configurar Swagger con soporte JWT
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ApiSII - WhatsApp Business API",
                    Version = "v1",
                    Description = "API para el envío de mensajes de WhatsApp Business mediante templates. " +
                                 "Requiere autenticación JWT para acceder a los endpoints.",
                    Contact = new OpenApiContact
                    {
                        Name = "Soporte ApiSII",
                        Email = "soporte@evoluziona.cl"
                    }
                });

                // Incluir comentarios XML para documentación
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Configurar ejemplos y esquemas personalizados
                c.EnableAnnotations();
                c.SchemaFilter<ExampleSchemaFilter>();
                c.OperationFilter<ExampleOperationFilter>();

                // Configurar JWT en Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header usando el esquema Bearer. Ingrese su token en el formato: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            
            // Servir archivos estáticos (para recursos CSS, JS, imágenes de Slate)
            // NOTA: Archivos estáticos disponibles en todos los entornos (Development y Production)
            app.UseStaticFiles();

            app.UseRouting();

            // Habilitar sesiones (debe ir antes de UseAuthentication)
            app.UseSession();
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            // Middleware para proteger Swagger y Docs con autenticación basada en cookies
            app.UseMiddleware<DocsAuthMiddleware>();
            
            // Middleware para ajustar rutas de Swagger UI cuando hay PathBase
            // Debe estar ANTES de UseSwaggerUI para poder interceptar la respuesta HTML
            app.UseMiddleware<SwaggerPathBaseMiddleware>();

            // Configurar Swagger - Disponible en todos los entornos (Development y Production)
            // NOTA: Swagger está habilitado explícitamente para producción
            app.UseSwagger(c =>
            {
                // Configurar Swagger para usar rutas relativas (funciona con PathBase)
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });
            
            app.UseSwaggerUI(c =>
            {
                // Usar ruta relativa para el JSON (funciona con cualquier PathBase)
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "ApiSII v1");
                c.RoutePrefix = "swagger"; // Swagger UI en /swagger - Accesible en producción
                c.DisplayRequestDuration();
                c.EnableDeepLinking();
                c.EnableFilter();
                c.ShowExtensions();
                c.EnableValidator();
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                c.DefaultModelsExpandDepth(2);
                c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
