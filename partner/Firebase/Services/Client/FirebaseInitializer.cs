using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.Text.Json;

namespace RentMaster.partner.Firebase.Services.Client;

public static class FirebaseClientConfig
{
    private static bool _initialized = false;
    private static readonly object _lock = new object();

    public static void Init(ILogger? logger = null)
    {
        lock (_lock)
        {
            if (_initialized) return;

            try
            {
                // Kiểm tra các biến môi trường cần thiết
                var requiredVars = new[]
                {
                    "GCP_PROJECT_ID", "GCP_PRIVATE_KEY", "GCP_CLIENT_EMAIL",
                    "GCP_PRIVATE_KEY_ID", "GCP_CLIENT_ID", "GCP_CLIENT_X509_CERT_URL"
                };

                foreach (var varName in requiredVars)
                {
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(varName)))
                    {
                        throw new ArgumentException($"Missing required environment variable: {varName}");
                    }
                }

                var privateKey = Environment.GetEnvironmentVariable("GCP_PRIVATE_KEY") ?? "";
                
                // Xử lý private key đúng cách
                privateKey = privateKey.Trim().Trim('"').Replace("\\n", "\n");
                
                // Kiểm tra định dạng private key
                if (!privateKey.Contains("BEGIN PRIVATE KEY"))
                {
                    throw new ArgumentException("Invalid private key format");
                }

                var firebaseDict = new Dictionary<string, object?>
                {
                    { "type", "service_account" },
                    { "project_id", Environment.GetEnvironmentVariable("GCP_PROJECT_ID") },
                    { "private_key_id", Environment.GetEnvironmentVariable("GCP_PRIVATE_KEY_ID") },
                    { "private_key", privateKey },
                    { "client_email", Environment.GetEnvironmentVariable("GCP_CLIENT_EMAIL") },
                    { "client_id", Environment.GetEnvironmentVariable("GCP_CLIENT_ID") },
                    { "auth_uri", "https://accounts.google.com/o/oauth2/auth" },
                    { "token_uri", "https://oauth2.googleapis.com/token" },
                    { "auth_provider_x509_cert_url", "https://www.googleapis.com/oauth2/v1/certs" },
                    { "client_x509_cert_url", Environment.GetEnvironmentVariable("GCP_CLIENT_X509_CERT_URL") }
                };

                var json = JsonSerializer.Serialize(firebaseDict);
                logger?.LogInformation("Firebase credentials configured for project: {ProjectId}", 
                    Environment.GetEnvironmentVariable("GCP_PROJECT_ID"));

                var credential = GoogleCredential.FromJson(json);

                // Tạo FirebaseApp với options
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential,
                        ProjectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID")
                    });
                }

                _initialized = true;
                logger?.LogInformation("Firebase initialized successfully.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error initializing Firebase");
                throw;
            }
        }
    }
}