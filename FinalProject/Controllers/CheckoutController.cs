using AspNetCoreHero.ToastNotification.Abstractions;
using BraintreeHttp;
using FinalProject.Email;
using FinalProject.Extension;
using FinalProject.Helpper;
using FinalProject.Models;
using FinalProject.ModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayPal.Core;
using PayPal.v1.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinalProject.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly FinalProjectContext _context;
        private readonly ISendMailService _emailSender;
        public INotyfService _notyfService { get; }
        private readonly string _clientId;
        private readonly string _secretKey;

        public double TyGiaUSD = 23300;
        public CheckoutController(FinalProjectContext context, INotyfService notyfService, IConfiguration config, ISendMailService emailSender)
        {
            _context = context;
            _notyfService = notyfService;
            _clientId = config["PaypalSettings:ClientId"];
            _secretKey = config["PaypalSettings:Secretkey"];
            _emailSender = emailSender;
        }
        public List<CartItem> GioHang
        {
            get
            {
                var gh = HttpContext.Session.Get<List<CartItem>>("GioHang");
                if (gh == default(List<CartItem>))
                {
                    gh = new List<CartItem>();
                }
                return gh;
            }
        }
        [Route("checkout.html", Name = "Checkout")]
        public IActionResult Index(string returnUrl = null)
        {
            //Lay gio hang ra de xu ly
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            MuaHangVM model = new MuaHangVM();
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.CustomerId;
                model.FullName = khachhang.FullName;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Phone;
                model.Address = khachhang.Address;
            }
            ViewData["lsTinhThanh"] = new SelectList(_context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(), "Location", "Name");
            ViewBag.GioHang = cart;
            return View(model);
        }

        [HttpPost]
        [Route("checkout.html", Name = "Checkout")]
        public IActionResult Index(MuaHangVM muaHang)
        {
            //Lay ra gio hang de xu ly
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            var emailCustomer = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
            if (taikhoanID == null)
            {
                return RedirectToAction("Login","Accounts", new { returnUrl = "/checkout.html" });
            }
            MuaHangVM model = new MuaHangVM();
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.CustomerId;
                model.FullName = khachhang.FullName;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Phone;
                model.Address = khachhang.Address;
                khachhang.Address = muaHang.Address;
                _context.Update(khachhang);
                _context.SaveChanges();
            }
            try
            {
                if (ModelState.IsValid)
                {
                    //Khoi tao don hang
                    var donhang = new Models.Order();
                    donhang.CustomerId = model.CustomerId;
                    donhang.Address = model.Address;

                    donhang.OrderDate = DateTime.Now;
                    donhang.TransactStatusId = 1;//Don hang moi
                    donhang.Deleted = false;
                    donhang.Paid = false;
                    donhang.Note = Utilities.StripHTML(model.Note);
                    donhang.Totalmoney = Convert.ToInt32(cart.Sum(x => x.TotalMoney));
                    _context.Add(donhang);
                    _context.SaveChanges();
                    //tao danh sach don hang

                    foreach (var item in cart)
                    {
                        OrderDetail orderDetail = new OrderDetail();
                        orderDetail.OrderId = donhang.OrderId;
                        orderDetail.ProductId = item.product.ProductId;
                        orderDetail.Quantity = item.amount;
                        orderDetail.Total = donhang.Totalmoney;
                        orderDetail.Price = item.product.Price;
                        orderDetail.CreateDate = DateTime.Now;
                        _context.Add(orderDetail);
                    }

                    _context.SaveChanges();
                    //clear gio hang
                    HttpContext.Session.Remove("GioHang");
                    //Xuat thong bao
                    _notyfService.Success("Đơn hàng đặt thành công");
                    _emailSender.SendEmailAsync(emailCustomer.Email, "Notification!", $"<main class=\"main-content\">\r\n           <div class=\"container h-100\">\r\n            <div class=\"row h-100\">\r\n                <div class=\"col-lg-12\">\r\n                    <div class=\"breadcrumb-item\">\r\n                        <h2 class=\"breadcrumb-heading\">THÔNG TIN MUA HÀNG</h2>\r\n                                                             </div>\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </div>\r\n    <div class=\"checkout-area section-space-y-axis-100\">\r\n        <div class=\"container\">\r\n            <form>\r\n                <div class=\"row\">\r\n                    <div class=\"col-lg-6 col-12\">\r\n\r\n                        <div class=\"checkbox-form\">\r\n                            <h3>Đặt hàng thành công</h3>\r\n                            <p>Mã đơn hàng: #{donhang.OrderId}</p>\r\n                            <p>Cảm ơn bạn đã đặt hàng</p>\r\n                            <br />\r\n                            <h3>THÔNG TIN ĐƠN HÀNG</h3>\r\n                            <p>Thông tin giao hàng</p>\r\n                            <p>Người nhận hàng: {donhang.Customer.FullName}</p>\r\n                            <p>Số điện thoại: {donhang.Customer.Phone}</p>\r\n                            <p>Địa chỉ: {donhang.Address}</p>\r\n                            <br />\r\n                            Để xem chi tiết đơn hàng vui lòng truy cập vào <a asp-controller=\"Accounts\" asp-action=\"Dashboard\"><strong>Tài khoản cá nhân.</strong></a> Cần hỗ trợ? Liên hệ với chúng tôi qua số điện thoại 0123456789\r\n                        </div>\r\n                    </div>\r\n                    <div class=\"col-lg-6 col-12\">\r\n                                          </div>\r\n                </div>\r\n            </form>\r\n\r\n        </div>\r\n    </div>\r\n</main>");
                    _emailSender.SendEmailAsync("namlgcd191254@fpt.edu.vn", "Notification!", $"You have a new order!");
                    //cap nhat thong tin khach hang
                    return RedirectToAction("Success");
                }
            }
            catch
            {
                ViewData["lsTinhThanh"] = new SelectList(_context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(), "Location", "Name");
                ViewBag.GioHang = cart;
                return View(model);
            }
            ViewData["lsTinhThanh"] = new SelectList(_context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(), "Location", "Name");
            ViewBag.GioHang = cart;
            return View(model);
        }
        [Route("paypal-checkout.html", Name = "Paypal")]
        [Authorize]
        public async System.Threading.Tasks.Task<IActionResult> PaypalCheckout()
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(taikhoanID))
            {
                return RedirectToAction("Login", "Accounts", new { returnUrl = "/paypal-checkout.html" });
            }
            var environment = new SandboxEnvironment(_clientId, _secretKey);
            var client = new PayPalHttpClient(environment);
            #region Create Paypal Order
            var itemList = new ItemList()
            {
                Items = new List<Item>()
            };
            var total = Math.Round(GioHang.Sum(p => p.TotalMoney) / TyGiaUSD, 2);
            foreach (var item in GioHang)
            {
                itemList.Items.Add(new Item()
                {
                    Name = item.product.ProductName,
                    Currency = "USD",
                    Price = Math.Round(Convert.ToDouble(item.product.Price) / TyGiaUSD, 2).ToString(),
                    Quantity = item.amount.ToString(),
                    Sku = "sku",
                    Tax = "0"
                });
            }
            #endregion
            var paypalOrderId = DateTime.Now.Ticks;
            var hostname = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            var payment = new Payment()
            {
                Intent = "sale",
                Transactions = new List<Transaction>()
                {
                    new Transaction()
                    {
                        Amount = new Amount()
                        {
                            Total = total.ToString(),
                            Currency = "USD",
                            Details = new AmountDetails
                            {
                                Tax = "0",
                                Shipping = "0",
                                Subtotal = total.ToString()
                            }
                        },
                        Custom = "lenam-seller@gmail.com",
                        ItemList = itemList,
                        Description = $"Invoice #{paypalOrderId}",
                        InvoiceNumber = paypalOrderId.ToString()
                    }
                },
                RedirectUrls = new RedirectUrls()
                {
                    CancelUrl = $"{hostname}/Checkout/CheckoutFail",
                    ReturnUrl = $"{hostname}/Checkout/CheckoutSuccess"
                },
                Payer = new Payer()
                {
                    PaymentMethod = "paypal"
                }
            };
            PaymentCreateRequest request = new PaymentCreateRequest();
            request.RequestBody(payment);

            try
            {
                var response = await client.Execute(request);
                var statusCode = response.StatusCode;
                Payment result = response.Result<Payment>();

                var links = result.Links.GetEnumerator();
                string paypalRedirectUrl = null;
                while (links.MoveNext())
                {
                    LinkDescriptionObject lnk = links.Current;
                    if (lnk.Rel.ToLower().Trim().Equals("approval_url"))
                    {
                        //saving the payapalredirect URL to which user will be redirected for payment  
                        paypalRedirectUrl = lnk.Href;
                    }
                }

                return Redirect(paypalRedirectUrl);
            }
            catch (HttpException httpException)
            {
                var statusCode = httpException.StatusCode;
                var debugId = httpException.Headers.GetValues("PayPal-Debug-Id").FirstOrDefault();

                //Process when Checkout with Paypal fails
                return Redirect("/Checkout/CheckoutFail");
            }
        }
        public IActionResult CheckoutFail()
        {
            _notyfService.Success("Đặt hàng thất bại");
            return RedirectToAction("Index");
        }

        public IActionResult CheckoutSuccess()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            MuaHangVM model = new MuaHangVM();
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                model.CustomerId = khachhang.CustomerId;
                model.FullName = khachhang.FullName;
                model.Email = khachhang.Email;
                model.Phone = khachhang.Phone;
                model.Address = khachhang.Address;
                _context.Update(khachhang);
                _context.SaveChanges();
            }
            try
            {
                if (ModelState.IsValid)
                {
                    //Khoi tao don hang
                    var donhang = new Models.Order();
                    donhang.CustomerId = model.CustomerId;
                    donhang.Address = model.Address;
                    donhang.OrderDate = DateTime.Now;
                    donhang.TransactStatusId = 2;
                    donhang.Deleted = false;
                    donhang.Paid = false;
                    donhang.Note = Utilities.StripHTML(model.Note);
                    donhang.Totalmoney = Convert.ToInt32(cart.Sum(x => x.TotalMoney));
                    _context.Add(donhang);
                    _context.SaveChanges();
                    //tao danh sach don hang

                    foreach (var item in cart)
                    {
                        OrderDetail orderDetail = new OrderDetail();
                        orderDetail.OrderId = donhang.OrderId;
                        orderDetail.ProductId = item.product.ProductId;
                        orderDetail.Quantity = item.amount;
                        orderDetail.Total = donhang.Totalmoney;
                        orderDetail.Price = item.product.Price;
                        orderDetail.CreateDate = DateTime.Now;
                        _context.Add(orderDetail);
                    }
                    var emailCustomer = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));

                    _context.SaveChanges();
                    //clear gio hang
                    HttpContext.Session.Remove("GioHang");
                    //Xuat thong bao
                    _notyfService.Success("Đơn hàng đặt thành công");
                    _emailSender.SendEmailAsync(emailCustomer.Email, "Notification!", $"<main class=\"main-content\">\r\n           <div class=\"container h-100\">\r\n            <div class=\"row h-100\">\r\n                <div class=\"col-lg-12\">\r\n                    <div class=\"breadcrumb-item\">\r\n                        <h2 class=\"breadcrumb-heading\">THÔNG TIN MUA HÀNG</h2>\r\n                                                             </div>\r\n                </div>\r\n            </div>\r\n        </div>\r\n    </div>\r\n    <div class=\"checkout-area section-space-y-axis-100\">\r\n        <div class=\"container\">\r\n            <form>\r\n                <div class=\"row\">\r\n                    <div class=\"col-lg-6 col-12\">\r\n\r\n                        <div class=\"checkbox-form\">\r\n                            <h3>Đặt hàng thành công</h3>\r\n                            <p>Mã đơn hàng: #{donhang.OrderId}</p>\r\n                            <p>Cảm ơn bạn đã đặt hàng</p>\r\n                            <br />\r\n                            <h3>THÔNG TIN ĐƠN HÀNG</h3>\r\n                            <p>Thông tin giao hàng</p>\r\n                            <p>Người nhận hàng: {donhang.Customer.FullName}</p>\r\n                            <p>Số điện thoại: {donhang.Customer.Phone}</p>\r\n                            <p>Địa chỉ: {donhang.Address}</p>\r\n                            <br />\r\n                            Để xem chi tiết đơn hàng vui lòng truy cập vào <a asp-controller=\"Accounts\" asp-action=\"Dashboard\"><strong>Tài khoản cá nhân.</strong></a> Cần hỗ trợ? Liên hệ với chúng tôi qua số điện thoại 0123456789\r\n                        </div>\r\n                    </div>\r\n                    <div class=\"col-lg-6 col-12\">\r\n                                          </div>\r\n                </div>\r\n            </form>\r\n\r\n        </div>\r\n    </div>\r\n</main>");
                    _emailSender.SendEmailAsync("namlgcd191254@fpt.edu.vn", "Notification!", $"You have a new order!");
                    //cap nhat thong tin khach hang
                }
                return RedirectToAction("Success");
            }
            catch
            {
                ViewData["lsTinhThanh"] = new SelectList(_context.Locations.Where(x => x.Levels == 1).OrderBy(x => x.Type).ToList(), "Location", "Name");
                ViewBag.GioHang = cart;
                return RedirectToAction("Index");
            }
        }
        [Route("dat-hang-thanh-cong.html", Name = "Success")]
        public IActionResult Success()
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(taikhoanID))
                {
                    return RedirectToAction("Login", "Accounts", new { returnUrl = "/dat-hang-thanh-cong.html" });
                }
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                var donhang = _context.Orders
                    .Where(x => x.CustomerId == Convert.ToInt32(taikhoanID))
                    .OrderByDescending(x => x.OrderDate)
                    .FirstOrDefault();
                MuaHangSuccessVM successVM = new MuaHangSuccessVM();
                successVM.FullName = khachhang.FullName;
                successVM.DonHangID = donhang.OrderId;
                successVM.Phone = khachhang.Phone;
                successVM.Address = khachhang.Address;
                return View(successVM);
            }
            catch
            {
                return View();
            }
        }

        public string GetNameLocation(int idlocation)
        {
            try
            {
                var location = _context.Locations.AsNoTracking().SingleOrDefault(x => x.LocationId == idlocation);
                if (location != null)
                {
                    return location.NameWithType;
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }
    }
}