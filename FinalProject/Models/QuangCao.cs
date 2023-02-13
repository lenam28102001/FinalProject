using System;
using System.Collections.Generic;

#nullable disable

namespace FinalProject.Models
{
    public partial class QuangCao
    {
        public int QuangCaoId { get; set; }
        public string SubTitle { get; set; }
        public string Title { get; set; }
        public string ImageBg { get; set; }
        public bool Published { get; set; }
        public string UrlLink { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
