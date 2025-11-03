# Introducción

Bienvenido a la documentación de la **ApiSII - WhatsApp Business API**. Esta API te permite enviar mensajes de WhatsApp Business mediante templates predefinidos y aprobados.

La API utiliza autenticación JWT para asegurar el acceso a los endpoints protegidos.

<aside class="notice">
Para usar esta API necesitas obtener un token JWT válido mediante el endpoint de autenticación.
</aside>

# Autenticación

La API utiliza autenticación mediante tokens JWT (JSON Web Tokens). Para acceder a los endpoints protegidos, debes incluir el token en el header `Authorization` de todas las peticiones.

## Obtener Token

> Para obtener un token, usa este código:

```bash
curl -X POST "https://api.example.com/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "usuario@example.com",
    "password": "tu_password"
  }'
```

```javascript
const response = await fetch('https://api.example.com/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    username: 'usuario@example.com',
    password: 'tu_password'
  })
});

const data = await response.json();
const token = data.token;
```

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/api/auth/login");

var loginData = new
{
    username = "usuario@example.com",
    password = "tu_password"
};

request.Content = new StringContent(
    JsonSerializer.Serialize(loginData),
    Encoding.UTF8,
    "application/json"
);

var response = await client.SendAsync(request);
var content = await response.Content.ReadAsStringAsync();
var result = JsonSerializer.Deserialize<LoginResponse>(content);
```

> La respuesta será un objeto JSON con el siguiente formato:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresAt": "2024-11-26T10:30:00Z",
  "username": "usuario@example.com"
}
```

### Endpoint

`POST /api/auth/login`

### Parámetros de Request

| Parámetro | Tipo   | Requerido | Descripción                                 |
| ---------- | ------ | --------- | -------------------------------------------- |
| username   | string | Sí       | Nombre de usuario o email (3-100 caracteres) |
| password   | string | Sí       | Contraseña del usuario (6-100 caracteres)   |

### Respuestas

| Código | Descripción                               |
| ------- | ------------------------------------------ |
| 200     | Autenticación exitosa, token JWT generado |
| 400     | Error de validación en los datos enviados |
| 401     | Credenciales inválidas                    |
| 500     | Error interno del servidor                 |

## Usar el Token

Una vez obtenido el token, inclúyelo en todas las peticiones a endpoints protegidos usando el header `Authorization`:

```bash
curl "https://api.example.com/api/whatsapp/send" \
  -H "Authorization: Bearer {tu_token_jwt}" \
  -H "Content-Type: application/json" \
  -d '{ ... }'
```

```javascript
const response = await fetch('https://api.example.com/api/whatsapp/send', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ ... })
});
```

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
```

<aside class="warning">
Los tokens JWT tienen una duración limitada. Si recibes un error 401, necesitarás obtener un nuevo token.
</aside>

# Endpoints

## Enviar Mensaje de WhatsApp

Este endpoint permite enviar mensajes de WhatsApp Business mediante templates predefinidos.

### Request

> Ejemplo de request:

```bash
curl -X POST "https://api.example.com/api/whatsapp/send" \
  -H "Authorization: Bearer {tu_token_jwt}" \
  -H "Content-Type: application/json" \
  -d '{
    "unidadDeNegocio": 5,
    "template": "welcome_message",
    "fono": "56912345678",
    "campaign": "CAMP2024-001",
    "text1": "Juan",
    "text2": "Bienvenido a nuestro servicio",
    "languageCode": "es"
  }'
```

```javascript
const response = await fetch('https://api.example.com/api/whatsapp/send', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    unidadDeNegocio: 5,
    template: 'welcome_message',
    fono: '56912345678',
    campaign: 'CAMP2024-001',
    text1: 'Juan',
    text2: 'Bienvenido a nuestro servicio',
    languageCode: 'es'
  })
});

const result = await response.json();
```

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

var messageData = new
{
    unidadDeNegocio = 5,
    template = "welcome_message",
    fono = "56912345678",
    campaign = "CAMP2024-001",
    text1 = "Juan",
    text2 = "Bienvenido a nuestro servicio",
    languageCode = "es"
};

var content = new StringContent(
    JsonSerializer.Serialize(messageData),
    Encoding.UTF8,
    "application/json"
);

var response = await client.PostAsync(
    "https://api.example.com/api/whatsapp/send",
    content
);
```

> Ejemplo de respuesta exitosa:

```json
{
  "message": "Mensaje enviado exitosamente",
  "response": {
    "messaging_product": "whatsapp",
    "contacts": [
      {
        "input": "56912345678",
        "wa_id": "56912345678"
      }
    ],
    "messages": [
      {
        "id": "wamid.XXX"
      }
    ]
  }
}
```

### Endpoint

`POST /api/whatsapp/send`

### Headers

| Header        | Tipo   | Requerido | Descripción                            |
| ------------- | ------ | --------- | --------------------------------------- |
| Authorization | string | Sí       | Token JWT en formato:`Bearer {token}` |
| Content-Type  | string | Sí       | Debe ser `application/json`           |

### Parámetros de Request

