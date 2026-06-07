namespace CertiWeb.API.Certifications.Interfaces.REST.Resources;

/// <summary>
/// DTO minimalista para la respuesta del endpoint GET /api/v1/reports/{id}.
/// Contiene exactamente los campos requeridos: Id, CarId, Owner, OwnerEmail, PdfUrl.
/// </summary>

public record GetReportByIdResponse(
    int Id,
    int CarId,
    string Owner,
    string OwnerEmail,
    string PdfUrl
);

