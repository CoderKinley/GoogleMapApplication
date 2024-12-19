using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mapAdvanced.View.UserControls
{
    public partial class ClerableTextBox : UserControl
    {
        public ClerableTextBox()
        {
            InitializeComponent();
        }

        private string placeHolder;

        public string PlaceHolder
        {
            get { return placeHolder; }
            set { 
                placeHolder = value;
                // Do not do this!!
                tbTextHolder.Text = placeHolder;
            }
        }

        private void clrBtn_Click(object sender, RoutedEventArgs e)
        {
            txtInput.Clear();
            txtInput.Focus();
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                tbTextHolder.Visibility = Visibility.Visible;
            }
            else
            {
                tbTextHolder.Visibility = Visibility.Hidden;
            }
        }
    }
}
