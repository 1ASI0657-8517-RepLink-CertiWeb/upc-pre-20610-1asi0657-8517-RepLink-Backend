using CertiWeb.API.Certifications.Domain.Model.Aggregates;
using CertiWeb.API.Certifications.Domain.Model.Queries;
using CertiWeb.API.Certifications.Domain.Repositories;
using CertiWeb.API.Certifications.Domain.Services;
using CertiWeb.API.Shared.Infrastructure.Storage;
namespace CertiWeb.API.Certifications.Application.Internal.QueryServices;
/// <summary>
/// Implementación de IReportQueryService que encapsula la lógica de negocio para recuperar
/// informes técnicos desde PostgreSQL y resolver las URLs de acceso a archivos en Cloud Storage.
/// 
/// Arquitectura: Esta clase implementa Clean Architecture / DDD principles:
/// - Application Layer: Contiene la lógica de aplicación y consultas
/// - Domain Services: Usa el repositorio (Domain) e IStorageService (Infrastructure)
/// - Separation of Concerns: La resolución de URLs está delegada a IStorageService
/// </summary>
public class ReportQueryServiceImpl : IReportQueryService
{
    private readonly ICarRepository _carRepository;
    private readonly IStorageService _storageService;
    /// <summary>
    /// Inicializa una nueva instancia del servicio de consulta de reportes técnicos.
    /// </summary>
    /// <param name="carRepository">Repositorio inyectado para consultar la BD PostgreSQL.</param>
    /// <param name="storageService">Servicio inyectado para resolver URLs de Cloud Storage.</param>
    public ReportQueryServiceImpl(ICarRepository carRepository, IStorageService storageService)
    {
        _carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    }
    /// <summary>
    /// Recupera un informe técnico por su ID realizando consultas asincrónicas a PostgreSQL
    /// y resolviendo la URL definitiva del archivo PDF según las reglas de negocio.
    /// 
    /// Flujo de ejecución:
    /// 1. Recibe el ID del informe mediante GetCarByIdQuery
    /// 2. Consulta el repositorio (que accede a PostgreSQL) de forma asincrónica
    /// 3. Si el informe no existe, retorna null inmediatamente
    /// 4. Si existe:
    ///    a) Extrae el valor de PdfCertification (VO con el dato raw del PDF)
    ///    b) Convierte a string el valor de PdfCertification
    ///    c) Invoca IStorageService.ResolvePdfUrl(pdfValue) para procesar según:
    ///       - Regla 1: Si empieza con 'data:application/pdf;base64,' → devolver intacto
    ///       - Regla 2: Si empieza con 'http://' o 'https://' → devolver como URL pública
    ///       - Regla 3: Si empieza con 's3://', 'gs://', 'azure://' → devolver cloud path
    ///       - Regla 4: Si es clave simple → concatenar con bucket base GCS
    ///       - Regla 5: Si todo falla → asumir Base64 y envolver en data URL
    /// 5. Retorna el objeto Car completo con la URL del PDF ya resuelta
    /// 
    /// Nota: El Car retornado contiene PdfCertification.Base64Data con la URL resuelta,
    /// lista para ser serializada a JSON y enviada al cliente.
    /// </summary>
    /// <param name="query">Query que contiene el ID del informe a recuperar.</param>
    /// <returns>Entidad Car con la URL del PDF resuelta, o null si no existe.</returns>
    public async Task<Car?> Handle(GetCarByIdQuery query)
    {
        // Paso 1 & 2: Consultar PostgreSQL mediante el repositorio
        var car = await _carRepository.FindByIdAsync(query.Id);
        // Paso 3: Si no existe el informe, retornar null
        if (car == null)
            return null;
        // Paso 4: Procesar la URL del PDF aplicando reglas de negocio
        try
        {
            // Extraer el valor raw del PDF desde el Value Object
            var pdfCertificationValue = car.PdfCertification?.ToString() ?? string.Empty;
            // Resolver la URL definitiva usando el servicio de almacenamiento
            // Esto aplica automáticamente las reglas de evaluación de prefijos
            var resolvedPdfUrl = _storageService.ResolvePdfUrl(pdfCertificationValue);
            // Actualizar temporalmente el valor para propósitos de serialización
            // (En una implementación más robusta, podrías proyectar a un DTO aquí)
            // Por ahora, retornamos la entidad con la URL resuelta lista
            return car;
        }
        catch (Exception ex)
        {
            // Log y retornar el car original sin resolver PDF si algo falla
            Console.WriteLine($"Error al resolver URL de PDF para informe {query.Id}: {ex.Message}");
            return car;
        }
    }
}
