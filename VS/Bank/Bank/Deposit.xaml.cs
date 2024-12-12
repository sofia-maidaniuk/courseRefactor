using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Bank
{
    public partial class Deposit : Window
    {
        private readonly int _clientId;
        DataBase dataBase = new DataBase();

        public Deposit(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
            LoadDeposits();
        }

        private void ComboBoxTerm_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboBoxTerm.SelectedItem != null)
            {
                int months = int.Parse((comboBoxTerm.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "0");

                // Встановлюємо процентну ставку залежно від терміну
                switch (months)
                {
                    case 3:
                        textBoxInterestRate.Text = "10";
                        break;
                    case 6:
                        textBoxInterestRate.Text = "11";
                        break;
                    case 12:
                        textBoxInterestRate.Text = "12";
                        break;
                }
            }
        }

        private void ButtonCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(textBoxDepositAmount.Text, out decimal depositAmount))
            {
                MessageBox.Show("Будь ласка, введіть коректну суму депозиту.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(textBoxInterestRate.Text, out decimal interestRate))
            {
                MessageBox.Show("Будь ласка, оберіть термін депозиту для встановлення процентної ставки.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (comboBoxTerm.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, оберіть термін депозиту.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int months = int.Parse((comboBoxTerm.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "0");
            int days = months * 30;

            // Перевірка високосного року
            int currentYear = DateTime.Now.Year;
            int daysInYear = DateTime.IsLeapYear(currentYear) ? 366 : 365;

            try
            {
                // Формула P = S * (I / (K * 100)) * T
                decimal dailyRate = interestRate / (daysInYear * 100);
                decimal profit = depositAmount * dailyRate * days;

                labelCalculatedInterest.Text = profit.ToString("0.00") + " грн";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonCreateDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(textBoxDepositAmount.Text, out decimal depositAmount))
            {
                MessageBox.Show("Будь ласка, введіть коректну суму депозиту.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(textBoxInterestRate.Text, out decimal interestRate))
            {
                MessageBox.Show("Будь ласка, оберіть термін депозиту для встановлення процентної ставки.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (comboBoxTerm.SelectedItem == null)
            {
                MessageBox.Show("Будь ласка, оберіть термін депозиту.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                dataBase.openConnection();

                // Перевірка валюти картки
                string currencyQuery = "SELECT currency FROM BankingCard WHERE ID_Klient = @clientId";
                SqlCommand currencyCommand = new SqlCommand(currencyQuery, dataBase.getSqlConnection());
                currencyCommand.Parameters.AddWithValue("@clientId", _clientId);
                string currency = currencyCommand.ExecuteScalar()?.ToString();

                if (currency != "UAH")
                {
                    MessageBox.Show("Депозити можна відкривати лише на гривневі картки.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Перевірка балансу клієнта
                string checkBalanceQuery = "SELECT balance FROM BankingCard WHERE ID_Klient = @clientId";
                SqlCommand checkBalanceCommand = new SqlCommand(checkBalanceQuery, dataBase.getSqlConnection());
                checkBalanceCommand.Parameters.AddWithValue("@clientId", _clientId);
                decimal currentBalance = (decimal)checkBalanceCommand.ExecuteScalar();

                if (currentBalance < depositAmount)
                {
                    MessageBox.Show("Недостатньо коштів на рахунку для створення депозиту.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using (SqlTransaction transaction = dataBase.getSqlConnection().BeginTransaction())
                {
                    // Списання коштів із рахунку
                    string updateBalanceQuery = "UPDATE BankingCard SET balance = balance - @amount WHERE ID_Klient = @clientId";
                    SqlCommand updateBalanceCommand = new SqlCommand(updateBalanceQuery, dataBase.getSqlConnection(), transaction);
                    updateBalanceCommand.Parameters.AddWithValue("@amount", depositAmount);
                    updateBalanceCommand.Parameters.AddWithValue("@clientId", _clientId);
                    updateBalanceCommand.ExecuteNonQuery();

                    // Створення депозиту
                    string insertDepositQuery = "INSERT INTO Deposits (depositAmount, depositDate, interestRate, ID_Klient) VALUES (@depositAmount, @depositDate, @interestRate, @clientId)";
                    SqlCommand insertDepositCommand = new SqlCommand(insertDepositQuery, dataBase.getSqlConnection(), transaction);
                    insertDepositCommand.Parameters.AddWithValue("@depositAmount", depositAmount);
                    insertDepositCommand.Parameters.AddWithValue("@depositDate", DateTime.Now);
                    insertDepositCommand.Parameters.AddWithValue("@interestRate", interestRate);
                    insertDepositCommand.Parameters.AddWithValue("@clientId", _clientId);
                    insertDepositCommand.ExecuteNonQuery();

                    transaction.Commit();
                }

                MessageBox.Show("Депозит успішно створено! Перевірте таблицю транзакцій.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadDeposits();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dataBase.closeConnection();
            }
        }


        private void LoadDeposits()
        {
            try
            {
                dataBase.openConnection();

                string query = "SELECT ID_Deposit, depositAmount, depositDate, interestRate FROM Deposits WHERE ID_Klient = @clientId";
                SqlCommand command = new SqlCommand(query, dataBase.getSqlConnection());
                command.Parameters.AddWithValue("@clientId", _clientId);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                dataGridDeposits.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dataBase.closeConnection();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}