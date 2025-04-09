using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LaboratoryMain.pages
{
    /// <summary>
    /// Логика взаимодействия для summaForCompany.xaml
    /// </summary>
    public partial class summaForCompany : Window
    {
        public database db = Classes.autorization.database;
        public decimal totalPrice = 0;
        public summaForCompany()
        {
            InitializeComponent();
            dp_dataPicker.DisplayDate = DateTime.Now;
            dp_dataPicker.DisplayDateEnd = DateTime.Now;


            cmb_company.ItemsSource = db.policy_company.ToList();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                policy_company selected = cmb_company.SelectedItem as policy_company;

                if (selected == null)
                    return;

                cheking_account newElement = new cheking_account()
                {
                    user_id = Classes.autorization.currentUser.id,
                    id_company = selected.id,
                    date = dp_dataPicker.DisplayDate,
                    sum = totalPrice
                };

                db.cheking_account.Add(newElement);
                db.SaveChanges();
                MessageBox.Show("Счёт сформирован");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка:\n{ex}");
                return;
            }
        }

        private void cmb_company_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DateTime date = dp_dataPicker.DisplayDate;
                change(date);
                tb_summa.Text = totalPrice.ToString();
            }
            catch
            {
                MessageBox.Show("Укажите дату и компанию");
            }
        }


        public void change(DateTime? date)
        {
            policy_company selected = cmb_company.SelectedItem as policy_company;
            var orders = db.orders.Where(x => x.id_status == 1 && x.users.id_policy_company == selected.id && x.date > date).ToList();

            foreach (var item in orders)
            {
                var actions = item.actions.Where(x => x.id_status == 1).ToList();
                foreach (var action in actions)
                {
                    totalPrice += action.services.Price;
                }
            }
        }

        private void dp_dataPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? date = dp_dataPicker.SelectedDate;
            try
            {
                change(date);
                tb_summa.Text = totalPrice.ToString();
            }
            catch
            {
                MessageBox.Show("Укажите дату и компанию");
            }
        }
    }
}
