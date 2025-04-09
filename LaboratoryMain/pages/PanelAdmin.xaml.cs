using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace LaboratoryMain.pages
{
    /// <summary>
    /// Логика взаимодействия для PanelAdmin.xaml
    /// </summary>

    public partial class PanelAdmin : Window
    {

        public database db = Classes.autorization.database;
        DateTime date1, date2;
        IQueryable<history> selectedData;
        public PanelAdmin()
        {
            InitializeComponent();
            allUsers.ItemsSource = db.users.Select(x => x).ToList();

            changeData();

            dp_1.SelectedDate = date1;
            dp_2.SelectedDate = date2;
            dp_2.DisplayDateEnd = DateTime.Today.AddDays(1);
        }

        /// <summary>
        /// Создание дат для отчетов
        /// </summary>
        private void changeData()
        {
            // Установка даты 
            date1 = (dp_1.SelectedDate.HasValue) ? dp_1.SelectedDate.Value : DateTime.Today.AddDays(-7);
            date2 = (dp_2.SelectedDate.HasValue) ? dp_2.SelectedDate.Value : DateTime.Today.AddDays(1);

            // Проверка на поиск
            if (src_tb.Text.Length != 0)
            {
                string search = src_tb.Text.Trim();
                switch (src_cb.SelectedIndex)
                {
                    case 01:
                        selectedData = db.history.Where(v => v.lastenter >= date1 && v.quit <= date2 && v.ip_user.Contains(search));
                        break;

                    default:
                        selectedData = db.history.Where(v => v.lastenter >= date1 && v.quit <= date2 && v.users.name.Contains(search));
                        break;
                }

                dg_paper.ItemsSource = selectedData.ToList();
                return;
            }

            selectedData = db.history.Where(v => v.lastenter >= date1 && v.quit <= date2);
            dg_paper.ItemsSource = selectedData.ToList();
        }

        private void changeDataFunc(object sender, SelectionChangedEventArgs e)
        {
            changeData();
        }

        /// <summary>
        /// Формирование отчетов
        /// </summary>
        public void GenerateReport(IQueryable<history> historyData)
        {
            if (historyData == null || !historyData.Any())
            {
                MessageBox.Show("Нет данных для формирования отчета.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Получаем текущую дату и время для названия файла
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            // Формируем название файла
            string fileName = $"Отчет_о_последних_входах_{currentDateTime}.pdf";

            // Запрашиваем у пользователя место сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF файл (*.pdf)|*.pdf";
            saveFileDialog.FileName = fileName; // Устанавливаем имя файла в диалоге сохранения
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        using (Document pdfDoc = new Document())
                        {
                            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, fs);

                            pdfDoc.Open();

                            // Создаем шрифт, поддерживающий кириллицу
                            BaseFont baseFont = BaseFont.CreateFont(@"C:\Windows\Fonts\Arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                            iTextSharp.text.Font russianFont = new iTextSharp.text.Font(baseFont, 12);

                            // Добавляем заголовок
                            pdfDoc.Add(new iTextSharp.text.Paragraph("Отчет о последних входах", russianFont));

                            // Добавляем текст с указанием периода отчета
                            pdfDoc.Add(new iTextSharp.text.Paragraph($"Период отчета: с {date1.ToShortDateString()} по {date2.ToShortDateString()}", russianFont));

                            // Добавляем отступ перед таблицей
                            pdfDoc.Add(new iTextSharp.text.Paragraph("\n"));



                            // Добавляем таблицу с данными из базы
                            PdfPTable table = new PdfPTable(6); // Здесь 6 - количество столбцов

                            // Устанавливаем ширину каждого столбца в процентах относительно ширины таблицы
                            // Устанавливаем процентное значение ширины для каждого столбца
                            table.SetWidths(new float[] { 10f, 20f, 20f, 20f, 20f, 10f });

                            table.AddCell(new PdfPCell(new Phrase("ID", russianFont)));
                            table.AddCell(new PdfPCell(new Phrase("ID пользователя", russianFont)));
                            table.AddCell(new PdfPCell(new Phrase("ФИО пользователя", russianFont)));
                            table.AddCell(new PdfPCell(new Phrase("Время входа", russianFont)));
                            table.AddCell(new PdfPCell(new Phrase("IP адрес пользователя", russianFont)));
                            table.AddCell(new PdfPCell(new Phrase("Выход", russianFont)));

                            foreach (var entry in historyData)
                            {
                                table.AddCell(new PdfPCell(new Phrase(entry.id.ToString(), russianFont)));
                                table.AddCell(new PdfPCell(new Phrase(entry.user_id.ToString(), russianFont)));
                                table.AddCell(new PdfPCell(new Phrase(entry.users.name.ToString(), russianFont)));
                                table.AddCell(new PdfPCell(new Phrase(entry.lastenter.ToString(), russianFont)));
                                table.AddCell(new PdfPCell(new Phrase(entry.ip_user, russianFont)));
                                table.AddCell(new PdfPCell(new Phrase(entry.quit.ToString(), russianFont)));
                            }

                            pdfDoc.Add(table);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка при формировании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            changeData();
            // Здесь выполняется формирование отчета и сохранение в PDF
            GenerateReport(selectedData);
            MessageBox.Show("Отчет сформирован и сохранен.");
        }


        // Добавить пользователя
        private void add_Click(object sender, RoutedEventArgs e)
        {
            Add_users win_addUser = new Add_users();
            win_addUser.Show();
        }

        private void src_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            changeData();
        }

        

        private void src_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            changeData();
        }
    }
}
