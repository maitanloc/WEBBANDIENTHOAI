using System.Threading.Tasks;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repositories
{
    public interface ICustomerRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> GetCustomerByIdAsync(int customerId);
        Task<Customer> GetCustomerByEmailAsync(string email);
    }
}