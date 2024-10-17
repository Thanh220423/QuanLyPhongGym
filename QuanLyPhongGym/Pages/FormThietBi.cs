using System;
using System.IO;
using System.Windows.Forms;
using QuanLy.BUS;
using QuanLy.DTO;

namespace QuanLyPhongGym.Pages
{
    public partial class FormThietBi : Form
    {
        private ThietBiCTL thietBiCTL = new ThietBiCTL();
        private ThietBiDTO tb = new ThietBiDTO();
        private int soMay;
        private int soTa;
        private string imgLoc;

        public FormThietBi(int soMay, int soTa)
        {
            InitializeComponent();
            cmbLoaiTB.SelectedIndex = 0;
            cmbTinhTrangTB.SelectedIndex = 0;
            this.soMay = soMay;
            this.soTa = soTa;
        }

        private void ClearTextBoxes()
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                    if (control is TextBox)
                        (control as TextBox).Clear();
                    else
                        func(control.Controls);
            };
            func(Controls);
        }

        private Byte[] ImageToByteArray(string imgLocation)
        {
            Byte[] img = null;
            FileStream fs = new FileStream(imgLocation, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            img = br.ReadBytes((int)fs.Length);

            return img;
        }

        private void LayThongTinThietBi()
        {
            int result = 0;

            tb.Ten = txtTenTB.Text;
            tb.Loai = cmbLoaiTB.Text;
            tb.HangSX = txtHangSXTB.Text;
            tb.TinhTrang = cmbTinhTrangTB.Text;
            tb.GhiChu = txtGhiChuTB.Text;

            bool sl = int.TryParse(txtSoLuongTB.Text, out result);
            tb.SoLuong = result;

            if (tb.TinhTrang != "Hư")
                tb.SoLuongHu = 0;
            else
            {
                bool slh = int.TryParse(txtSoLuongHuTB.Text, out result);
                tb.SoLuongHu = result;
            }

            if (tb.Loai == "Máy")
            {
                soMay++;
                tb.ID_TB = "MA00" + soMay.ToString();
            }
            else if (tb.Loai == "Tạ")
            {
                soTa++;
                tb.ID_TB = "TA00" + soTa.ToString();
            }

            if (picBoxTB.Image != null)
                tb.HinhAnh = ImageToByteArray(imgLoc);
        }

        private void btnLuuSP_Click_1(object sender, EventArgs e)
        {
            try
            {
                LayThongTinThietBi();
                thietBiCTL.ThietBi = tb;
                thietBiCTL.insert();

                MessageBox.Show("Thêm THÀNH CÔNG!", "Thông báo");
            }
            catch (Exception)
            {
                MessageBox.Show("Bạn chưa thêm ảnh!", "Thông báo");
            }
        }

        private void btnXoaHetSP_Click_1(object sender, EventArgs e)
        {
            ClearTextBoxes();
        }

        private void txtSoLuongTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void txtSoLuongHuTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void cmbTinhTrangTB_TextChanged(object sender, EventArgs e)
        {
            if (cmbTinhTrangTB.Text == "Hư")
                txtSoLuongHuTB.Enabled = true;
            else
            {
                txtSoLuongHuTB.Enabled = false;
                txtSoLuongHuTB.Text = "0";
            }
        }

        private void picBoxHV_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Chọn ảnh đại diện";
                dlg.Filter = "JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imgLoc = dlg.FileName;
                    picBoxTB.ImageLocation = imgLoc;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
