using System.Collections.Generic;
using System.Threading.Tasks;
using AasDemoapp.Database;
using Microsoft.EntityFrameworkCore;

namespace AasDemoapp.ToolRepos
{
    public class ToolRepoService
    {
        private readonly AasDemoappContext _context;

        public ToolRepoService(AasDemoappContext context)
        {
            _context = context;
        }

        public async Task<Database.Model.ToolRepo> AddAsync(Database.Model.ToolRepo toolRepo)
        {
            _context.ToolRepos.Add(toolRepo);
            await _context.SaveChangesAsync();
            return toolRepo;
        }

        public async Task<List<Database.Model.ToolRepo>> GetAllAsync()
        {
            return await _context.ToolRepos.ToListAsync();
        }

        public async Task<Database.Model.ToolRepo?> GetByIdAsync(long id)
        {
            return await _context.ToolRepos.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Database.Model.ToolRepo?> UpdateAsync(Database.Model.ToolRepo toolRepo)
        {
            var existing = await _context.ToolRepos.FirstOrDefaultAsync(s => s.Id == toolRepo.Id);
            if (existing == null)
            {
                return null;
            }

            existing.Name = toolRepo.Name;
            existing.Logo = toolRepo.Logo;
            existing.RemoteAasRepositoryUrl = toolRepo.RemoteAasRepositoryUrl;
            existing.RemoteSmRepositoryUrl = toolRepo.RemoteSmRepositoryUrl;
            existing.RemoteAasRegistryUrl = toolRepo.RemoteAasRegistryUrl;
            existing.RemoteSmRegistryUrl = toolRepo.RemoteSmRegistryUrl;
            existing.RemoteDiscoveryUrl = toolRepo.RemoteDiscoveryUrl;
            existing.RemoteCdRepositoryUrl = toolRepo.RemoteCdRepositoryUrl;
            existing.SecuritySetting = toolRepo.SecuritySetting;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ToolRepos.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
            {
                return false;
            }

            _context.ToolRepos.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
