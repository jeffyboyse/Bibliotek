using System;

namespace bibliotek.Models
{
    public class MediaAttribute
    {
        public int MediaID { get; set; }
        public Media Media { get; set; } = null!; // Länk till Media-objektet

        public int AttributeID { get; set; }
        public Attribute Attribute { get; set; } = null!; // Länk till Attribute-objektet

        public string Value { get; set; } = string.Empty;
    }
}