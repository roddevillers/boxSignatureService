using System.Collections.Generic;

namespace boxSignatureService.Models
{
    /// <summary>
    /// Represents a signature request to be created in Box
    /// </summary>
    public class SignatureRequest
    {
        /// <summary>
        /// Box file ID to request signature on
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// List of signer email addresses
        /// </summary>
        public List<string> SignerEmails { get; set; }

        /// <summary>
        /// Subject line for the signature request email
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Message body for the signature request email
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Optional - Due date for signature in ISO 8601 format (YYYY-MM-DD)
        /// </summary>
        public string DueDate { get; set; }

        /// <summary>
        /// Optional - Parent folder ID for signature workflow document
        /// </summary>
        public string ParentFolderId { get; set; }
    }
}
