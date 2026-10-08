using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebStorageSample.Pages
{
    [RequestSizeLimit(IndexModel.MaxFileSize + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = IndexModel.MaxFileSize + 1024 * 1024)]
    public class IndexModel : PageModel
    {
        public const long MaxFileSize = 20 * 1024 * 1024;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UploadedFile == null)
            {
                ModelState.AddModelError(string.Empty, "Selecciona un archivo antes de subirlo.");
                return Page();
            }

            if (UploadedFile.Length > MaxFileSize)
            {
                ModelState.AddModelError(string.Empty, "El archivo supera el límite de 20 MB. Selecciona uno más pequeño.");
                return Page();
            }

            // Quita rutas enviadas por el cliente (Windows y Linux), conservando el nombre.
            string fileName = Path.GetFileName(UploadedFile.FileName.Replace('\\', '/'));
            if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 1024 || fileName.Any(char.IsControl))
            {
                ModelState.AddModelError(string.Empty, "El nombre del archivo no es válido. Renómbralo y vuelve a intentarlo.");
                return Page();
            }

            if (!ModelState.IsValid)
                return Page();

            string endpoint = Environment.GetEnvironmentVariable(Const.ENDPOINT_ENV_KEY);
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                _logger.LogError("Storage endpoint {EnvironmentKey} is not configured.", Const.ENDPOINT_ENV_KEY);
                ModelState.AddModelError(string.Empty, "El almacenamiento no está configurado. Contacta al administrador.");
                return Page();
            }

            try
            {
                using (var stream = UploadedFile.OpenReadStream())
                {
                    await StorageHelper.UploadBlob(endpoint, Const.CONTAINER_NAME, fileName,
                        stream, HttpContext.RequestAborted);
                }

                SuccessMessage = $"El archivo «{fileName}» se subió correctamente.";
                // Recargar la página no debe volver a subir el archivo.
                return RedirectToPage();
            }
            catch (RequestFailedException exception) when (exception.ErrorCode == "BlobAlreadyExists"
                || exception.ErrorCode == "ConditionNotMet")
            {
                ModelState.AddModelError(string.Empty, "Ya existe un archivo con ese nombre. Renómbralo y vuelve a intentarlo.");
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                _logger.LogError(exception, "File upload failed.");
                ModelState.AddModelError(string.Empty, "No se pudo subir el archivo. Vuelve a intentarlo; si el problema continúa, contacta al administrador.");
            }

            return Page();
        }
    }
}