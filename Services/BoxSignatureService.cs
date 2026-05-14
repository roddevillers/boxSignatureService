using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Box.V2.Models;
using boxSignatureService.Models;

namespace boxSignatureService.Services
{
    /// <summary>
    /// Service for managing Box file uploads and signature requests
    /// </summary>
    public class BoxSignatureService
    {
        /// <summary>
        /// Uploads a file to Box and returns the file ID
        /// </summary>
        /// <param name="fileUpload">File upload request containing file data</param>
        /// <param name="parentFolderId">Optional parent folder ID (defaults to root)</param>
        /// <returns>Uploaded file ID</returns>
        public async Task<string> UploadFileAsync(FileUploadRequest fileUpload, string parentFolderId = "0")
        {
            try
            {
                var boxClient = BoxAuthenticationService.GetBoxClient();

                // Convert base64 string to byte array
                byte[] fileBytes = Convert.FromBase64String(fileUpload.FileContent);

                // Create memory stream from bytes
                using (var stream = new MemoryStream(fileBytes))
                {
                    // Prepare upload request
                    var uploadRequest = new BoxFileRequest
                    {
                        Name = fileUpload.FileName,
                        Parent = new BoxRequestEntity { Id = parentFolderId }
                    };

                    // Upload file to Box
                    var uploadedFile = await boxClient.FilesManager.UploadAsync(uploadRequest, stream);

                    return uploadedFile.Id;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to upload file to Box: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a signature request workflow for a file
        /// </summary>
        /// <param name="signatureRequest">Signature request details</param>
        /// <returns>Response with signature request ID</returns>
        public async Task<SignatureResponse> CreateSignatureRequestAsync(SignatureRequest signatureRequest)
        {
            try
            {
                var boxClient = BoxAuthenticationService.GetBoxClient();

                // Build list of signers
                var signers = signatureRequest.SignerEmails
                    .Select(email => new BoxSignRequestSignerInput
                    {
                        Email = email
                    })
                    .ToList();

                // Build signature request
                var signRequestInput = new BoxSignRequestInput
                {
                    Signers = signers,
                    Subject = signatureRequest.Subject ?? "Please sign this document",
                    Message = signatureRequest.Message ?? "Please review and sign this document",
                    SourceFiles = new List<BoxFileReference>
                    {
                        new BoxFileReference { Id = signatureRequest.FileId }
                    }
                };

                // Add due date if provided
                if (!string.IsNullOrEmpty(signatureRequest.DueDate))
                {
                    if (DateTime.TryParse(signatureRequest.DueDate, out var dueDate))
                    {
                        signRequestInput.DueDate = dueDate;
                    }
                }

                // Add parent folder if specified
                if (!string.IsNullOrEmpty(signatureRequest.ParentFolderId))
                {
                    signRequestInput.ParentFolder = new BoxFolderReference
                    {
                        Id = signatureRequest.ParentFolderId
                    };
                }

                // Create the signature request
                var signRequest = await boxClient.SignRequestsManager.CreateSignRequestAsync(signRequestInput);

                return new SignatureResponse
                {
                    Success = true,
                    SignatureRequestId = signRequest.Id,
                    Message = "Signature request created successfully"
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create signature request: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the status of a signature request
        /// </summary>
        /// <param name="signRequestId">The signature request ID</param>
        /// <returns>Signature request details</returns>
        public async Task<BoxSignRequest> GetSignatureRequestStatusAsync(string signRequestId)
        {
            try
            {
                var boxClient = BoxAuthenticationService.GetBoxClient();
                var signRequest = await boxClient.SignRequestsManager.GetSignRequestAsync(signRequestId);
                return signRequest;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to get signature request status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cancels a signature request
        /// </summary>
        /// <param name="signRequestId">The signature request ID</param>
        public async Task CancelSignatureRequestAsync(string signRequestId)
        {
            try
            {
                var boxClient = BoxAuthenticationService.GetBoxClient();
                await boxClient.SignRequestsManager.CancelSignRequestAsync(signRequestId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to cancel signature request: {ex.Message}", ex);
            }
        }
    }
}
