using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Bank.DataClasses;
using System.Security.Authentication;
using System.Windows.Input;

namespace Bank
{
    public partial class Credit : Window
    {
        private readonly int _clientId;
        DataBase dataBase = new DataBase();
        Random random = new Random();
        SqlDataAdapter adapter = new SqlDataAdapter();
        DataTable table = new DataTable();
        Validation validation = new Validation();
        public Credit(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
            LoadCreditData();
        }

        private void Credit_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCreditData();
        }

        private void LoadCreditData()
        {
            string totalSum = "0";
            string sum = "0";
            DateTime date = new DateTime();
            StackPanelCredit.Visibility = Visibility.Hidden;
            button_pay.Visibility = Visibility.Hidden;
            payCredit.Visibility = Visibility.Hidden;
            string idCredit = "";

            double totalSumCheck = 0;
            double sumCheck = 0;

            string querystringCredit = $"SELECT creditTotalSum, creditSum, ID_Credit FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = '{DataStorage.cardNumber}')";
            SqlCommand commandCheck = new SqlCommand(querystringCredit, dataBase.getSqlConnection());
            dataBase.openConnection();
            SqlDataReader reader = commandCheck.ExecuteReader();
            while (reader.Read())
            {
                totalSumCheck = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader[0]);
                sumCheck = reader.IsDBNull(1) ? 0 : Convert.ToDouble(reader[1]);
                idCredit = reader[2].ToString();
            }
            reader.Close();

            // Якщо кредит повністю погашений - видаляємо його
            if (sumCheck >= totalSumCheck)
            {
                string querystringDelete = $"DELETE FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = '{DataStorage.cardNumber}')";
                SqlCommand commandDelete = new SqlCommand(querystringDelete, dataBase.getSqlConnection());
                commandDelete.ExecuteNonQuery();

                // Після видалення ховаємо панелі
                StackPanelCredit.Visibility = Visibility.Hidden;
                button_pay.Visibility = Visibility.Hidden;
                payCredit.Visibility = Visibility.Hidden;

                dataBase.closeConnection();
                return;
            }

            string querySelectIdCard = $"SELECT Credits.creditTotalSum, Credits.creditSum, Credits.creditDate " +
                $"FROM Credits INNER JOIN BankingCard ON Credits.ID_Card = BankingCard.ID_Card WHERE BankingCard.cardNumber = '{DataStorage.cardNumber}'";
            SqlCommand commandSelectCredit = new SqlCommand(querySelectIdCard, dataBase.getSqlConnection());
            SqlDataReader reader1 = commandSelectCredit.ExecuteReader();
            while (reader1.Read())
            {
                totalSum = reader1.IsDBNull(0) ? "0" : reader1[0].ToString();
                sum = reader1.IsDBNull(1) ? "0" : reader1[1].ToString();
                date = reader1.IsDBNull(2) ? DateTime.MinValue : Convert.ToDateTime(reader1[2]);
            }
            reader1.Close();

