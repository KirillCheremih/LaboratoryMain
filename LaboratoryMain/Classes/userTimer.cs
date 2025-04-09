using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LaboratoryMain.Classes
{
    class userTimer
    {
        private static DispatcherTimer timer; // Таймер
        public static List<Action<int, int, int>> userFunctions = new List<Action<int, int, int>>(); // Список функций, принимающих Часы, Минуты, секунды
        public static int value; // Время секунд, оставшееся до завершения таймера
        private static bool mes = true; // Отправлять сообщение, если времени мало?
        private static bool run = false; // Приложение запущено
        public const int seconds = 9000; // Время, выделенное на сессию

        public static void startTimer(int endTimer = seconds)
        {

            // Если таймер уже запущен, выдавать ошибку
            if (run)
                throw new Exception("Таймер уже запущен");

            // Настройка таймера
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 1);
            value = endTimer;

            // Запуск
            timer.Start();
            run = true;
        }

        // Остановить таймер
        public static void stopTimer()
        {
            timer.Stop();
            Classes.autorization.reloadApplication();
        }

        private static void timerTick(object sender, EventArgs e)
        {
            // Прошла 1 секунда
            value--;

            // Высчитывание времени по секундам: Часы, Минуты, Секунды
            Tuple<int, int, int> time = getTimeFromSeconds(value);

            // Выполнение отрисовки в приложении
            if (userFunctions.Count > 0)
            {
                foreach (Action<int, int, int> func in userFunctions)
                {
                    // Передача времени в параметры
                    func(time.Item1, time.Item2, time.Item3);
                }
            }

            // Осталось меньше 15 минут, пора выводить сообщение об этом
            if (time.Item2 < 15)
            {
                if (mes)
                {
                    MessageBox.Show("Осталось меньше 15 минут, приготовьтесь.");
                    mes = false;
                }
            }

            // Таймер нужно остановить
            if (value == 0)
                stopTimer();
        }

        // Обработать общее количество секунд во время
        public static Tuple<int, int, int> getTimeFromSeconds(int secs)
        {
            int hours = secs / 3600;
            int minutes = secs % 3600;
            int seconds = minutes % 60;
            minutes /= 60;
            return new Tuple<int, int, int>(hours, minutes, seconds);
        }
    
    }
}
