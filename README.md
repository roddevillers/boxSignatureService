# Box Signature Service

ASP.NET 4.8.1 Web API for requesting signatures on files via Box.com Sign API.

## Overview

This service provides a backend API for:
1. **File Upload**: Accept files from a JavaScript frontend and upload them to Box
2. **Signature Requests**: Create signature workflows for the uploaded files
3. **Status Tracking**: Monitor the status of signature requests
4. **Cancellation**: Cancel pending signature requests

## Features

- ASP.NET 4.8.1 Web API
- Box.com Sign API integration
- Enterprise key authentication
- CORS enabled for JavaScript frontend integration
- Base64 file upload handling
- Email validation for signers
- File size validation

## Prerequisites

- .NET Framework 4.8.1
- Visual Studio 2019 or later (or command-line build tools)
- Box.com enterprise account with Sign API access
- Enterprise key JSON file from Box

## Setup

### 1. Configure Box Enterprise Key

Edit `Web.config` and add your Box enterprise key:

```xml
<appSettings>
  <add key="BoxEnterpriseKey" value='{"type":"enterprise","id":"...","private_key":"..."}' />
</appSettings>
```

The enterprise key should be a complete JSON object from your Box developer console.

### 2. Restore NuGet Packages

```bash
nuget restore boxSignatureService.csproj
```

Or in Visual Studio Package Manager Console:

```powershell
Install-Package Microsoft.AspNet.WebApi.Core -Version 5.2.7
Install-Package Microsoft.AspNet.WebApi.WebHost -Version 5.2.7
Install-Package Newtonsoft.Json -Version 13.0.1
Install-Package Box.V2 -Version 4.16.0
```

### 3. Build the Project

```bash
msbuild boxSignatureService.csproj /p:Configuration=Release
```

### 4. Deploy

Deploy to IIS or run locally using IIS Express in Visual Studio.

## API Endpoints

### Upload File

**POST** `/api/signature/upload`

Upload a file to Box (from JavaScript frontend).

**Request Body:**
```json
{
  "fileName": "document.pdf",
  "fileContent": "base64-encoded-file-content",
  "contentType": "application/pdf"
}
```

**Response:**
```json
{
  "success": true,
  "fileId": "123456789",
  "message": "File uploaded successfully"
}
```

### Create Signature Request

**POST** `/api/signature/create`

Create a signature workflow for a file.

**Request Body:**
```json
{
  "fileId": "123456789",
  "signerEmails": ["user1@example.com", "user2@example.com"],
  "subject": "Please sign this agreement",
  "message": "Please review and sign the attached document",
  "dueDate": "2026-06-30",
  "parentFolderId": "0"
}
```

**Response:**
```json
{
  "success": true,
  "signatureRequestId": "abc123def456",
  "message": "Signature request created successfully"
}
```

### Get Signature Request Status

**GET** `/api/signature/status/{signRequestId}`

Get the current status of a signature request.

**Response:**
```json
{
  "id": "abc123def456",
  "status": "sent",
  "signers": [...],
  "sourceFiles": [...],
  ...
}
```

### Cancel Signature Request

**POST** `/api/signature/cancel/{signRequestId}`

Cancel a pending signature request.

**Response:**
```json
{
  "success": true,
  "message": "Signature request cancelled successfully"
}
```

## JavaScript Frontend Example

```javascript
// Step 1: Upload file to backend
async function uploadFile(file) {
  const reader = new FileReader();
  
  reader.onload = async (event) => {
    const base64Content = event.target.result.split(',')[1]; // Remove data URI prefix
    
    const response = await fetch('http://localhost:5000/api/signature/upload', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        fileName: file.name,
        fileContent: base64Content,
        contentType: file.type
      })
    });
    
    const result = await response.json();
    return result.fileId;
  };
  
  reader.readAsDataURL(file);
}

// Step 2: Request signatures
async function requestSignatures(fileId, signerEmails, subject, message) {
  const response = await fetch('http://localhost:5000/api/signature/create', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      fileId: fileId,
      signerEmails: signerEmails,
      subject: subject,
      message: message
    })
  });
  
  const result = await response.json();
  return result.signatureRequestId;
}

// Step 3: Monitor status
async function checkSignatureStatus(signRequestId) {
  const response = await fetch(`http://localhost:5000/api/signature/status/${signRequestId}`);
  const result = await response.json();
  console.log('Status:', result.status);
}
```

## Configuration

Key settings in `Web.config`:

- `BoxEnterpriseKey`: Your Box enterprise key (JSON string)
- `MaxFileSize`: Maximum file size in MB (default: 100)

## Error Handling

All endpoints return appropriate HTTP status codes:

- `200 OK`: Operation successful
- `400 Bad Request`: Invalid input
- `500 Internal Server Error`: Server error with details

Error responses include an `error` field with details.

## Security Considerations

1. **HTTPS Only**: Always use HTTPS in production
2. **Authentication**: Consider adding API key or OAuth authentication to your endpoints
3. **Rate Limiting**: Implement rate limiting to prevent abuse
4. **Enterprise Key**: Keep your Box enterprise key secure (use environment variables in production)
5. **CORS**: The current configuration allows all origins; restrict this in production

## Troubleshooting

### "BoxEnterpriseKey is not configured"

Ensure your `Web.config` contains a valid Box enterprise key JSON string.

### "Failed to authenticate with Box API"

Verify:
- Enterprise key JSON is valid
- The key has the correct permissions
- The key's service account is activated in Box

### File upload fails

Check:
- File size doesn't exceed `MaxFileSize` setting
- File content is properly base64 encoded
- The service account has write access to the target folder

### Signature request creation fails

Verify:
- File ID exists in Box
- Signer emails are valid
- The file format supports signing (PDF, Word, etc.)

## License

MIT License

## Support

For Box API issues, see: https://developer.box.com/docs/box-sign
