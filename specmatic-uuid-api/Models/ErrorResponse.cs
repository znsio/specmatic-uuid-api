using System.Text.Json.Serialization;

namespace specmatic_uuid_api.Models
{
    public class ErrorResponse
    {
        [JsonPropertyName("timestamp")]
        public required string TimeStamp { get; set; }

        [JsonPropertyName("error")]
        public required string Error { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        public ErrorResponse() { }
    }
}
