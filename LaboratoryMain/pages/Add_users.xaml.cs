using MAIL_LIB;
using System;
using System.Linq;
using System.Windows;

namespace LaboratoryMain.pages
{
    public partial class Add_users : Window
    {
        private static string[] fio = new string[3]
        {
            "Фамилия", "Имя", "Отчество"
        };
        public database db = Classes.autorization.database;


        public Add_users()
        {
            InitializeComponent();
            lb_role.ItemsSource = db.roles.ToList();
        }

        public bool lib_checker(string login, string mail, string password)
        {
            if (Checker.login_check(login) != true)
            {
                MessageBox.Show("Логин должен состоять только из латинских букв и цифр и быть не меньше 6 символов!");
                return false;
            }
            if (Checker.password_check(password) != true)
            {
                MessageBox.Show("Пароль должен иметь 1 спец. символ, 1 загл. букву, 1 цифру и быть не менее 8 символов");
                return false;
            }
            if (Checker.mail_check(mail) != true)
            {
                MessageBox.Show("Введите корректную почту");
                return false;

            }

            return true;
        }


        private void btn_reg_Click(object sender, RoutedEventArgs e)
        {


            string login = lb_login.Text;
            string password = lb_password.Text;
            string email = lb_email.Text;
            string fullname = lb_fio.Text;
            string phone = lb_phone.Text;
            int role = (int)lb_role.SelectedValue;

            if (lib_checker(login, email, password))
            {
                // Проверка на существующего пользователя
                if (db.users.FirstOrDefault(x => x.login == login) != null)
                {
                    MessageBox.Show($"Логин <{login}> занят");
                    return;
                }

                // Проверка на правильность фио
                string[] splited = fullname.Trim().Split(' ');

                if (splited.Length < 3)
                {
                    MessageBox.Show("Вы ввели не полное имя!");
                }

                string message = "";
                for (int i = 0; i < 3; i++)
                {
                    string word = splited[i];
                    if (word.Length < 3)
                    {
                        if (message.Length > 0)
                        {
                            message += ", ";
                        }

                        message += $"{fio[i]}";
                    }
                }

                if (message.Length > 0)
                {
                    string ok = (message.Length == 1) ? "о" : "ы";
                    MessageBox.Show($"{message} должн{ok} быть больше 3 символов");
                    return;
                }

                users newUser = new users()
                {
                    login = login,
                    password = password,
                    email = email,
                    name = fullname,
                    tel = phone,
                    role = role
                };

                try
                {
                    db.users.Add(newUser);
                    db.SaveChanges();
                    MessageBox.Show("Пользователь зарегистрирован!");
                }
                catch (Exception error)
                {
                    MessageBox.Show($"Ошибка при сохранении пользователя:\n{error}");
                }
            }


        }

    }
}
