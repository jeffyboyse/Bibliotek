using System;

namespace bibliotek.Models
{
    public class Media
    {
        public int MediaID { get; set; } //primary key
        public string Title { get; set; }
        public int Publish_Year { get; set; }
        public decimal MediaValue { get; set; }
    }
        
        
}