using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using FinalProject.Models;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ExportDataController : Controller
    {
        private readonly FinalProjectContext _context;
        private IHostingEnvironment _IHosting;
        public ExportDataController(FinalProjectContext context, IHostingEnvironment iHosting)
        {
            _context = context;
            _IHosting = iHosting;
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
        [Route("DownLoadZip.html", Name = "DownLoadZip")]
        public FileResult DownLoadZip()
        {
            var webRoot = _IHosting.WebRootPath;
            var fileName = "MyZip.zip";
            var tempOutput = webRoot + "/images/" + fileName;

            using (ZipOutputStream IzipOutputStream = new ZipOutputStream(System.IO.File.Create(tempOutput)))
            {
                IzipOutputStream.SetLevel(9);
                byte[] buffer = new byte[4096];
                var imageList = new List<string>();
                var ideasURL = _context.Products.Where(x => x.Thumb != null).ToList();
                foreach (var item in ideasURL)
                {
                    imageList.Add(webRoot + $"/images/products/{item.Thumb}");
                }

                for (int i = 0; i < imageList.Count; i++)
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(imageList[i]));
                    entry.DateTime = DateTime.Now;
                    entry.IsUnicodeText = true;
                    IzipOutputStream.PutNextEntry(entry);

                    using (FileStream oFileStream = System.IO.File.OpenRead(imageList[i]))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = oFileStream.Read(buffer, 0, buffer.Length);
                            IzipOutputStream.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }
                IzipOutputStream.Finish();
                IzipOutputStream.Flush();
                IzipOutputStream.Close();
            }

            byte[] finalResult = System.IO.File.ReadAllBytes(tempOutput);
            if (System.IO.File.Exists(tempOutput))
            {
                System.IO.File.Delete(tempOutput);
            }
            if (finalResult == null || !finalResult.Any())
            {
                throw new Exception(String.Format("Nothing found"));

            }

            return File(finalResult, "application/zip", fileName);
        }
    }
}
