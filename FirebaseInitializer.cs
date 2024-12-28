using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

public class FirebaseInitializer {
  public static void Initialize() {
    FirebaseApp.Create(new AppOptions {
      Credential = GoogleCredential.FromFile("./serviceAccountKey.json")
    });
  }
}
