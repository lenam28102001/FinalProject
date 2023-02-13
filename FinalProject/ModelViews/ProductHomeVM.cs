using System;
using System.Collections.Generic;
using FinalProject.Models;

namespace FinalProject.ModelViews
{
    public class ProductHomeVM
    {
        public Category category { get; set; }
        public List<Product> lsProducts { get; set; }
    }
}