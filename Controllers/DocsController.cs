using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace ApiSII.Controllers
{
    /// <summary>
    /// Controlador para servir la documentación de Slate
    /// NOTA: Disponible en todos los entornos (Development y Production)
    /// Accesible en: /docs y /docs/index.html
    /// </summary>
    [ApiController]
    [Route("docs")]
    [ApiExplorerSettings(IgnoreApi = true)] // Excluir de Swagger y documentación
    public class DocsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public DocsController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Sirve la página principal de la documentación Slate
        /// </summary>
        [HttpGet]
        [HttpGet("index.html")]
        public IActionResult Index()
        {
            var filePath = Path.Combine(_env.WebRootPath, "docs", "index.html");
            
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            // Leer el HTML y reemplazar las rutas absolutas con rutas relativas o PathBase
            var html = System.IO.File.ReadAllText(filePath);
            var pathBase = HttpContext.Request.PathBase.Value ?? "";
            
            // Reemplazar rutas absolutas con rutas que incluyan PathBase
            if (!string.IsNullOrEmpty(pathBase))
            {
                html = html.Replace("href=\"/docs/", $"href=\"{pathBase}/docs/");
                html = html.Replace("src=\"/docs/", $"src=\"{pathBase}/docs/");
                html = html.Replace("'/docs/", $"'{pathBase}/docs/");
                html = html.Replace("\"/docs/", $"\"{pathBase}/docs/");
            }
            
            // Inyectar el PathBase en el JavaScript para que app.js pueda usarlo
            // Buscar y reemplazar la línea del script app.js
            var scriptPattern = @"<script src=[""']/docs/js/app\.js[""']></script>";
            var scriptReplacement = $@"<script>
                    window.APP_PATH_BASE = '{pathBase}';
                </script>
                <script src=""{pathBase}/docs/js/app.js""></script>";
            
            html = System.Text.RegularExpressions.Regex.Replace(html, scriptPattern, scriptReplacement);

            return Content(html, "text/html");
        }

        /// <summary>
        /// Sirve el archivo markdown de la documentación
        /// </summary>
        [HttpGet("index.md")]
        public IActionResult Markdown()
        {
            var filePath = Path.Combine(_env.WebRootPath, "docs", "index.md");
            
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, "text/markdown");
        }
    }
}

