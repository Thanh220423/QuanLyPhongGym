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
        public int? ChucVu_Id { get; set; }
        public short? TrangThai { get; set; }

        private string _ChucVu;

        [Ignore]
        public string ChucVu
        {
            get
            {
                if (ChucVu_Id == null) return null;
                ChucVuModel chucVu = new ChucVuModel { ID = ChucVu_Id.Value };
                chucVu = Select<ChucVuModel>(chucVu);
                return chucVu.TenChucVu;
            }
            set { _ChucVu = value; }
        }
    }
}
