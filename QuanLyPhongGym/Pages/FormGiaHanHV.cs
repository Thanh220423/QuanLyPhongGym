using System;
using System.Windows.Forms;

namespace QuanLyPhongGym.Pages
{
    public partial class FormGiaHanHV : Form
    {
        private DataGridViewRow curRow;
        //private HoiVienCTL hoiVienCTL;
        //private HoiVienDTO hv;

        public FormGiaHanHV(DataGridViewRow curRow)
        {
            InitializeComponent();
            this.curRow = curRow;
            cmbGoiTap.SelectedIndex = 0;
            //hoiVienCTL = new HoiVienCTL();
        }

        private void btn_Luu_Click(object sender, EventArgs e)
        {
            try
            {
                //hv = new HoiVienDTO();
                //hv.GoiTap = cmbGoiTap.Text;
                //hv.ID_HV = curRow.Cells["id_hv"].Value.ToString();
                //hv.NgayHetHan = (DateTime)curRow.Cells["ngayhethan"].Value;
                //hv.GioiTinh = curRow.Cells["gioitinh"].Value.ToString();
                //hv.HoTen = curRow.Cells["hoten"].Value.ToString();
                //hv.SDT = curRow.Cells["sdt"].Value.ToString();
                //hoiVienCTL.HoiVien = hv;
                //hoiVienCTL.update();

                MessageBox.Show("Gia hạn THÀNH CÔNG!", "Thông báo");
            }
            catch (Exception)
            {
                MessageBox.Show("Gia hạn THẤT BẠI!", "Thông báo");
            }
        }
    }
}
