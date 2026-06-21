using System;

namespace CertiWeb.API.Shared.Infrastructure.Storage
{
    /// <summary>
    /// Servicio encargado de resolver la URL final de un PDF a partir de
    /// contenido Base64 almacenado en la base de datos o a partir de una
    /// key/identificador que apunte a un objeto en Cloud Storage.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// </summary>
        /// <param name="pdfDataOrKey">Contenido base64, data URL o key/path del archivo en cloud.</param>
        /// <returns>URL final lista para consumir desde el navegador.</returns>
        string ResolvePdfUrl(string pdfDataOrKey);
    }
}

