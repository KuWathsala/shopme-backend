using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using webapi.Entities;

namespace webapi.Entities
{
    public class Login
    {
        public int Id { get; set; }
        public string Role  { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }
        public virtual  Customer Customer { get; set; }
        public virtual Seller Seller { get; set; }
        public virtual Deliverer Deliverer { get; set; }
        public virtual Admin Admin { get; set; }
        //[Timestamp]
        //public byte[] RowVersion { get; set; }
    }
}
