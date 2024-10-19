using QuanLyPhongGym.Areas;
using QuanLyPhongGym.Controller;
using QuanLyPhongGym.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QuanLyPhongGym.Pages
{
    public partial class FormGiaHanHV : Form
    {
        private DBController _dbController = new DBController();
        private CmmFunc _cmmFunc = new CmmFunc();
        private DateTime _Date_GoiTap;
        private HoiVienModel _hoiVien;

        public FormGiaHanHV(string MaHV)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(MaHV))
            {
                HoiVienModel hoiVien = new HoiVienModel { MaHV = MaHV };
                _hoiVien = _dbController.Select<HoiVienModel>(hoiVien);
            }
            cbb_GoiTap.SelectedIndex = 0;
        }

        private void btn_Luu_Click(object sender, EventArgs e)
        {
            try
            {
                _hoiVien.GoiTap = cbb_GoiTap.Text;
                _hoiVien.NgayHetHan = _Date_GoiTap;
                _dbController.Update(_hoiVien, new List<string> { "GoiTap", "NgayHetHan" });
                if (MessageBox.Show("Gia hạn thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Gia hạn thất bại!", "Thông báo");
            }
        }

        private void cbb_GoiTap_TextChanged(object sender, EventArgs e)
        {
            _Date_GoiTap = _hoiVien.NgayHetHan.Value;
            switch (cbb_GoiTap.Text)
            {
                case "1 tháng":
                    _Date_GoiTap = _Date_GoiTap.AddMonths(1);
                    break;
                case "3 tháng":
                    _Date_GoiTap = _Date_GoiTap.AddMonths(3);
                    break;
                case "Thường":
                    _Date_GoiTap = _Date_GoiTap.AddMonths(7);
                    break;
                case "VIP":
                    _Date_GoiTap = _Date_GoiTap.AddMonths(13);
                    break;
            }
        }
    }
}
