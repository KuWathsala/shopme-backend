using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using webapi.Dtos;
using webapi.Entities;
using webapi.Helpers;
using webapi.Repositories;
using webapi.ViewModels;

namespace webapi.Services
{
    public class UserService : IUserService
    {
        private readonly AppSettings _jwtSettings;
        private readonly ICommonRepository<Customer> _customerRepository;
        private readonly ICommonRepository<Seller> _sellerRepository;
        private readonly ICommonRepository<Deliverer> _delivererRepository;
        private readonly ICommonRepository<Admin> _adminRepository;
        private readonly ICommonRepository<Login> _loginRepository;
        private readonly ICommonRepository<Location> _locationRepository;
        private string key = "1234567890-abcde";
        

        public UserService(IOptions<AppSettings> jwtSettings, ICommonRepository<Customer> customerRepository,
            ICommonRepository<Seller> sellerRepository, ICommonRepository<Deliverer> delivererRepository
            , ICommonRepository<Admin> adminRepository, ICommonRepository<Login> loginRepository, ICommonRepository<Location> locationRepository)
        {
            _jwtSettings = jwtSettings.Value;
            _customerRepository = customerRepository;
            _sellerRepository = sellerRepository;
            _delivererRepository = delivererRepository;
            _adminRepository = adminRepository;
            _loginRepository = loginRepository;
            _locationRepository = locationRepository;
        }

        public Object SignIn(string email, string password)
        {
            //retrive data from db
            var user = (dynamic)null;
            var login = _loginRepository.Get(x => x.Email == email).FirstOrDefault();//Decrypt(x.Password, key)

            if (login == null)
                return "your email does not exists";

            else if(password == Decrypt(login.Password, key))
            {
                if (login.Status != "verified")
                {
                    SendEmail(email);
                    return "not verified";
                }
                else if (login.Role == "Customer")
                {
                    var details = _customerRepository.Get(x => x.LoginId == login.Id).FirstOrDefault();
                    if (details != null)
                        user = new
                        {
                            Data = details,
                            Role = login.Role,
                        };
                }

                else if (login.Role == "Deliverer")
                {
                    var details = _delivererRepository.Get(x => x.LoginId == login.Id).FirstOrDefault();
                    if (details != null)
                        user = new
                        {
                            Data = details,
                            Role = login.Role
                        };
                }

                else if (login.Role == "Seller")
                {
                    var details = _sellerRepository.Get(x => x.LoginId == login.Id).FirstOrDefault();
                    if (details != null)
                        user = new
                        {
                            Data = details,
                            Role = login.Role
                        };
                }
                // authentication successful so generate jwt token
                user.Data.Token = Authentication(user.Data.Id, user.Data.Token);

                return user;
            }
            else return "your entered password incorect";
        }

