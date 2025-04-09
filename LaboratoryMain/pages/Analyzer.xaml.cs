using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace LaboratoryMain.pages
{
    public partial class Analyzer : Window
    {
        public database db = Classes.autorization.database;
        DispatcherTimer timer = new DispatcherTimer();
        private actions currentProcessingAction; // Текущий анализируемый заказ
        /// <summary>
        /// Логика взаимодействия для Analyzer.xaml
        /// </summary>
        public Analyzer()
        {
            InitializeComponent();
            updateInfo();
            Analyzers.ItemsSource = db.analizators.ToList();
            timer.Tick += timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1); // Обновляем каждую секунду
        }

        /// <summary>
        /// Обновляет прогресс и проверяет, можно ли забрать анализы
        /// </summary>
        public void timer_Tick(object sender, EventArgs e)
        {
            if (currentProcessingAction != null)
            {
                // Увеличиваем прогресс на ~3.33% каждую секунду (100% / 30 сек)
                currentProcessingAction.ProgressPercent = Math.Min(
                    currentProcessingAction.ProgressPercent + 3,
                    100
                );

                // Обновляем отображение в DataGrid
                var view = CollectionViewSource.GetDefaultView(ordersTable.ItemsSource);
                view?.Refresh();

                // Если прошло 30 секунд, разрешаем забрать анализы
                if (currentProcessingAction.ProgressPercent >= 100)
                {
                    getAnalysis.IsEnabled = true;
                    timer.Stop();
                }
            }
        }

        

        public bool choiceCheck(int analyzer, int patient)
        {
            if (analyzer == -1)
            {
                MessageBox.Show("Необходимо выбрать анализатор");
                return false;
            }
            if (patient == -1)
            {
                MessageBox.Show("Необходимо выбрать пациента");
                return false;
            }

            return true;
        }

        public bool checkWork(analizators analyzer)
        {
            return true;
        }

        public bool checkServices()
        {
            return true;
        }

        public void updateInfo()
        {
            ordersTable.ItemsSource = db.actions.Where(x => x.id_status == 2).ToList();
        }

        /// <summary>
        /// Запрос для работы в анализаторе
        /// </summary>
        private void postAnalysis_Click(object sender, RoutedEventArgs e)
        {
            bool check = choiceCheck(Analyzers.SelectedIndex, ordersTable.SelectedIndex);
            bool work = checkWork((Analyzers.SelectedItem as analizators));

            if (check && work)
            {
                currentProcessingAction = (ordersTable.SelectedItem as actions);
                currentProcessingAction.ProgressPercent = 0; // Начинаем с 0%

                // Отправляем запрос на API )
                try
                {
                    analizators analyzerName = (Analyzers.SelectedItem as analizators);
                    Classes.GetAnalyzer getAnalyzer = new Classes.GetAnalyzer();
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{analyzerName.name}");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        string json = new JavaScriptSerializer().Serialize(new
                        {
                            patient = currentProcessingAction.id.ToString(),
                            services = new List<Classes.Services>
                        {
                            new Classes.Services { serviceCode = currentProcessingAction.id_service }
                        }
                        });
                        streamWriter.Write(json);
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        getAnalysis.IsEnabled = false;
                        timer.Start(); // Запускаем таймер (обновляет ProgressPercent)
                        MessageBox.Show("Анализ начат, ожидайте...");
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Ошибка отправки: {err.Message}");
                }
            }
        }
        /// <summary>
        /// Получения из анализатора в Базу данных
        /// </summary>
        private void getAnalysis_Click(object sender, RoutedEventArgs e)
        {
            analizators analyzerName = (Analyzers.SelectedItem as analizators);
            Classes.GetAnalyzer getAnalyzer = new Classes.GetAnalyzer();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{analyzerName.name}");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream stream = httpResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        string json = reader.ReadToEnd();
                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        getAnalyzer = serializer.Deserialize<Classes.GetAnalyzer>(json);
                    }

                    int actionID = Convert.ToInt32(getAnalyzer.patient);
                    int analyzerID = db.analizators.FirstOrDefault(x => x.name == analyzerName.name).id;
                    foreach (var item in getAnalyzer.services)
                    {
                        actions serviceAction = db.actions.FirstOrDefault(x => x.id == actionID);
                        if (MessageBox.Show($"Выполнена услуга №{item.serviceCode}, результат: {item.result}", "Устаривает?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            serviceAction.result = item.result;
                            serviceAction.id_status = 1;
                        }
                        else
                        {
                            serviceAction.id_status = 3;
                        }

                        if (checkOrderComplete(serviceAction.id))
                        {
                            orders completeOrder = db.orders.FirstOrDefault(x => x.id == serviceAction.id_order);
                            completeOrder.id_status = 1;
                        }

                        data_analizators newData = new data_analizators();
                        newData.id_analizator = analyzerID;
                        newData.date_of_receipt = DateTime.Now;
                        newData.id_actions = serviceAction.id;
                        newData.time = "15";

                        db.data_analizators.Add(newData);

                        actions New = new actions();
                        New.id = serviceAction.id;
                        New.result = item.result.ToString();
                        db.SaveChanges();
                    }
                    updateInfo();
                    MessageBox.Show("Данные сохранены");
                    currentProcessingAction = null; // Сбрасываем текущий анализ

                }
            }
            catch (Exception err)
            {
                MessageBox.Show($"Ошибка получения результатов: {err}");
            }
        }
        /// <summary>
        /// проверка на работу
        /// </summary>
        public bool checkOrderComplete(int idAction)
        {
            var action = db.actions.FirstOrDefault(x => x.id == idAction);
            orders order = db.orders.FirstOrDefault(x => x.id == action.id_order);
            var allAction = order.actions.ToList();
            foreach(var act in allAction)
            {
                if(act.id_status == 2)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
