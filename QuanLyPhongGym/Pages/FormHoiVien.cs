using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace QuanLyPhongGym.Pages
{
    public partial class FormHoiVien : Form
    {
        //private HoiVienCTL hoiVienCTL = new HoiVienCTL();
        //private HoiVienDTO hv = new HoiVienDTO();
        private int iLastRowID;
        private string imgLoc;

        public FormHoiVien(string lastRowID)
        {
            InitializeComponent();
            cmbGoiTap.SelectedIndex = 0;
            cmbGioiTinh.SelectedIndex = 0;
            string sub = lastRowID.Substring(Math.Max(0, lastRowID.Length - 3));
            iLastRowID = Int32.Parse(sub);
            DateTime dt = DateTime.Now;
            dt = dt.AddMonths(1);
            lblHetHan.Text = dt.ToString("d/M/yyyy", CultureInfo.InvariantCulture);
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

        private void LayThongTinHoiVien()
        {
            //hv.HoTen = txtHoTen.Text;
            //hv.GioiTinh = cmbGioiTinh.Text;
            //hv.SDT = txtSDT.Text;
            //hv.GoiTap = cmbGoiTap.Text;
            //hv.NgayHetHan = DateTime.Now;
            //iLastRowID++;
            //hv.ID_HV = "KH00" + iLastRowID.ToString();

            //if (picBoxHV.Image != null)
            //    hv.HinhAnh = ImageToByteArray(imgLoc);
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                LayThongTinHoiVien();
                //hoiVienCTL.HoiVien = hv;
                //hoiVienCTL.insert();

                MessageBox.Show("Thêm THÀNH CÔNG!", "Thông báo");
            }
            catch (Exception)
            {
                MessageBox.Show("Bạn chưa thêm ảnh!", "Thông báo");
            }
        }

        private void btnXoaHet_Click(object sender, EventArgs e)
        {
            ClearTextBoxes();
        }

        private void cmbGoiTap_TextChanged(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            switch (cmbGoiTap.Text)
            {
                case "1 tháng":
                    dt = dt.AddMonths(1);
                    break;
                case "3 tháng":
                    dt = dt.AddMonths(3);
                    break;
                case "VIP":
                    dt = dt.AddMonths(13);
                    break;
                case "Thường":
                    dt = dt.AddMonths(7);
                    break;
            }
            lblHetHan.Text = dt.ToString("d/M/yyyy", CultureInfo.InvariantCulture);
        }

        private void txtSDT_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
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
                    picBoxHV.ImageLocation = imgLoc;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
