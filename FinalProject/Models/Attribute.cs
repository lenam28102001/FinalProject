using System;
using System.Collections.Generic;

#nullable disable

namespace FinalProject.Models
{
    public partial class Attribute
    {
        public Attribute()
        {
            AttribustesPrices = new HashSet<AttribustesPrice>();
        }

        public int AttributeId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<AttribustesPrice> AttribustesPrices { get; set; }
    }
}
