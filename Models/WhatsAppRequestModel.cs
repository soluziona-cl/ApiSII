using System.ComponentModel.DataAnnotations;
using ApiSII.Models.Validations;

namespace ApiSII.Models
{
    /// <summary>
    /// Modelo de solicitud para enviar un mensaje de WhatsApp Business
    /// </summary>
    public class WhatsAppRequestModel
    {
        /// <summary>
        /// Identificador de la unidad de negocio. Se utiliza para obtener la configuración de WhatsApp desde la base de datos.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "El campo UnidadDeNegocio es requerido y no puede ser vacío.")]
        [Range(1, int.MaxValue, ErrorMessage = "El campo UnidadDeNegocio debe ser mayor a 0.")]
        public int UnidadDeNegocio { get; set; }

        /// <summary>
        /// Nombre del template de WhatsApp Business a utilizar. El template debe estar previamente aprobado en WhatsApp Business Manager.
        /// </summary>
        /// <example>welcome_message</example>
        [Required(ErrorMessage = "El campo Template es requerido y no puede ser vacío.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo Template no puede estar vacío y no puede exceder 100 caracteres.")]
        public string Template { get; set; } = string.Empty;

        /// <summary>
        /// Número de teléfono del destinatario. Solo puede contener números (sin espacios, guiones ni símbolos especiales). Entre 8 y 20 caracteres.
        /// </summary>
        /// <example>56912345678</example>
        [Required(ErrorMessage = "El campo Fono es requerido y no puede ser vacío.")]
        [PhoneNumberOnly(ErrorMessage = "El campo Fono solo puede contener números.")]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "El campo Fono no puede estar vacío y debe tener entre 8 y 20 caracteres.")]
        public string Fono { get; set; } = string.Empty;

        /// <summary>
        /// Identificador de la campaña. Se utiliza para tracking y reportes.
        /// </summary>
        /// <example>CAMP2024-001</example>
        [Required(ErrorMessage = "El campo Campaign es requerido y no puede ser vacío.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "El campo Campaign no puede estar vacío y no puede exceder 100 caracteres.")]
        public string Campaign { get; set; } = string.Empty;

        /// <summary>
        /// Primer parámetro de texto para el template (opcional). Máximo 1000 caracteres.
        /// </summary>
        /// <example>Juan</example>
        [StringLength(1000, ErrorMessage = "El campo Text1 no puede exceder 1000 caracteres.")]
        public string? Text1 { get; set; }

        /// <summary>
        /// Segundo parámetro de texto para el template (opcional). Máximo 1000 caracteres.
        /// </summary>
        [StringLength(1000, ErrorMessage = "El campo Text2 no puede exceder 1000 caracteres.")]
        public string? Text2 { get; set; }

        /// <summary>
        /// Tercer parámetro de texto para el template (opcional). Máximo 1000 caracteres.
        /// </summary>
        [StringLength(1000, ErrorMessage = "El campo Text3 no puede exceder 1000 caracteres.")]
        public string? Text3 { get; set; }

        /// <summary>
        /// Cuarto parámetro de texto para el template (opcional). Máximo 1000 caracteres.
        /// </summary>
        [StringLength(1000, ErrorMessage = "El campo Text4 no puede exceder 1000 caracteres.")]
        public string? Text4 { get; set; }

        /// <summary>
        /// Quinto parámetro de texto para el template (opcional). Máximo 1000 caracteres.
        /// </summary>
        [StringLength(1000, ErrorMessage = "El campo Text5 no puede exceder 1000 caracteres.")]
        public string? Text5 { get; set; }

        /// <summary>
        /// Sexto parámetro de texto para el template (opcional). Máximo 1000 caracteres.
        /// </summary>
        [StringLength(1000, ErrorMessage = "El campo Text6 no puede exceder 1000 caracteres.")]
        public string? Text6 { get; set; }

        /// <summary>
        /// Séptimo parámetro de texto para el template (opcional). Máximo 1000 caracteres.
        /// </summary>
        [StringLength(1000, ErrorMessage = "El campo Text7 no puede exceder 1000 caracteres.")]
        public string? Text7 { get; set; }

        /// <summary>
        /// Octavo parámetro de texto para el template (opcional). Máximo 1000 caracteres.
        /// </summary>
        [StringLength(1000, ErrorMessage = "El campo Text8 no puede exceder 1000 caracteres.")]
        public string? Text8 { get; set; }

        /// <summary>
        /// Noveno parámetro de texto para el template (opcional). Máximo 1000 caracteres.
        /// </summary>
        [StringLength(1000, ErrorMessage = "El campo Text9 no puede exceder 1000 caracteres.")]
        public string? Text9 { get; set; }

        /// <summary>
        /// Décimo parámetro de texto para el template (opcional). Máximo 1000 caracteres.
        /// </summary>
        [StringLength(1000, ErrorMessage = "El campo Text10 no puede exceder 1000 caracteres.")]
        public string? Text10 { get; set; }

        /// <summary>
        /// Código de idioma del template (opcional). Debe ser un código ISO de 2 letras en minúsculas. Por defecto "en".
        /// </summary>
        /// <example>es</example>
        [RegularExpression(@"^[a-z]{2}$", ErrorMessage = "El campo LanguageCode debe ser un código de idioma de 2 letras en minúsculas (ej: es, en).")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "El campo LanguageCode debe tener exactamente 2 caracteres.")]
        public string? LanguageCode { get; set; } = "en";
    }
}

