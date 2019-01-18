using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace ProductCrud
{



    public partial class Form1 : Form
    {
        /////////////////////////////////////////////////////////////////
        ////////////////////// Member variables /////////////////////////
        /////////////////////////////////////////////////////////////////

        public enum SelectionItemTypes
        {
            Product = 0,
            Barcode = 1,
            Supplier = 2
        }

        private Regex numberReg = new Regex(@"^\b[0-9]+\b$");
        private Regex textReg = new Regex(@"^\b[a-zA-Z\'-]{0,50}\b$");
        private Regex doubleReg = new Regex(@"^\b[0-9]+\b(\.\b[0-9][0-9])?$");
        private Regex sanitisationReg = new Regex(@"^\b[0-9a-zA-Z\'-.]+\b$");

        private const int _INPUTCOUNT = 7;
        private const int _PKCOLUMN = 8;

        private int action = 0;

        private bool editing = false;

        private int currentEditIndex = -1;

        private string[] _names = {
            "PRO_Code", "PRO_Description", "PRO_WidthM", "PRO_HeightM",
            "PRO_SellingPrice", "PRO_CostPrice", "PRO_Department",
            "BAR_Code", "BAR_ShortDesc", "SUP_Code", "SUP_Name",
            "SUP_Description" };

        private List<Label> _userEntryHeaders = new List<Label>();
        private List<TextBox> _userEntryProducts = new List<TextBox>();
        private List<TextBox> _userEntryBarcodes = new List<TextBox>();
        private List<TextBox> _userEntrySuppliers = new List<TextBox>();
        private List<int> _IDs = new List<int>();

        private Dictionary<SelectionItemTypes, int> _indices;
        private Dictionary<int, string> _dict = new Dictionary<int, string>
        {
            {0,"0" },
            {1,"Enter Data" },
            {2,"0.00" }
        };

        private EditEnabledArgs _addProductArgs = new EditEnabledArgs
        {
            Edit = false,
            Delete = false,
            Add = false,
            Barcode = false,
            Suppliers = false,
            Confirm = true,
            Cancel = true,
            Products = true

        };
        private EditEnabledArgs _addBarcodeArgs = new EditEnabledArgs
        {
            Edit = false,
            Delete = false,
            Add = false,
            Barcode = true,
            Suppliers = false,
            Confirm = true,
            Cancel = true,
            Products = false

        };
        private EditEnabledArgs _addSupplierArgs = new EditEnabledArgs
        {
            Edit = false,
            Delete = false,
            Add = false,
            Barcode = false,
            Suppliers = true,
            Confirm = true,
            Cancel = true,
            Products = false
        };
        private EditEnabledArgs _editProductArgs = new EditEnabledArgs
        {

            Edit = false,
            Delete = false,
            Add = false,
            Barcode = false,
            Suppliers = false,
            Confirm = true,
            Cancel = true,
            Products = true
        };
        private EditEnabledArgs _editBarcodeArgs = new EditEnabledArgs
        {
            Edit = false,
            Delete = false,
            Add = false,
            Barcode = true,
            Suppliers = false,
            Confirm = true,
            Cancel = true,
            Products = false
        };
        private EditEnabledArgs _editSupplierArgs = new EditEnabledArgs
        {
            Edit = false,
            Delete = false,
            Add = false,
            Barcode = false,
            Suppliers = true,
            Confirm = true,
            Cancel = true,
            Products = false
        };
        private EditEnabledArgs _idleArgs = new EditEnabledArgs
        {
            Edit = true,
            Delete = true,
            Add = true,
            Barcode = false,
            Suppliers = false,
            Confirm = false,
            Cancel = false,
            Products = false
        };

        StreamWriter outputFile;



        /////////////////////////////////////////////////////////////////
        ///////////////////////// De/Constructors ///////////////////////
        /////////////////////////////////////////////////////////////////

        public Form1()
        {
#if DEBUG
            string mydocpath =
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            outputFile = new StreamWriter(Path.Combine(mydocpath,  DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".txt"));

#endif
            InitializeComponent();

            SetupForm();

            FitToScreen();


        }



        /////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////
        ///////////////////////// Custom Functions //////////////////////
        /////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////

        string TwoDecimalPlaces(string s)
        {
            return decimal.Round(decimal.Parse(s), 2).ToString();
        }

        void InsertDummyData(List<TextBox> ltb, int[] typeKey)
        {

            for (int i = 0; i < ltb.Count; i++)
            {
                ltb[i].Text = _dict[typeKey[i]];
            }

        }

        void ButtonPressedActions(EditEnabledArgs eea, int actionInt, string status)
        {
            EditEnabled(eea);
            action = actionInt;
            label1.Text = status;
        }

        private int? GetSelectedIndexOfType(SelectionItemTypes type)
        {

            if (_indices.ContainsKey(type))
            {
                return _indices[type];
            }

            return null;
        }

        void StoreIndices()
        {

            _indices = new Dictionary<SelectionItemTypes, int>();

            IndexAssign(SelectionItemTypes.Product, dataGridView1);
            IndexAssign(SelectionItemTypes.Barcode, dataGridView2);
            IndexAssign(SelectionItemTypes.Supplier, dataGridView3);

        }

        void IndexAssign(SelectionItemTypes sit, DataGridView dgv)
        {

            if (dgv.SelectedRows.Count > 0)
            {
                _indices.Add(sit, dgv.SelectedRows[0].Index);
            }
        }

        void PopulateAllTables()
        { 

            PopulateTable("SELECT BAR_Code, BAR_ShortDesc, PRO_PK, BAR_PK FROM Barcodes WHERE PRO_PK = ",
                dataGridView2);
            dataGridView2.Columns[2].Visible = false;
            dataGridView2.Columns[3].Visible = false;

            PopulateTable("SELECT SUP_Code, SUP_Name, SUP_Description, PRO_PK, SUP_PK FROM Suppliers WHERE PRO_PK = ",
                dataGridView3);
            dataGridView3.Columns[3].Visible = false;
            dataGridView3.Columns[4].Visible = false;
        }

        void PopulateTable(string query, DataGridView dgv)
        {

            try
            {
                var select = query + dataGridView1.SelectedRows[0].Cells[_PKCOLUMN].Value;
                var c = new SqlConnection(@"Server=.\SQLEXPRESS;Database=BirthdaySystem;Trusted_Connection=true"); // Your Connection String here
                var dataAdapter = new SqlDataAdapter(select, c);

                var commandBuilder = new SqlCommandBuilder(dataAdapter);
                var ds = new DataSet();
                dataAdapter.Fill(ds);
                dgv.ReadOnly = true;
                dgv.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {

            }
        }

        void RefreshTables()
        {

            productsTableAdapter.Fill(birthdaySystemDataSet3.Products);
            barcodesTableAdapter.Fill(birthdaySystemDataSet2.Barcodes);
            suppliersTableAdapter.Fill(birthdaySystemDataSet1.Suppliers);
        }

        void SetupForm()
        { 

            //add to arrays
            {

                _userEntryHeaders.Add(label2);
                _userEntryHeaders.Add(label3);

                _userEntryHeaders.Add(label4);
                _userEntryHeaders.Add(label5);

                _userEntryHeaders.Add(label6);
                _userEntryHeaders.Add(label7);

                _userEntryHeaders.Add(label8);
                _userEntryHeaders.Add(label12);

                _userEntryHeaders.Add(label13);
                _userEntryHeaders.Add(label14);

                _userEntryHeaders.Add(label15);
                _userEntryHeaders.Add(label16);

                _userEntryProducts.Add(textBox1);
                _userEntryProducts.Add(textBox2);

                _userEntryProducts.Add(textBox3);
                _userEntryProducts.Add(textBox4);

                _userEntryProducts.Add(textBox5);
                _userEntryProducts.Add(textBox6);

                _userEntryBarcodes.Add(textBox7);
                _userEntryBarcodes.Add(textBox8);

                _userEntrySuppliers.Add(textBox9);
                _userEntrySuppliers.Add(textBox10);

                _userEntrySuppliers.Add(textBox11);


            }

            //set text, disable
            {
                for (int i = 0; i < _userEntryHeaders.Count; i++)
                {
                     _userEntryHeaders[i].Text = _names[i];
                }
                for (int i = 0; i < _userEntryProducts.Count; i++)
                {
                    _userEntryProducts[i].Enabled = false;
                }
                for (int i = 0; i < _userEntryBarcodes.Count; i++)
                {
                    _userEntryBarcodes[i].Enabled = false;
                }
                for (int i = 0; i < _userEntrySuppliers.Count; i++)
                {
                    _userEntrySuppliers[i].Enabled = false;
                }

                departmentsCombo.Enabled = false;

            }

            //combobox1 populate
            {
                SQLClass net = new SQLClass("SELECT DEP_Name FROM Departments", false);
                List<string> read = net.Read();

                for (int i = 0; i < read.Count; i++)
                {
                    Label temp1 = new Label
                    {
                        Name = "lb" + i.ToString(),
                        Text = read[i]
                    };

                    departmentsCombo.Items.Add(temp1);
                    departmentsCombo.DisplayMember = "Text";
                }
            }


        }

        bool ConfimationBox(string s)
        {


            DialogResult dialogResult = MessageBox.Show(s, "", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {

                return true;

            }
            return false;
        }

        private void TrySelectRow(DataGridView dgv, int? index)
        {

            if (index != null)
            {
                dgv.Rows[index.Value].Selected = true;
            }
            else
            {
                dgv.SelectedRows.Clear();
            }
        }

        /////////////////////////////////////////////////////////////////
        //////////////////// Populate Table Functions ///////////////////
        /////////////////////////////////////////////////////////////////

        string CreateAddProductQueryString()
        {
            SetUserInputBackgroundWhite();
            if (!SanitiseUserInput(_userEntryProducts[0], 0)) return null;
            if (!SanitiseUserInput(_userEntryProducts[1], 1)) return null;
            if (!SanitiseUserInput(_userEntryProducts[2], 2)) return null;
            if (!SanitiseUserInput(_userEntryProducts[3], 2)) return null;
            if (!SanitiseUserInput(_userEntryProducts[4], 2)) return null;
            if (!SanitiseUserInput(_userEntryProducts[5], 2)) return null;
            if (!IsProductNumberFree(textBox1.Text, textBox1)) return null;
            int inc = 0;
#if DEBUG
            outputFile.WriteLine(" added product");
            outputFile.WriteLine(DateTime.Now.ToShortTimeString() + " added product");
#endif
            return ("INSERT INTO Products(PRO_Code, PRO_Description, PRO_WidthM, PRO_HeightM, PRO_SellingPrice, PRO_CostPrice, PRO_Department) VALUES (" +
                 _userEntryProducts[inc++].Text + @", '" +//code
                 _userEntryProducts[inc++].Text + @"', " +//desc
                 _userEntryProducts[inc++].Text + ", " +//width
                 _userEntryProducts[inc++].Text + ", " +//height
                 _userEntryProducts[inc++].Text + ", " +//sell
                 _userEntryProducts[inc++].Text + @",'" +//cost
                 departmentsCombo.Text + @"')");//dept
        }

        string CreateAddBarcodeQueryString()
        {

            SetUserInputBackgroundWhite();
            if (!SanitiseUserInput(_userEntryBarcodes[0], 0)) return null;
            if (!SanitiseUserInput(_userEntryBarcodes[1], 1)) return null;
            if (!IsBarcodeFree(textBox7.Text, textBox7)) return null;

#if DEBUG
            outputFile.WriteLine(DateTime.Now + "" + "added barcode" + "(" +
                _userEntryBarcodes[0].Text + @", '" +
                _userEntryBarcodes[1].Text + @"', " +
                dataGridView1.SelectedRows[0].Cells[_PKCOLUMN].Value + ")");
#endif
            string s = "INSERT INTO Barcodes(BAR_Code, BAR_ShortDesc, PRO_PK) VALUES (" +
                _userEntryBarcodes[0].Text + @", '" +
                _userEntryBarcodes[1].Text + @"', " +
                dataGridView1.SelectedRows[0].Cells[_PKCOLUMN].Value + ")";
            return s;
        }

        string CreateAddSupplierQueryString()
        {

            SetUserInputBackgroundWhite();
            if (!SanitiseUserInput(_userEntrySuppliers[0], 0)) return null;
            if (!SanitiseUserInput(_userEntrySuppliers[1], 1)) return null;
            if (!SanitiseUserInput(_userEntrySuppliers[2], 1)) return null;
            if (!IsSupplierFree(textBox9.Text, textBox9)) return null;

#if DEBUG
            outputFile.WriteLine(DateTime.Now + "" + "added supplier" + "(" +
                _userEntrySuppliers[0].Text + @", '" +
                _userEntrySuppliers[1].Text + @"', '" +
                _userEntrySuppliers[2].Text + @"', " +
                dataGridView1.SelectedRows[0].Cells[_PKCOLUMN].Value + ")");
#endif

            string s = "INSERT INTO Suppliers(SUP_Code, SUP_Name, SUP_Description, PRO_PK) VALUES (" +
                _userEntrySuppliers[0].Text + @", '" +
                _userEntrySuppliers[1].Text + @"', '" +
                _userEntrySuppliers[2].Text + @"', " +
                dataGridView1.SelectedRows[0].Cells[_PKCOLUMN].Value + ")";
            return s;
        }

        string CreateUpdateProductQueryString()
        { 
            SetUserInputBackgroundWhite();
            if (!SanitiseUserInput(_userEntryProducts[0], 0)) return null;
            if (!SanitiseUserInput(_userEntryProducts[1], 1)) return null;
            if (!SanitiseUserInput(_userEntryProducts[2], 2)) return null;
            if (!SanitiseUserInput(_userEntryProducts[3], 2)) return null;
            if (!SanitiseUserInput(_userEntryProducts[4], 2)) return null;
            if (!SanitiseUserInput(_userEntryProducts[5], 2)) return null;
            if (!IsProductNumberFree(textBox1.Text, textBox1)) return null;
            int inc = 0;
#if DEBUG
            outputFile.WriteLine(DateTime.Now + "" + "updated product where PK is " + dataGridView1.Rows[currentEditIndex].Cells[_PKCOLUMN].Value);
#endif
            return ("UPDATE Products SET " +
                "PRO_Code = " + _userEntryProducts[inc++].Text +
                @", PRO_Description = '" + _userEntryProducts[inc++].Text +
                @"', PRO_WidthM = " + _userEntryProducts[inc++].Text +
                ", PRO_HeightM = " + _userEntryProducts[inc++].Text +
                ", PRO_SellingPrice = " + _userEntryProducts[inc++].Text +
                ", PRO_CostPrice = " + _userEntryProducts[inc++].Text +
                @", PRO_Department = '" + departmentsCombo.Text +
                @"' WHERE PRO_PK = " + dataGridView1.Rows[currentEditIndex].Cells[_PKCOLUMN].Value);
        }

        string CreateUpdateBarcodeQueryString()
        {

            SetUserInputBackgroundWhite();
            if (!SanitiseUserInput(_userEntryBarcodes[0], 0)) return null;
            if (!SanitiseUserInput(_userEntryBarcodes[1], 1)) return null;
            if (!IsBarcodeFree(textBox7.Text, textBox7)) return null;
            int inc = 0;

#if DEBUG
            outputFile.WriteLine(DateTime.Now + "" + "updated barcode where PK is " + dataGridView2.Rows[currentEditIndex].Cells[3].Value);
#endif
            string s =
            "UPDATE Barcodes SET " +
                "BAR_Code = " + _userEntryBarcodes[inc++].Text +
                @", BAR_shortDesc = '" + _userEntryBarcodes[inc++].Text +
                @"', PRO_PK = " + dataGridView2.Rows[currentEditIndex].Cells[2].Value +
                @" WHERE BAR_PK = " + dataGridView2.Rows[currentEditIndex].Cells[3].Value;
            return s;
        }

        string CreateUpdateSupplierQueryString()
        {

            SetUserInputBackgroundWhite();
            if (!SanitiseUserInput(_userEntrySuppliers[0], 0)) return null;
            if (!SanitiseUserInput(_userEntrySuppliers[1], 1)) return null;
            if (!IsSupplierFree(textBox9.Text, textBox9)) return null;
            int inc = 0;

#if DEBUG
            outputFile.WriteLine(DateTime.Now + "" + "updated supplier where PK is " + dataGridView3.Rows[currentEditIndex].Cells[4].Value);
#endif
            string s =
            "UPDATE Suppliers SET " +
                "SUP_Code = " + _userEntrySuppliers[inc++].Text +
                @", SUP_Name = '" + _userEntrySuppliers[inc++].Text +
                @"', SUP_Description = '" + _userEntrySuppliers[inc++].Text +
                @"', PRO_PK = " + dataGridView3.Rows[currentEditIndex].Cells[3].Value +
                @" WHERE SUP_PK = " + dataGridView3.Rows[currentEditIndex].Cells[4].Value;
            return s;
        }

        /////////////////////////////////////////////////////////////////
        ////////////// Data validation & form control ///////////////////
        /////////////////////////////////////////////////////////////////

        void EditEnabled(EditEnabledArgs args)
        {

            button1.Enabled = args.Edit;
            button10.Enabled = args.Edit;
            button11.Enabled = args.Edit;

            button2.Enabled = args.Delete;
            button8.Enabled = args.Delete;
            button9.Enabled = args.Delete;

            button3.Enabled = args.Add;
            button6.Enabled = args.Add;
            button7.Enabled = args.Add;


            button4.Enabled = args.Confirm;
            button5.Enabled = args.Cancel;

            for (int i = 0; i < _userEntryProducts.Count; i++)
            {
                _userEntryProducts[i].Enabled = args.Products;
            }

            departmentsCombo.Enabled = args.Products;
            textBox7.Enabled = args.Barcode;
            textBox8.Enabled = args.Barcode;
            textBox9.Enabled = args.Suppliers;
            textBox10.Enabled = args.Suppliers;
            textBox11.Enabled = args.Suppliers;

        }

        //checks DB for other products with the same product ID
        bool IsProductNumberFree(string s, TextBox t)
        {

            SQLClass net = new SQLClass((@"SELECT PRO_Code FROM Products WHERE PRO_Code = '" + s + @"'"), false);
            List<string> read = net.Read();
            if (read.Count == 0)
            {

                return true;

            }

            else
            {
                if (dataGridView1.SelectedRows[0].Cells[0].Value.ToString() == s && editing == true)
                {
                    return true;
                }
            }
            InvalidInput(t, "Product ID already in use");
            return false;
        }

        bool IsBarcodeFree(string s, TextBox t)
        {

            SQLClass net = new SQLClass((@"SELECT BAR_Code FROM Barcodes WHERE BAR_Code = '" + s + @"'"), false);
            List<string> read = net.Read();
            if (read.Count == 0)
            {

                return true;

            }

            else
            {
                if (dataGridView2.SelectedRows[0].Cells[0].Value.ToString() == s && editing == true)
                {
                    return true;
                }
            }
            InvalidInput(t, "Barcode already in use");
            return false;
        }

        bool IsSupplierFree(string s, TextBox t)
        {

            SQLClass net = new SQLClass((@"SELECT SUP_Code FROM Suppliers WHERE SUP_Code = '" + s + @"'"), false);
            List<string> read = net.Read();
            if (read.Count == 0)
            {

                return true;

            }

            else
            {
                if (dataGridView3.SelectedRows[0].Cells[0].Value.ToString() == s && editing == true)
                {
                    return true;
                }
            }
            InvalidInput(t, "Supplier code already in use");
            return false;
        }

        //quick and dirty, are all characters valid and contents exist
        bool ValidateInput(string s)
        {

            return (sanitisationReg.Matches(s).Count > 0);

        }

        //sanitise input specific to a given regex
        private bool CustomSanitise(string s, Regex r)
        {

            if (r.Matches(s).Count > 0)
            {
                return true;
            }
            return false;
        }

        //focusses and colours a textbox red if the data is invalid
        void InvalidInput(TextBox t, string s)
        {

            MessageBox.Show(s);
            t.BackColor = Color.Red;
            t.Focus();
        }

        //adjust objects with screen scale
        private void FitToScreen()
        {

            dataGridView1.Height = (int)(Height * 0.50);
            dataGridView1.Width = (int)(Width * 0.63);
            dataGridView1.Location = new Point(5, Height - dataGridView1.Height - 45);

            dataGridView2.Height = dataGridView1.Height / 2 - 5;
            dataGridView2.Width = (int)(Width * 0.33);
            dataGridView2.Location = new Point(dataGridView1.Width + 10, dataGridView1.Location.Y);

            dataGridView3.Height = dataGridView1.Height - dataGridView2.Height;
            dataGridView3.Width = (int)(Width * 0.33);
            dataGridView3.Location = new Point(dataGridView1.Width + 10, dataGridView2.Location.Y + dataGridView2.Height);


            button2.Location = new Point((int)(Width * 0.98) - button3.Width - 10, 10);
            button8.Location = new Point(button2.Location.X, button2.Location.Y + button2.Height + 5);
            button9.Location = new Point(button2.Location.X, button8.Location.Y + button8.Height + 5);

            button1.Location = new Point(button2.Location.X - button2.Width - 5, button2.Location.Y);
            button6.Location = new Point(button1.Location.X, button1.Location.Y + button1.Height + 5);
            button7.Location = new Point(button6.Location.X, button6.Location.Y + button6.Height + 5);

            button3.Location = new Point(button1.Location.X - button1.Width - 5, button1.Location.Y);
            button10.Location = new Point(button3.Location.X, button3.Location.Y + button3.Height + 5);
            button11.Location = new Point(button10.Location.X, button10.Location.Y + button10.Height + 5);

            button4.Location = new Point(button2.Location.X, dataGridView1.Location.Y - button4.Height - 5);
            button5.Location = new Point(button2.Location.X, button4.Location.Y - button5.Height - 5);


            label1.Location = new Point(dataGridView1.Location.X, dataGridView1.Location.Y - label1.Height - 5);

        }

        //santisise for insert
        private bool SanitiseUserInput(TextBox t, int i)
        {

            string tempText = t.Text;
            if (!ValidateInput(tempText))
            {

                InvalidInput(t, "No input");
                return false;

            }
            switch (i)
            {
                case 0:
                    if (CustomSanitise(tempText, numberReg) == false)
                    {
                        InvalidInput(t, "Invalid Input");
                        return false;
                    }
                    break;
                case 1:
                    if (CustomSanitise(tempText, textReg) == false)
                    {
                        InvalidInput(t, "Invalid Input");
                        return false;
                    }
                    break;
                case 2:
                    if (CustomSanitise(tempText, doubleReg) == false)
                    {
                        InvalidInput(t, "Invalid Input");
                        return false;
                    }
                    break;


            }
            return true;
        }

        //clear background on submission so we can recolour if input is (still) not valid
        private void SetUserInputBackgroundWhite()
        {

            for (int i = 0; i < _userEntryProducts.Count; i++)
            {
                _userEntryProducts[i].BackColor = Color.White;
            }
            for (int i = 0; i < _userEntryBarcodes.Count; i++)
            {
                _userEntryBarcodes[i].BackColor = Color.White;
            }
            for (int i = 0; i < _userEntrySuppliers.Count; i++)
            {
                _userEntrySuppliers[i].BackColor = Color.White;
            }
        }


        /////////////////////////////////////////////////////////////////
        ///////////////// On form action functions //////////////////////
        /////////////////////////////////////////////////////////////////

        //when form loaded
        private void Form1_Load(object sender, EventArgs e)
        {

            productsTableAdapter.Fill(birthdaySystemDataSet3.Products);
            barcodesTableAdapter.Fill(birthdaySystemDataSet2.Barcodes);
            suppliersTableAdapter.Fill(birthdaySystemDataSet1.Suppliers);

            string query1 = @"SELECT DEP_Description FROM Departments WHERE DEP_Name = '" + departmentsCombo.Text + @"'";
            SQLClass net1 = new SQLClass(query1, false);
            List<string> read1 = net1.Read();
            //label10.Text = read1[0];

        }

        //adjust window size and scale objects positions and sizes appropriately
        private void Resize_Window(object sender, EventArgs e)
        {
            FitToScreen();
        }

        //Add product Button
        private void Button1_Click(object sender, EventArgs e)
        {

            ButtonPressedActions(_addProductArgs, 1, "You are currently Adding a new Product");

            int[] temp = { 0, 1, 2, 2, 2, 2 };
            InsertDummyData(_userEntryProducts, temp);

        }

        //Delete product Button
        private void Button2_Click(object sender, EventArgs e)
        {

            //if a something is selected
            if (dataGridView1.SelectedCells.Count > 0)
            {

                if (ConfimationBox("Are you sure you wish to delete?"))
                {

                    SQLClass net = new SQLClass("DELETE FROM Products WHERE PRO_PK = " +
                        dataGridView1.SelectedRows[0].Cells[_PKCOLUMN].Value, true);
#if DEBUG
                    outputFile.WriteLine(DateTime.Now + "" + "Deleted Product where PK was " + dataGridView1.SelectedRows[0].Cells[_PKCOLUMN].Value);
#endif

                    RefreshTables();


                    editing = false;
                }


            }

        }

        //Edit product Button
        private void Button3_Click(object sender, EventArgs e)
        {
            editing = true;
            EditEnabled(_editProductArgs);
            action = 2;
            label1.Text = "You are currently Editing a Product";
            currentEditIndex = dataGridView1.SelectedRows[0].Index;
        }

        //Confirm button, locked behind edit
        private void Button4_Click(object sender, EventArgs e)
        {

            string query = null;
            switch (action)
            {
                case 1:
                    query = CreateAddProductQueryString();
                    break;
                case 2:
                    query = CreateUpdateProductQueryString();
                    break;
                case 3:
                    query = CreateAddBarcodeQueryString();
                    break;
                case 4:
                    query = CreateAddSupplierQueryString();
                    break;
                case 5:
                    query = CreateUpdateBarcodeQueryString();
                    break;
                case 6:
                    query = CreateUpdateSupplierQueryString();
                    break;
            }
            if (query != null)
            {
                if (ConfimationBox("Submit Change?"))
                {
                    SQLClass net = new SQLClass(query, true);

                    SetUserInputBackgroundWhite();

                    action = 0;

                    EditEnabled(_idleArgs);

                    StoreIndices();

                    RefreshTables();


                    TrySelectRow(dataGridView1, GetSelectedIndexOfType(SelectionItemTypes.Barcode).Value);
                    dataGridView2.Rows[_indices[SelectionItemTypes.Barcode]].Selected = true;
                    dataGridView3.Rows[_indices[SelectionItemTypes.Supplier]].Selected = true;

                    label1.Text = "You are currently doing nothing";
                    editing = false;
                    currentEditIndex = -1;

                }
            }

        }

        //Cancel Button, locked behind edit
        private void Button5_Click(object sender, EventArgs e)
        {

            if (ConfimationBox("Cancel?"))
            {
                SetUserInputBackgroundWhite();
                EditEnabled(_idleArgs);
                label1.Text = "You are currently doing nothing";
                editing = false;
                currentEditIndex = -1;
            }
        }

        //add barcode button
        private void Button6_Click(object sender, EventArgs e)
        {

            ButtonPressedActions(_addBarcodeArgs, 3, "You are currently Adding a new Barcode");

            int[] temp = { 0, 1 };
            InsertDummyData(_userEntryBarcodes, temp);
        }

        //add supplier button
        private void Button7_Click(object sender, EventArgs e)
        {

            ButtonPressedActions(_addSupplierArgs, 4, "You are currently Adding a new Supplier");

            int[] temp = { 0, 1, 1 };
            InsertDummyData(_userEntrySuppliers, temp);
        }

        //delete barcode button
        private void Button8_Click(object sender, EventArgs e)
        {

            //if a something is selected
            if (dataGridView2.SelectedCells.Count > 0)
            {

                if (ConfimationBox("Are you sure you wish to delete?"))
                {
                    string q1 = "DELETE FROM Barcodes WHERE BAR_Code = " + dataGridView2.SelectedRows[0].Cells[0].Value;
                    SQLClass net1 = new SQLClass(q1, true);
#if DEBUG
                    outputFile.WriteLine(DateTime.Now + "" + "Deleted Barcode where PK was " + dataGridView2.SelectedRows[0].Cells[3].Value);
#endif
                    StoreIndices();
                    RefreshTables();
                    dataGridView1.Rows[_indices[SelectionItemTypes.Product]].Selected = true;
                    dataGridView3.Rows[_indices[SelectionItemTypes.Supplier]].Selected = true;
                    editing = false;
                }


            }
        }

        //delete supplier button
        private void Button9_Click(object sender, EventArgs e)
        {

            //if a something is selected
            if (dataGridView3.SelectedCells.Count > 0)
            {

                if (ConfimationBox("Are you sure you wish to delete?"))
                {
                    string q1 = "DELETE FROM Suppliers WHERE SUP_Code = " + dataGridView3.SelectedRows[0].Cells[0].Value;
                    SQLClass net1 = new SQLClass(q1, true);
#if DEBUG
                    outputFile.WriteLine(DateTime.Now + "" + "Deleted Supplier where PK was " + dataGridView3.SelectedRows[0].Cells[4].Value);
#endif
                    StoreIndices();
                    RefreshTables();
                    dataGridView1.Rows[_indices[SelectionItemTypes.Product]].Selected = true;
                    dataGridView2.Rows[_indices[SelectionItemTypes.Barcode]].Selected = true;
                    editing = false;
                }


            }
        }

        //edit barcode button
        private void Button10_Click(object sender, EventArgs e)
        {

            editing = true;
            EditEnabled(_editBarcodeArgs);
            action = 5;
            label1.Text = "You are currently Editing a Barcode";
            currentEditIndex = dataGridView2.SelectedRows[0].Index;
        }

        //edit supplier button
        private void Button11_Click(object sender, EventArgs e)
        {

            editing = true;
            EditEnabled(_editSupplierArgs);
            action = 6;
            label1.Text = "You are currently Editing a Supplier";
            currentEditIndex = dataGridView3.SelectedRows[0].Index;
        }

        private void ComboBox1_TextChanged(object sender, EventArgs e)
        {

            try
            {
                string query = @"SELECT DEP_Description FROM Departments WHERE DEP_Name = '" + departmentsCombo.Text + @"'";
                SQLClass net = new SQLClass(query, false);
                List<string> read = net.Read();
                label10.Text = read[0];
            }
            catch (Exception ex)
            {

            }
        }

        private void DataGridView1_RowStateChanged(object sender, EventArgs e)
        {

            if (!editing)
            {
                for (int i = 0; i < _INPUTCOUNT; i++)
                {
                    try
                    {
                        if (i < 6)
                        {
                            _userEntryProducts[i].Text = dataGridView1.SelectedRows[0].Cells[i].Value.ToString();
                        }

                        else if (i == 6)
                        {
                            departmentsCombo.Text = dataGridView1.SelectedRows[0].Cells[i + 1].Value.ToString();
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                }
                try
                {
                    _userEntryProducts[4].Text = TwoDecimalPlaces(_userEntryProducts[4].Text);
                    _userEntryProducts[5].Text = TwoDecimalPlaces(_userEntryProducts[5].Text);

                    departmentsCombo.Text = dataGridView1.SelectedRows[0].Cells[7].Value.ToString();

                    PopulateAllTables();



                }
                catch (Exception ex)
                {

                }

            }
        }

        private void DataGridView2_RowStateChanged(object sender, EventArgs e)
        {

            if (!editing)
            {
                try
                {
                    _userEntryBarcodes[0].Text = dataGridView2.SelectedRows[0].Cells[0].Value.ToString();
                    _userEntryBarcodes[1].Text = dataGridView2.SelectedRows[0].Cells[1].Value.ToString();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void DataGridView3_RowStateChanged(object sender, EventArgs e)
        {

            if (!editing)
            {
                try
                {
                    _userEntrySuppliers[0].Text = dataGridView3.SelectedRows[0].Cells[0].Value.ToString();
                    _userEntrySuppliers[1].Text = dataGridView3.SelectedRows[0].Cells[1].Value.ToString();
                    _userEntrySuppliers[2].Text = dataGridView3.SelectedRows[0].Cells[2].Value.ToString();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
#if DEBUG
            outputFile.Close();
#endif
        }
    }



}


