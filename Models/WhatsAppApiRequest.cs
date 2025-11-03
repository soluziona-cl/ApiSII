namespace ApiSII.Models
{
    public class WhatsAppApiRequest
    {
        public string messaging_product { get; set; } = "whatsapp";
        public string to { get; set; } = string.Empty;
        public string type { get; set; } = "template";
        public Template template { get; set; } = new Template();
    }

    public class Template
    {
        public string name { get; set; } = string.Empty;
        public Language language { get; set; } = new Language();
        public List<Component> components { get; set; } = new List<Component>();
    }

    public class Language
    {
        public string code { get; set; } = "en";
    }

    public class Component
    {
        public string type { get; set; } = "body";
        public List<Parameter> parameters { get; set; } = new List<Parameter>();
    }

    public class Parameter
    {
        public string type { get; set; } = "text";
        public string text { get; set; } = string.Empty;
    }
}

