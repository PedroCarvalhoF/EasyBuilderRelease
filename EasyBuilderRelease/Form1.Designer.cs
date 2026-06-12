namespace EasyBuilderRelease
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            label1 = new Label();
            textBox1 = new TextBox();
            button1 = new Button();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            contextMenuStrip1 = new ContextMenuStrip(components);
            removerSelecionadoToolStripMenuItem = new ToolStripMenuItem();
            button2 = new Button();
            button3 = new Button();
            textBox2 = new TextBox();
            label2 = new Label();
            button4 = new Button();
            textBox3 = new TextBox();
            label3 = new Label();
            button7 = new Button();
            textBox8 = new TextBox();
            label4 = new Label();
            textBox7 = new TextBox();
            textBox6 = new TextBox();
            textBox5 = new TextBox();
            textBox4 = new TextBox();
            button5 = new Button();
            button6 = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            contextMenuStrip1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(26, 31);
            label1.Name = "label1";
            label1.Size = new Size(83, 15);
            label1.TabIndex = 0;
            label1.Text = "MAUI Android";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(26, 49);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(443, 23);
            textBox1.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new Point(475, 49);
            button1.Name = "button1";
            button1.Size = new Size(142, 23);
            button1.TabIndex = 2;
            button1.Text = "Adicionar Android";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            listView1.ContextMenuStrip = contextMenuStrip1;
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Location = new Point(26, 532);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new Size(591, 236);
            listView1.TabIndex = 3;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            listView1.KeyDown += listView1_KeyDown;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Tipo";
            columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Projeto";
            columnHeader2.Width = 250;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Saida";
            columnHeader3.Width = 150;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Status";
            columnHeader4.Width = 90;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { removerSelecionadoToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(188, 26);
            // 
            // removerSelecionadoToolStripMenuItem
            // 
            removerSelecionadoToolStripMenuItem.Name = "removerSelecionadoToolStripMenuItem";
            removerSelecionadoToolStripMenuItem.Size = new Size(187, 22);
            removerSelecionadoToolStripMenuItem.Text = "Remover selecionado";
            removerSelecionadoToolStripMenuItem.Click += removerSelecionadoToolStripMenuItem_Click;
            // 
            // button2
            // 
            button2.Location = new Point(329, 501);
            button2.Name = "button2";
            button2.Size = new Size(288, 23);
            button2.TabIndex = 4;
            button2.Text = "Release paralelo";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(475, 99);
            button3.Name = "button3";
            button3.Size = new Size(142, 23);
            button3.TabIndex = 7;
            button3.Text = "Adicionar Windows";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(26, 99);
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new Size(443, 23);
            textBox2.TabIndex = 6;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(26, 81);
            label2.Name = "label2";
            label2.Size = new Size(89, 15);
            label2.TabIndex = 5;
            label2.Text = "MAUI Windows";
            // 
            // button4
            // 
            button4.Location = new Point(475, 150);
            button4.Name = "button4";
            button4.Size = new Size(142, 23);
            button4.TabIndex = 10;
            button4.Text = "Adicionar API";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(26, 150);
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
            textBox3.Size = new Size(443, 23);
            textBox3.TabIndex = 9;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(26, 132);
            label3.Name = "label3";
            label3.Size = new Size(25, 15);
            label3.TabIndex = 8;
            label3.Text = "API";
            // 
            // button7
            // 
            button7.Location = new Point(475, 201);
            button7.Name = "button7";
            button7.Size = new Size(142, 23);
            button7.TabIndex = 16;
            button7.Text = "Adicionar Impressao";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // textBox8
            // 
            textBox8.Location = new Point(26, 201);
            textBox8.Name = "textBox8";
            textBox8.ReadOnly = true;
            textBox8.Size = new Size(443, 23);
            textBox8.TabIndex = 15;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(26, 183);
            label4.Name = "label4";
            label4.Size = new Size(107, 15);
            label4.TabIndex = 14;
            label4.Text = "Servidor Impressao";
            // 
            // textBox7
            // 
            textBox7.Dock = DockStyle.Fill;
            textBox7.Font = new Font("Consolas", 9F);
            textBox7.Location = new Point(3, 483);
            textBox7.Multiline = true;
            textBox7.Name = "textBox7";
            textBox7.ReadOnly = true;
            textBox7.ScrollBars = ScrollBars.Both;
            textBox7.Size = new Size(617, 156);
            textBox7.TabIndex = 3;
            textBox7.WordWrap = false;
            // 
            // textBox6
            // 
            textBox6.Dock = DockStyle.Fill;
            textBox6.Font = new Font("Consolas", 9F);
            textBox6.Location = new Point(3, 323);
            textBox6.Multiline = true;
            textBox6.Name = "textBox6";
            textBox6.ReadOnly = true;
            textBox6.ScrollBars = ScrollBars.Both;
            textBox6.Size = new Size(617, 154);
            textBox6.TabIndex = 2;
            textBox6.WordWrap = false;
            // 
            // textBox5
            // 
            textBox5.Dock = DockStyle.Fill;
            textBox5.Font = new Font("Consolas", 9F);
            textBox5.Location = new Point(3, 163);
            textBox5.Multiline = true;
            textBox5.Name = "textBox5";
            textBox5.ReadOnly = true;
            textBox5.ScrollBars = ScrollBars.Both;
            textBox5.Size = new Size(617, 154);
            textBox5.TabIndex = 1;
            textBox5.WordWrap = false;
            // 
            // textBox4
            // 
            textBox4.Dock = DockStyle.Fill;
            textBox4.Font = new Font("Consolas", 9F);
            textBox4.Location = new Point(3, 3);
            textBox4.Multiline = true;
            textBox4.Name = "textBox4";
            textBox4.ReadOnly = true;
            textBox4.ScrollBars = ScrollBars.Both;
            textBox4.Size = new Size(617, 154);
            textBox4.TabIndex = 0;
            textBox4.WordWrap = false;
            // 
            // button5
            // 
            button5.Location = new Point(26, 472);
            button5.Name = "button5";
            button5.Size = new Size(591, 23);
            button5.TabIndex = 12;
            button5.Text = "Remover selecionado";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button6
            // 
            button6.Location = new Point(26, 501);
            button6.Name = "button6";
            button6.Size = new Size(288, 23);
            button6.TabIndex = 13;
            button6.Text = "Release em fila";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(textBox4, 0, 0);
            tableLayoutPanel1.Controls.Add(textBox7, 0, 3);
            tableLayoutPanel1.Controls.Add(textBox6, 0, 2);
            tableLayoutPanel1.Controls.Add(textBox5, 0, 1);
            tableLayoutPanel1.Location = new Point(623, 49);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.Size = new Size(623, 642);
            tableLayoutPanel1.TabIndex = 17;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1252, 696);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button7);
            Controls.Add(textBox8);
            Controls.Add(label4);
            Controls.Add(button4);
            Controls.Add(textBox3);
            Controls.Add(label3);
            Controls.Add(button3);
            Controls.Add(textBox2);
            Controls.Add(label2);
            Controls.Add(button2);
            Controls.Add(listView1);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Easy Builder Release";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            contextMenuStrip1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox textBox1;
        private Button button1;
        private ListView listView1;
        private Button button2;
        private Button button3;
        private TextBox textBox2;
        private Label label2;
        private Button button4;
        private TextBox textBox3;
        private Label label3;
        private TextBox textBox7;
        private TextBox textBox6;
        private TextBox textBox5;
        private TextBox textBox4;
        private Button button5;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem removerSelecionadoToolStripMenuItem;
        private Button button6;
        private Button button7;
        private TextBox textBox8;
        private Label label4;
        private TableLayoutPanel tableLayoutPanel1;
    }
}
