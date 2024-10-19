namespace QuanLyPhongGym.Pages
{
    partial class FormGiaHanHV
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLuu = new System.Windows.Forms.Button();
            this.cbb_GoiTap = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnLuu
            // 
            this.btnLuu.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLuu.Location = new System.Drawing.Point(149, 75);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(75, 23);
            this.btnLuu.TabIndex = 5;
            this.btnLuu.Text = "Gia hạn";
            this.btnLuu.UseVisualStyleBackColor = true;
            this.btnLuu.Click += new System.EventHandler(this.btn_Luu_Click);
            // 
            // cbb_GoiTap
            // 
            this.cbb_GoiTap.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbb_GoiTap.FormattingEnabled = true;
            this.cbb_GoiTap.Items.AddRange(new object[] {
            "1 tháng",
            "3 tháng",
            "Thường",
            "VIP"});
            this.cbb_GoiTap.Location = new System.Drawing.Point(85, 34);
            this.cbb_GoiTap.Name = "cbb_GoiTap";
            this.cbb_GoiTap.Size = new System.Drawing.Size(139, 24);
            this.cbb_GoiTap.TabIndex = 4;
            this.cbb_GoiTap.TextChanged += new System.EventHandler(this.cbb_GoiTap_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(19, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 16);
            this.label1.TabIndex = 3;
            this.label1.Text = "Gói tập";
            // 
            // FormGiaHanHV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(251, 124);
            this.Controls.Add(this.btnLuu);
            this.Controls.Add(this.cbb_GoiTap);
            this.Controls.Add(this.label1);
            this.Name = "FormGiaHanHV";
            this.Text = "FormGiaHanHV";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLuu;
        private System.Windows.Forms.ComboBox cbb_GoiTap;
        private System.Windows.Forms.Label label1;
    }
}