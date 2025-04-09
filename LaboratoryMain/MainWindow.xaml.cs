using EasyCaptcha.Wpf;
using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LaboratoryMain
{
    public partial class MainWindow : Window
    {

        private string captcha = null;
        private int errorCounter = 0;
        private bool autoCaptchaDebug = true;
        private bool showPassword = false;
        private users selectedUser;
        private DateTime? blockTime = null;
        public database db;
        private string Password
        {
            get
            {
                return (showPassword) ? passwordVisible.Text : passwordHide.Password.ToString();
            }
        }
        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
            generateCaptcha();

            MyCaptcha.Visibility = Visibility.Hidden;
            Captcha_Textbox.Visibility = Visibility.Hidden;
            refreshImg.Visibility = Visibility.Hidden;
            db = Classes.autorization.database;

            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 10);
        }

        private void generateCaptcha()
        {
            MyCaptcha.CreateCaptcha(Captcha.LetterOption.Alphanumeric, 4);
            captcha = MyCaptcha.CaptchaText;

        }

        public void timer_Tick(object sender, EventArgs e)
        {
            auth.IsEnabled = true;
        }

        private bool checkCapcha()
        {
            if (MyCaptcha.Visibility == Visibility.Hidden)
            {
                return true;
            }
            return captcha.ToUpper() == Captcha_Textbox.Text.ToUpper();
        }

        private bool checkLogin()
        {
            string login = loginBox.Text;
            string password = Password;

            try
            {
                selectedUser = db.users.FirstOrDefault(x => x.login == login && x.password == password);
                return selectedUser != null;
            }
            catch (Exception error)
            {
                MessageBox.Show($"Произошла ошибка при проверке логина:\n{error}");
                return false;
            }
        }

        private bool checkAccess()
        {
            // Если установлено ограничение по времени
            if (blockTime.HasValue)
            {
                // /Пользователь не отсидел положенный срок
                if (DateTime.Now < blockTime)
                {
                    string dateFormat = blockTime.Value.ToString("dd.MM.yy - HH:mm.ss");
                    MessageBox.Show($"Вы не можете заходить в систему до {dateFormat}");
                    return false;
                }

                // Блокировка снята. Пользовательбольше не попадёт в эту секцию кода
                blockTime = null;
                return true;
            }

            // Получение последней записи о входе / выходе пользователя
            history lastHistory = db.history.ToList().LastOrDefault(val => val.user_id == selectedUser.id);

            // Последней истории не найдено
            if (lastHistory == null)
                return true;

            // Проверка корректности Временной разницы
            // Value - 13:30 - 13:00 ~=> 00:30.00
            TimeSpan? value = lastHistory.quit - lastHistory.lastenter;

            if (value.HasValue)
            {
                TimeSpan sessionDelta = value.Value;

                // Если 00.30.00 > 30 * 60s (То есть сессия прошла полностью) 
                if (sessionDelta.TotalSeconds >= Classes.userTimer.seconds)
                {
                    // Если 14:00 (13:30 (время окончания сессии) + 30 минут блокировки) > Время сейчас, То блокировка работает
                    blockTime = lastHistory.quit.Value.AddMinutes(30);
                    return checkAccess();
                }

                // Сессия не прошла полностью
                return true;
            }

            // Произошла ошибка, значения нету
            return true;
        }

        public void onErrorInput()
        {
            MessageBox.Show("Логин или пароль введены не правильно");
            errorCounter++;

            if (errorCounter == 1)
            {
                MyCaptcha.Visibility = Visibility.Visible;
                Captcha_Textbox.Visibility = Visibility.Visible;
                refreshImg.Visibility = Visibility.Visible;
            }
            else if(errorCounter == 2)
            {
                timer.Start();
                auth.IsEnabled = false;
                MessageBox.Show("Вход заблокирован, подождите 10 секунд");
            }
            else if (errorCounter > 3)
            {
                Captcha_Textbox.Text = "";
                generateCaptcha();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (checkCapcha() && checkLogin())
            {
                if (checkAccess())
                {
                    Classes.autorization.currentUser = selectedUser;
                    createHistory();
                    Classes.userTimer.startTimer();

                    pages.Hub hub = new pages.Hub();
                    hub.Show();
                    Hide();
                    return;
                }
                else
                {
                    MessageBox.Show("Вход заблокирован");
                }
               

            }

            // Пользователь допустил ошибку
            onErrorInput();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            showPassword = !showPassword;

            if (showPassword)
            {
                passwordVisible.Text = passwordHide.Password.ToString();
                passwordVisible.Visibility = Visibility.Visible;
                passwordHide.Visibility = Visibility.Hidden;
            }
            else
            {
                passwordHide.Password = passwordVisible.Text;
                passwordVisible.Visibility = Visibility.Hidden;
                passwordHide.Visibility = Visibility.Visible;
            }
        }

        private void Image_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            generateCaptcha();
        }

        private void createHistory()
        {
            string hostName = Dns.GetHostName();
            IPAddress ip = Dns.GetHostByName(hostName).AddressList[0];

            history history = new history();
            history.user_id = Classes.autorization.currentUser.id;
            history.ip_user = ip.ToString();
            history.lastenter = DateTime.Now;
            db.history.Add(history);
            db.SaveChanges();
        }
    }
}
