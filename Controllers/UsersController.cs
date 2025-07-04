﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Data;
using ProyectoFinal.Models;
using Microsoft.Extensions.Localization;

namespace ProyectoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IStringLocalizer<Messages> _localizer;

        public UsersController(AppDbContext context, IStringLocalizer<Messages> localizer)
        {
            _context = context;
            _localizer = localizer;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUsers(int id)
        {
            var users = await _context.Users.FindAsync(id);

            if (users == null)
            {
                return NotFound(new { message = _localizer["NotFound"] });
            }

            return users;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(int id, Users users)
        {
            if (id != users.Id)
            {
                return BadRequest(new { message = _localizer["InvalidRequest"] });
            }

            _context.Entry(users).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound(new { message = _localizer["NotFound"] });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = _localizer["Updated"] });
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<Users>> PostUsers(Users users)
        {
            if (string.IsNullOrWhiteSpace(users.Password))
            {
                return BadRequest(new { message = _localizer["InvalidRequest"] });
            }

            users.Password = BCrypt.Net.BCrypt.HashPassword(users.Password);

            _context.Users.Add(users);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsers), new { id = users.Id }, new { message = _localizer["Created"], data = users });
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsers(int id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound(new { message = _localizer["NotFound"] });
            }

            _context.Users.Remove(users);
            await _context.SaveChangesAsync();

            return Ok(new { message = _localizer["Deleted"] });
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}

