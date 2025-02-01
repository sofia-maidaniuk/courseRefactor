using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Bank.DataClasses;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Bank.Classes;
using System.Collections.Generic;
using System.Linq;

namespace Bank
{
    public partial class Payment : Window
    {
        private readonly int _clientId;
        public string BankCard { get; set; }
        public string CardNumber { get; set; }
        public string CVV { get; set; }
        public string ExpiryDate { get; set; }
        private string selectedServiceName;

        DataBase dataBase = new DataBase();
        Random random = new Random();

        public Payment(int clientId, string bankCard, string cardNumber, string cvvCode, string expiryDate)
        {
            InitializeComponent();
            _clientId = clientId;
            BankCard = bankCard;
            CardNumber = cardNumber;
            CVV = cvvCode;
            ExpiryDate = expiryDate;
            DataContext = this;
            LoadServices();
        }

        private void Payment_Loaded(object sender, RoutedEventArgs e)
        {
            textBox_cardNumber.Text = DataStorage.cardNumber;
        }

        private void LoadServices()
        {
            DataTable services = new DataTable();
            string querystringServices = "SELECT ID_Service, serviceName FROM Services";

            dataBase.openConnection();

            using (SqlCommand command = new SqlCommand(querystringServices, dataBase.getSqlConnection()))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(services);
            }

            dataBase.closeConnection();

            if (services.Rows.Count == 0)
            {
                MessageBox.Show("Немає доступних послуг.", "Повідомлення");
                return;
            }

            servicesComboBox.Items.Clear();
            servicesComboBox.Items.Add("Виберіть послугу");
            foreach (DataRow row in services.Rows)
            {
                servicesComboBox.Items.Add(row["serviceName"]);
            }

            servicesComboBox.SelectedIndex = 0;
        }

        private void servicesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (servicesComboBox.SelectedIndex > 0)
            {
                selectedServiceName = servicesComboBox.SelectedItem.ToString();
            }
        }

        private int ExecuteNonQuery(string query, Dictionary<string, object> parameters, SqlTransaction transaction = null)
        {
            using (var command = new SqlCommand(query, dataBase.getSqlConnection(), transaction))
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
                return command.ExecuteNonQuery();
            }
        }

        private bool ValidateInputs(out double sum)
        {
            sum = 0;

            if (servicesComboBox.SelectedIndex == 0 || string.IsNullOrEmpty(selectedServiceName))
            {
                MessageBox.Show("Будь ласка, виберіть послугу.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!double.TryParse(textBox_sum.Text, out sum))
            {
                MessageBox.Show("Будь ласка, введіть коректну суму.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (!Regex.IsMatch(textBox_bill.Text, "^UA[0-9]{27}$"))
            {
                MessageBox.Show("Номер рахунку повинен мати формат UAXXXXXXXXXXXXXXXXXXXXXXXXXXX", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool CheckCardDetails(out double cardBalance, out string currency)
        {
            cardBalance = 0;
            currency = "";

            string queryCheckCard = "SELECT cvvCode, CONCAT(FORMAT(cardDate, '%M'),'/', FORMAT(cardDate, '%y')), balance, currency FROM BankingCard WHERE cardNumber = @cardNumber";

            dataBase.openConnection();
            using (var command = new SqlCommand(queryCheckCard, dataBase.getSqlConnection()))
            {
                command.Parameters.AddWithValue("@cardNumber", textBox_cardNumber.Text);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (textBox_cvvCode.Text != reader[0].ToString() ||
                            textBox_date.Text != reader[1].ToString())
                        {
                            MessageBox.Show("Невірні дані картки.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }

                        cardBalance = Convert.ToDouble(reader[2]);
                        currency = reader[3].ToString();
                    }
                    else
                    {
                        MessageBox.Show("Картку не знайдено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            return true;
        }

        private bool IsValidCurrency(string currency)
        {
            if (currency == "USD" || currency == "EUR")
            {
                MessageBox.Show("Можливі тільки перекази в гривнях.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void ProcessTransaction(double totalAmount, double sum)
        {
            DateTime transactionDate = DateTime.Now;
            var transactionNumber = "P" + new string(Enumerable.Range(0, 10).Select(_ => random.Next(0, 10).ToString()[0]).ToArray());

            dataBase.openConnection();
            SqlTransaction dbTransaction = dataBase.getSqlConnection().BeginTransaction();

            try
            {
                ExecuteNonQuery("UPDATE BankingCard SET balance = balance - @totalAmount WHERE cardNumber = @cardNumber",
                    new Dictionary<string, object>
                    {
                        {"@totalAmount", totalAmount},
                        {"@cardNumber", textBox_cardNumber.Text}
                    },
                    dbTransaction);

                ExecuteNonQuery("INSERT INTO Transactions (transactionType, transactionDestination, transactionDate, transactionNumber, transactionValue, ID_Card) " +
                                "VALUES (@transactionType, @transactionDestination, @transactionDate, @transactionNumber, @totalAmount, " +
                                "(SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber))",
                    new Dictionary<string, object>
                    {
                        {"@transactionType", selectedServiceName},
                        {"@transactionDestination", textBox_bill.Text},
                        {"@transactionDate", transactionDate},
                        {"@transactionNumber", transactionNumber},
                        {"@totalAmount", totalAmount},
                        {"@cardNumber", textBox_cardNumber.Text}
                    },
                    dbTransaction);

                ExecuteNonQuery("UPDATE Services SET serviceBalance = serviceBalance + @sum WHERE serviceName = @selectedServiceName",
                    new Dictionary<string, object>
                    {
                        {"@sum", sum},
                        {"@selectedServiceName", selectedServiceName}
                    },
                    dbTransaction);

                dbTransaction.Commit();
                MessageBox.Show("Транзакція успішно завершена.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                dbTransaction.Rollback();
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dataBase.closeConnection();
            }
        }

        private void button_payment_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs(out double sum)) return;
            if (!CheckCardDetails(out double cardBalance, out string currency)) return;
            if (!IsValidCurrency(currency)) return;

            double totalAmount = sum * 1.01;

            if (totalAmount > cardBalance)
            {
                MessageBox.Show("Недостатньо коштів на картці.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ProcessTransaction(totalAmount, sum);
            Close();
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
