using PayPal.Api;
using SneakerStore.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;


namespace SneakerStore.Payment
{
	public interface IChoice
	{
		void Choice(HttpContextBase context, string DiaDiemGiaoHang, List<ViewModel> cart);
	}

	internal class PayPalChoice : IChoice
	{
		private DBSneakerStoreEntities database = new DBSneakerStoreEntities();
		private Func<double> _tinhTongTien;

		public PayPalChoice(Func<double> tinhTongTien)
		{
			_tinhTongTien = tinhTongTien;
		}

		public void Choice(HttpContextBase context, string DiaDiemGiaoHang, List<ViewModel> cart)
		{
			//getting the apiContext
			APIContext apiContext = PaypalConfiguration.GetAPIContext();
			try
			{
				//A resource representing a Payer that funds a payment Payment Method as paypal
				//Payer Id will be returned when payment proceeds or click to pay
				string payerId = context.Request.Params["PayerID"];
				if (string.IsNullOrEmpty(payerId))
				{
					//this section will be executed first because PayerID doesn't exist
					//it is returned by the create function call of the payment class
					// Creating a payment
					// baseURL is the url on which paypal sendsback the data.
					string baseURI = context.Request.Url.Scheme + "://" + context.Request.Url.Authority + "/ShoppingCart/PaymentWithPayPal?";
					//here we are generating guid for storing the paymentID received in session
					//which will be used in the payment execution
					var guid = Convert.ToString((new Random()).Next(100000));
					//CreatePayment function gives us the payment approval url
					//on which payer is redirected for paypal account payment
					var createdPayment = CreatePayment(apiContext, baseURI + "guid=" + guid);
					//get links returned from paypal in response to Create function call
					var links = createdPayment.links.GetEnumerator();
					string paypalRedirectUrl = null;
					while (links.MoveNext())
					{
						Links lnk = links.Current;
						if (lnk.rel.ToLower().Trim().Equals("approval_url"))
						{
							//saving the payapalredirect URL to which user will be redirected for payment
							paypalRedirectUrl = lnk.href;
						}
					}
					// saving the paymentID in the key guid
					context.Session.Add(guid, createdPayment.id);
					context.Response.Redirect(paypalRedirectUrl, true);
				}
				else
				{
					// This function exectues after receving all parameters for the payment
					var guid = context.Request.Params["guid"];
					var executedPayment = ExecutePayment(apiContext, payerId, context.Session[guid] as string);
					//If executed payment failed then we will show payment failure message to user
					if (executedPayment.state.ToLower() != "approved")
					{
						return;
					}
					else
					{
						string user = (string)context.Session["NameUser"];
						Customer getUser = database.Customers.FirstOrDefault(u => u.UserName == user);
						//Help me save info order
						cart = context.Session["GioHang"] as List<ViewModel>;

						decimal totalM = 0;

						foreach (var i in cart)
						{
							totalM += (decimal)i.Sum_Quantity * (decimal)i.PricePro;
						}
						context.Session["total"] = totalM;

						var DonHangTemp = new OrderPro
						{
							IDCus = getUser.IDCus,
							AddressDeliverry = DiaDiemGiaoHang,
							DateOrder = DateTime.Now,
						};

						database.OrderProes.Add(DonHangTemp);
						database.SaveChanges();

						foreach (var i in cart)
						{
							i.Total_Money = (decimal)i.Sum_Quantity * (decimal)i.PricePro;
							var CTDH = new OrderDetail
							{
								IDOrder = DonHangTemp.ID,
								IDProduct = i.IDPro,
								Quantity = i.Sum_Quantity,
								UnitPrice = (double)i.PricePro
							};

							database.OrderDetails.Add(CTDH);
							database.SaveChanges();
						}
						//Thanh toan thanh cong

						if (getUser != null)
						{
							var getcart = database.OrderProes.Where(x => x.IDCus == getUser.IDCus).ToList();
							database.OrderProes.RemoveRange(getcart);
							database.SaveChanges();
						}

						cart.Clear();
						context.Session["GioHang"] = cart;
					}
				}
			}
			catch (Exception ex)
			{
				context.Response.Write("<h1>Error occured: </h1>" + ex.Message);
			}
		}

