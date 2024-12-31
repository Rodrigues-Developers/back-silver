using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

public class FirebaseInitializer {
  public static void Initialize() {
    var serviceAccountJson = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_JSON");
    FirebaseApp.Create(new AppOptions {
      Credential = GoogleCredential.FromJson(serviceAccountJson),
    });
  }
}
