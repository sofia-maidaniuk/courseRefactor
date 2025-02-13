﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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

namespace Bank
{
    public partial class AdminWindow : Window
    {
        DataBase dataBase = new DataBase();

        public AdminWindow()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                dataBase.openConnection();

                // Виклик процедури GetTotalCounts
                using (SqlCommand command = new SqlCommand("GetTotalCounts", dataBase.getSqlConnection()))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Виконання команди
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            textBlok_countKlient.Text = reader["TotalCountKlient"].ToString();
                        }

                        if (reader.NextResult() && reader.HasRows && reader.Read())
                        {
                            textBlok_countCard.Text = reader["TotalCountBankingCard"].ToString();
                        }

                        if (reader.NextResult() && reader.HasRows && reader.Read())
                        {
                            textBlok_countDeposit.Text = reader["TotalCountDeposits"].ToString();
                        }

                        if (reader.NextResult() && reader.HasRows && reader.Read())
                        {
                            textBlok_countCredit.Text = reader["TotalCountCredits"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні статистичних даних: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dataBase.closeConnection();
            }
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Log_in log_in = new Log_in();
            log_in.Show();
            this.Close();
        }

        private void ButtonopenDataBase_Click(object sender, RoutedEventArgs e)
        {
            DatabaseManagementWindow dbWindow = new DatabaseManagementWindow();
            dbWindow.Show();
            this.Close();
        }

        private void ShowClientDetails_Click(object sender, RoutedEventArgs e)
        {
            ClientDetailsWindow detailsWindow = new ClientDetailsWindow();
            detailsWindow.Show();
            this.Close();
        }
        private void OpenNewClientsWindow_Click(object sender, RoutedEventArgs e)
        {
            NewClientsWindow newClientsWindow = new NewClientsWindow();
            newClientsWindow.Show();
            this.Close();
        }
        
        private void OpenTransactionView_Click(object sender, RoutedEventArgs e)
        {
            TransactionsWindow transactionsWindow = new TransactionsWindow();
            transactionsWindow.Show();
            this.Close();
        }
    }
}
