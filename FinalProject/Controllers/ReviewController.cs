using AspNetCoreHero.ToastNotification.Abstractions;
using FinalProject.Email;
using FinalProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    public class ReviewController : Controller
    {
        private readonly FinalProjectContext _context;
        public INotyfService _notyfService { get; }
        private readonly ISendMailService _emailSender;
        public ReviewController(FinalProjectContext context, INotyfService notyfService, ISendMailService emailSender)
        {
            _context = context;
            _notyfService = notyfService;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview(int idProduct, string message, bool anonymoust,int rating,string alias)
        {
            var url = $"/{alias}-{idProduct}.html";
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID == null)
            {
                return Json(new { status = "success", redirectUrl = "/dang-nhap.html" });
            }
            var product = _context.Products.FirstOrDefault(x => x.ProductId == idProduct);
            var taikhoan = _context.Customers.SingleOrDefault(x => x.CustomerId == (Convert.ToInt32(taikhoanID)));
            var review = new Review();
            review.CustomerId = taikhoan.CustomerId;
            review.ProductId = idProduct;
            review.Message = message;
            review.Anonymoust = anonymoust;
            review.DateTime = DateTime.Now;
            review.Rating = rating;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            _notyfService.Success("Create success!");
            _emailSender.SendEmailAsync("namlgcd191254@fpt.edu.vn", "Notification!", $"Product {alias}-{idProduct} has a new review!");
            return Json(new { status = "success", redirectUrl = url });
        }
    }
}
