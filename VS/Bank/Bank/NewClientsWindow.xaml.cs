using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace Bank
{
    public partial class NewClientsWindow : Window
    {
        private readonly DataBase _dataBase = new DataBase();

        public NewClientsWindow()
        {
            InitializeComponent();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Отримуємо дати з елементів DatePicker
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Будь ласка, оберіть обидві дати.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = EndDatePicker.SelectedDate.Value;

            // Перевіряємо правильність діапазону дат
            if (startDate > endDate)
            {
                MessageBox.Show("Початкова дата не може бути пізніше кінцевої.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoadNewClients(startDate, endDate);
        }

        private void LoadNewClients(DateTime startDate, DateTime endDate)
        {
            try
            {
                _dataBase.openConnection();

                using (SqlCommand command = new SqlCommand("GetNewClientsByDateRange", _dataBase.getSqlConnection()))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    command.CommandTimeout = 120;

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable newClientsTable = new DataTable();
                        adapter.Fill(newClientsTable);
                        NewClientsGrid.ItemsSource = newClientsTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні нових клієнтів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _dataBase.closeConnection();
            }
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            AdminWindow adminWindow = new AdminWindow();
            adminWindow.Show();
            this.Close();
        }
    }
}
