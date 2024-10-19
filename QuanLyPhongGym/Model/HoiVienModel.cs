using QuanLyPhongGym.Controller;
using System;

namespace QuanLyPhongGym.Model
{
    [Table("HoiVien")]
    public class HoiVienModel : DBController
    {
        [PrimaryKey]
        public string MaHV { get; set; }
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public string SDT { get; set; }
        public DateTime? NgayHetHan { get; set; }
        public string GoiTap { get; set; }
        public byte[] HinhAnh { get; set; }
    }
}