using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using boxSignatureService.Models;
using boxSignatureService.Services;

namespace boxSignatureService.Controllers
{
    /// <summary>
    /// Web API controller for handling signature requests and file uploads
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/signature")]
    public class SignatureController : ApiController
    {
        private readonly BoxSignatureService _boxSignatureService = new BoxSignatureService();

        /// <summary>
        /// Uploads a file to Box
        /// </summary>
        /// <param name="fileUpload">File upload request with base64 encoded content</param>
        /// <returns>Response with uploaded file ID</returns>
        [HttpPost]
        [Route("upload")]
        public async Task<IHttpActionResult> UploadFile([FromBody] FileUploadRequest fileUpload)
        {
            try
            {
                if (fileUpload == null || string.IsNullOrEmpty(fileUpload.FileContent))
                {
                    return BadRequest("File content is required");
                }

                if (string.IsNullOrEmpty(fileUpload.FileName))
                {
                    return BadRequest("File name is required");
                }

                // Validate file size (from Web.config)
                int maxFileSizeMB = int.TryParse(ConfigurationManager.AppSettings["MaxFileSize"], out int size) ? size : 100;
                long maxFileSize = maxFileSizeMB * 1024 * 1024;
                
                long contentLength = fileUpload.FileContent.Length; // Rough estimate (base64 is ~33% larger)
                if (contentLength > maxFileSize * 1.4) // Add 40% buffer for base64 encoding
                {
                    return BadRequest($"File size exceeds maximum allowed size of {maxFileSizeMB}MB");
                }

                // Upload file to Box
                string fileId = await _boxSignatureService.UploadFileAsync(fileUpload);

                return Ok(new SignatureResponse
                {
                    Success = true,
                    FileId = fileId,
                    Message = "File uploaded successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return InternalServerError(new Exception(ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"An unexpected error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// Creates a signature request for an uploaded file
        /// </summary>
        /// <param name="signatureRequest">Signature request details</param>
        /// <returns>Response with signature request ID</returns>
        [HttpPost]
        [Route("create")]
        public async Task<IHttpActionResult> CreateSignatureRequest([FromBody] SignatureRequest signatureRequest)
        {
            try
            {
                if (signatureRequest == null)
                {
                    return BadRequest("Signature request data is required");
                }

                if (string.IsNullOrEmpty(signatureRequest.FileId))
                {
                    return BadRequest("File ID is required");
                }

                if (signatureRequest.SignerEmails == null || signatureRequest.SignerEmails.Count == 0)
                {
                    return BadRequest("At least one signer email is required");
                }

                // Validate email addresses
                foreach (var email in signatureRequest.SignerEmails)
                {
                    if (!IsValidEmail(email))
                    {
                        return BadRequest($"Invalid email address: {email}");
                    }
                }

                // Create signature request
                var response = await _boxSignatureService.CreateSignatureRequestAsync(signatureRequest);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return InternalServerError(new Exception(ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"An unexpected error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// Gets the status of a signature request
        /// </summary>
        /// <param name="signRequestId">The signature request ID</param>
        /// <returns>Signature request status</returns>
        [HttpGet]
        [Route("status/{signRequestId}")]
        public async Task<IHttpActionResult> GetSignatureStatus(string signRequestId)
        {
            try
            {
                if (string.IsNullOrEmpty(signRequestId))
                {
                    return BadRequest("Sign request ID is required");
                }

                var signRequest = await _boxSignatureService.GetSignatureRequestStatusAsync(signRequestId);
                return Ok(signRequest);
            }
            catch (InvalidOperationException ex)
            {
                return InternalServerError(new Exception(ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"An unexpected error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cancels a signature request
        /// </summary>
        /// <param name="signRequestId">The signature request ID</param>
        /// <returns>Success response</returns>
        [HttpPost]
        [Route("cancel/{signRequestId}")]
        public async Task<IHttpActionResult> CancelSignatureRequest(string signRequestId)
        {
            try
            {
                if (string.IsNullOrEmpty(signRequestId))
                {
                    return BadRequest("Sign request ID is required");
                }

                await _boxSignatureService.CancelSignatureRequestAsync(signRequestId);
                return Ok(new SignatureResponse
                {
                    Success = true,
                    Message = "Signature request cancelled successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return InternalServerError(new Exception(ex.Message));
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception($"An unexpected error occurred: {ex.Message}"));
            }
        }

        /// <summary>
        /// Validates an email address format
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
