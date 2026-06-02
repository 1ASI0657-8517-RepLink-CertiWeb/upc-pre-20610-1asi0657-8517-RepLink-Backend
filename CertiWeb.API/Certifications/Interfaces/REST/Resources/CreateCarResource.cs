using System.ComponentModel.DataAnnotations;

namespace CertiWeb.API.Certifications.Interfaces.REST.Resources;

/// <summary>
/// Data transfer object for creating a new car certification.
/// </summary>
/// <param name="Title">The title of the car.</param>
/// <param name="Owner">The owner's name.</param>
/// <param name="OwnerEmail">The owner's email address.</param>
/// <param name="Year">The car's year.</param>
/// <param name="BrandId">The brand ID.</param>
/// <param name="Model">The car model.</param>
/// <param name="Description">The car description.</param>
/// <param name="PdfCertification">The PDF certification as Base64.</param>
/// <param name="ImageUrl">The car image URL.</param>
/// <param name="Price">The car price.</param>
/// <param name="LicensePlate">The license plate.</param>
/// <param name="OriginalReservationId">The original reservation ID.</param>
public record CreateCarResource(
    [MaxLength(200)] string? Title = null,
    [MaxLength(100)] string? Owner = null,
    [EmailAddress] [MaxLength(100)] string? OwnerEmail = null,
    int Year = 0,
    int BrandId = 0,
    [MaxLength(100)] string? Model = null,
    [MaxLength(500)] string? Description = null,
    [MaxLength(5000)] string? PdfCertification = null,
    [MaxLength(500)] string? ImageUrl = null,
    decimal Price = 0,
    [MaxLength(15)] string? LicensePlate = null,
    int OriginalReservationId = 0
);