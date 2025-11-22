using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels;
using WEBBANDIENTHOAI.Repository;
using System.Threading.Tasks;
using System.Linq;

namespace WEBBANDIENTHOAI.Controllers
{
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository;

        public RoleController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        // GET: Role
        public async Task<IActionResult> Index()
        {
            var roles = await _roleRepository.GetAllRolesAsync();
            return View("~/Views/Admin/Role.cshtml", roles);
        }

        // POST: Role/Create (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _roleRepository.RoleNameExistsAsync(model.RoleName))
                {
                    return Json(new { success = false, message = "Tên vai trò đã tồn tại" });
                }

                var role = new Role
                {
                    RoleName = model.RoleName,
                    Description = model.Description
                };

                var result = await _roleRepository.CreateRoleAsync(role);
                if (result)
                {
                    return Json(new { success = true, message = "Thêm vai trò thành công!" });
                }
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm vai trò" });
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
        }

        // POST: Role/Edit (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleViewModel model)
        {
            if (id != model.RoleId)
            {
                return Json(new { success = false, message = "Không tìm thấy vai trò" });
            }

            if (ModelState.IsValid)
            {
                if (await _roleRepository.RoleNameExistsAsync(model.RoleName, id))
                {
                    return Json(new { success = false, message = "Tên vai trò đã tồn tại" });
                }

                var role = await _roleRepository.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy vai trò" });
                }

                role.RoleName = model.RoleName;
                role.Description = model.Description;

                var result = await _roleRepository.UpdateRoleAsync(role);
                if (result)
                {
                    return Json(new { success = true, message = "Cập nhật vai trò thành công!" });
                }
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật vai trò" });
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
        }

        // POST: Role/Delete (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roleRepository.DeleteRoleAsync(id);
            if (result)
            {
                return Json(new { success = true, message = "Xóa vai trò thành công!" });
            }
            return Json(new { success = false, message = "Không thể xóa vai trò này vì có người dùng đang sử dụng!" });
        }

        // GET: Role/GetRole (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _roleRepository.GetRoleByIdAsync(id);
            if (role == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vai trò" });
            }

            // Sửa lại để đảm bảo property names chính xác
            var roleData = new
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description
            };

            return Json(new
            {
                success = true,
                data = roleData
            });
        }
    }
}