            adapter.SelectCommand = commandSelectCredit;
            adapter.Fill(table);
            if (table.Rows.Count > 0)
            {
                label_totalSum.Content = Math.Round(Convert.ToDouble(totalSum), 2);
                label_sum.Content = Math.Round(Convert.ToDouble(sum), 2);
                label_creditDate.Content = date.ToShortDateString();

                StackPanelCredit.Visibility = Visibility.Visible;
                button_pay.Visibility = Visibility.Visible;
                payCredit.Visibility = Visibility.Visible;

                LoadRepaymentData(idCredit);
            }
            dataBase.closeConnection();
        }

        private void CalcCredit()
        {
            double monthPercent = 0.01;
            double sum = Convert.ToDouble(textBox_creditSum.Text);
            int month = Convert.ToInt32(textBox_month.Text);
            double res = sum * (monthPercent + (monthPercent / (Math.Pow(1 + monthPercent, month) - 1)));
            label_monthPay.Content = Math.Round(res, 2).ToString();
        }

        private void LoadRepaymentData(string idCredit)
        {
            double monthPay = 0;
            DateTime dateRepay = DateTime.MinValue;

            string querystringRepay = $"SELECT repaymentDate, repaymentSum FROM Credits WHERE ID_Credit = '{idCredit}'";
            SqlCommand commandRepay = new SqlCommand(querystringRepay, dataBase.getSqlConnection());
            SqlDataReader reader2 = commandRepay.ExecuteReader();
            if (reader2.Read())
            {
                dateRepay = Convert.ToDateTime(reader2["repaymentDate"]);
                monthPay = Convert.ToDouble(reader2["repaymentSum"]); 
            }
            reader2.Close();

            label_toPaySum.Content = Math.Round(monthPay, 2).ToString();
            label_nextPayDate.Content = dateRepay.ToShortDateString();
        }


        private void button_take_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(textBox_creditSum.Text, out double sum))
            {
                MessageBox.Show("Будь ласка, введіть коректну суму.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(textBox_month.Text, out int month))
            {
                MessageBox.Show("Будь ласка, введіть коректну кількість місяців.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(textBox_creditSum.Text) || string.IsNullOrEmpty(textBox_month.Text))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string cardType = "";
            string checkCardTypeQuery = "SELECT cardType FROM BankingCard WHERE cardNumber = @cardNumber";
            SqlCommand checkCardTypeCommand = new SqlCommand(checkCardTypeQuery, dataBase.getSqlConnection());
            checkCardTypeCommand.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);

            dataBase.openConnection();
            SqlDataReader cardTypeReader = checkCardTypeCommand.ExecuteReader();
            if (cardTypeReader.Read())
            {
                cardType = cardTypeReader["cardType"].ToString();
            }
            cardTypeReader.Close();
            dataBase.closeConnection();

            if (cardType != "Кредитна")
            {
                MessageBox.Show("Кредит можна взяти лише власникам кредитної карти.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string checkCreditQuery = "SELECT COUNT(*) FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber)";
            SqlCommand checkCreditCommand = new SqlCommand(checkCreditQuery, dataBase.getSqlConnection());
            checkCreditCommand.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);

            dataBase.openConnection();
            int creditCount = (int)checkCreditCommand.ExecuteScalar();
            dataBase.closeConnection();

            if (creditCount > 0)
            {
                MessageBox.Show("У вас вже є активний кредит. Неможливо взяти ще один кредит.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CalcCredit();

            DataStorage.bankCard = DataStorage.cardNumber;
            validation.ValidationCompleted += OnValidationCompleted;
            validation.Show();
        }
        private void OnValidationCompleted(bool isValid)
        {
            if (isValid)
            {
                if (DataStorage.attempts > 0)
                {
                    double totalSum = Convert.ToDouble(label_monthPay.Content) * Convert.ToInt32(textBox_month.Text);
                    double creditAmount = Convert.ToDouble(textBox_creditSum.Text);
                    DateTime creditDate = DateTime.Now;
                    DateTime repaymentDate = creditDate.AddMonths(1);
                    double payment = Convert.ToDouble(label_monthPay.Content);

                    dataBase.openConnection();
                    SqlTransaction dbTransaction = dataBase.getSqlConnection().BeginTransaction();
                    try
                    {
                        string queryCredit = "INSERT INTO Credits (creditTotalSum, creditSum, creditDate, creditStatus, repaymentDate, repaymentSum, ID_Card) " +
                                             "VALUES (@totalSum, @creditSum, @creditDate, @creditStatus, @repaymentDate, @repaymentSum, " +
                                             "(SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber))";
                        SqlCommand commandCredit = new SqlCommand(queryCredit, dataBase.getSqlConnection(), dbTransaction);
                        commandCredit.Parameters.AddWithValue("@totalSum", totalSum);
                        commandCredit.Parameters.AddWithValue("@creditSum", 0);
                        commandCredit.Parameters.AddWithValue("@creditDate", creditDate);
                        commandCredit.Parameters.AddWithValue("@creditStatus", 0);
                        commandCredit.Parameters.AddWithValue("@repaymentDate", repaymentDate);
                        commandCredit.Parameters.AddWithValue("@repaymentSum", payment);
                        commandCredit.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);
                        commandCredit.ExecuteNonQuery();

                        string idCredit = "";
                        string querySelectIdCredit = "SELECT ID_Credit FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber)";
                        SqlCommand commandSelectIdCredit = new SqlCommand(querySelectIdCredit, dataBase.getSqlConnection(), dbTransaction);
                        commandSelectIdCredit.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);
                        SqlDataReader reader = commandSelectIdCredit.ExecuteReader();
                        while (reader.Read())
                        {
                            idCredit = reader[0].ToString();
                        }
                        reader.Close();

                        string queryUpdateBalance = "UPDATE BankingCard SET balance = balance + @creditAmount WHERE cardNumber = @cardNumber";
                        SqlCommand commandUpdateBalance = new SqlCommand(queryUpdateBalance, dataBase.getSqlConnection(), dbTransaction);
                        commandUpdateBalance.Parameters.AddWithValue("@creditAmount", creditAmount);
                        commandUpdateBalance.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);
                        commandUpdateBalance.ExecuteNonQuery();

                        CreateTransaction("Кредит", creditAmount, dbTransaction);

                        dbTransaction.Commit();

                        MessageBox.Show("Кредит успішно взято", "Успішно", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadCreditData();
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
            }

            validation.ValidationCompleted -= OnValidationCompleted;
        }


        private void button_payment_Click(object sender, RoutedEventArgs e)
        {
            dataBase.openConnection();

            // Отримуємо залишок балансу картки
            double cardBalanceCheck = 0;
            string querystringCheckCard = "SELECT balance FROM BankingCard WHERE cardNumber = @cardNumber";
            SqlCommand commandCheckCard = new SqlCommand(querystringCheckCard, dataBase.getSqlConnection());
            commandCheckCard.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);

            SqlDataReader reader = commandCheckCard.ExecuteReader();
            if (reader.Read())
            {
                cardBalanceCheck = Convert.ToDouble(reader[0]);
            }
            reader.Close();

            // Отримуємо дані кредиту
            double totalSum = 0, paidSum = 0, nextPayment = 0;
            DateTime nextPayDate = DateTime.MinValue;

            string queryCredit = "SELECT creditTotalSum, creditSum, repaymentSum, repaymentDate FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber)";
            SqlCommand commandCredit = new SqlCommand(queryCredit, dataBase.getSqlConnection());
            commandCredit.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);

            SqlDataReader creditReader = commandCredit.ExecuteReader();
            if (creditReader.Read())
            {
                totalSum = Convert.ToDouble(creditReader["creditTotalSum"]);
                paidSum = Convert.ToDouble(creditReader["creditSum"]);
                nextPayment = Convert.ToDouble(creditReader["repaymentSum"]);
                nextPayDate = Convert.ToDateTime(creditReader["repaymentDate"]);
            }
            creditReader.Close();

            // Якщо баланс недостатній - виводимо помилку
            if (nextPayment > cardBalanceCheck)
            {
                MessageBox.Show("Недостатньо коштів на картці", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                dataBase.closeConnection();
                return;
            }

            // Виконуємо платіж
            ProcessPayment(nextPayDate, nextPayment);
        }


        private void ProcessPayment(DateTime toPayDate, double payment)
        {
            bool success = false;
            SqlTransaction dbTransaction = dataBase.getSqlConnection().BeginTransaction();

            try
            {
                // Оновлюємо виплачену суму та дату наступного платежу
                string queryUpdateCredit = @"
            UPDATE Credits 
            SET repaymentDate = DATEADD(MONTH, 1, repaymentDate), 
                repaymentSum = @payment, 
                creditSum = creditSum + @payment 
            WHERE ID_Credit = (SELECT ID_Credit FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber))";

                SqlCommand commandUpdateCredit = new SqlCommand(queryUpdateCredit, dataBase.getSqlConnection(), dbTransaction);
                commandUpdateCredit.Parameters.AddWithValue("@payment", payment);
                commandUpdateCredit.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);
                commandUpdateCredit.ExecuteNonQuery();

                // Створюємо транзакцію
                CreateTransaction("Платіж по кредиту", payment, dbTransaction);

                // Оновлюємо баланс картки (списуємо гроші)
                string queryUpdateBalance = "UPDATE BankingCard SET balance = balance - @payment WHERE cardNumber = @cardNumber";
                SqlCommand commandUpdateBalance = new SqlCommand(queryUpdateBalance, dataBase.getSqlConnection(), dbTransaction);
                commandUpdateBalance.Parameters.AddWithValue("@payment", payment);
                commandUpdateBalance.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);
                commandUpdateBalance.ExecuteNonQuery();

                // Перевіряємо, чи повністю виплачено кредит
                double creditSum = 0, totalSum = 0;
                string checkCreditQuery = "SELECT creditSum, creditTotalSum FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber)";
                SqlCommand checkCreditCommand = new SqlCommand(checkCreditQuery, dataBase.getSqlConnection(), dbTransaction);
                checkCreditCommand.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);

                SqlDataReader reader = checkCreditCommand.ExecuteReader();
                if (reader.Read())
                {
                    creditSum = Convert.ToDouble(reader["creditSum"]);
                    totalSum = Convert.ToDouble(reader["creditTotalSum"]);
                }
                reader.Close();

                if (creditSum >= totalSum)
                {
                    // Видаляємо кредит, якщо все виплачено
                    string deleteCreditQuery = "DELETE FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber)";
                    SqlCommand deleteCreditCommand = new SqlCommand(deleteCreditQuery, dataBase.getSqlConnection(), dbTransaction);
                    deleteCreditCommand.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);
                    deleteCreditCommand.ExecuteNonQuery();

                    StackPanelCredit.Visibility = Visibility.Hidden;
                    button_pay.Visibility = Visibility.Hidden;
                    payCredit.Visibility = Visibility.Hidden;

                    MessageBox.Show("Кредит повністю погашено.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                dbTransaction.Commit();
                success = true;
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

            if (success)
            {
                MessageBox.Show("Платіж успішно виконано.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCreditData();
            }
        }

        private void CreateTransaction(string transactionType, double transactionValue, SqlTransaction dbTransaction)
        {
            string transactionNumber = "P";
            for (int i = 0; i < 10; i++)
            {
                transactionNumber += random.Next(0, 10).ToString();
            }

            string queryTransaction = "INSERT INTO Transactions (transactionType, transactionDestination, transactionDate, transactionNumber, transactionValue, ID_Card) " +
                    "VALUES (@transactionType, @transactionDestination, @transactionDate, @transactionNumber, @transactionValue, (SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber))";

            SqlCommand commandTransaction = new SqlCommand(queryTransaction, dataBase.getSqlConnection(), dbTransaction);
            commandTransaction.Parameters.AddWithValue("@transactionType", transactionType);
            commandTransaction.Parameters.AddWithValue("@transactionDestination", DataStorage.cardNumber);
            commandTransaction.Parameters.AddWithValue("@transactionDate", DateTime.Now);
            commandTransaction.Parameters.AddWithValue("@transactionNumber", transactionNumber);
            commandTransaction.Parameters.AddWithValue("@transactionValue", transactionValue);
            commandTransaction.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);

            commandTransaction.ExecuteNonQuery();
        }

        private void ClearCreditData()
        {
            string queryClearCredits = "DELETE FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber)";
            SqlCommand commandClearCredits = new SqlCommand(queryClearCredits, dataBase.getSqlConnection());
            commandClearCredits.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);

            dataBase.openConnection();
            commandClearCredits.ExecuteNonQuery();
            dataBase.closeConnection();
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}