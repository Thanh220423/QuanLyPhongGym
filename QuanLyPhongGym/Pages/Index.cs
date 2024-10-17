using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Windows;
using FontStyle = System.Drawing.FontStyle;

namespace QuanLyPhongGym.Pages
{
    public partial class Index : Form
    {
        private HoiVienCTL hoiVienCTL = new HoiVienCTL();
        private SanPhamCTL sanPhamCTL = new SanPhamCTL();
        private ThietBiCTL thietBiCTL = new ThietBiCTL();
        private HoiVienDTO hoiVienDTO = new HoiVienDTO();
        private SanPhamDTO sanPhamDTO = new SanPhamDTO();
        private ThietBiDTO thietBiDTO = new ThietBiDTO();
        private ArrayList dsHoiVien;
        private ArrayList dsSanPham;
        private ArrayList dsThietBi;
        private string imgLoc;
        private bool isAnotherImage = false;

        public Index()
        {
            InitializeComponent();
            tabCtrl.DrawItem += new DrawItemEventHandler(tabCtrl_DrawItem);
        }

        // Search boxes placeholders
        private void HoiVienSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearchHV.Text == "Search...")
            {
                txtSearchHV.Text = "";
                txtSearchHV.ForeColor = Color.Black;
            }
        }

        private void HoiVienSearch_Leave(object sender, EventArgs e)
        {
            if (txtSearchHV.Text == "")
            {
                txtSearchHV.Text = "Search...";
                txtSearchHV.ForeColor = Color.Gray;
            }
        }

        private void txtSearchSP_Enter(object sender, EventArgs e)
        {
            if (txtSearchSP.Text == "Search...")
            {
                txtSearchSP.Text = "";
                txtSearchSP.ForeColor = Color.Black;
            }
        }

        private void txtSearchSP_Leave(object sender, EventArgs e)
        {
            if (txtSearchSP.Text == "")
            {
                txtSearchSP.Text = "Search...";
                txtSearchSP.ForeColor = Color.Gray;
            }
        }

        private void txtSearchTB_Enter(object sender, EventArgs e)
        {
            if (txtSearchTB.Text == "Search...")
            {
                txtSearchTB.Text = "";
                txtSearchTB.ForeColor = Color.Black;
            }
        }

        private void txtSearchTB_Leave(object sender, EventArgs e)
        {
            if (txtSearchTB.Text == "")
            {
                txtSearchTB.Text = "Search...";
                txtSearchTB.ForeColor = Color.Gray;
            }
        }

        // Support functions
        private void loadHoiVien(string keyword = null)
        {
            if (dsHoiVien != null)
                dsHoiVien.Clear();

            dsHoiVien = hoiVienCTL.getDsHocVien(keyword);
            if (dsHoiVien.Count == 0)
                dtgvHoiVien.DataSource = null;
            dtgvHoiVien.DataSource = dsHoiVien;
            if (dtgvHoiVien.RowCount > 0)
            {
                dtgvHoiVien.Columns[0].HeaderText = "Mã học viên";
                dtgvHoiVien.Columns[1].HeaderText = "Họ tên";
                dtgvHoiVien.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dtgvHoiVien.Columns[2].Visible = false;
                dtgvHoiVien.Columns[3].Visible = false;
                dtgvHoiVien.Columns[4].Visible = false;
                dtgvHoiVien.Columns[5].Visible = false;
                dtgvHoiVien.Columns[6].Visible = false;
            }
        }

        private void loadSanPham(string keyword = null)
        {
            if (dsSanPham != null)
                dsSanPham.Clear();

            dsSanPham = sanPhamCTL.getDsSanPham(keyword);
            dtgvSanPham.DataSource = dsSanPham;
            if (dtgvSanPham.RowCount > 0)
            {
                dtgvSanPham.Columns[0].HeaderText = "Mã sản phẩm";
                dtgvSanPham.Columns[1].HeaderText = "Tên";
                dtgvSanPham.Columns[2].HeaderText = "Loại";
                dtgvSanPham.Columns[3].HeaderText = "Ngày nhập";
                dtgvSanPham.Columns[4].HeaderText = "Số lượng";
                dtgvSanPham.Columns[5].HeaderText = "Đơn giá";
                dtgvSanPham.Columns[6].HeaderText = "Trọng lượng";
                dtgvSanPham.Columns[7].HeaderText = "Hãng sản xuất";
                dtgvSanPham.Columns[8].HeaderText = "Tình trạng";
                dtgvSanPham.Columns[9].Visible = false;
            }
            dtgvSanPham.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void loadThietBi(string keyword = null)
        {
            if (dsThietBi != null)
                dsThietBi.Clear();

            dsThietBi = thietBiCTL.getDsThietBi(keyword);
            dtgvThietBi.DataSource = dsThietBi;
            if (dtgvThietBi.RowCount > 0)
            {
                dtgvThietBi.Columns[0].HeaderText = "Mã thiết bị";
                dtgvThietBi.Columns[1].HeaderText = "Tên";
                dtgvThietBi.Columns[2].HeaderText = "Loại";
                dtgvThietBi.Columns[3].HeaderText = "Số lượng";
                dtgvThietBi.Columns[4].HeaderText = "Số lượng hư";
                dtgvThietBi.Columns[5].HeaderText = "Tình trạng";
                dtgvThietBi.Columns[6].HeaderText = "Hãng sản xuất";
                dtgvThietBi.Columns[7].HeaderText = "Ghi chú";
                dtgvThietBi.Columns[8].Visible = false;
            }
            dtgvThietBi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void getCurrentRowHVInfo()
        {
            DataGridViewRow row = dtgvHoiVien.CurrentRow;
            hoiVienDTO.HoTen = row.Cells["hoten"].Value.ToString();
            hoiVienDTO.GioiTinh = row.Cells["gioitinh"].Value.ToString();
            hoiVienDTO.SDT = row.Cells["sdt"].Value.ToString();
            hoiVienDTO.ID_HV = row.Cells["id_hv"].Value.ToString();
            hoiVienDTO.NgayHetHan = (DateTime)row.Cells["ngayhethan"].Value;
            hoiVienDTO.GoiTap = row.Cells["goitap"].Value.ToString();
            hoiVienDTO.HinhAnh = (Byte[])row.Cells["hinhanh"].Value;
        }

        public Byte[] ImageToByteArray(string imgLocation)
        {
            Byte[] img = null;
            FileStream fs = new FileStream(imgLocation, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            img = br.ReadBytes((int)fs.Length);

            return img;
        }

        private void getCurrentRowSPInfo()
        {
            DataGridViewRow row = dtgvSanPham.CurrentRow;
            sanPhamDTO.ID_SP = row.Cells["id_sp"].Value.ToString();
            sanPhamDTO.Ten = row.Cells["ten"].Value.ToString();
            sanPhamDTO.Loai = row.Cells["loai"].Value.ToString();
            sanPhamDTO.NgayNhap = (DateTime)row.Cells["ngaynhap"].Value;
            sanPhamDTO.SoLuong = Convert.ToInt32(row.Cells["soluong"].Value);
            sanPhamDTO.DonGia = row.Cells["dongia"].Value.ToString();
            sanPhamDTO.TrongLuong = row.Cells["trongluong"].Value.ToString();
            sanPhamDTO.HangSX = row.Cells["hangsx"].Value.ToString();
            sanPhamDTO.TinhTrang = row.Cells["tinhtrang"].Value.ToString();
            sanPhamDTO.HinhAnh = (Byte[])row.Cells["hinhanh"].Value;
        }

        private void getCurrentRowTBInfo()
        {
            DataGridViewRow row = dtgvThietBi.CurrentRow;
            thietBiDTO.ID_TB = row.Cells["id_tb"].Value.ToString();
            thietBiDTO.Ten = row.Cells["tentb"].Value.ToString();
            thietBiDTO.Loai = row.Cells["loaitb"].Value.ToString();
            thietBiDTO.SoLuong = Convert.ToInt32(row.Cells["soluongtb"].Value);
            thietBiDTO.TinhTrang = row.Cells["tinhtrangtb"].Value.ToString();
            thietBiDTO.SoLuongHu = Convert.ToInt32(row.Cells["soluonghu"].Value);
            thietBiDTO.HangSX = row.Cells["hangsxtb"].Value.ToString();
            thietBiDTO.GhiChu = row.Cells["ghichu"].Value.ToString();
            thietBiDTO.HinhAnh = (Byte[])row.Cells["hinhanh"].Value;
        }

        private void getHVRowToTxtBOX(DataGridViewRow row)
        {
            txtHoTen.Text = row.Cells["hoten"].Value.ToString();
            cmbGioiTinh.Text = row.Cells["gioitinh"].Value.ToString();
            txtSDT.Text = row.Cells["sdt"].Value.ToString();
            txtMaHV.Text = row.Cells["id_hv"].Value.ToString();
            cmbGoiTap.Text = row.Cells["goitap"].Value.ToString();

            DateTime dt = DateTime.ParseExact(row.Cells["ngayhethan"].Value.ToString(), "MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            lblHetHan.Text = dt.ToString("M/d/yyyy");

            Byte[] data = new Byte[0];
            data = (Byte[])(row.Cells["hinhanh"].Value);
            MemoryStream mem = new MemoryStream(data);
            picBoxHV.Image = Image.FromStream(mem);
        }

        private void getSPRowToTxtBOX(DataGridViewRow row)
        {
            txtTenSP.Text = row.Cells["ten"].Value.ToString();
            txtLoaiSP.Text = row.Cells["loai"].Value.ToString();
            txtSoLuongSP.Text = row.Cells["soluong"].Value.ToString();
            txtDonGiaSP.Text = row.Cells["dongia"].Value.ToString();

            string tt = row.Cells["tinhtrang"].Value.ToString();
            lblTinhTrangSP.Text = tt;
            if (tt == "Còn hàng")
                lblTinhTrangSP.ForeColor = Color.MediumBlue;
            else if (tt == "Hết hàng")
                lblTinhTrangSP.ForeColor = Color.Red;

            Byte[] data = new Byte[0];
            data = (Byte[])(row.Cells["hinhanh"].Value);
            MemoryStream mem = new MemoryStream(data);
            picBoxSP.Image = Image.FromStream(mem);
        }

        private void getTBRowToTxtBOX(DataGridViewRow row)
        {
            txtTenTB.Text = row.Cells[1].Value.ToString();
            txtLoaiTB.Text = row.Cells[2].Value.ToString();
            txtSoLuongTB.Text = row.Cells[3].Value.ToString();
            txtHangSXTB.Text = row.Cells[4].Value.ToString();

            string tt = row.Cells[5].Value.ToString();
            lblTinhTrangTB.Text = tt;
            if (tt == "Mới")
                lblTinhTrangTB.ForeColor = Color.MediumBlue;
            else if (tt == "Tốt")
                lblTinhTrangTB.ForeColor = Color.Green;
            else if (tt == "Hư")
                lblTinhTrangTB.ForeColor = Color.Red;

            Byte[] data = new Byte[0];
            data = (Byte[])(row.Cells["hinhanh"].Value);
            MemoryStream mem = new MemoryStream(data);
            picBoxTB.Image = Image.FromStream(mem);
        }

        public void LayThongTinHoiVien()
        {
            hoiVienDTO.HoTen = txtHoTen.Text;
            hoiVienDTO.GioiTinh = cmbGioiTinh.Text;
            hoiVienDTO.SDT = txtSDT.Text;
            hoiVienDTO.GoiTap = cmbGoiTap.Text;
            if (isAnotherImage)
                hoiVienDTO.HinhAnh = ImageToByteArray(picBoxHV.ImageLocation);
            else
                hoiVienDTO.HinhAnh = (Byte[])dtgvHoiVien.CurrentRow.Cells["hinhanh"].Value;
        }

        // Events
        private void Index_Load(object sender, EventArgs e)
        {
            loadHoiVien();
            loadSanPham();
            loadThietBi();
        }

        private void tabCtrl_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            TabPage _tabPage = tabCtrl.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = tabCtrl.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {
                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.White);
                g.FillRectangle(Brushes.DarkCyan, e.Bounds);
            }
            else
            {
                _textBrush = new SolidBrush(e.ForeColor);
                //e.DrawBackground();
                //_textBrush = new SolidBrush(Color.White);
                g.FillRectangle(Brushes.LightBlue, e.Bounds);
            }

            // Use our own font.
            Font _tabFont = new Font("Arial", (float)13.0, FontStyle.Regular, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Near;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));

            // Draw image if available
            int indent = 3;
            Rectangle rect = new Rectangle(e.Bounds.X, e.Bounds.Y + indent, e.Bounds.Width, e.Bounds.Height - indent);
            if (tabCtrl.TabPages[e.Index].ImageIndex >= 0)
            {
                Image img = tabCtrl.ImageList.Images[tabCtrl.TabPages[e.Index].ImageIndex];
                float _x = (rect.X + rect.Width) - img.Width - 2 * indent;
                float _y = ((rect.Height - img.Height) / 2.0f) + rect.Y;
                e.Graphics.DrawImage(img, _x, _y);
            }
        }

        private void btn_themhv_Click(object sender, EventArgs e)
        {
            DataGridViewRow lastRow = dtgvHoiVien.Rows[dtgvHoiVien.Rows.Count - 1];
            string lastRowID = lastRow.Cells["id_hv"].Value.ToString();
            fThemHV fadd = new fThemHV(lastRowID);
            fadd.ShowDialog();
            loadHoiVien();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Chấp nhận xóa dữ liệu ?", "Thông báo", MessageBoxButtons.OKCancel)
                != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                getCurrentRowHVInfo();
                hoiVienCTL.HoiVien = hoiVienDTO;
                hoiVienCTL.delete();

                MessageBox.Show("Xóa THÀNH CÔNG!");
                loadHoiVien();
            }
            catch (Exception)
            {
                MessageBox.Show("Xóa THẤT BẠI!");
            }
        }

        private void btn_giahanhv_Click(object sender, EventArgs e)
        {
            DataGridViewRow curRow = dtgvHoiVien.CurrentRow;
            fGiaHanHV fGiaHan = new fGiaHanHV(curRow);
            fGiaHan.ShowDialog();
            loadHoiVien();
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

        private void btnSua_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dtgvHoiVien.CurrentRow;
            getHVRowToTxtBOX(row);
            txtHoTen.ReadOnly = false;
            txtSDT.ReadOnly = false;
            cmbGioiTinh.Enabled = true;
            btnLuuHV.Enabled = true;
        }

        private void txtSDT_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dtgvHoiVien.CurrentRow;
            try
            {
                LayThongTinHoiVien();
                hoiVienDTO.ID_HV = row.Cells["id_hv"].Value.ToString();
                hoiVienDTO.NgayHetHan = (DateTime)row.Cells["ngayhethan"].Value;
                hoiVienCTL.HoiVien = hoiVienDTO;
                hoiVienCTL.update();

                MessageBox.Show("Sửa THÀNH CÔNG!");

                // Reload thong tin hoi vien
                loadHoiVien();

                btnLuuHV.Enabled = false;
                txtHoTen.ReadOnly = true;
                txtSDT.ReadOnly = true;
                cmbGioiTinh.Enabled = false;
            }
            catch (Exception)
            {
                MessageBox.Show("Sửa THẤT BẠI!");
            }
        }

        private void dtgvHoiVien_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            btnGiaHanHV.Enabled = true;
            DataGridViewRow row = dtgvHoiVien.CurrentRow;
            getHVRowToTxtBOX(row);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            loadHoiVien(txtSearchHV.Text);
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                loadHoiVien(txtSearchHV.Text);
        }

        private void dtgvSanPham_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dtgvSanPham.CurrentRow;
            getSPRowToTxtBOX(row);
        }

        private void btnThemSP_Click(object sender, EventArgs e)
        {
            DataGridViewRow lastRow = dtgvSanPham.Rows[dtgvSanPham.Rows.Count - 1];
            string lastRowID = lastRow.Cells["id_sp"].Value.ToString();
            fThemSP fadd = new fThemSP(lastRowID);
            fadd.ShowDialog();
            loadSanPham();
        }

        private void btnXoaSP_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Chấp nhận xóa dữ liệu ?", "Thông báo", MessageBoxButtons.OKCancel)
                != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                getCurrentRowSPInfo();
                sanPhamCTL.SanPham = sanPhamDTO;
                sanPhamCTL.delete();

                MessageBox.Show("Xóa THÀNH CÔNG!");
                loadSanPham();
            }
            catch (Exception)
            {
                MessageBox.Show("Xóa THẤT BẠI!");
            }
        }

        private void btnSuaSP_Click(object sender, EventArgs e)
        {
            DataGridViewRow curRow = dtgvSanPham.CurrentRow;
            fSuaSP fEdit = new fSuaSP(curRow);
            fEdit.ShowDialog();
            loadSanPham();
        }

        private void txtSearchSP_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                loadSanPham(txtSearchSP.Text);
        }

        private void btnSearchSP_Click(object sender, EventArgs e)
        {
            loadSanPham(txtSearchSP.Text);
        }

        private void dtgvThietBi_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dtgvThietBi.CurrentRow;
            getTBRowToTxtBOX(row);
        }

        private void btnThemTB_Click(object sender, EventArgs e)
        {
            int soMay = thietBiCTL.countTBType("Máy");
            int soTa = thietBiCTL.countTBType("Tạ");
            fThemTB fadd = new fThemTB(soMay, soTa);
            fadd.ShowDialog();
            loadThietBi();
        }

        private void btnXoaTB_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Chấp nhận xóa dữ liệu ?", "Thông báo", MessageBoxButtons.OKCancel)
                != System.Windows.Forms.DialogResult.OK)
                return;

            try
            {
                getCurrentRowTBInfo();
                thietBiCTL.ThietBi = thietBiDTO;
                thietBiCTL.delete();

                MessageBox.Show("Xóa THÀNH CÔNG!");
                loadThietBi();
            }
            catch (Exception)
            {
                MessageBox.Show("Xóa THẤT BẠI!");
            }
        }

        private void btnSuaTB_Click(object sender, EventArgs e)
        {
            DataGridViewRow curRow = dtgvThietBi.CurrentRow;
            fSuaTB fEdit = new fSuaTB(curRow);
            fEdit.ShowDialog();
            loadThietBi();
        }

        private void txtSearchTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                loadThietBi(txtSearchTB.Text);
        }

        private void btnSearchTB_Click(object sender, EventArgs e)
        {
            loadThietBi(txtSearchTB.Text);
        }

        private void picBoxHV_DoubleClick(object sender, EventArgs e)
        {
            if (btnLuuHV.Enabled == false)
                return;

            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Chọn ảnh đại diện";
                dlg.Filter = "JPG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imgLoc = dlg.FileName;
                    picBoxHV.ImageLocation = imgLoc;
                    hoiVienDTO.HinhAnh = ImageToByteArray(imgLoc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
