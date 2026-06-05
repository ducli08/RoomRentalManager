using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomRentalManagerServer.Application.Model.UsersModel.Dto
{
    public class UserFilterDto
    {
        public string NameFilter { get; set; }
        public string EmailFilter { get; set; }
        public string ProvinceCodeFilter { get; set; }
        public string DistrictCodeFilter { get; set; }
        public string WardCodeFilter { get; set; }
        public string AddressFilter{ get; set; }
        public string IDCardFilter { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}
