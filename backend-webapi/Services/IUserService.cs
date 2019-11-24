using webapi.Dtos;
using webapi.ViewModels;

namespace webapi.Services
{
    public interface IUserService
    {
        string Authentication(int id, string token);
        object SignIn(string email, string password);
        object SignUp(CustomerVM customerVM);
        DelivererDto SignUp(DelivererVM delivererVM);
        SellerDto SignUp(SellerVM sellerVM);
        CustomerDto Update(CustomerVM customerVM);
        SellerDto Update(SellerVM sellerVM);
        DelivererDto Update(DelivererVM delivererVM);
        bool SendEmail(string email);
        string Code();
        bool Veryfy(string code);
        object ResetPassword(string email, string password);
        string Email(string email);
    }
}