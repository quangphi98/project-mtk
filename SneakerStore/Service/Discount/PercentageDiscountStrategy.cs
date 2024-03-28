using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SneakerStore.Service.Discount
{
    public class PercentageDiscountStrategy : IDiscountStrategy
    {
        private readonly int _percent;

        public PercentageDiscountStrategy(int percent)
        {
            _percent = percent;
        }

        public decimal ApplyDiscount(decimal totalPrice)
        {
            return totalPrice - (totalPrice * _percent / 100);
        }
    }
}
