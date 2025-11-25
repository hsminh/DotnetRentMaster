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
                var projectId     = Environment.GetEnvironmentVariable("GCP_PROJECT_ID");
                var privateKeyId  = Environment.GetEnvironmentVariable("GCP_PRIVATE_KEY_ID");
                var privateKeyRaw = Environment.GetEnvironmentVariable("GCP_PRIVATE_KEY");
                var clientEmail   = Environment.GetEnvironmentVariable("GCP_CLIENT_EMAIL");
                var clientId      = Environment.GetEnvironmentVariable("GCP_CLIENT_ID");
                var clientX509Url = Environment.GetEnvironmentVariable("GCP_CLIENT_X509_CERT_URL");

                if (string.IsNullOrWhiteSpace(privateKeyRaw))
                    throw new ArgumentException("GCP_PRIVATE_KEY missing");

                var privateKey = privateKeyRaw.Trim().Trim('"').Replace("\\n", "\n");

                var firebaseDict = new Dictionary<string, object?>
                {
                    { "type", "service_account" },
                    { "project_id", projectId },
                    { "private_key_id", privateKeyId },
                    { "private_key", privateKey },
                    { "client_email", clientEmail },
                    { "client_id", clientId },
                    { "auth_uri", "https://accounts.google.com/o/oauth2/auth" },
                    { "token_uri", "https://oauth2.googleapis.com/token" },
                    { "auth_provider_x509_cert_url", "https://www.googleapis.com/oauth2/v1/certs" },
                    { "client_x509_cert_url", clientX509Url },
                    { "universe_domain", "googleapis.com" }
                };

                var json = JsonSerializer.Serialize(firebaseDict);
                var credential = GoogleCredential.FromJson(json);

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential,
                        ProjectId = projectId
                    });
                }

                _initialized = true;
                logger?.LogInformation("Firebase initialized successfully (env-based).");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error initializing Firebase");
                throw;
            }
        }
    }
}
