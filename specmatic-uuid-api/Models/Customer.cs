using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace specmatic_uuid_api.Models
{
    public class Customer
    {
        [JsonPropertyName("firstName")]
        [Required(ErrorMessage = "First name is required.")]
        public required string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        [Required(ErrorMessage = "Last name is required.")]
        public required string LastName { get; set; }

        [JsonPropertyName("email")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }
}
