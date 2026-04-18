using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWorkz.StarterKitMVC.Shared.DTOs
{
    public class LookupDto
    {
        public Guid LookupId { get; set; }
        public string CategoryKey { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string DisplayName { get; set; }
        public int SortOrder { get; set; } = 0;
        public string TenantId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
