namespace CertiWeb.API.Shared.Infrastructure.Storage;

/// <summary>
/// Implementación local de IStorageService que resuelve URLs basándose en
/// contenido Base64 almacenado en la base de datos o URLs en Cloud Storage.
/// 

/// </summary>
public class LocalBase64StorageService : IStorageService
{
    /// <summary>
    /// URL base del bucket público de Google Cloud Storage para reportes.
    /// </summary>
    private const string GcsPublicBucketBase = "https://storage.googleapis.com/replink-certiweb-reports/";

    /// <summary>
    /// Resuelve la URL final del PDF a partir de Base64 o una clave de Cloud Storage.
    /// </summary>
    /// <param name="pdfDataOrKey">Contenido Base64, data URL, URL HTTPS o key/path de Cloud Storage.</param>
    /// <returns>URL final lista para consumir (data URL o URL HTTPS).</returns>
    public string ResolvePdfUrl(string pdfDataOrKey)
    {
        if (string.IsNullOrWhiteSpace(pdfDataOrKey))
            return string.Empty;

        var trimmedValue = pdfDataOrKey.Trim();

        // Regla 1: Si ya es una data URL válida (Base64 embebido), devolverla intacto
        if (trimmedValue.StartsWith("data:application/pdf;base64,", StringComparison.OrdinalIgnoreCase))
            return trimmedValue;

        // Regla 2: Si es una URL HTTP/HTTPS completa, es de Cloud Storage o pública
        if (trimmedValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmedValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return trimmedValue;

        // Regla 3: Si es un cloud path (s3://, gs://, azure://), devolverlo como es
        if (trimmedValue.StartsWith("s3://", StringComparison.OrdinalIgnoreCase) ||
            trimmedValue.StartsWith("gs://", StringComparison.OrdinalIgnoreCase) ||
            trimmedValue.StartsWith("azure://", StringComparison.OrdinalIgnoreCase))
            return trimmedValue;

        // Regla 4: Si es una clave simple, procesarla como clave de Google Cloud Storage
        // Concatenar con la URL base pública del bucket
        if (!trimmedValue.Contains("://") && !trimmedValue.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return $"{GcsPublicBucketBase}{trimmedValue}";
        }

        // Regla 5: Si todo lo demás falla, asumir que es contenido Base64 y envolver en data URL
        if (!trimmedValue.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return $"data:application/pdf;base64,{trimmedValue}";
        }

        return trimmedValue;
    }
}
