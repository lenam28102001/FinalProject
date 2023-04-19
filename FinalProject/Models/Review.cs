using System;
using System.Collections.Generic;

#nullable disable

namespace FinalProject.Models
{
    public partial class Review
    {
        public int ReviewId { get; set; }
        public string Message { get; set; }
        public int? Rating { get; set; }
        public int? ProductId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime? DateTime { get; set; }
        public bool Anonymoust { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Product Product { get; set; }
    }
}
