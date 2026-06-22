using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Common.CommonDto
{
    public class PagedRequestDto<TFilter>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = string.Empty;
        public string? SortOrder { get; set; } = string.Empty;
        public TFilter? Filter { get; set; }
    }
}
