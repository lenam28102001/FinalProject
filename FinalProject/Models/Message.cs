using System;
using System.Collections.Generic;

#nullable disable

namespace FinalProject.Models
{
    public partial class Message
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        public DateTime DateTime { get; set; }
        public int CustomerId { get; set; }

        public virtual Customer Customer { get; set; }
    }
}
