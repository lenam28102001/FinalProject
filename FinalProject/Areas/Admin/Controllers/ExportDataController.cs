using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using FinalProject.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ExportDataController : Controller
    {
        private readonly FinalProjectContext _context;
        public ExportDataController(FinalProjectContext context)
        {
            _context = context;
        }
        public IActionResult ExportAllData ()
        {
            var lsOrder = new List<Order>();
            lsOrder = _context.Orders.Include(x=>x.TransactStatus).Include(x=>x.Customer).AsNoTracking().OrderByDescending(x => x.OrderDate).ToList();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("OrderID,Tên khách hàng,Địa chỉ giao hàng,Ngày đặt hàng,Tổng tiền,Trạng thái");
            foreach (var item in lsOrder)
            {
                sb.AppendLine($"{item.OrderId},{item.Customer.FullName},{item.Address},{item.OrderDate},{item.Totalmoney},{item.TransactStatus.Status}");
            }
            var data = Encoding.UTF8.GetBytes(sb.ToString());
            var result = Encoding.UTF8.GetPreamble().Concat(data).ToArray();
            return File(result, "text/csv", "Data.csv");
        }
    }
}
