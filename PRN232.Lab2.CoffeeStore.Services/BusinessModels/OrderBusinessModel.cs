using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Services.BusinessModels
{
    public class OrderBusinessModel
    {
        public int OrderId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? PaymentId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderDetailBusinessModel> OrderDetails { get; set; } = new();
    }
}
