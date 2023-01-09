using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserRolePermissionService.DTOs;
using UserService.Data;
using UserService.Interfaces;
using UserService.Models;

namespace UserRolePermissionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserServices _services;
        private readonly IMapper _mapper;

        public UserManagementController(DataContext context, IUserServices services, IMapper mapper)
        {
            _context = context;
            _services = services;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDTO>>> GetAllUserAsync()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(_mapper.Map<List<UserDTO>>(users));
        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<UserDTO>> GetUserAsync(string Id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == Guid.Parse(Id));
            if (user == null) return BadRequest("User Not Found!");

            return Ok(_mapper.Map<UserDTO>(user));
        }


        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUserAsync(UserDTO userDTO)
        {
            if (_context.Users.Any(u => u.Email == userDTO.Email))
            {
                return BadRequest("User Already Exist!");
            }
            var password = userDTO.Password;
            var confirmPassword = userDTO.ConfirmPassword;
            if (password != confirmPassword)
            {
                return BadRequest("Password must match!");
            }
            if (password.Length < 6)
            {
                return BadRequest("Please enter at least 6 characters");
            }

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == userDTO.RoleId);
            if(userRole == null)
            {
                return BadRequest("Role does not exist!");
            }

            _services.CreatePasswordHash(userDTO.Password,
                 out byte[] passwordHash,
                 out byte[] passwordSalt);

            var user = new User
            {
                Email = userDTO.Email,
                Address = userDTO.Address,
                passwordHash = passwordHash,
                passwordSalt = passwordSalt,
                verificationToken = _services.CreateRandomToken(userDTO.Email.ToString(), userRole.Name.ToString()),
                RoleId = userDTO.RoleId,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Create User Successfully!");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> EditUserAsync(Guid id, User userDTO)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return BadRequest("User Does Not Exist!");
            }

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == userDTO.RoleId);
            if (userRole == null)
            {
                return BadRequest("Role does not exist!");
            }
            
            user.Email = userDTO.Email;
            user.Address = userDTO.Address;
            user.UpdatedAt = DateTime.Now;
            user.RoleId = userDTO.RoleId;
            _context.Update(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw new Exception("Something is wrong when trying to update user!");
            }
            return NoContent();
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult> DeleteUserAsync(Guid Id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == Id);
            if (user == null)
            {
                return BadRequest("User Does Not Exist!");
            }

            user.Status = false;
            _context.Update(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new Exception("Something is wrong when trying to remove user!");
            }

            return NoContent();
        }

    }
}
