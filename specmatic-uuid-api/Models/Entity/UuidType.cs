using System.Runtime.Serialization;

namespace specmatic_uuid_api.Models.Entity
{
    public enum UuidType
    {
        [EnumMember(Value = "Regular")]
        Regular,

        [EnumMember(Value = "Premium")]
        Premium,

        [EnumMember(Value = "Business")]
        Business,

        [EnumMember(Value = "Enterprise")]
        Enterprise
    }
}
