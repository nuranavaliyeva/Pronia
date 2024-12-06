using Microsoft.EntityFrameworkCore;
using ProniaMVC.DAL;
using ProniaMVC.Services.Interfaces;

namespace ProniaMVC.Services.Implementations
{
    public class LayoutServices:ILayoutService
    {
        private readonly AppDbContex _context;

        public LayoutServices(AppDbContex context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
              return settings;
        }
       


    }
}
