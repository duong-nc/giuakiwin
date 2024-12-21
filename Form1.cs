using de01.Models;
using System;
using System.Linq;
using System.Windows.Forms;

namespace de01
{
    public partial class Form1 : Form
    {
        private SvDbContext db = new SvDbContext();

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadData()
        {
            using (var db = new SvDbContext())
            {
                var data = db.Sinhviens.Select(sv => new
                {
                    sv.MaSV,
                    sv.HotenSV,
                    sv.NgaySinh,
                    TenLop = sv.Lop.TenLop
                }).ToList();

                dgvSinhvien.DataSource = data;

                // Nạp dữ liệu vào ComboBox
                cboLop.DataSource = db.Lops.ToList();
                cboLop.DisplayMember = "TenLop";
                cboLop.ValueMember = "MaLop";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
            dgvSinhvien.CellClick += dgvSinhvien_CellClick;
        }

        private void dgvSinhvien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSinhvien.Rows[e.RowIndex];
                txtMaSV.Text = row.Cells["MaSV"].Value.ToString();
                txtHotenSV.Text = row.Cells["HoTenSV"].Value.ToString();
                dtNgaysinh.Value = DateTime.Parse(row.Cells["NgaySinh"].Value.ToString());
                cboLop.Text = row.Cells["TenLop"].Value.ToString();
            }
        }

        private void txttim_TextChanged(object sender, EventArgs e)
        {
            using (var db = new SvDbContext())
            {
                string searchValue = txttim.Text.ToLower();
                var data = db.Sinhviens
                    .Where(sv => sv.HotenSV.ToLower().Contains(searchValue))
                    .Select(sv => new
                    {
                        sv.MaSV,
                        sv.HotenSV,
                        sv.NgaySinh,
                        TenLop = sv.Lop.TenLop
                    })
                    .ToList();

                dgvSinhvien.DataSource = data;
            }
        }

        private void btntim_Click(object sender, EventArgs e)
        {
            txttim_TextChanged(sender, e);
        }

        private void btnthoat_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Bạn có chắc chắn muốn thoát không?",
                                                "Xác nhận thoát",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btThem_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input fields
                if (string.IsNullOrWhiteSpace(txtMaSV.Text) || string.IsNullOrWhiteSpace(txtHotenSV.Text) || cboLop.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin sinh viên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Create new Sinhvien object
                var newSinhvien = new Sinhvien
                {
                    MaSV = txtMaSV.Text.Trim(),
                    HotenSV = txtHotenSV.Text.Trim(),
                    NgaySinh = dtNgaysinh.Value,
                    MaLop = (string)cboLop.SelectedValue // Use the MaLop from ComboBox
                };

                // Add to the DbContext and save changes
                db.Sinhviens.Add(newSinhvien);
                db.SaveChanges();

                // Refresh DataGridView
                LoadData();
                MessageBox.Show("Thêm sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear input fields
                txtMaSV.Clear();
                txtHotenSV.Clear();
                dtNgaysinh.Value = DateTime.Now;
                cboLop.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm sinh viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btXoa_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate if a row is selected
                if (string.IsNullOrWhiteSpace(txtMaSV.Text))
                {
                    MessageBox.Show("Vui lòng chọn sinh viên cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Find the student by MaSV
                var sinhvien = db.Sinhviens.FirstOrDefault(sv => sv.MaSV == txtMaSV.Text.Trim());
                if (sinhvien != null)
                {
                    db.Sinhviens.Remove(sinhvien);
                    db.SaveChanges();
                    MessageBox.Show("Xóa sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh DataGridView
                    LoadData();

                    // Clear input fields
                    txtMaSV.Clear();
                    txtHotenSV.Clear();
                    dtNgaysinh.Value = DateTime.Now;
                    cboLop.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy sinh viên để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa sinh viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btSua_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input fields
                if (string.IsNullOrWhiteSpace(txtMaSV.Text) || string.IsNullOrWhiteSpace(txtHotenSV.Text) || cboLop.SelectedItem == null)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin sinh viên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Find the student to edit by MaSV
                var sinhvien = db.Sinhviens.FirstOrDefault(sv => sv.MaSV == txtMaSV.Text.Trim());
                if (sinhvien != null)
                {
                    // Update student data
                    sinhvien.HotenSV = txtHotenSV.Text.Trim();
                    sinhvien.NgaySinh = dtNgaysinh.Value;
                    sinhvien.MaLop = (string)cboLop.SelectedValue; // Get MaLop from ComboBox

                    db.SaveChanges();
                    MessageBox.Show("Sửa thông tin sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh DataGridView
                    LoadData();

                    // Clear input fields
                    txtMaSV.Clear();
                    txtHotenSV.Clear();
                    dtNgaysinh.Value = DateTime.Now;
                    cboLop.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Không tìm thấy sinh viên để sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi sửa sinh viên: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btLuu_Click(object sender, EventArgs e)
        {
            try
            {
                // Save changes in DataGridView to the database
                foreach (DataGridViewRow row in dgvSinhvien.Rows)
                {
                    if (row.Cells["MaSV"].Value != null)
                    {
                        string maSV = row.Cells["MaSV"].Value.ToString();
                        var sinhvien = db.Sinhviens.FirstOrDefault(sv => sv.MaSV == maSV);
                        if (sinhvien != null)
                        {
                            sinhvien.HotenSV = row.Cells["HotenSV"].Value.ToString();
                            sinhvien.NgaySinh = DateTime.Parse(row.Cells["NgaySinh"].Value.ToString());
                            sinhvien.MaLop = db.Lops.First(l => l.TenLop == row.Cells["TenLop"].Value.ToString()).MaLop;
                        }
                    }
                }
                db.SaveChanges();
                MessageBox.Show("Dữ liệu đã được lưu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btThoat_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
