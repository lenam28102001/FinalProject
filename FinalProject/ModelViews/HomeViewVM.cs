using FinalProject.Models;
using System.Collections.Generic;

namespace FinalProject.ModelViews
{
    public class HomeViewVM
    {
        public List<News> News { get; set; }
        public List<ProductHomeVM> Products { get; set; }
        public QuangCao quangcao { get; set; }

    }
}
