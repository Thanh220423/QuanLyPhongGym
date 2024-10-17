using QuanLyPhongGym.Controller;
using System;

namespace QuanLyPhongGym.Model
{
    [Table("ChucVu")]
    public class ChucVuModel : DBController
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string TenChucVu { get; set; }
        public short? TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
    }
}