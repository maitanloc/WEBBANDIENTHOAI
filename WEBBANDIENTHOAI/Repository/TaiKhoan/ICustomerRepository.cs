using System.Threading.Tasks;
using System.Collections.Generic;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository.TaiKhoan
{
    public interface ICustomerRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> GetCustomerByIdAsync(int customerId);
        Task<Customer> GetCustomerByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetAllAsync(); // Thêm method này
    }
}