using Newtonsoft.Json;



namespace CloudService
{
    public class CreateSaveRequest
    {
        public string userId;
        public string saveId;
        public string name;
        [JsonProperty("namespace")]
        public string saveNamespace;
        public string progressType;
        public FileUploadConfirmation fileUploadRequest;
    }
}