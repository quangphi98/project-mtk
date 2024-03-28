using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace SneakerStore.Models
{
    public class ViewModel
    {
        private DBSneakerStoreEntities db = new DBSneakerStoreEntities();
        public string NamePro { get; set; }

        public string ImgPro { get; set; }

        public decimal PricePro { get; set; }

        public string NameCate { get; set; }

        public string DesPro { get; set; }
        [System.ComponentModel.DataAnnotations.Key]

        public int IDPro { get; set; }

        public decimal Total_Money { get; set; }

        public Product product { get; set; }


        public Category category { get; set; }

        public OrderDetail orderDetail { get; set; }

        public IEnumerable<Product> ListProduct { get; set; }

        public int? Top5_Quantity { get; set; }

        public int? Sum_Quantity { get; set; }

		public double Total()
		{
			return (double)PricePro * (double)Sum_Quantity;
		}
		public ViewModel(int IDPro)
        {
            var getSP = db.Products.FirstOrDefault(x => x.ProductID == IDPro);
            this.IDPro = IDPro;
            this.NamePro = getSP.NamePro.ToString();
            this.ImgPro = getSP.ImagePro.ToString();
            this.PricePro = decimal.Parse(getSP.Price.ToString());

		}
    }
}