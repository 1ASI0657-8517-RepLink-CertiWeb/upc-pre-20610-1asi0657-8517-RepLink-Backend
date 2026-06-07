namespace CertiWeb.API.Certifications.Interfaces.REST.Resources;

/// <summary>
/// DTO para la respuesta del informe técnico de un vehículo.
/// </summary>

public record ReportResource(
    int Id,
    int CarId,
    string Title,
    string Owner,
    string OwnerEmail,
    string Brand,
    string Model,
    int Year,
    string LicensePlate,
    string PdfUrl
);

