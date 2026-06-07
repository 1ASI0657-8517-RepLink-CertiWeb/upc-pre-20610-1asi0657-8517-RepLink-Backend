using System.Net.Mime;
using System.Security.Claims;
using CertiWeb.API.Certifications.Domain.Model.Queries;
using CertiWeb.API.Certifications.Interfaces.REST.Resources;
using CertiWeb.API.Certifications.Domain.Services;
using CertiWeb.API.Shared.Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CertiWeb.API.Certifications.Interfaces.REST;

/// <summary>
/// Controlador REST para gestionar la consulta de informes técnicos de vehículos certificados.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Report Endpoints")]
public class ReportsController : ControllerBase
{
    private readonly ICarQueryService _carQueryService;
    private readonly IStorageService _storageService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de informes.
    /// </summary>
    /// <param name="carQueryService">Servicio para consultar información de vehículos.</param>
    /// <param name="storageService">Servicio para resolver URLs de archivos almacenados.</param>
    public ReportsController(ICarQueryService carQueryService, IStorageService storageService)
    {
        _carQueryService = carQueryService;
        _storageService = storageService;
    }

    /// <summary>
    /// Obtiene el informe técnico (PDF) de un vehículo por su identificador.
    /// Endpoint protegido con autenticación JWT.
    /// 
    /// Reglas de Seguridad (US06):
    /// 1. Extrae el email del Usuario autenticado desde los Claims del JWT.
    /// 2. Consulta la BD (PostgreSQL) para obtener el vehículo por ID.
    /// 3. Valida que el owner_email del vehículo coincida con el email del JWT.
    /// 4. Si no coinciden, retorna 403 (Forbid) por razones de seguridad.
    /// 5. Si coinciden, resuelve la URL del PDF (data URL o Cloud Storage).
    /// 6. Devuelve el DTO con exactamente: Id, CarId, Owner, OwnerEmail, PdfUrl.
    /// </summary>
    /// <param name="id">Identificador único del vehículo/informe (desde URL).</param>
    /// <returns>Un DTO con exactamente los campos: Id, CarId, Owner, OwnerEmail, PdfUrl.</returns>
    /// <response code="200">Informe técnico obtenido exitosamente.</response>
    /// <response code="403">El usuario no tiene permiso para acceder este informe (owner mismatch).</response>
    /// <response code="404">El informe técnico no fue encontrado.</response>
    /// <response code="401">No autorizado (token JWT inválido o expirado).</response>
    /// <response code="500">Error interno del servidor.</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obtener informe técnico por ID (US06)",
        Description = "Recupera el informe técnico minimalista de un vehículo certificado. " +
                      "Valida que el usuario autenticado sea el propietario del vehículo. " +
                      "Devuelve exactamente: Id, CarId, Owner, OwnerEmail y PdfUrl.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Informe técnico recuperado exitosamente", typeof(GetReportByIdResponse))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "El usuario no tiene permiso para acceder este informe")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "El informe técnico no fue encontrado")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "No autorizado (token JWT inválido o expirado)")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Error interno del servidor")]
    public async Task<IActionResult> GetReportById([FromRoute] int id)
    {
        try
        {
            // Regla 1: Extraer email del usuario autenticado desde los Claims del JWT
            var authenticatedUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(authenticatedUserEmail))
            {
                // Si no hay email en el token, retornar Unauthorized
                return Unauthorized(new { message = "Token JWT no contiene el claim de email", details = "ClaimTypes.Email no encontrado" });
            }

            // Regla 2: Usar servicio inyectado para recuperar vehículo/informe desde PostgreSQL
            var getCarByIdQuery = new GetCarByIdQuery(id);
            var car = await _carQueryService.Handle(getCarByIdQuery);

            // Regla 3: Devolver 404 si no existe
            if (car == null)
            {
                return NotFound(new { message = $"El informe técnico con ID {id} no fue encontrado" });
            }

            // Regla 4: Validación de Seguridad - Comparar emails
            // Comparación case-insensitive del email del JWT con el owner_email de BD
            if (!car.OwnerEmail.Equals(authenticatedUserEmail, StringComparison.OrdinalIgnoreCase))
            {
                // El usuario intenta acceder a un informe que no le pertenece
                // Por seguridad, retornamos 403 Forbid
                return Forbid();
            }

            // Regla 5: Obtener URL real del PDF usando IStorageService.ResolvePdfUrl()
            // El valor puede ser:
            // - Un data URL (data:application/pdf;base64,...)
            // - Una clave simple que se concatenará con la URL base de GCS
            // - Una URL completa (http/https)
            var pdfValue = (string)car.PdfCertification; // Implicit conversion operator devuelve Base64Data
            var pdfUrl = _storageService.ResolvePdfUrl(pdfValue);

            // Regla 6: Mapear a DTO con exactamente Id, CarId, Owner, OwnerEmail, PdfUrl
            var response = new GetReportByIdResponse(
                Id: car.Id,
                CarId: car.Id,
                Owner: car.Owner,
                OwnerEmail: car.OwnerEmail,
                PdfUrl: pdfUrl
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener el informe técnico con ID {id}: {ex.Message}");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "Error al obtener el informe técnico", details = ex.Message }
            );
        }
    }
}

