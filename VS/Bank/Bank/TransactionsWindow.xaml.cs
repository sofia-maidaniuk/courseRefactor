using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bank
{
    public partial class TransactionsWindow : Window
    {
        private readonly DataBase _dataBase = new DataBase();

        public TransactionsWindow()
        {
            InitializeComponent();
            LoadTransactionTypes();
            LoadTransactions();
            LoadTransactionStatistics();
        }


        private void LoadTransactionTypes()
        {
            try
            {
                _dataBase.openConnection();

                using (SqlCommand command = new SqlCommand("GetTransactionTypes", _dataBase.getSqlConnection()))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        TransactionTypeComboBox.Items.Add("Усі"); // Додаємо опцію для перегляду всіх транзакцій
                        while (reader.Read())
                        {
                            TransactionTypeComboBox.Items.Add(reader["transactionType"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні типів транзакцій: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _dataBase.closeConnection();
            }
        }

        private void LoadTransactions(string transactionType = null)
        {
            try
            {
                _dataBase.openConnection();

                using (SqlCommand command = new SqlCommand("GetTransactions", _dataBase.getSqlConnection()))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@TransactionType", (object)transactionType ?? DBNull.Value);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable transactionsTable = new DataTable();
                        adapter.Fill(transactionsTable);
                        TransactionsGrid.ItemsSource = transactionsTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні транзакцій: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _dataBase.closeConnection();
            }
        }

        private void TransactionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TransactionTypeComboBox.SelectedItem != null)
            {
                string selectedType = TransactionTypeComboBox.SelectedItem.ToString();
                if (selectedType == "Усі")
                {
                    LoadTransactions(); 
                }
                else
                {
                    LoadTransactions(selectedType); 
                }
            }
        }

        private void LoadTransactionStatistics()
        {
            try
            {
                _dataBase.openConnection();

                using (SqlCommand command = new SqlCommand("GetTransactionStatistics", _dataBase.getSqlConnection()))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Завантажуємо найпопулярніший тип транзакції
                        if (reader.HasRows && reader.Read())
                        {
                            MostPopularTransactionType.Text = $"{reader["transactionType"]} ({reader["TransactionCount"]} шт.)";
                        }
                        else
                        {
                            MostPopularTransactionType.Text = "Дані відсутні";
                        }

                        // Завантажуємо найменш популярний тип транзакції
                        if (reader.NextResult() && reader.HasRows && reader.Read())
                        {
                            LeastPopularTransactionType.Text = $"{reader["transactionType"]} ({reader["TransactionCount"]} шт.)";
                        }
                        else
                        {
                            LeastPopularTransactionType.Text = "Дані відсутні";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні статистики транзакцій: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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
