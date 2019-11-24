using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webapi.Entities;
using webapi.ViewModels;
using webapi.Services;
using webapi.Dtos;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    public class UserAuthController : Controller
    {
        private IUserService _userService;

        public UserAuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Route("signin")]
        public IActionResult SignIn([FromBody]LoginVM login)
        {
            var user = _userService.SignIn(login.Email, login.Password);

            if (user == null)
                return BadRequest(new { message = "your email or password is incorrect" });
        
            return Ok(user);
        }

        [HttpPost]
        [Route("SignUp-Customer")]
        public IActionResult SignUp([FromBody]CustomerVM customerVM)
        {
            var customer = _userService.SignUp(customerVM);

            if (customer == null)
                return BadRequest(new { message = "your entered password already exist" });

            return Ok(customer);
        }

        [HttpPost]
        [Route("SignUp-Seller")]
        public IActionResult SignUp([FromBody]SellerVM sellerVM)
        {
            var seller = _userService.SignUp(sellerVM);

            if (seller == null)
                return BadRequest(new { message = "your entered password already exist" });

            return Ok(seller);
        }

        [HttpPost]
        [Route("SignUp-Deliverer")]
        public IActionResult SignUp([FromBody]DelivererVM delivererVM)
        {
            var deliverer = _userService.SignUp(delivererVM);

            if (deliverer == null)
                return BadRequest(new { message = "your entered password already exist" });

            return Ok(deliverer);
        }

        [HttpPost]
        [Route("Update-Customer")]
        public IActionResult UpdateCustomer([FromBody]CustomerVM customerVM)
        {
            var customer = _userService.Update(customerVM);
            return Ok(customer);
        }

        [HttpPost]
        [Route("Update-Seller")]
        public IActionResult UpdateSeller([FromBody]SellerVM sellerVM)
        {
            var seller = _userService.Update(sellerVM);
            return Ok(seller);
        }

        [HttpPost]
        [Route("Update-Deliverer")]
        public IActionResult UpdateDeliverer([FromBody]DelivererVM delivererVM)
        {
            var deliverer = _userService.Update(delivererVM);
            return Ok(deliverer);
        }

        [HttpPost]
        [Route("forgetPassword/{email?}")]
        public IActionResult FogetPassword(string email)
        {
            var result = _userService.SendEmail(email);
            return Ok(result);
        }
        /*
        [HttpPost]
        [Route("resetPassword")]
        public IActionResult ResetPassword(LoginVM loginVM)
        {
            var result = _userService.ResetPassword(loginVM.Email, loginVM.Password);
            return Ok(result);
        }
        */
        [HttpPost]
        [Route("resetPassword")]
        public IActionResult ResetPassword([FromBody]LoginVM login)
        {
            var user = _userService.ResetPassword(login.Email, login.Password);
            
            return Ok(user);
        }

        [HttpPost]
        [Route("verify/{code?}")]
        public IActionResult Verify(string code)
        {
            var result = _userService.Veryfy(code);
            return Ok(result);
        }

        [HttpPost]
        [Route("email/{email?}")]
        public IActionResult SendEmail(string email)
        {
            var result = _userService.SendEmail(email);
            return Ok(result);
        }

        [HttpPost]
        [Route("test")]
        public IActionResult Test(string email)
        {
            var result = _userService.Email(email);
            return Ok(result);
        }
    }
}