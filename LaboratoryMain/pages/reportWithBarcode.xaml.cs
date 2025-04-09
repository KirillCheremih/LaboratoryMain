using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;
using Barcode;
using System.IO;


namespace LaboratoryMain.pages
{
    public partial class reportWithBarcode : Window
    {
        public database db = Classes.autorization.database;
        public reportWithBarcode()
        {
            InitializeComponent();
            patientService.ItemsSource = db.actions.ToList();
            patientOrders.ItemsSource = db.orders.ToList();
        }

        private void checkBarcode_Click(object sender, RoutedEventArgs e)
        {
            if(patientService.SelectedItem != null)
            {
                actions takeService = (patientService.SelectedItem as actions);
                int userID = db.orders.FirstOrDefault(x => x.id == takeService.id_order).user_id;
                users patient = db.users.FirstOrDefault(x => x.id == userID);
                if (patient != null)
                {
                    string path = Environment.CurrentDirectory;
                    Bitmap barcode = Barcode.Barcode.GenerateBarcode(takeService.barcode);
                    Barcode.Barcode.SaveBarcodeToPdf(barcode, takeService.id.ToString(), "barcode.pdf");
                    img_barcode.Source = Barcode.Barcode.BarcodeToBitmapImage(barcode);
                }
            }
            else
            {
                MessageBox.Show("Для отображения штрих-кода выберите пациента");
                return;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            patientService.ItemsSource = db.actions.Where(x => x.id_order.ToString().Contains(search.Text)).ToList();
        }
    }
}
