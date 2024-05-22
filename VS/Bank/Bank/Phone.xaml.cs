using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
using Bank.DataClasses;
using Bank.Classes;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;

namespace Bank
{
    public partial class Phone : Window
    {
        private readonly int _clientId;
        public string BankCard { get; set; }
        public string CardNumber { get; set; }
        public string CVV { get; set; }
        public string ExpiryDate { get; set; }
        public string PhoneNumber { get; set; }

        DataBase dataBase = new DataBase();
        Random random = new Random();
        SqlDataAdapter adapter = new SqlDataAdapter();
        DataTable table = new DataTable();
        public Phone(int clientId, string bankCard, string cardNumber, string cvvCode, string expiryDate, string phoneNumber)
        {
            InitializeComponent();
            _clientId = clientId;
            BankCard = bankCard;
            CardNumber = cardNumber;
            CVV = cvvCode;
            ExpiryDate = expiryDate;
            PhoneNumber = phoneNumber;
            DataContext = this;
        }

        private void Phone_Loaded(object sender, RoutedEventArgs e)
        {
            textBox_phoneNumber.Text = DataStorage.phone_Number;
            textBox_cardNumber.Text = DataStorage.cardNumber;
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void button_phone_Click(object sender, RoutedEventArgs e)
        {
            bool phoneNumberCheck = false;

            if (phoneNumberCheck == false)
            {
                var phoneNumber = textBox_phoneNumber.Text;
                if (!double.TryParse(textBox_sum.Text, out double sum))
                {
                    MessageBox.Show("Будь ласка, введіть коректну суму.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var cardNumber = textBox_cardNumber.Text;
                var cvvCode = textBox_cvvCode.Text;
                var cardDate = textBox_date.Text;

                var cvvCodeCheck = "";
                var cardDateCheck = "";
                var currency = "";
                double cardBalanceCheck = 0;
                bool error = false;

                MessageBoxButton btn = MessageBoxButton.OK;
                MessageBoxImage img = MessageBoxImage.Information;

                string caption = "Дата збереження";

                if (!Regex.IsMatch(textBox_phoneNumber.Text, "^[+][3][8][0][0-9]{7,14}$"))
                {
                    MessageBox.Show("Номер телефону повинен бути у форматі +380XXXXXXXXX", caption, btn, img);
                    textBox_phoneNumber.Focus();
                    return;
                }

                string queryCheckCard = $"select cvvCode, CONCAT(FORMAT(cardDate, '%M'),'/', FORMAT(cardDate, '%y')), " +
                    $"balance, currency from BankingCard where cardNumber = @cardNumber";

                SqlCommand commandCheckCard = new SqlCommand(queryCheckCard, dataBase.getSqlConnection());
                commandCheckCard.Parameters.AddWithValue("@cardNumber", cardNumber);
                dataBase.openConnection();
                SqlDataReader reader = commandCheckCard.ExecuteReader();

                while (reader.Read())
                {
                    cvvCodeCheck = reader[0].ToString();
                    cardDateCheck = reader[1].ToString();
                    cardBalanceCheck = Convert.ToDouble(reader[2].ToString());
                    currency = reader[3].ToString();
                }
                reader.Close();

                if(cardNumber == null)
                {
                    MessageBox.Show("Введіть дані картки.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    error = true;
                }

                if (currency == "USD" || currency == "EUR")
                {
                    MessageBox.Show("Вибачте, але можливі тільки перекази в гривнях.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    error = true;
                }

                if (cvvCode != cvvCodeCheck)
                {
                    MessageBox.Show("Невірний CVV код.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    error = true;
                }

                if (cardDate != cardDateCheck)
                {
                    MessageBox.Show("Невірний термін дії картки.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    error = true;
                }

                if (!error)
                {
                    DataStorage.bankCard = textBox_cardNumber.Text;
                    Validation validation = new Validation();
                    validation.ShowDialog();

                    if (DataStorage.attempts > 0)
                    {
                        DateTime transactionDate = DateTime.Now;
                        var transactionNumber = "P";
                        for (int i = 0; i < 10; i++)
                        {
                            transactionNumber += random.Next(0, 10).ToString();
                        }

                        double commissionRate = 0.01;
                        TransferTransaction transaction = new TransferTransaction(commissionRate)
                        {
                            TransactionType = "Поповнення мобільного",
                            TransactionDestination = phoneNumber,
                            TransactionDate = transactionDate,
                            TransactionNumber = transactionNumber,
                            TransactionValue = sum
                        };

                        double totalAmount = transaction.CalculateTotalAmount();

                        if (totalAmount > cardBalanceCheck)
                        {
                            MessageBox.Show("Недостатньо коштів на картці.", "Відміна", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        if (totalAmount < 5)
                        {
                            MessageBox.Show("Мінімальна сума переказу 5 гривень.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        var queryPhone1 = "update BankingCard set balance = balance - @totalAmount where cardNumber = @cardNumber";
                        var queryTransaction = "INSERT INTO Transactions (transactionType, transactionDestination, transactionDate, transactionNumber, transactionValue, ID_Card) " +
                            "VALUES ('Поповнення мобільного', @transactionDestination, @transactionDate, @transactionNumber, @totalAmount, " +
                            "(SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber))";

                        var commandPhone1 = new SqlCommand(queryPhone1, dataBase.getSqlConnection());
                        commandPhone1.Parameters.AddWithValue("@totalAmount", totalAmount);
                        commandPhone1.Parameters.AddWithValue("@cardNumber", cardNumber);

                        var commandTransaction = new SqlCommand(queryTransaction, dataBase.getSqlConnection());
                        commandTransaction.Parameters.AddWithValue("@transactionType", transaction.TransactionType);
                        commandTransaction.Parameters.AddWithValue("@transactionDestination", transaction.TransactionDestination);
                        commandTransaction.Parameters.AddWithValue("@transactionDate", transaction.TransactionDate);
                        commandTransaction.Parameters.AddWithValue("@transactionNumber", transaction.TransactionNumber);
                        commandTransaction.Parameters.AddWithValue("@totalAmount", totalAmount);
                        commandTransaction.Parameters.AddWithValue("@cardNumber", cardNumber);

                        dataBase.openConnection();

                        SqlTransaction dbTransaction = dataBase.getSqlConnection().BeginTransaction();
                        commandPhone1.Transaction = dbTransaction;
                        commandTransaction.Transaction = dbTransaction;

                        try
                        {
                            int rowsAffected1 = commandPhone1.ExecuteNonQuery();
                            int rowsAffected2 = commandTransaction.ExecuteNonQuery();

                            if (rowsAffected1 > 0 && rowsAffected2 > 0)
                            {
                                dbTransaction.Commit();
                                MessageBox.Show("Транзакція успішно завершена.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                dbTransaction.Rollback();
                                MessageBox.Show("Помилка під час виконання транзакції.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
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

                        Close();
                    }
                }
            }
        }

    }
}
