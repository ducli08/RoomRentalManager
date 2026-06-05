using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Common.CommonDto
{
    public class PagedResultDto<T> where T : class
    {
        public List<T> ListItem { get; set; }
        public int TotalCount { get; set; }

        public PagedResultDto(List<T> listItem, int totalCount)
        {
            ListItem = listItem;
            TotalCount = totalCount;
        }
    }
}
