namespace boxSignatureService.Models
{
    /// <summary>
    /// Response from signature request operations
    /// </summary>
    public class SignatureResponse
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The Box file ID that was uploaded (in file upload response)
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// The signature request ID (in signature creation response)
        /// </summary>
        public string SignatureRequestId { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Error details if operation failed
        /// </summary>
        public string Error { get; set; }
    }
}
