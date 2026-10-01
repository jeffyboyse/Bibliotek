using System.Collections.Generic;

namespace bibliotek.Models
{
    public class Attribute
    {
        public int AttributeID { get; set; }
        public string Name { get; set; } = string.Empty;

        // Gör att EF Core kan hämta alla kopplingar till detta attribut
        public ICollection<MediaAttribute> MediaAttribute { get; set; } = new List<MediaAttribute>();
    }
}