        public string Authentication(int id, string token)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, id.ToString())
                }),
                //Expires = DateTime.UtcNow.AddDays(7),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var Token = tokenHandler.CreateToken(tokenDescriptor);
            token = tokenHandler.WriteToken(Token);
            return token;
        }

        public static string Encrypt(string password, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(password);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        public static string Decrypt(string password, string keyString)
        {
            var fullCipher = Convert.FromBase64String(password);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public object SignUp(CustomerVM customerVM)
        {
            var isAlreadyAvailable = _loginRepository.Get(x => x.Email == customerVM.LoginVM.Email).FirstOrDefault();
            if (isAlreadyAvailable != null)
                return null;

            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    var customerDto = new CustomerDto()
                    {
                        FirstName = customerVM.FirstName,
                        LastName = customerVM.LastName,
                        MobileNumber = customerVM.MobileNumber,
                        ProfileImage = customerVM.ProfileImage,
                    };

                    Login login = Mapper.Map<Login>(customerVM.LoginVM);
                    login.Password = Encrypt(login.Password, key);
                    //login.Status = "not verified";
                    _loginRepository.Add(login);
                    bool r = _loginRepository.Save();

                    if (r)
                    {
                        Login loginId = _loginRepository.Get(x => x.Email == login.Email).FirstOrDefault();
                        customerDto.LoginId = loginId.Id;

                        Customer toAdd = Mapper.Map<Customer>(customerDto);
                        _customerRepository.Add(toAdd);
                        var result = _customerRepository.Save();

                        customerDto.Token = Authentication(customerDto.Id, customerDto.Token);
                        //SendEmail(login.Email);

                        if (result)
                            scope.Complete();
                        return customerDto;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(new Exception(ex.Message));
                return ex.Message;
            }
            
        }

        public SellerDto SignUp(SellerVM sellerVM)
        {
            var isAlreadyAvailable = _loginRepository.Get(x => x.Email == sellerVM.LoginVM.Email).FirstOrDefault();
            if (isAlreadyAvailable != null)
                return null;
            try
            {
                SellerDto sellerDto = new SellerDto()
                {
                    FirstName = sellerVM.FirstName,
                    LastName = sellerVM.LastName,
                    MobileNumber = sellerVM.MobileNumber,
                    ProfileImage = sellerVM.ProfileImage,
                    AccountNo = sellerVM.AccountNo,
                    ShopAddress = sellerVM.ShopAddress,
                    ShopName = sellerVM.ShopName,
                    ShopLocationLatitude = sellerVM.ShopLocationLatitude,
                    ShopLocationLongitude = sellerVM.ShopLocationLongitude,
                    Image = sellerVM.Image,
                    Rating = 0.0
                };
                using (TransactionScope scope = new TransactionScope())
                {
                    Login login = Mapper.Map<Login>(sellerVM.LoginVM);
                    login.Password = Encrypt(login.Password, key);
                    _loginRepository.Add(login);
                    _loginRepository.Save();

                    Login loginId = _loginRepository.Get(x => x.Email == login.Email).FirstOrDefault();
                    sellerDto.LoginId = loginId.Id;

                    Seller toAdd = Mapper.Map<Seller>(sellerDto);
                    _sellerRepository.Add(toAdd);
                    _sellerRepository.Save();

                    sellerDto.Token = Authentication(sellerDto.Id, sellerDto.Token);

                    //var result = SendEmail(login.Email);
                    scope.Complete();
                }
                return sellerDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(new Exception(ex.Message));
            }
            return null;
        }

        public DelivererDto SignUp(DelivererVM delivererVM)
        {
            var isAlreadyAvailable = _loginRepository.Get(x => x.Email == delivererVM.LoginVM.Email).FirstOrDefault();
            if (isAlreadyAvailable != null)
                return null;
            try
            {
                var delivererDto = new DelivererDto()
                {
                    FirstName = delivererVM.FirstName,
                    LastName = delivererVM.LastName,
                    MobileNumber = delivererVM.MobileNumber,
                    ProfileImage = delivererVM.ProfileImage,
                    NIC = delivererVM.NIC,
                    VehicleNo = delivererVM.VehicleNo,
                    VehicleType = delivererVM.VehicleType,
                    Rating = 0.0
                };

                using (TransactionScope scope = new TransactionScope())
                {
                    Login login = Mapper.Map<Login>(delivererVM.LoginVM);
                    login.Password = Encrypt(login.Password, key);
                    _loginRepository.Add(login);
                    _loginRepository.Save();

                    Login loginId = _loginRepository.Get(x => x.Email == login.Email).FirstOrDefault();
                    delivererDto.LoginId = loginId.Id;

                    Deliverer deliverer = Mapper.Map<Deliverer>(delivererDto);
                    _delivererRepository.Add(deliverer);
                    _delivererRepository.Save();

                    Location location = new Location()
                    {
                        DelivererId = deliverer.Id,
                    };
                    _locationRepository.Add(location);
                    _locationRepository.Save();

                    delivererDto.Token = Authentication(delivererDto.Id, delivererDto.Token);
                    //var result = SendEmail(login.Email);
                    
                        scope.Complete();
                }
                return delivererDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(new Exception(ex.Message));
            }
            return null;
        }

        public CustomerDto Update(CustomerVM customerVM)
        {
            Customer customer = _customerRepository.Get(x => x.Id == customerVM.Id).FirstOrDefault();

            if (customerVM.FirstName != null)
                customer.FirstName = customerVM.FirstName;
            if (customerVM.LastName != null)
                customer.LastName = customerVM.LastName;
            if (customerVM.MobileNumber != null)
                customer.MobileNumber = customerVM.MobileNumber;
            if (customerVM.ProfileImage != null)
                customer.ProfileImage = customerVM.ProfileImage;

            _customerRepository.Update(customer);
            _customerRepository.Save();
            return Mapper.Map<CustomerDto>(customer);
        }

        public bool SendEmail(string email)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    MailMessage mailMessage = new MailMessage("174025d@uom.lk", email);
                    string code = Code();

                    mailMessage.Body = "Please use following secuirity code for the shopMe: \n Secuirity code is:  " + code + "\n if you did not request this code , you can safely ignore this code";
                    mailMessage.Subject = "shopMe verification code";

                    SmtpClient smtp = new SmtpClient("submit.uom.lk", 587);
                    smtp.UseDefaultCredentials = false;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    NetworkCredential networkCredential = new NetworkCredential("174025d@uom.lk", "KuWathsala@97");//mhnedsmhjtmhrt
                    smtp.Credentials = networkCredential;
                    smtp.Send(mailMessage);

                    Login login = _loginRepository.Get(x => x.Email == email).FirstOrDefault();
                    if (login == null)
                        return false;
                    else
                    {
                        login.Status = code;
                        _loginRepository.Update(login);
                        _loginRepository.Save();
                    }

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

        public string Code()
        {
            Random random = new Random();
            StringBuilder builder = new StringBuilder();

            char ch;
            for (int i = 0; i < 4; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            builder.ToString();
            builder.Append(random.Next(1000, 1999));

            return builder.ToString();
        }

        public bool Veryfy(string code)
        {
            Login login = _loginRepository.Get(x => x.Status==code).FirstOrDefault();
            if (login == null)
                return false;
            else
            {
                try
                {
                    using (TransactionScope scope = new TransactionScope())
                    {
                        login.Status = "verified";
                        _loginRepository.Update(login);
                        _loginRepository.Save();
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

        public object ResetPassword(string email, string password)
        {
            try
            {
                //using (TransactionScope scope = new TransactionScope())
                {
                    Login login = _loginRepository.Get(x=>x.Email==email).FirstOrDefault();
                    if (login == null)
                        return false;
                    else
                    {
                        login.Status = "verified";
                        login.Password = Encrypt(password, key);
                        _loginRepository.Update(login);
                        _loginRepository.Save();
                        //scope.Complete();
                        return SignIn(email, password);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(new Exception(ex.Message));
                return ex.Message;
            }
        }

        public SellerDto Update(SellerVM sellerVM)
        {
            Seller seller = _sellerRepository.Get(x => x.Id == sellerVM.Id).FirstOrDefault();

            if (sellerVM.FirstName != null)
                seller.FirstName = sellerVM.FirstName;
            if (sellerVM.LastName != null)
                seller.LastName = sellerVM.LastName;
            if (sellerVM.MobileNumber != null)
                seller.MobileNumber = sellerVM.MobileNumber;
            if (sellerVM.AccountNo != null)
                seller.AccountNo = sellerVM.AccountNo;
            if (sellerVM.Image != null)
                seller.Image = sellerVM.Image;
            if (sellerVM.ProfileImage != null)
                seller.ProfileImage = sellerVM.ProfileImage;
            if (sellerVM.ShopName != null)
                seller.ShopName = sellerVM.ShopName;
            if (sellerVM.ShopAddress != null)
                seller.ShopAddress = sellerVM.ShopAddress;

            _sellerRepository.Update(seller);
            _sellerRepository.Save();
            return Mapper.Map<SellerDto>(seller);
        }

        public DelivererDto Update(DelivererVM delivererVM)
        {
            Deliverer deliverer = _delivererRepository.Get(x => x.Id == delivererVM.Id).FirstOrDefault();

            if (delivererVM.FirstName != null)
                deliverer.FirstName = delivererVM.FirstName;
            if (delivererVM.LastName != null)
                deliverer.LastName = delivererVM.LastName;
            if (delivererVM.MobileNumber != null)
                deliverer.MobileNumber = delivererVM.MobileNumber;
            if (delivererVM.VehicleType != null)
                deliverer.VehicleType = delivererVM.VehicleType;
            if (delivererVM.VehicleNo != null)
                deliverer.VehicleNo = delivererVM.VehicleNo;

            _delivererRepository.Update(deliverer);
            _customerRepository.Save();
            return Mapper.Map<DelivererDto>(deliverer);
        }
        public string Email(string email)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    MailMessage mailMessage = new MailMessage("174025d@uom.lk", email);
                    string code = Code();

                    mailMessage.Body = "Please use following secuirity code for the shopMe: \n Secuirity code is:  " + code + "\n if you did not request this code , you can safely ignore this code";
                    mailMessage.Subject = "shopMe verification code";

                    SmtpClient smtp = new SmtpClient("submit.uom.lk", 587);
                    smtp.UseDefaultCredentials = false;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    NetworkCredential networkCredential = new NetworkCredential("174025d@uom.lk", "KuWathsala@97");//mhnedsmhjtmhrt
                    smtp.Credentials = networkCredential;
                    smtp.Send(mailMessage);

                    Login login = _loginRepository.Get(x => x.Email == email).FirstOrDefault();
                    if (login == null)
                        return "no email in db";
                    else
                    {
                        login.Status = code;
                        _loginRepository.Update(login);
                        _loginRepository.Save();
                    }

                    scope.Complete();
                }
                return "true";
            }
            catch (Exception ex)
            {
                Console.WriteLine(new Exception(ex.Message));
                return ex.Message;
            }
        }
    }
}
