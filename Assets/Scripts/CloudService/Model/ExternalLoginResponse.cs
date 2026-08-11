namespace CloudService
{
    public class ExternalLoginResponse
    {
        public string personaAccessToken;
        public string personaRefreshToken;
        public int expiresAt;
        public PersonaInfo persona;
    }
}