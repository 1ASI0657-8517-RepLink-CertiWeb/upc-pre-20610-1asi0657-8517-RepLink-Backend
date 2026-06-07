using CertiWeb.API.Certifications.Domain.Model.Aggregates;
using CertiWeb.API.Certifications.Domain.Model.Queries;
namespace CertiWeb.API.Certifications.Domain.Services;
/// <summary>
/// Servicio de dominio especializado en la lógica de negocio para recuperar informes técnicos.
/// 
/// Responsabilidades:
/// - Recuperar información del informe técnico desde PostgreSQL
/// - Evaluar y resolver la URL definitiva del PDF desde Cloud Storage o almacenamiento local
/// - Aplicar reglas de negocio para diferentes tipos de almacenamiento (Base64, Cloud Storage, URLs públicas)
/// - Retornar un DTO con toda la información mapeada y lista para el cliente
/// </summary>
public interface IReportQueryService
{
    /// <summary>
    /// Recupera un informe técnico por su ID de forma asincrónica.
    /// 
    /// Proceso:
    /// 1. Consulta la BD PostgreSQL mediante el repositorio inyectado
    /// 2. Si no existe, retorna null
    /// 3. Si existe, extrae el valor de pdf_certification
    /// 4. Evalúa el valor según estas reglas:
    ///    - Si empieza con 'data:application/pdf;base64,' → mantenerlo intacto (almacenamiento local)
    ///    - Si es una URL HTTPS completa → devolverla tal cual
    ///    - Si es una clave simple → concatenarla con URL base de Google Cloud Storage
    /// 5. Retorna el aggregado Car con la URL del PDF resuelta
    /// </summary>
    /// <param name="query">Query de tipo GetCarByIdQuery que contiene el ID del informe a recuperar.</param>
    /// <returns>Entidad Car si existe, null en caso contrario. El PDF_certification estará resuelto.</returns>
    Task<Car?> Handle(GetCarByIdQuery query);
}
