using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace LaboratoryMain.pages
{
    public partial class Hub : Window
    {
        static TextBlock tb_staticTimer;
        static TextBox[] textboxes;
        List<Classes.userButton> buttons = new List<Classes.userButton>();
        List<Classes.userButton> allButtons = new List<Classes.userButton>()
        {
            new Classes.userButton("Открыть панель администратора", new int[] { 4 }, admin_panel),
            new Classes.userButton("Принять биоматериал", new int[] { 3 }, take_bio),
            new Classes.userButton("Сформировать отчет c штрих-кодами", new int[] { 3 }, check_barcode_report),
            new Classes.userButton("Работа с анализатором", new int[] { 2 }, analyzer_work),
            new Classes.userButton("Сформировать счет", new int[] { 5 }, create_payment_account),
        };


        // Пульт управления
        static int admin_panel()
        {
            PanelAdmin adminPanel = new PanelAdmin();
            adminPanel.Show();

            return 0;
        }

        static int take_bio()
        {
            takeBio bio = new takeBio();
            bio.Show();

            return 0;
        }

        static int create_payment_account()
        {
            summaForCompany checking_account = new summaForCompany();
            checking_account.Show();

            return 0;
        }

        static int check_barcode_report()
        {
            reportWithBarcode report = new reportWithBarcode();
            report.Show();

            return 0;
        }

        static int analyzer_work()
        {
            Analyzer work = new Analyzer();
            work.Show();

            return 0;
        }
        
        /// <summary>
        /// Изменение картинки пользователя
        /// </summary>
        /// <param name="newImage"></param>
        void changeImage(string newImage = null)
        {
            if (newImage != null)
            {
                try
                {
                    Classes.autorization.currentUser.image = newImage;
                    Classes.autorization.database.SaveChanges();
                }
                catch (Exception error)
                {
                    MessageBox.Show($"Произошла ошибка при изменении изображения в базе:\n{error}");
                }
            }

            string img = Classes.autorization.currentUser.image;
            img_avatar.Source = Classes.imageHelper.getImageByName(img, "Res");
            utb_imagePicker.Text = Classes.imageHelper.getFilePath(img, "Res");

        }

        /// <summary>
        /// Загрузка информации о пользователе в TextBoxs
        /// </summary>
        private void loadUserInfo()
        {
            users current = Classes.autorization.currentUser;

            string[] splited = current.name.Split(' ');


            for (int i = 0; i < splited.Length; i++)
            {
                textboxes[i].Text = splited[i].Trim();
            }

            utb_phone.Text = current.tel;
            utb_email.Text = current.email;
            changeImage();
        }
        /// <summary>
        /// Открытие файлового диалога для назначения новой картинки пользователя
        /// </summary>
        void openFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            
            // Если пользователь выбрал файл, а не закрыл окно
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedPath = openFileDialog.FileName;                               // Путь до выбранного изображения C:/users/...
                string selectedImage = System.IO.Path.GetFileName(selectedPath);             // Файл выбранного изображения image.type
                string filePath = Classes.imageHelper.getFilePath(selectedImage, "Avatars"); // Путь до предроложительного файла в папке с изображениями
                string imageUser = Classes.autorization.currentUser.image;                   // Название изображения, записанного в базе данных

                // Файл существует и нужен новый
                if (Classes.imageHelper.checkFileExists(filePath))
                {
                    string newFilePath = Classes.imageHelper.getNewFilename(selectedImage, Classes.imageHelper.ProjectPath + "\\Avatars\\");
                    Classes.imageHelper.addOrReplaceFile(selectedPath, newFilePath);
                    changeImage(System.IO.Path.GetFileName(newFilePath));
                }

                // Файла не существует, новый не нужен
                else
                {
                    Classes.imageHelper.addOrReplaceFile(selectedPath, filePath);
                    changeImage(selectedImage);
                }
            }
        }
        /// <summary>
        /// Когда таймер меняет значение текст меняется
        /// </summary>
        /// <param name="hours"></param>
        /// <param name="minutes"></param>
        /// <param name="seconds"></param>
        static void tick(int hours, int minutes, int seconds)
        {
            tb_staticTimer.Text = $"Осталось: {hours}:{minutes:D2}.{seconds:D2}";
        }

        
        public Hub()
        {
            InitializeComponent();

            textboxes = new TextBox[]
            {
                utb_surname,
                utb_name,
                utb_patronymic
            };

            Tuple<int, int, int> time = Classes.userTimer.getTimeFromSeconds(Classes.userTimer.seconds);
            tbl_timer.Text = $"Осталось: {time.Item1:D2}:{time.Item2:D2}.{time.Item3:D2}";

            tb_staticTimer = tbl_timer;
            Classes.userTimer.userFunctions.Add(tick);

            users currentUser = Classes.autorization.currentUser;
            if (currentUser != null)
            {
                loadUserInfo();
                tbl_login.Text = currentUser.login;
                tbl_fio.Text = currentUser.name;
                tbl_role.Text = Convert.ToString(currentUser.role);
                img_avatar.Source = getImage(currentUser.image);

                foreach (Classes.userButton btn in allButtons)
                {
                    if (btn.Role.Contains(currentUser.role))
                    {
                        buttons.Add(btn);
                    }
                }
                listButtons.ItemsSource = buttons;
            }

        }
        /// <summary>
        /// Клик по кнопке для вывода возможных действий пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Classes.userButton context = btn.DataContext as Classes.userButton;
            context.execute();
        }


        /// <summary>
        /// Получение фотографии пользователя
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>
        /// Если фото не найдено или его просто нет, то выводится default изображение, если найдено, то фото пользователя
        /// </returns>
        public static BitmapImage getImage(string filename)
        {
            string res = "Res";
            string path = Environment.CurrentDirectory;
            string defaultPath = $"{path.Remove(path.Length - 10, 10)}\\{res}\\unknown.png";
            BitmapImage defaultImage;
            if (filename != null)
            {
                path = $"{path.Remove(path.Length - 10, 10)}\\{res}\\{filename}";
                BitmapImage userImage;
                try
                {
                    userImage = new BitmapImage(new Uri(path));
                    return userImage;
                }
                catch
                {
                    defaultImage = new BitmapImage(new Uri(defaultPath));
                    return defaultImage;
                }
            }
            defaultImage = new BitmapImage(new Uri(defaultPath));
            return defaultImage;
        }
        /// <summary>
        /// Кнопка для выхода из приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void utb_imagePicker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            openFileDialog();
        }

        private void img_avatar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            openFileDialog();
        }

        /// <summary>
        /// Кнопка для сохранения введенных данных
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                users current = Classes.autorization.currentUser;
                current.name = $"{utb_surname.Text} {utb_name.Text} {utb_patronymic.Text}".Trim();
                current.tel = utb_phone.Text.Trim();
                current.email = utb_email.Text.Trim();
                Classes.autorization.database.SaveChanges();

                tbl_fio.Text = current.name;
                MessageBox.Show("Изменения сохранены");
            }
            catch (Exception error)
            {
                MessageBox.Show($"Ошибка при сохранении данных:\n{error}");
            }
        }

        /// <summary>
        /// Выход из аккаунта - перезагрузка приложения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitAcc_Click(object sender, RoutedEventArgs e)
        {
            Classes.autorization.reloadApplication();
        }

        /// <summary>
        /// При закрытии приложения записывает информацию о выходе пользователя в БД
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                history lastHistory = Classes.autorization.database.history.ToList().Last(x => x.user_id == Classes.autorization.currentUser.id);
                lastHistory.quit = DateTime.Now;
                Classes.autorization.database.SaveChanges();
            }
            catch (Exception error)
            {
                MessageBox.Show($"Данные истории не сохранены из-за ошибки:\n{error}");
            }
        }
    }
}
