using Syncfusion.Data;
using Syncfusion.Drawing;
using Syncfusion.Windows.Forms;
using Syncfusion.WinForms.DataGrid;
using Syncfusion.WinForms.DataGrid.Events;
using Syncfusion.WinForms.DataGrid.Renderers;
using Syncfusion.WinForms.DataGrid.Styles;
using Syncfusion.WinForms.GridCommon.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SfDataGridDemo
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public partial class Form1 : Form
    {
        DataTable employeeCollection;
        public Form1()
        {
            InitializeComponent();

            //To add custom renderer into SfDataGrid.
            this.sfDataGrid.CellRenderers.Add("ColorPicker", new GridColorPickeCellRenderer(this.sfDataGrid));          
                        
            //To add GridColorPickeColumn in grid
            this.sfDataGrid.Columns.Add(new GridColorPickeColumn() { HeaderText = "Color Column", MappingName = "CustomerID", Width = 140 });

            //DataTable Collection binded in SfDataGrid
            //var table = this.GetDataTable();
            //sfDataGrid.DataSource = table;

            //ObservableCollection binded in SfDataGrid
            sfDataGrid.DataSource = new ViewModel().Orders;
        }

        public DataTable GetDataTable()
        {
            employeeCollection = new DataTable();

            employeeCollection.Columns.Add("OrderID", typeof(int));
            employeeCollection.Columns.Add("CustomerID", typeof(string));
            employeeCollection.Columns.Add("CustomerName", typeof(string));
            employeeCollection.Columns.Add("Country", typeof(string));
            employeeCollection.Columns.Add("ShipCity", typeof(string));

            employeeCollection.Rows.Add(1001, "Green", "Maria Anders", "Germany", "Berlin");
            employeeCollection.Rows.Add(1002, "Red", "Ana Trujilo", "Mexico", "Mexico D.F.");
            employeeCollection.Rows.Add(1003, "Blue", "Antonio Moreno", "Mexico", "Mexico D.F.");
            employeeCollection.Rows.Add(1004, "Yellow", "Thomas Hardy", "UK", "London");
            employeeCollection.Rows.Add(1005, "White", "Christina Berglund", "Sweden", "Lula");
            employeeCollection.Rows.Add(1006, "Pink", "Hanna Moos", "Germany", "Mannheim");
            employeeCollection.Rows.Add(1007, "LightGreen", "Frederique Citeaux", "France", "Strasbourg");
            employeeCollection.Rows.Add(1008, "Silver", "Martin Sommer", "Spain", "Madrid");
            employeeCollection.Rows.Add(1009, "Brown", "Laurence Lebihan", "France", "Marseille");
            employeeCollection.Rows.Add(1010, "Gray", "Elizabeth Lincoln", "Canada", "Tsawassen");

            return employeeCollection;
        }
    }

    #region New CustomColumn
    public class GridColorPickeColumn : GridTextColumn
    {
        private CellButton cellButton;

        public CellButton CellButton
        {
            get { return cellButton; }
            set { cellButton = value; }
        }
        
        public GridColorPickeColumn()
        {
            SetCellType("ColorPicker");
        }
    }
    #endregion     
    
    #region Custom Column Cell renderer
    public class GridColorPickeCellRenderer : GridTextBoxCellRenderer
    {       
        public GridColorPickeCellRenderer(SfDataGrid dataGrid)
        {
            IsEditable = true;
            DataGrid = dataGrid;
            this.DataGrid.CellClick += DataGrid_CellClick;
            DropDownContainer = new PopupControlContainer();            
        }

        private void DataGrid_CellClick(object sender, CellClickEventArgs e)
        {
            if (dropdownContainer.Visible)
            {
                if (this.colorUI != null)
                {                   
                    colorUI.Visible = false;
                    dropdownContainer.HidePopup(PopupCloseType.Done);
                }
            }
        }
      
        protected SfDataGrid DataGrid { get; set; }

        protected override void OnRender(Graphics paint, Rectangle cellRect, string cellValue, CellStyleInfo style, DataColumnBase column, RowColumnIndex rowColumnIndex)
        {
            this.SelectedColor = ColorConvert.ColorFromString(cellValue);
            style.BackColor = SelectedColor;
            base.OnRender(paint, cellRect, cellValue, style, column, rowColumnIndex);            
            //To set the rectangle for button in the cell.
            var rect = new Rectangle(cellRect.Location.X + cellRect.Width - 22, cellRect.Location.Y, 20, cellRect.Height);

            (column.GridColumn as GridColorPickeColumn).CellButton = new CellButton();
            (column.GridColumn as GridColorPickeColumn).CellButton.Image = Image.FromFile(@"..\..\Images\icons.png");
            (column.GridColumn as GridColorPickeColumn).CellButton.TextImageRelation = TextImageRelation.ImageBeforeText;

            PropertyInfo highlightedItemProperty = (column.GridColumn as GridColorPickeColumn).CellButton.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Single(pi => pi.Name == "Bounds");
            highlightedItemProperty.SetValue((column.GridColumn as GridColorPickeColumn).CellButton, rect);

            //To draw the button in cell
            DrawButton(paint, cellRect, rect, "...", new ButtonCellStyleInfo(), column, rowColumnIndex);
        }

        protected override void OnInitializeEditElement(DataColumnBase column, RowColumnIndex rowColumnIndex, TextBox uiElement)
        {          
            base.OnInitializeEditElement(column, rowColumnIndex, uiElement);            
            uiElement.BackColor = ColorConvert.ColorFromString(uiElement.Text); ;
        }        

        public virtual void DrawButton(Graphics paint, Rectangle cellRect, Rectangle buttonRect, string cellValue, ButtonCellStyleInfo style, DataColumnBase column, Syncfusion.WinForms.GridCommon.ScrollAxis.RowColumnIndex rowColumnIndex, int buttonIndex = 0)
        {
            // No need to draw the button when its not visible on the cell bounds.
            if (cellRect.Width < 5)
                return;         

            var clipBound = paint.ClipBounds;
            var cellButton = (column.GridColumn as GridColorPickeColumn).CellButton;

            bool drawHovered = false;

            DrawBackground(paint, buttonRect, style, drawHovered,cellButton);

            if (cellRect.Contains(buttonRect))
                paint.SetClip(buttonRect);
            else if (cellRect.IntersectsWith(buttonRect))
            {
                Rectangle intersectRect = Rectangle.Intersect(cellRect, buttonRect);
                paint.SetClip(intersectRect);
            }

            if (cellButton.Image != null)
            {
                var imageSize = cellButton.Image.Size.IsEmpty ? cellButton.Image.Size : Size.Empty;
                Rectangle imageRectangle = buttonRect;
                DrawImage(paint, imageRectangle, cellButton.Image);
            }
               
            paint.SetClip(cellRect);
            DrawBorder(paint, buttonRect, style, drawHovered);
            paint.SetClip(clipBound);
        }

        private void DrawBorder(Graphics paint, Rectangle buttonRect, ButtonCellStyleInfo style, bool drawHovered)
        {
            if (style.Enabled)
            {
                if (style.BorderColor != null)
                    paint.DrawRectangle(style.BorderColor, Rectangle.Round(buttonRect));
            }
            else
            {
                if (style.DisabledBorderColor != null)
                    paint.DrawRectangle(style.DisabledBorderColor, Rectangle.Round(buttonRect));
            }
        }
       
        private void DrawBackground(Graphics paint, Rectangle buttonRect, ButtonCellStyleInfo style, bool drawHovered,CellButton cellButton)
        {
            Color color = style.BackColor;
            if (style.Enabled)
            {

                color = style.BackColor;
            }
            else
            {
                color = style.DisabledBackColor;
            }

            paint.FillRectangle(new SolidBrush(color), buttonRect);
        }

        protected internal virtual void DrawImage(Graphics graphics, Rectangle bounds, Image image)
        {
            graphics.DrawImage(image, Rectangle.Ceiling(bounds), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
        }        

        protected override void OnMouseDown(DataColumnBase dataColumn, RowColumnIndex rowColumnIndex, MouseEventArgs e)
        {
            base.OnMouseDown(dataColumn, rowColumnIndex, e);
            var cellButton = (dataColumn.GridColumn as GridColorPickeColumn).CellButton;
            PropertyInfo highlightedItemProperty = (dataColumn.GridColumn as GridColorPickeColumn).CellButton.GetType().GetProperty("Bounds",BindingFlags.NonPublic|BindingFlags.Instance);//.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Single(pi => pi.Name == "Bounds");
            Rectangle rect =(Rectangle)highlightedItemProperty.GetValue((dataColumn.GridColumn as GridColorPickeColumn).CellButton);
            var cellLocation = new Point(450, (300 + (this.DataGrid.RowHeight * (this.DataGrid.TableControl.ResolveToRecordIndex(rowColumnIndex.RowIndex)))));
            if (e.Location.X > rect.X && e.Location.X < (rect.X + rect.Width))
            {
                this.dropdownContainer.ParentControl = DataGrid.TableControl;
                DropDownContainer.Location = cellLocation;
                InitializeDropdownContainer();
                colorUI.Location = cellLocation;               
                DropDownContainer.Size = new Size(208, 230);
                DropDownContainer.FocusParent();
                DropDownContainer.ShowPopup(cellLocation);
                var cellRect = DataGrid.TableControl.GetCellRectangle(rowColumnIndex.RowIndex, rowColumnIndex.ColumnIndex, true);
                DataGrid.Invalidate(cellRect); 
            }   
        }        

        private void UpdateSummaryValues(int rowIndex, int columnIndex)
        {           
            columnIndex = this.TableControl.ResolveToGridVisibleColumnIndex(columnIndex);
            if (columnIndex < 0)
                return;
            var mappingName = DataGrid.Columns[columnIndex].MappingName;
            var recordIndex = this.TableControl.ResolveToRecordIndex(rowIndex);
            if (recordIndex < 0)
                return;
            if (DataGrid.View.TopLevelGroup != null)
            {
                var record = DataGrid.View.TopLevelGroup.DisplayElements[recordIndex];
                if (!record.IsRecords)
                    return;
                var data = (record as RecordEntry).Data;

                //below case using for DataTable Collection when dataGrid Grouped to set value
                //(data as DataRowView).Row[mappingName] = ColorConvert.ColorToString(colorUI.SelectedColor, true);

                //below case using for ObservableCollection when dataGrid Grouped to set value
                data.GetType().GetProperty(mappingName).SetValue(data, ColorConvert.ColorToString(colorUI.SelectedColor, true));
            }
            else
            {
                var record1 = DataGrid.View.Records.GetItemAt(recordIndex);

                //below case using for DataTable Collection to update based on selected value
                //(record1 as DataRowView).Row[mappingName] = ColorConvert.ColorToString(SelectedColor, true);

                //below case using for ObservableCollection to set value
                record1.GetType().GetProperty(mappingName).SetValue(record1, ColorConvert.ColorToString(SelectedColor, true));
            }
        }        

        ColorUIControl colorUI;

        private PopupControlContainer dropdownContainer = null;        

        public PopupControlContainer DropDownContainer
        { 
            get
            {               
                return dropdownContainer;
            }

            set
            {
                dropdownContainer = value;
            }
        }

        void InitializeDropdownContainer()
        {            
            if (this.dropdownContainer != null)
            {
                colorUI = new ColorUIControl();
                colorUI.Name = "ColorUIControl";
                colorUI.ColorSelected += new EventHandler(this.OnCUIColorSelected);
                colorUI.Dock = DockStyle.Fill;
                colorUI.Visible = true;               
                this.dropdownContainer.Controls.Add(colorUI);
            }
        }

        Color color;

        Color SelectedColor
        {

            get
            {
                return color;
            }

            set
            {
                color = value;
            }
        }

        void OnCUIColorSelected(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.colorUI.SelectedColor.Name))
                this.SelectedColor = this.colorUI.SelectedColor;
            colorUI.Visible = false;
            dropdownContainer.HidePopup(PopupCloseType.Done);
            UpdateSummaryValues(this.CurrentCellIndex.RowIndex, this.CurrentCellIndex.ColumnIndex);
        }
    }
    #endregion

    public class OrderInfo : INotifyPropertyChanged
    {
        decimal? orderID;
        string customerId;
        string country;
        string customerName;
        string shippingCity;
        bool isShipped;

        public OrderInfo()
        {

        }

        public decimal? OrderID
        {
            get { return orderID; }
            set { orderID = value; this.OnPropertyChanged("OrderID"); }
        }

        public string CustomerID
        {
            get { return customerId; }
            set { customerId = value; this.OnPropertyChanged("CustomerID"); }
        }

        public string CustomerName
        {
            get { return customerName; }
            set { customerName = value; this.OnPropertyChanged("CustomerName"); }
        }

        public string Country
        {
            get { return country; }
            set { country = value; this.OnPropertyChanged("Country"); }
        }

        public string ShipCity
        {
            get { return shippingCity; }
            set { shippingCity = value; this.OnPropertyChanged("ShipCity"); }
        }

        public bool IsShipped
        {
            get { return isShipped; }
            set { isShipped = value; this.OnPropertyChanged("IsShipped"); }
        }


        public OrderInfo(decimal? orderId, string customerName, string country, string customerId, string shipCity, bool isShipped)
        {
            this.OrderID = orderId;
            this.CustomerName = customerName;
            this.Country = country;
            this.CustomerID = customerId;
            this.ShipCity = shipCity;
            this.IsShipped = isShipped;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ViewModel
    {
        private ObservableCollection<OrderInfo> orders;
        public ObservableCollection<OrderInfo> Orders
        {
            get { return orders; }
            set { orders = value; }
        }

        public ViewModel()
        {
            orders = new ObservableCollection<OrderInfo>();
            orders.Add(new OrderInfo(1001, "Thomas Hardy", "Germany", "Green", "Berlin", true));
            orders.Add(new OrderInfo(1002, "Laurence Lebihan", "Mexico", "Red", "Mexico D.F.", false));
            orders.Add(new OrderInfo(1003, "Antonio Moreno", "Mexico", "Blue", "Mexico D.F.", true));
            orders.Add(new OrderInfo(1004, "Thomas Hardy", "UK", "Orange", "London", true));
            orders.Add(new OrderInfo(1005, "Christina Berglund", "UK", "Pink", "Lula", false));
            orders.Add(new OrderInfo(1006, "Thomas Hardy", "Germany", "Yellow", "Berlin", true));
            orders.Add(new OrderInfo(1007, "Laurence Lebihan", "Mexico", "Brown", "Mexico D.F.", false));
            orders.Add(new OrderInfo(1008, "Antonio Moreno", "Mexico", "Gray", "Mexico D.F.", true));
            orders.Add(new OrderInfo(1009, "Thomas Hardy", "UK", "Silver", "London", true));
            orders.Add(new OrderInfo(1000, "Christina Berglund", "Germany", "LightGreen", "Lula", false));
        }
    }
}