		private PayPal.Api.Payment payment;

		private PayPal.Api.Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
		{
			var paymentExecution = new PaymentExecution()
			{
				payer_id = payerId
			};
			this.payment = new PayPal.Api.Payment()
			{
				id = paymentId
			};
			return this.payment.Execute(apiContext, paymentExecution);
		}

		private PayPal.Api.Payment CreatePayment(APIContext apiContext, string redirectUrl)
		{
			//create itemlist and add item objects to it
			var itemList = new ItemList()
			{
				items = new List<Item>()
			};
			//Adding Item Details like name, currency, price etc
			itemList.items.Add(new Item()
			{
				name = "Item Name comes here",
				currency = "USD",
				price = "1",
				quantity = "1",
				sku = "sku"
			});
			var payer = new Payer()
			{
				payment_method = "paypal"
			};
			// Configure Redirect Urls here with RedirectUrls object
			var redirUrls = new RedirectUrls()
			{
				cancel_url = redirectUrl + "&Cancel=true",
				return_url = redirectUrl
			};
			// Adding Tax, shipping and Subtotal details
			var details = new Details()
			{
				tax = "0.00",
				shipping = "0.00",
				subtotal = "1"
			};
			//Final amount with details
			var amount = new Amount()
			{
				currency = "USD",
				total = "1", // Total must be equal to sum of tax, shipping and subtotal.
				details = details
			};
			var transactionList = new List<Transaction>();
			// Adding description about the transaction
			var paypalOrderId = DateTime.Now.Ticks;
			transactionList.Add(new Transaction()
			{
				description = $"Invoice #{paypalOrderId}",
				invoice_number = paypalOrderId.ToString(), //Generate an Invoice No
				amount = amount,
				item_list = itemList
			});
			this.payment = new PayPal.Api.Payment()
			{
				intent = "sale",
				payer = payer,
				transactions = transactionList,
				redirect_urls = redirUrls
			};
			// Create a payment using a APIContext
			return payment.Create(apiContext);
		}
	}

	internal class CODChoice : IChoice
	{
		private DBSneakerStoreEntities db = new DBSneakerStoreEntities();
		private readonly Func<double> _tinhTongTien;

		public CODChoice(Func<double> tinhTongTien)
		{
			_tinhTongTien = tinhTongTien;
		}

		public void Choice(HttpContextBase context, string DiaDiemGiaoHang, List<ViewModel> cart)
		{
			string getUser = (string)context.Session["UserName"];
			Customer user = db.Customers.FirstOrDefault(u => u.UserName == getUser);

			// Handle the case where the user is not found in the session
			if (getUser == null)
			{
				// Redirect to the login page or handle appropriately
				return;
			}

			// Retrieve the cart from the session
			cart = context.Session["GioHang"] as List<ViewModel>;

			// Handle the case where the cart is empty or not found in the session
			if (cart == null || !cart.Any())
			{
				// Redirect to the home page or handle appropriately
				return;
			}

			// Calculate the total amount of the order
			decimal totalM = cart.Sum(item => (decimal)item.Sum_Quantity * (decimal)item.PricePro);
			context.Session["total"] = totalM;

			// Create a new order
			var order = new OrderPro
			{
				IDCus = user.IDCus,
				AddressDeliverry = DiaDiemGiaoHang,
				DateOrder = DateTime.Now,
			};

			// Add the order to the database
			db.OrderProes.Add(order);
			db.SaveChanges();

			// Add order details for each item in the cart
			foreach (var item in cart)
			{
				decimal totalTemp = (decimal)item.Sum_Quantity * (decimal)item.PricePro;
				var orderDetail = new OrderDetail
				{
					IDProduct = item.IDPro,
					Quantity = item.Sum_Quantity,
					UnitPrice = (double)item.PricePro,
					IDOrder = order.ID // Link the order detail to the order
				};

				// Add the order detail to the database
				db.OrderDetails.Add(orderDetail);
			}

			// Save all changes to the database
			db.SaveChanges();

			// Clear the cart and update session variables
			cart.Clear();
			context.Session["GioHang"] = cart;
			context.Session["totalCart"] = 0;
		}
	}
}