using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Barcode;

namespace LaboratoryMain.pages
{
    public partial class takeBio : Window
    {

        users currentPatient;
        public database db = Classes.autorization.database;
        public takeBio()
        {
            InitializeComponent();
            serviceList.ItemsSource = db.services.ToList();
            patients.ItemsSource = db.users.ToList();
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Add_users add = new Add_users();
            add.Show();
        }

        private void take_bio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (patientOrders.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите номер заказа");
                    return;
                }

                int currentOrderID = (patientOrders.SelectedItem as orders).id;
                int serviceID = (serviceList.SelectedItem as services).id;

                actions serviceAction = new actions();

                serviceAction.id_order = currentOrderID; 
                serviceAction.id_service = serviceID;
                serviceAction.id_status = 2;
                serviceAction.barcode = prob.Text;

                db.actions.Add(serviceAction);
                db.SaveChanges();
                
                if(MessageBox.Show($"Услуга на заявку № {currentOrderID} добавлена", "Те же анализы?", MessageBoxButton.YesNo) != MessageBoxResult.Yes )
                {
                    serviceList.SelectedItem = null;
                    prob.Text = "";
                    patients.SelectedItem = null;
                    patientOrders.SelectedItem = null;
                }
                else
                {
                    serviceList.SelectedIndex = -1;
                }

            }
            catch
            {
                MessageBox.Show("Internal server error");
                return;
            }
        }

        private void patients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentPatient = patients.SelectedItem as users;
            patientOrders.ItemsSource = db.orders.Where(x => x.user_id == currentPatient.id && x.id_status != 1).ToList();
        }

        private void createOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                orders newOrder = new orders();
                newOrder.date = DateTime.Now;
                newOrder.days = 3;
                newOrder.id_status = 2;
                newOrder.user_id = currentPatient.id;
                db.orders.Add(newOrder);
                db.SaveChanges();
                MessageBox.Show("Заявка создана");
            }
            catch
            {
                MessageBox.Show("Internal server error");
                return;
            }
        }

        private void createCode_Click(object sender, RoutedEventArgs e)
        {
            if(patientOrders.SelectedItem != null)
            {
                int orderID = (patientOrders.SelectedItem as orders).id;
                DateTime date = DateTime.Now;
                prob.Text = Barcode.Barcode.GenerateRandomCode(date, orderID);
            }
            else
            {
                MessageBox.Show("Ошибка создания кода");
                return;
            }
            
        }

        private void patientOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int orderID = (patientOrders.SelectedItem as orders).id;
            prob.Text = DateTime.Now + "" + orderID;
        }
    }
}
