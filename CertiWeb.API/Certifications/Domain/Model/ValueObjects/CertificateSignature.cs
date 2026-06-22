using System.Security.Cryptography;
using System.Text;

namespace CertiWeb.API.Certifications.Domain.Model.ValueObjects;

/// <summary>
/// Value Object que representa la firma digital SHA256 de un certificado.
/// Garantiza la integridad y verificabilidad del certificado.
/// </summary>
public record CertificateSignature
{
    public string Hash { get; }

    public CertificateSignature(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Certificate signature hash cannot be empty.");
        Hash = hash;
    }

    /// <summary>
    /// Genera la firma SHA256 a partir de los datos del certificado.
    /// </summary>
    public static CertificateSignature Generate(
        string licensePlate, string ownerEmail, string model, int year, DateTime createdAt)
    {
        var rawData = $"{licensePlate}|{ownerEmail}|{model}|{year}|{createdAt:O}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        var hash = Convert.ToHexString(bytes).ToLowerInvariant();
        return new CertificateSignature(hash);
    }

    /// <summary>
    /// Verifica si la firma es válida para los datos dados.
    /// </summary>
    public bool Verify(
        string licensePlate, string ownerEmail, string model, int year, DateTime createdAt)
    {
        var expected = Generate(licensePlate, ownerEmail, model, year, createdAt);
        return Hash == expected.Hash;
    }

    public static implicit operator string(CertificateSignature sig) => sig.Hash;
}

