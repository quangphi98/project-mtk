using SneakerStore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;
using System.Data;
using System.Data.Entity;
using SneakerStore.Service.Send_Email;
using System.Net.Mail;
using System.Net;

namespace SneakerStore.Controllers
{
    public class ProductController : Controller
    {
        DBSneakerStoreEntities database = new DBSneakerStoreEntities();
        private readonly IEmail _sendemail;

        public ProductController(Email email)
        {
            _sendemail = email;
        }

        public ActionResult SearchOption(int? page, double min = double.MinValue, double max = double.MaxValue)
        {
            int pageSize = 8;
            int pageNum = (page ?? 1);
            var list = database.Products.OrderByDescending(x => x.NamePro).Where(p => (double)p.Price >= min && (double)p.Price <= max).ToList();
            return View(list.ToPagedList(pageNum, pageSize));
        }

        // GET: Product
        public ActionResult Index(int? category, string SearchString, int? page)
        {
            int pageSize = 20;
            int pageNum = (page ?? 1);
            var productList = database.Products.Include(p => p.Category);
            string searchMessage = ""; // Tạo biến để lưu thông điệp tìm kiếm
            if (category == null)
            {
                productList = database.Products.OrderByDescending(x => x.NamePro);

            }
            else
            {
                productList = database.Products.OrderByDescending(x => x.NamePro).Where(p => p.CateID == category);

            }

            if (!String.IsNullOrEmpty(SearchString))
            {
                productList = productList.Where(s => s.NamePro.Contains(SearchString));
                searchMessage = "SẢN PHẨM BẠN TÌM: " + SearchString; // Thiết lập thông điệp tìm kiếm
            }
            ViewBag.SearchMessage = searchMessage;
            string successMessage = TempData["SuccessMessage"] as string;
            if (!string.IsNullOrEmpty(successMessage))
            {
                ViewBag.SuccessMessage = successMessage;
            }

            return View(productList.ToPagedList(pageNum, pageSize));

        }
        public ActionResult Create()
        {
            List<Category> list = database.Categories.ToList();
            ViewBag.listCategory = new SelectList(list, "IDCate", "NameCate", "");
            Product pro = new Product();
            return View(pro);
        }
        [HttpPost]
        public ActionResult Create(Product pro)
        {
            List<Category> list = database.Categories.ToList();
            try
            {
                if (pro.UploadImage != null)
                {
                    string filename = Path.GetFileNameWithoutExtension(pro.UploadImage.FileName);
                    string extent = Path.GetExtension(pro.UploadImage.FileName);
                    filename = filename + extent;
                    pro.ImagePro = "~/Content/images/" + filename;
                    pro.UploadImage.SaveAs(Path.Combine(Server.MapPath("~/Content/images/"), filename));
                }
                ViewBag.listCategory = new SelectList(list, "IDCate", "NameCate");
                database.Products.Add(pro);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        public ActionResult SelectCate()
        {
            Category se_cate = new Category();
            se_cate.ListCate = database.Categories.ToList<Category>();
            return PartialView(se_cate);
        }
        public ActionResult Search()
        {
            return View();
        }

        public ActionResult DetailPro(int id)
        {
            return View(database.Products.Where(s => s.ProductID == id).FirstOrDefault());
        }

        public ActionResult Pro(string _namePro)
        {
            if (_namePro == null)
            {
                return View(database.Products.ToList());
            }
            else
            {
                return View(database.Products.Where(s => s.NamePro.Contains(_namePro)).ToList());
            }
        }

        public ActionResult Details(int id)
        {
            return View(database.Products.Where(s => s.ProductID == id).FirstOrDefault());
        }
        public ActionResult Edit(int id)
        {
            return View(database.Products.Where(s => s.ProductID == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Edit(int id, Product pro)
        {
            database.Entry(pro).State = System.Data.Entity.EntityState.Modified;
            database.SaveChanges();
            return RedirectToAction("Pro");
        }
        public ActionResult Delete(int id)
        {
            return View(database.Products.Where(s => s.ProductID == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Delete(int id, Product pro)
        {
            try
            {
                pro = database.Products.Where(s => s.ProductID == id).FirstOrDefault();
                database.Products.Remove(pro);
                database.SaveChanges();
                return RedirectToAction("Pro");
            }
            catch
            {
                return Content("Dữ liệu đang được sử dụng ở nơi khác, Lỗi!");
            }
        }

        public ActionResult TopNew()
        {

            List<Product> proList = database.Products.ToList();
            var query = (from item in database.Products
                         orderby item.ProductID descending
                         select item);

            return View(query.Take(8));
        }

        [HttpPost]
        public ActionResult SendEmail(int productId)
        {
            var product = database.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null)
            {
                return HttpNotFound(); // Hoặc xử lý trường hợp sản phẩm không tồn tại
            }

            try
            {
                MailMessage message = _sendemail.CreateMailMessage();
                SmtpClient smtp = _sendemail.CreateSmtpClient();
                message.From = new MailAddress("laiquangphi@gmail.com"); // Địa chỉ email của bạn
                message.To.Add(new MailAddress("laiquangphi4576@gmail.com")); // Địa chỉ email của người nhận

                message.Subject = "Thông tin sản phẩm mới";
                message.Body = $"Sản phẩm mới đã được thêm: {product.NamePro}, Giá: {product.Price}";

                smtp.Port = 587; // Cổng SMTP của dịch vụ email của bạn
                smtp.Host = "smtp.gmail.com"; // Địa chỉ máy chủ SMTP của dịch vụ email của bạn
                smtp.EnableSsl = true; // Bật SSL nếu cần
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("laiquangphi@gmail.com", "laiquangphi21102003"); // Tài khoản email của bạn

                smtp.Send(message);
                ViewBag.Message = "Email đã được gửi thành công!";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Gửi email thất bại: {ex.Message}";
            }
            return View();
        }
}