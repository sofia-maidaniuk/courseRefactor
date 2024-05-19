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
            string queryString = $"SELECT Transactions.transactionType, Transactions.transactionDestination, Transactions.transactionDate, Transactions.transactionNumber, Transactions.transactionValue " +
                                 $"FROM Transactions " +
                                 $"JOIN BankingCard ON Transactions.ID_transaction = BankingCard.ID_Card " +
                                 $"JOIN Klient ON Klient.ID_Klient = BankingCard.ID_Klient " +
                                 $"WHERE Klient.ID_Klient = {_clientId}";
            DataTable dataTable = GetDataFromDatabase(queryString);
            dataGrid.ItemsSource = dataTable.DefaultView;
        }

        private DataTable GetDataFromDatabase(string queryString)
        {
            DataTable dataTable = new DataTable();
            try
            {
                DataBase.openConnection();
                using (SqlCommand command = new SqlCommand(queryString, DataBase.getSqlConnection()))
                {
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
            string queryString = $"SELECT Transactions.transactionType, Transactions.transactionDestination, Transactions.transactionDate, Transactions.transactionNumber, Transactions.transactionValue " +
                                 $"FROM Transactions " +
                                 $"JOIN BankingCard ON Transactions.ID_transaction = BankingCard.ID_Card " +
                                 $"JOIN Klient ON Klient.ID_Klient = BankingCard.ID_Klient " +
                                 $"WHERE Klient.ID_Klient = {_clientId} " +
                                 $"AND (Transactions.transactionType LIKE '%{searchText}%' OR Transactions.transactionDestination LIKE '%{searchText}%' " +
                                 $"OR Transactions.transactionDate LIKE '%{searchText}%' OR Transactions.transactionNumber LIKE '%{searchText}%' " +
                                 $"OR Transactions.transactionValue LIKE '%{searchText}%')";
            DataTable dataTable = GetDataFromDatabase(queryString);
            dataGrid.ItemsSource = dataTable.DefaultView;
        }

        private void Exit_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
