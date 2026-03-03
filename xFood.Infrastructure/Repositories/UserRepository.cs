using Microsoft.EntityFrameworkCore;
using xFood.Application.DTOs;
using xFood.Application.Interfaces;
using xFood.Domain.Entities;
using xFood.Infrastructure.Persistence;

namespace xFood.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly xFoodDbContext _context;

        public UserRepository(xFoodDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 🔐 O NOVO MÉTODO DE LOGIN (O QUE ESTAVA FALTANDO)
        // ============================================================
        public async Task<UserDto?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.TypeUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user == null || !user.Active) return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                DateBirth = user.DateBirth,
                TypeUserId = user.TypeUserId,
                TypeUserDescription = user.TypeUser?.Description,
                Active = user.Active
            };
        }

        // ============================================================
        // 📋 MÉTODOS PADRÃO (CRUD)
        // ============================================================

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.TypeUser)
                .AsNoTracking()
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    DateBirth = u.DateBirth,
                    TypeUserId = u.TypeUserId,
                    TypeUserDescription = u.TypeUser != null ? u.TypeUser.Description : null,
                    Active = u.Active
                }).ToListAsync();
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var u = await _context.Users
                .Include(x => x.TypeUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (u == null) return null;

            return new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                DateBirth = u.DateBirth,
                TypeUserId = u.TypeUserId,
                TypeUserDescription = u.TypeUser?.Description,
                Active = u.Active
            };
        }

        public async Task<int> CreateAsync(UserCreateUpdateDto dto)
        {
            var entity = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password, // Em produção, criptografar isso!
                DateBirth = dto.DateBirth,
                TypeUserId = dto.TypeUserId,
                Active = dto.Active
            };

            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task UpdateAsync(int id, UserCreateUpdateDto dto)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity != null)
            {
                entity.Name = dto.Name;
                entity.Email = dto.Email;
                entity.DateBirth = dto.DateBirth;
                entity.TypeUserId = dto.TypeUserId;
                entity.Active = dto.Active;

                // Só atualiza a senha se ela não vier vazia ou como máscara
                if (!string.IsNullOrEmpty(dto.Password) && dto.Password != "(manter)" && dto.Password != entity.Email)
                {
                    entity.Password = dto.Password;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity != null)
            {
                _context.Users.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // ============================================================
        // 🔍 FILTROS E SOFT DELETE
        // ============================================================

        public async Task<IEnumerable<UserDto>> GetByFilterAsync(bool? active)
        {
            var query = _context.Users
                .Include(u => u.TypeUser)
                .AsNoTracking()
                .AsQueryable();

            if (active.HasValue)
            {
                query = query.Where(u => u.Active == active.Value);
            }

            return await query.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                DateBirth = u.DateBirth,
                TypeUserId = u.TypeUserId,
                TypeUserDescription = u.TypeUser != null ? u.TypeUser.Description : "-",
                Active = u.Active
            }).ToListAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            await SetActiveAsync(id, false);
        }

        public async Task SetActiveAsync(int id, bool active)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity != null)
            {
                entity.Active = active;
                await _context.SaveChangesAsync();
            }
        }
    }
}