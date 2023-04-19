using DocumentFormat.OpenXml.Office2010.Excel;
using FinalProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject.Controllers
{
    public class ProductController : Controller
    {
        private readonly FinalProjectContext _context;
        public ProductController(FinalProjectContext context)
        {
            _context = context;
        }

        [Route("shop.html", Name = ("ShopProduct"))]
        public IActionResult Index(int? page, int CatID = 0)
        {
            try
            {
                var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                var pageSize = 10;

                var lsProducts = _context.Products
                     .AsNoTracking()
                     .Include(x => x.Cat)
                     .OrderBy(x => x.ProductId);
                if (CatID != 0)
                {
                     lsProducts = _context.Products
                    .AsNoTracking()
                    .Where(x => x.CatId == CatID)
                    .Include(x => x.Cat)
                    .OrderBy(x => x.ProductId);
                }
               
                PagedList<Product> models = new PagedList<Product>(lsProducts, pageNumber, pageSize);
                var lsReview = _context.Reviews.AsNoTracking().Include(x => x.Customer).Include(x => x.Product).OrderByDescending(x => x.DateTime).ToList();
                ViewBag.Review = lsReview;
                ViewBag.CurrentPage = pageNumber;
                ViewData["DanhMuc"] = new SelectList(_context.Categories, "CatId", "CatName");
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public IActionResult Filtter(int CatID = 0)
        {
            var url = $"/shop.html?CatID={CatID}";
            if (CatID == 0)
            {
                url = $"/shop.html";
            }
            return Json(new { status = "success", redirectUrl = url });
        }
        [Route("danhmuc/{Alias}", Name = ("ListProduct"))]
        public IActionResult List(string Alias, int page = 1)
        {
            try
            {
                var pageSize = 10;
                var danhmuc = _context.Categories.AsNoTracking().SingleOrDefault(x => x.Alias == Alias);

                var lsTinDangs = _context.Products
                    .AsNoTracking()
                    .Where(x => x.CatId == danhmuc.CatId)
                    .OrderByDescending(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(lsTinDangs, page, pageSize);
                var lsReview = _context.Reviews.AsNoTracking().Include(x => x.Customer).Include(x => x.Product).OrderByDescending(x => x.DateTime).ToList();
                ViewBag.Review = lsReview;
                ViewBag.CurrentPage = page;
                ViewBag.CurrentCat = danhmuc;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("/{Alias}-{id}.html", Name = ("ProductDetails"))]
        public IActionResult Details(int id)
        {
            try
            {
                var product = _context.Products.Include(x => x.Cat).FirstOrDefault(x => x.ProductId == id);
                if (product == null)
                {
                    return RedirectToAction("Index");
                }
                var lsReview = _context.Reviews.AsNoTracking().Include(x => x.Customer).Include(x => x.Product).Where(x => x.ProductId == id).OrderByDescending(x => x.DateTime).ToList();
                var lsProduct = _context.Products
                    .AsNoTracking()
                    .Where(x => x.CatId == product.CatId && x.ProductId != id && x.Active == true)
                    .Take(4)
                    .OrderByDescending(x => x.DateCreated)
                    .ToList();
                ViewBag.SanPham = lsProduct;
                ViewBag.Review = lsReview;
                return View(product);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }


        }
    }
}
