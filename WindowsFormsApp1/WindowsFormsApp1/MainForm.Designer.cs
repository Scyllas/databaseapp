namespace WindowsFormsApp1
{
    partial class MainForm
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
            this.viewButton = new System.Windows.Forms.Button();
            this.entryButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // viewButton
            // 
            this.viewButton.Location = new System.Drawing.Point(233, 186);
            this.viewButton.Name = "viewButton";
            this.viewButton.Size = new System.Drawing.Size(128, 23);
            this.viewButton.TabIndex = 0;
            this.viewButton.Text = "View Birthdays";
            this.viewButton.UseVisualStyleBackColor = true;
            this.viewButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // entryButton
            // 
            this.entryButton.Location = new System.Drawing.Point(438, 185);
            this.entryButton.Name = "entryButton";
            this.entryButton.Size = new System.Drawing.Size(106, 23);
            this.entryButton.TabIndex = 1;
            this.entryButton.Text = "Add Birthdays";
            this.entryButton.UseVisualStyleBackColor = true;
            this.entryButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.entryButton);
            this.Controls.Add(this.viewButton);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button viewButton;
        private System.Windows.Forms.Button entryButton;
    }
}