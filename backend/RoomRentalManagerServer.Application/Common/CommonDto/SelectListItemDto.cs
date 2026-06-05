using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Common.CommonDto
{
    public class SelectListItemDto
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public string? CascaderId { get; set; }
    }
}
