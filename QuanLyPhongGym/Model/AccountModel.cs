using QuanLyPhongGym.Controller;
using System;

namespace QuanLyPhongGym.Model
{
    [Table("Account")]
    public class AccountModel : DBController
    {
        [PrimaryKey]
        public string ID { get; set; }
        public string TaiKhoan { get; set; }
        public string MatKhau { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? ChucVu { get; set; }
        public short? TrangThai { get; set; }
    }
}
