using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Models.Backend
{
    public class GoogleSyncToken
    {
        public required string UserId { get; set; }
        public required string SyncToken { get; set; }
    }
}
