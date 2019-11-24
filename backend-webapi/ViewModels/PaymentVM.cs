using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.ViewModels
{
    public class PaymentVM
    {
        public int Id { get; set; }
        public int status_code { get; set; }
        public string order_id { get; set; }
        public int payment_id { get; set; }
        public string payhere_amount { get; set; }
        public string payhere_currency { get; set; }
    }
}
