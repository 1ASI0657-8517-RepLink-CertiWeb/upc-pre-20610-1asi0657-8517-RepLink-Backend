namespace CertiWeb.API.Certifications.Interfaces.REST.Resources;

public record CertificateValidityResource(
    int CarId,
    DateTime CreatedAt,
    DateTime ExpirationDate,
    bool IsValid,
    int RemainingDays
    );