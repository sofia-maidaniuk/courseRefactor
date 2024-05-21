using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Bank
{
    public partial class TransactionList : Window
    {
        DataBase DataBase = new DataBase();
        private readonly int _clientId;

        public TransactionList(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
            Loaded += TransactionList_Loaded;
        }

        private void TransactionList_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDataIntoGrid();
        }

        private void LoadDataIntoGrid()
        {
            string queryString = @"SELECT Transactions.transactionType, Transactions.transactionDestination, 
                           FORMAT(Transactions.transactionDate, 'yyyy-MM-dd') AS transactionDate, 
                           Transactions.transactionNumber, Transactions.transactionValue 
                           FROM Transactions 
                           JOIN BankingCard ON Transactions.ID_Card = BankingCard.ID_Card 
                           JOIN Klient ON Klient.ID_Klient = BankingCard.ID_Klient 
                           WHERE Klient.ID_Klient = @clientId";
            DataTable dataTable = GetDataFromDatabase(queryString, _clientId);
            dataGrid.ItemsSource = dataTable.DefaultView;
        }


        private DataTable GetDataFromDatabase(string queryString, int clientId)
        {
            DataTable dataTable = new DataTable();
            try
            {
                DataBase.openConnection();
                using (SqlCommand command = new SqlCommand(queryString, DataBase.getSqlConnection()))
                {
                    command.Parameters.AddWithValue("@clientId", clientId);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при отриманні даних з БД: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DataBase.closeConnection();
            }
            return dataTable;
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = searchTextBox.Text;
            string queryString = @"SELECT Transactions.transactionType, Transactions.transactionDestination, Transactions.transactionDate, Transactions.transactionNumber, Transactions.transactionValue 
                                   FROM Transactions 
                                   JOIN BankingCard ON Transactions.ID_Card = BankingCard.ID_Card 
                                   JOIN Klient ON Klient.ID_Klient = BankingCard.ID_Klient 
                                   WHERE Klient.ID_Klient = @clientId 
                                   AND (Transactions.transactionType LIKE @searchText OR Transactions.transactionDestination LIKE @searchText 
                                   OR Transactions.transactionValue LIKE @searchText)";
            DataTable dataTable = GetDataFromDatabase(queryString, _clientId, searchText);
            dataGrid.ItemsSource = dataTable.DefaultView;
        }

        private DataTable GetDataFromDatabase(string queryString, int clientId, string searchText)
        {
            DataTable dataTable = new DataTable();
            try
            {
                DataBase.openConnection();
                using (SqlCommand command = new SqlCommand(queryString, DataBase.getSqlConnection()))
                {
                    command.Parameters.AddWithValue("@clientId", clientId);
                    command.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

                    string convertedSearchText = "%" + searchText + "%";
                    command.Parameters.AddWithValue("@convertedSearchText", convertedSearchText);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при отриманні даних з БД: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DataBase.closeConnection();
            }
            return dataTable;
        }

        private void Exit_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
