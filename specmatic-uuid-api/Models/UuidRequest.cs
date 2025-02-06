using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using specmatic_uuid_api.Models.Entity;

namespace specmatic_uuid_api.Models
{
    public class UuidRequest: Customer
    {
        [JsonPropertyName("uuidType")]
        [Required(ErrorMessage = "UUID type is required")]
        public UuidType UuidType { get; set; }
    }
}
