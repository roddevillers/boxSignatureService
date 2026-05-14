using System;
using System.Configuration;
using Box.V2;
using Box.V2.Auth;
using Box.V2.Config;

namespace boxSignatureService.Services
{
    /// <summary>
    /// Service for handling Box API authentication using Enterprise Key
    /// </summary>
    public class BoxAuthenticationService
    {
        private static BoxClient _boxClient;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Gets or creates a Box API client using enterprise key authentication
        /// </summary>
        /// <returns>Authenticated BoxClient instance</returns>
        public static BoxClient GetBoxClient()
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
                            // Parse the enterprise key JSON
                            var boxConfig = new BoxConfig(enterpriseKeyJson);

                            // Create Box client with enterprise authentication
                            _boxClient = new BoxClient(boxConfig);

                            // Authenticate the client
                            _boxClient.Auth.Authenticate().Wait();
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
