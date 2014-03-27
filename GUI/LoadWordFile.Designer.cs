namespace TextEditor.GUI
{
    partial class LoadWordFile
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private global::System.ComponentModel.IContainer components = null;

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
            System.Windows.Forms.Label label2;
            this.TopictextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.LoadFileButton = new System.Windows.Forms.Button();
            this.WordFileTextBox = new System.Windows.Forms.TextBox();
            this.UploadWordFile = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TopicComboBox = new System.Windows.Forms.ComboBox();
            this.topicsCheckBox = new System.Windows.Forms.CheckBox();
            this.Ok = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(74, 81);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(18, 13);
            label2.TabIndex = 9;
            label2.Text = "Or";
            // 
            // TopictextBox
            // 
            this.TopictextBox.Location = new System.Drawing.Point(198, 110);
            this.TopictextBox.Name = "TopictextBox";
            this.TopictextBox.Size = new System.Drawing.Size(348, 20);
            this.TopictextBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.label1.Location = new System.Drawing.Point(34, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 19);
            this.label1.TabIndex = 1;
            this.label1.Text = "Add new topic";
            // 
            // LoadFileButton
            // 
            this.LoadFileButton.Location = new System.Drawing.Point(562, 176);
            this.LoadFileButton.Name = "LoadFileButton";
            this.LoadFileButton.Size = new System.Drawing.Size(36, 21);
            this.LoadFileButton.TabIndex = 2;
            this.LoadFileButton.Text = "...";
            this.LoadFileButton.UseVisualStyleBackColor = true;
            this.LoadFileButton.Click += new System.EventHandler(this.LoadFile_Click);
            // 
            // WordFileTextBox
            // 
            this.WordFileTextBox.Location = new System.Drawing.Point(198, 176);
            this.WordFileTextBox.Name = "WordFileTextBox";
            this.WordFileTextBox.ReadOnly = true;
            this.WordFileTextBox.Size = new System.Drawing.Size(348, 20);
            this.WordFileTextBox.TabIndex = 3;
            // 
            // UploadWordFile
            // 
            this.UploadWordFile.AutoSize = true;
            this.UploadWordFile.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.UploadWordFile.Location = new System.Drawing.Point(34, 175);
            this.UploadWordFile.Name = "UploadWordFile";
            this.UploadWordFile.Size = new System.Drawing.Size(117, 19);
            this.UploadWordFile.TabIndex = 4;
            this.UploadWordFile.Text = "UploadWordFile";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(label2);
            this.panel1.Controls.Add(this.TopicComboBox);
            this.panel1.Controls.Add(this.topicsCheckBox);
            this.panel1.Controls.Add(this.Ok);
            this.panel1.Controls.Add(this.Cancel);
            this.panel1.Controls.Add(this.UploadWordFile);
            this.panel1.Controls.Add(this.WordFileTextBox);
            this.panel1.Controls.Add(this.LoadFileButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.TopictextBox);
            this.panel1.Location = new System.Drawing.Point(1, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(627, 278);
            this.panel1.TabIndex = 0;
            // 
            // TopicComboBox
            // 
            this.TopicComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TopicComboBox.Location = new System.Drawing.Point(198, 49);
            this.TopicComboBox.Name = "TopicComboBox";
            this.TopicComboBox.Size = new System.Drawing.Size(348, 21);
            this.TopicComboBox.TabIndex = 8;
            // 
            // topicsCheckBox
            // 
            this.topicsCheckBox.AutoSize = true;
            this.topicsCheckBox.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.topicsCheckBox.Location = new System.Drawing.Point(20, 47);
            this.topicsCheckBox.Name = "topicsCheckBox";
            this.topicsCheckBox.Size = new System.Drawing.Size(170, 23);
            this.topicsCheckBox.TabIndex = 7;
            this.topicsCheckBox.Text = "Choose existing topic";
            this.topicsCheckBox.UseVisualStyleBackColor = true;
            this.topicsCheckBox.CheckedChanged += new System.EventHandler(this.topicsList_CheckedChanged);
            // 
            // Ok
            // 
            this.Ok.Location = new System.Drawing.Point(353, 226);
            this.Ok.Name = "Ok";
            this.Ok.Size = new System.Drawing.Size(75, 23);
            this.Ok.TabIndex = 6;
            this.Ok.Text = "Ok";
            this.Ok.UseVisualStyleBackColor = true;
            this.Ok.Click += new System.EventHandler(this.Ok_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(213, 226);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 5;
            this.Cancel.Text = "Close";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // LoadWordFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 275);
            this.Controls.Add(this.panel1);
            this.Name = "LoadWordFile";
            this.Text = "Load Word File";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private global::System.Windows.Forms.Panel panel1;
        private global::System.Windows.Forms.Label label1;
        private global::System.Windows.Forms.TextBox TopictextBox;
        private global::System.Windows.Forms.Button LoadFileButton;
        private global::System.Windows.Forms.Label UploadWordFile;
        private global::System.Windows.Forms.TextBox WordFileTextBox;
        private global::System.Windows.Forms.Button Ok;
        private global::System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.CheckBox topicsCheckBox;
        private System.Windows.Forms.ComboBox TopicComboBox;


    }
}