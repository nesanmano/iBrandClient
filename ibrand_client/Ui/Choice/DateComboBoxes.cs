using System;
using System.Drawing;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public partial class DateComboBoxes : UserControl
    {

        private readonly int _minYear = 1900;
        private readonly int _maxYear = DateTime.Now.Year;
        private readonly string _defaultSelectedYear = "1980";

        public DateComboBoxes()
        {
            InitializeComponent();

            monthComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            dayComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            yearComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            
            PopulateDayComboBox();
            PopulateMonthComboBox();
            PopulateYearComboBox();
        }

        public override string Text
        {
            get
            {
                if (dayComboBox.SelectedIndex == -1 ||
                    monthComboBox.SelectedIndex == -1 ||
                    yearComboBox.SelectedIndex == -1)
                    return "";

                var day = dayComboBox.Text;
                var month = monthComboBox.Text;
                var year = yearComboBox.Text;
                return day + "/" + month + "/" + year;
            }
        }

        public void SetSize(Size size)
        {
            Size = size;
            var comboWidth = Convert.ToInt32(Math.Floor((size.Width - firstSlash.Width - secondSlash.Width - 20) / 3.5)); // 5px by space between combos and slash
            dayComboBox.Width = comboWidth;
            monthComboBox.Width = comboWidth;
            
            yearComboBox.Width = Convert.ToInt32(Math.Floor(comboWidth * 1.5)); // Larger than the other ones.
            FormatDayMonthYear();
        }

        private void FormatDayMonthYear()
        {
            var initialLocationLabels = DayLabel.Location;
            var initialLocationSlashes = firstSlash.Location;
            var initialLocationCombos = dayComboBox.Location;
            dayComboBox.Location = new Point(0, initialLocationCombos.Y);
            DayLabel.Location = new Point(0, initialLocationLabels.Y);
            firstSlash.Location = new Point(dayComboBox.Width + 5, initialLocationSlashes.Y);
            monthComboBox.Location = new Point(firstSlash.Location.X + firstSlash.Width + 5, initialLocationCombos.Y);
            monthLabel.Location = new Point(firstSlash.Location.X + firstSlash.Width + 5, initialLocationLabels.Y);
            secondSlash.Location = new Point(monthComboBox.Location.X + monthComboBox.Width + 5, initialLocationSlashes.Y);
            yearComboBox.Location = new Point(secondSlash.Location.X + firstSlash.Width + 5, initialLocationCombos.Y);
            yearLabel.Location = new Point(secondSlash.Location.X + firstSlash.Width + 5, initialLocationLabels.Y);
        }


        public void Reset()
        {
            dayComboBox.SelectedIndex = -1;
            monthComboBox.SelectedIndex = -1;
            yearComboBox.SelectedItem = _defaultSelectedYear;
        }

        private void PopulateDayComboBox()
        {
            
            dayComboBox.Items.Clear();
            for (var i = 1; i <= 31; i++)
            {
                dayComboBox.Items.Add(i.ToString());
            }
        }

        private void PopulateMonthComboBox()
        {
            
            monthComboBox.Items.Clear();
            for (var i = 1; i <= 12; i++)
            {
                monthComboBox.Items.Add(i.ToString());
            }
        }

        private void PopulateYearComboBox()
        {
            yearComboBox.Items.Clear();
            for (var i = _maxYear; i >= _minYear; i--)
            {
                yearComboBox.Items.Add(i.ToString());
            }

            yearComboBox.SelectedItem = _defaultSelectedYear;
        }
    }
}