| Parámetro      | Tipo    | Requerido | Descripción                                                          |
| --------------- | ------- | --------- | --------------------------------------------------------------------- |
| unidadDeNegocio | integer | Sí       | Identificador de la unidad de negocio (>= 1)                          |
| template        | string  | Sí       | Nombre del template de WhatsApp (1-100 caracteres)                    |
| fono            | string  | Sí       | Número de teléfono del destinatario (8-20 dígitos, solo números)  |
| campaign        | string  | Sí       | Identificador de la campaña (1-100 caracteres)                       |
| text1           | string  | No        | Primer parámetro de texto para el template (máx. 1000 caracteres)   |
| text2           | string  | No        | Segundo parámetro de texto para el template (máx. 1000 caracteres)  |
| text3           | string  | No        | Tercer parámetro de texto para el template (máx. 1000 caracteres)   |
| text4           | string  | No        | Cuarto parámetro de texto para el template (máx. 1000 caracteres)   |
| text5           | string  | No        | Quinto parámetro de texto para el template (máx. 1000 caracteres)   |
| text6           | string  | No        | Sexto parámetro de texto para el template (máx. 1000 caracteres)    |
| text7           | string  | No        | Séptimo parámetro de texto para el template (máx. 1000 caracteres) |
| text8           | string  | No        | Octavo parámetro de texto para el template (máx. 1000 caracteres)   |
| text9           | string  | No        | Noveno parámetro de texto para el template (máx. 1000 caracteres)   |
| text10          | string  | No        | Décimo parámetro de texto para el template (máx. 1000 caracteres)  |
| languageCode    | string  | No        | Código de idioma ISO de 2 letras (ej: "es", "en"). Por defecto "en"  |

### Respuestas

| Código | Descripción                                  |
| ------- | --------------------------------------------- |
| 200     | Mensaje enviado exitosamente                  |
| 400     | Error de validación en los datos enviados    |
| 401     | No autorizado - Token JWT inválido o ausente |
| 502     | Error al comunicarse con la API de WhatsApp   |
| 500     | Error interno del servidor                    |

### Errores

#### Error 400 - Validación

```json
{
  "message": "Error de validación",
  "errors": {
    "fono": ["El campo Fono solo puede contener números."],
    "template": ["El campo Template es requerido y no puede ser vacío."]
  }
}
```

#### Error 401 - No Autorizado

```json
{
  "message": "No autorizado",
  "details": "Token JWT inválido o ausente"
}
```

#### Error 502 - Error de WhatsApp

```json
{
  "message": "Error al comunicarse con la API de WhatsApp",
  "details": "Mensaje de error específico"
}
```

# Modelos de Datos

## LoginRequestModel

Modelo para solicitud de autenticación.

```json
{
  "username": "usuario@example.com",
  "password": "tu_password"
}
```

### Propiedades

| Propiedad | Tipo   | Requerido | Descripción                                 |
| --------- | ------ | --------- | -------------------------------------------- |
| username  | string | Sí       | Nombre de usuario o email (3-100 caracteres) |
| password  | string | Sí       | Contraseña del usuario (6-100 caracteres)   |

## LoginResponseModel

Modelo de respuesta de autenticación.

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresAt": "2024-11-26T10:30:00Z",
  "username": "usuario@example.com"
}
```

### Propiedades

| Propiedad | Tipo     | Descripción                                |
| --------- | -------- | ------------------------------------------- |
| token     | string   | Token JWT generado                          |
| tokenType | string   | Tipo de token (siempre "Bearer")            |
| expiresAt | datetime | Fecha y hora de expiración del token (UTC) |
| username  | string   | Nombre de usuario autenticado               |

## WhatsAppRequestModel

Modelo para solicitud de envío de mensaje de WhatsApp.

```json
{
  "unidadDeNegocio": 5,
  "template": "welcome_message",
  "fono": "56912345678",
  "campaign": "CAMP2024-001",
  "text1": "Juan",
  "text2": "Bienvenido",
  "text3": "Gracias por usar nuestro servicio",
  "languageCode": "es"
}
```

### Propiedades

| Propiedad       | Tipo    | Requerido | Descripción                                          |
| --------------- | ------- | --------- | ----------------------------------------------------- |
| unidadDeNegocio | integer | Sí       | ID de la unidad de negocio (>= 1)                     |
| template        | string  | Sí       | Nombre del template (1-100 caracteres)                |
| fono            | string  | Sí       | Teléfono destinatario (8-20 dígitos)                |
| campaign        | string  | Sí       | ID de campaña (1-100 caracteres)                     |
| text1-text10    | string  | No        | Parámetros de texto (máx. 1000 caracteres cada uno) |
| languageCode    | string  | No        | Código de idioma ISO (2 letras, por defecto "en")    |

# Códigos de Estado HTTP

La API utiliza los siguientes códigos de estado HTTP estándar:

| Código | Descripción                                                         |
| ------- | -------------------------------------------------------------------- |
| 200     | OK - La petición fue exitosa                                        |
| 400     | Bad Request - Error de validación en los datos enviados             |
| 401     | Unauthorized - Credenciales inválidas o token ausente/inválido     |
| 500     | Internal Server Error - Error interno del servidor                   |
| 502     | Bad Gateway - Error al comunicarse con servicios externos (WhatsApp) |

# Errores Comunes

## Error de Validación

Cuando los datos enviados no cumplen con las validaciones requeridas, recibirás un error 400 con detalles de los campos que fallaron.

```json
{
  "message": "Error de validación",
  "errors": {
    "campo": ["mensaje de error específico"]
  }
}
```

## Token Expirado

Si tu token JWT ha expirado, recibirás un error 401. Debes obtener un nuevo token mediante el endpoint de login.

## Template No Encontrado

Si el template especificado no existe o no está aprobado en WhatsApp Business Manager, recibirás un error 400 o 502 según el caso.

# Recursos Adicionales

- [Swagger UI](/swagger) - Documentación interactiva de la API (disponible en desarrollo y producción)
- [Documentación de WhatsApp Business API](https://developers.facebook.com/docs/whatsapp)
- [Información sobre Templates de WhatsApp](https://developers.facebook.com/docs/whatsapp/message-templates)

<aside class="notice">
Tanto Swagger como esta documentación de Slate están disponibles en todos los entornos, incluyendo producción.
</aside>

# Soporte

Para soporte técnico, contacta a: **soporte@evoluziona.cl**
