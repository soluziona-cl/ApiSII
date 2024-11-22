using CajValp.Interfaces;
using CajValp.Repositories;
using CajValp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.IO;

namespace CajValp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // Este método se usa para agregar servicios al contenedor.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
           

            // Registrar dependencias
            services.AddScoped<IFtpService, FtpService>();
            services.AddScoped<IFileProcessingService, FileProcessingService>();
            services.AddScoped<IFileProcessingService2, FileProcessingService2>();
            services.AddScoped<IFileRepository, FileRepository>();

            string connectionString = Configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("DefaultConnection string is not configured properly.");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("DefaultConnection string is not configured properly.");
            }
            services.AddSingleton(connectionString);

            // Registrar la cadena de conexión para el repositorio
            services.AddScoped<FtpControlRepository>(provider =>
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("DefaultConnection string is not configured properly.");
                }
                return new FtpControlRepository(connectionString);
            });

            services.AddScoped<ExportService>(provider =>
              {
                  var connectionString = Configuration.GetConnectionString("DefaultConnection");
                  if (string.IsNullOrEmpty(connectionString))
                  {
                      throw new Exception("DefaultConnection string is not configured properly.");
                  }
                  return new ExportService(connectionString);
              });

            services.AddSwaggerGen();
            // Registrar la cadena de conexión como Singleton

            services.AddScoped<FileProcessingService>();
            services.AddScoped<FileProcessingService2>();

            services.AddScoped<FtpService>(); // Añadir esta línea para registrar FtpService

        }

        // Este método se usa para configurar el pipeline de solicitudes HTTP.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "FtpCsvApi v1");
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Crear la carpeta Download si no existe
            CreateDownloadFolder();
            CreateUploadFolder();
            CreateExcelFolder();
        }

        // Método para crear la carpeta Download si no existe
        private static void CreateDownloadFolder()
        {
            string downloadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Download");
            if (!Directory.Exists(downloadFolderPath))
            {
                Directory.CreateDirectory(downloadFolderPath);
            }
        }
        private static void CreateUploadFolder()
        {
            string uploadFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Upload");
            if (!Directory.Exists(uploadFolderPath))
            {
                Directory.CreateDirectory(uploadFolderPath);
            }
        }
        private static void CreateExcelFolder()
        {
            string excelFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Carga", "Excel");
            if (!Directory.Exists(excelFolderPath))
            {
                Directory.CreateDirectory(excelFolderPath);
            }
        }
    }
}
