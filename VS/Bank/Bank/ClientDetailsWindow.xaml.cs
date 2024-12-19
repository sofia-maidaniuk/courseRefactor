using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bank
{
    public partial class ClientDetailsWindow : Window
    {
        private readonly DataBase _dataBase = new DataBase();
        private int _selectedClientId;

        public ClientDetailsWindow()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients(string searchTerm = "")
        {
            try
            {
                _dataBase.openConnection();

                using (SqlCommand command = new SqlCommand("SearchClients", _dataBase.getSqlConnection()))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable clientsTable = new DataTable();
                        adapter.Fill(clientsTable);
                        ClientsGrid.ItemsSource = clientsTable.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні клієнтів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _dataBase.closeConnection();
            }
        }

        private void SearchClient_Click(object sender, RoutedEventArgs e)
        {
            LoadClients(SearchBox.Text);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadClients(SearchBox.Text);
        }

        private void ClientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is DataRowView selectedRow)
            {
                _selectedClientId = Convert.ToInt32(selectedRow["ID_Klient"]);
                LoadClientDetails();
            }
        }

        private void LoadClientDetails()
        {
            try
            {
                _dataBase.openConnection();

                using (SqlCommand command = new SqlCommand("GetClientDetails", _dataBase.getSqlConnection()))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ID_Klient", _selectedClientId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        using (DataSet dataSet = new DataSet())
                        {
                            adapter.Fill(dataSet);

                            // Завантаження основної інформації про клієнта
                            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                            {
                                DataRow clientRow = dataSet.Tables[0].Rows[0];
                                ClientName.Text = $"{clientRow["last_Name"]} {clientRow["first_Name"]} {clientRow["surname"]}";
                            }

                            // Завантаження даних банківських карт клієнта
                            if (dataSet.Tables.Count > 1)
                            {
                                CardsGrid.ItemsSource = dataSet.Tables[1].DefaultView;
                            }

                            // Завантаження даних депозитів клієнта
                            if (dataSet.Tables.Count > 2)
                            {
                                DepositsGrid.ItemsSource = dataSet.Tables[2].DefaultView;
                            }

                            // Завантаження даних кредитів клієнта
                            if (dataSet.Tables.Count > 3)
                            {
                                CreditsGrid.ItemsSource = dataSet.Tables[3].DefaultView;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні даних клієнта: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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
