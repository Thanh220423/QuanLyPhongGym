using QuanLyPhongGym.Controller;
using System;

namespace QuanLyPhongGym.Model
{
    [Select("WF_DCMOP_Account_SelectByCustomerId", Cols = "ID,MaHV")]
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
        public bool? HinhAnh { get; set; }
    }
}