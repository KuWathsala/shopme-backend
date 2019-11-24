using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using webapi.Services;
using webapi.ViewModels;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : Controller
    {
        private IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            
            _orderService = orderService;
        }

        [HttpGet]
        [Route("GetOrderDetailsById/{id?}")]
        public IActionResult GetOrderDetailsById(int id)
        {
            var result = _orderService.GetOrderDetailsById(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("getAllOrderDetailsByCustomer/{id?}")]
        public IActionResult GetAllOrderDetailsByCustomer(int id) //customerId
        {
            var result = _orderService.GetAllOrderDetailsByCustomer(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("getWaitingOrderDetailsBySeller/{id?}")]
        public IActionResult GetWaitingOrderDetailsBySeller(int id) 
        {
            var result = _orderService.GetWaitingOrderDetailsBySeller(id);
            return Ok(result);
        }
        
        [HttpPost]
        [Route("getAllOrderDetailsByDeliverer/{id?}")]
        public IActionResult getAllOrderDetailsByDeliverer(int id) 
        {
            var result = _orderService.GetAllOrderDetailsByDeliverer(id);
            return Ok(result);
        }
        
        [HttpGet]
        [Route("updateOrderStatus/{id},{status}")]
        public IActionResult UpdateOrderStatus(int id, string status)
        {
            _orderService.UpdateOrderStatus(id, status);
            return Ok();
        }

        [HttpGet]
        [Route("orderCongirmed/{orderId},{delivererId}")]
        public IActionResult UpdateOrderDeliverer(int orderId, int delivererId)
        {
            _orderService.UpdateOrderDeliverer(orderId, delivererId);
            return Ok();
        }

        [HttpPost]
        [Route("rate/{id},{sellerRate},{delivererRate}")]
        public IActionResult RateShop(int id, double sellerRate, double delivererRate)
        {
            _orderService.Rate(id, sellerRate, delivererRate);
            return Ok();
        }

        [HttpPost]
        [Route("createNewOrder")]
        public IActionResult CreateNewOrder(OrderVM orderVM) //orderVM
        {
            return Ok(_orderService.CreateNewOrder(orderVM));
        }

        [HttpPost]
        [Route("deleteOrder/{id?}")]
        public IActionResult DeleteOrder(int id) 
        {
            return Ok(_orderService.DeleteOrder(id));
        }

        [HttpPost]
        [Route("update-payment")]
        public IActionResult UpdatePayment()//string order_id, string status_code
        {
            var s = new StreamReader(Request.Body).ReadToEnd();
            //return Ok(SendEmail("wathdanthasinghe@gmail.com", s.ReadToEnd()));
            return Ok(_orderService.UpdatePayment(Int32.Parse(s.Substring(29, 3)), Int32.Parse(s.Substring(112, 1))));
            //Task<string> x;
            //using (StreamReader reader = new StreamReader(Request.Query, Encoding.UTF8))
            //{
            // x= reader.ReadToEndAsync();
            //}
            //var s = new StreamReader(Request.Body);

            //PaymentVM payment = JsonConvert.DeserializeObject<PaymentVM>(s.ReadToEnd());
            //return Ok(SendEmail("wathdanthasinghe@gmail.com",));// payment.order_id  

            //return Ok(_orderService.UpdatePayment(payment.order_id, payment.status_code));
            //return Ok(order_id) ;
            //return Ok();
        }



        public bool SendEmail(string email, string s)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    MailMessage mailMessage = new MailMessage("174025d@uom.lk", email);

                    mailMessage.Body = "you have new order which order id " + s + " by at ";
                    mailMessage.Subject = "shopMe. new order available";

                    SmtpClient smtp = new SmtpClient("submit.uom.lk", 587);
                    smtp.UseDefaultCredentials = false;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    NetworkCredential networkCredential = new NetworkCredential("174025d@uom.lk", "KuWathsala@97");//mhnedsmhjtmhrt
                    smtp.Credentials = networkCredential;
                    smtp.Send(mailMessage);
                    scope.Complete();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(new Exception(ex.Message));
                return false;
            }
        }
    }
}
