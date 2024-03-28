using PayPal.Api;
using SneakerStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SneakerStore.Payment
{
	public class Context
	{
		private IChoice _choice;

		public Context(IChoice choice)
		{
			_choice = choice;
		}

		public void SetChoice(IChoice choice)
		{
			_choice = choice;
		}

		public void Choice(HttpContextBase context, string DiaDiemGiaoHang, List<ViewModel> cart)
		{
			_choice.Choice(context, DiaDiemGiaoHang, cart);
		}
	}
}