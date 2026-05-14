using System;
using System.Configuration;
using System.Threading.Tasks;
using Box.V2;
using Box.V2.Auth;
using Box.V2.Config;
using Box.V2.JwtAuth;

namespace boxSignatureService.Services
{
    /// <summary>
    /// Service for handling Box API authentication using Enterprise Key
    /// </summary>
    public class BoxAuthenticationService
    {
        private static IBoxClient _boxClient;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Gets or creates a Box API client using enterprise key authentication
        /// </summary>
        /// <returns>Authenticated BoxClient instance</returns>
        public static IBoxClient GetBoxClient()
        {
            if (_boxClient == null)
            {
                lock (_lockObject)
                {
                    if (_boxClient == null)
                    {
                        var enterpriseKeyJson = ConfigurationManager.AppSettings["BoxEnterpriseKey"];
                        
                        if (string.IsNullOrEmpty(enterpriseKeyJson))
                        {
                            throw new InvalidOperationException(
                                "BoxEnterpriseKey is not configured. Please add your Box enterprise key JSON to Web.config");
                        }

                        try
                        {
                            // Initialize JWT auth with enterprise key
                            var boxJwtAuth = new BoxJwtAuth(enterpriseKeyJson);
                            
                            // Get access token
                            var token = boxJwtAuth.GetUnmanagedToken();
                            
                            // Create Box client
                            _boxClient = new BoxClient(
                                new Uri("https://api.box.com/2.0/"),
                                new OAuthSession(token.AccessToken)
                            );
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException(
                                "Failed to authenticate with Box API. Ensure your enterprise key is valid.", ex);
                        }
                    }
                }
            }

            return _boxClient;
        }
    }
}
