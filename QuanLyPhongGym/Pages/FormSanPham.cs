using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace QuanLyPhongGym.Pages
{
    public partial class FormSanPham : Form
    {
        private SanPhamCTL sanPhamCTL = new SanPhamCTL();
        private SanPhamDTO sp = new SanPhamDTO();
        private DataGridViewRow curRow;
        private bool isAnotherImage = false;
        private string imgLoc;

        public FormSanPham(DataGridViewRow curRow)
        {
            InitializeComponent();
            this.curRow = curRow;

            // row data to form
            txtTenSP.Text = curRow.Cells["ten"].Value.ToString();
            cmbLoaiSP.Text = curRow.Cells["loai"].Value.ToString();
            dateTimePicker.Value = (DateTime)curRow.Cells["ngaynhap"].Value;
            txtSoLuongSP.Text = curRow.Cells["soluong"].Value.ToString();
            txtDonGiaSP.Text = curRow.Cells["dongia"].Value.ToString();
            txtTrongLuongSP.Text = curRow.Cells["trongluong"].Value.ToString();
            txtHangSXSP.Text = curRow.Cells["hangsx"].Value.ToString();
            cmbTinhTrangSP.Text = curRow.Cells["tinhtrang"].Value.ToString();

            Byte[] data = new Byte[0];
            data = (Byte[])(curRow.Cells["hinhanh"].Value);
            MemoryStream mem = new MemoryStream(data);
            picBoxSP.Image = Image.FromStream(mem);
        }

        private Byte[] ImageToByteArray(string imgLocation)
        {
            Byte[] img = null;
            FileStream fs = new FileStream(imgLocation, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            img = br.ReadBytes((int)fs.Length);

            return img;
        }

        private void LayThongTinSanPham()
        {
            sp.ID_SP = curRow.Cells["id_sp"].Value.ToString();
            sp.Ten = txtTenSP.Text;
            sp.Loai = cmbLoaiSP.Text;
            sp.NgayNhap = dateTimePicker.Value.Date;

            int result = 0;
            bool b = int.TryParse(txtSoLuongSP.Text, out result);
            sp.SoLuong = result;

            sp.DonGia = txtDonGiaSP.Text;
            sp.TrongLuong = txtTrongLuongSP.Text;
            sp.HangSX = txtHangSXSP.Text;
            sp.TinhTrang = cmbTinhTrangSP.Text;
            if (!isAnotherImage)
                sp.HinhAnh = (Byte[])curRow.Cells["hinhanh"].Value;
            else
                sp.HinhAnh = ImageToByteArray(imgLoc);
        }

        private void btnLuuSP_Click(object sender, EventArgs e)
        {
            LayThongTinSanPham();
            sanPhamCTL.SanPham = sp;
            sanPhamCTL.update();

            MessageBox.Show("Lưu THÀNH CÔNG!", "Thông báo");
        }

        private void txtSoLuongSP_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void picBoxSP_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Chọn ảnh đại diện";
                dlg.Filter = "JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imgLoc = dlg.FileName;
                    picBoxSP.ImageLocation = imgLoc;
                    isAnotherImage = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
