namespace boxSignatureService.Models
{
    /// <summary>
    /// Represents a file upload request from the client
    /// </summary>
    public class FileUploadRequest
    {
        /// <summary>
        /// Original filename
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File content as base64 string
        /// </summary>
        public string FileContent { get; set; }

        /// <summary>
        /// File MIME type
        /// </summary>
        public string ContentType { get; set; }
    }
}
