﻿using QuanLyPhongGym.Controller;

namespace QuanLyPhongGym.Model
{
    [Table("ThietBi")]
    public class ThietBiModel : DBController
    {
        [PrimaryKey]
        public string ID { get; set; }
        public string Ten { get; set; }
        public string Loai { get; set; }
        public int? SoLuong { get; set; }
        public int? SoLuongHu { get; set; }
        public string HangSX { get; set; }
        public string GhiChu { get; set; }
        public bool? HinhAnh { get; set; }
    }
}