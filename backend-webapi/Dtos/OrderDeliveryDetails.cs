﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.Dtos
{
    public class OrderDeliveryDetails
    {
        public CustomerDto Customer;
        public SellerDto Seller;
        public OrderDto Order;
    }
}
