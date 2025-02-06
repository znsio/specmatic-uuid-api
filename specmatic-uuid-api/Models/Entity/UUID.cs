using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace specmatic_uuid_api.Models.Entity
{
    [Table("uuid")]
    public class UUID
    {

        [Key]
        [Column("uuid")]
        public Guid Uuid { get; set; }

        [Column("first_name")]
        public required string FirstName { get; set; }

        [Column("last_name")]
        public required string LastName { get; set; }

        [Column("email")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Email { get; set; }

        [Column("uuid_type")]
        public required UuidType UuidType { get; set; }
    }
}
