using QuanLyPhongGym.Controller;
using System;

namespace QuanLyPhongGym.Model
{
    [Table("SanPham")]
    public class SanPhamModel : DBController
    {
        [PrimaryKey]
        public string MaSP { get; set; }
        public string Ten { get; set; }
        public string Loai { get; set; }
        public DateTime? NgayNhap { get; set; }
        public int? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public string TrongLuong { get; set; }
        public string HangSX { get; set; }
        public string TinhTrang { get; set; }
        public bool? HinhAnh { get; set; }
    }
}