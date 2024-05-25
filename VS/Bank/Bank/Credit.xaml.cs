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
            string idCredit = "";

            double totalSumCheck = 0;
            double sumCheck = 0;

            string querystringCredit = $"SELECT creditTotalSum, creditSum FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = '{DataStorage.cardNumber}')";
            SqlCommand commandCheck = new SqlCommand(querystringCredit, dataBase.getSqlConnection());
            dataBase.openConnection();
            SqlDataReader reader = commandCheck.ExecuteReader();
            while (reader.Read())
            {
                totalSumCheck = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader[0]);
                sumCheck = reader.IsDBNull(1) ? 0 : Convert.ToDouble(reader[1]);
            }
            reader.Close();

            if (sumCheck >= totalSumCheck)
            {
                string querystringDelete = $"DELETE FROM Credits WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = '{DataStorage.cardNumber}')";
                SqlCommand commandDelete = new SqlCommand(querystringDelete, dataBase.getSqlConnection());
                commandDelete.ExecuteNonQuery();
            }

            string querySelectIdCard = $"SELECT Credits.ID_Card, Credits.creditTotalSum, Credits.creditSum, Credits.creditDate, Credits.ID_Credit " +
                $"FROM Credits INNER JOIN BankingCard ON Credits.ID_Card = BankingCard.ID_Card WHERE BankingCard.cardNumber = '{DataStorage.cardNumber}'";
            SqlCommand commandSelectCredit = new SqlCommand(querySelectIdCard, dataBase.getSqlConnection());
            SqlDataReader reader1 = commandSelectCredit.ExecuteReader();
            while (reader1.Read())
            {
                totalSum = reader1.IsDBNull(1) ? "0" : reader1[1].ToString();
                sum = reader1.IsDBNull(2) ? "0" : reader1[2].ToString();
                date = reader1.IsDBNull(3) ? DateTime.MinValue : Convert.ToDateTime(reader1[3]);
                idCredit = reader1[4].ToString();
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
            double toPaySum = 0;
            DateTime dateRepay = new DateTime();

            string querystringRepay = $"SELECT repaymentDate, repaymentSum FROM Credits WHERE ID_Credit = '{idCredit}'";
            SqlCommand commandRepay = new SqlCommand(querystringRepay, dataBase.getSqlConnection());
            SqlDataReader reader2 = commandRepay.ExecuteReader();
            while (reader2.Read())
            {
                dateRepay = Convert.ToDateTime(reader2[0].ToString());
                toPaySum = Convert.ToDouble(reader2[1].ToString());
            }
            reader2.Close();

            label_toPaySum.Content = Math.Round(toPaySum, 2).ToString();
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
            DateTime toPayDate = Convert.ToDateTime(label_nextPayDate.Content);
            toPayDate = toPayDate.AddMonths(1);
            double sumToPay = Convert.ToDouble(label_toPaySum.Content);
            bool error = false;

            dataBase.openConnection();
            double cardBalanceCheck = 0;
            string querystringCheckCard = $"SELECT balance FROM BankingCard WHERE cardNumber = '{DataStorage.cardNumber}'";
            SqlCommand commandCheckCard = new SqlCommand(querystringCheckCard, dataBase.getSqlConnection());
            SqlDataReader reader = commandCheckCard.ExecuteReader();
            while (reader.Read())
            {
                cardBalanceCheck = Convert.ToDouble(reader[0]);
            }
            reader.Close();

            double checkSum = Convert.ToDouble(label_toPaySum.Content);
            double checkTotalSum = Convert.ToDouble(label_totalSum.Content);
            bool check = false;


            if (checkSum == checkTotalSum)
            {
                StackPanelCredit.Visibility = Visibility.Hidden;
                button_pay.Visibility = Visibility.Hidden;

                MessageBox.Show("Кредит погашено", "Сповіщення", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearCreditData();
                dataBase.closeConnection();
                Close();
                return;
            }


            if (!check)
            {
                double payment = Convert.ToDouble(label_toPaySum.Content);

                if (payment > cardBalanceCheck)
                {
                    MessageBox.Show("Недостатньо коштів на картці", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    error = true;
                }

                if (!error)
                {
                    ProcessPayment(toPayDate, payment);
                }
            }
        }

        private void ProcessPayment(DateTime toPayDate, double payment)
        {
            bool success = false;

            SqlTransaction dbTransaction = dataBase.getSqlConnection().BeginTransaction();
            try
            {
                string querystringPayCredit = $"UPDATE Credits SET repaymentDate = @repaymentDate, creditSum = creditSum + @payment " +
                    $"WHERE ID_Card = (SELECT ID_Card FROM BankingCard WHERE cardNumber = @cardNumber)";
                string querystringPay = $"UPDATE BankingCard SET balance = balance - @payment WHERE cardNumber = @cardNumber";

                SqlCommand commandPayCredit = new SqlCommand(querystringPayCredit, dataBase.getSqlConnection(), dbTransaction);
                commandPayCredit.Parameters.AddWithValue("@repaymentDate", toPayDate);
                commandPayCredit.Parameters.AddWithValue("@payment", payment);
                commandPayCredit.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);

                SqlCommand commandPay = new SqlCommand(querystringPay, dataBase.getSqlConnection(), dbTransaction);
                commandPay.Parameters.AddWithValue("@payment", payment);
                commandPay.Parameters.AddWithValue("@cardNumber", DataStorage.cardNumber);

                int rowsAffected1 = commandPayCredit.ExecuteNonQuery();
                int rowsAffected2 = commandPay.ExecuteNonQuery();

                if (rowsAffected1 > 0 && rowsAffected2 > 0)
                {
                    CreateTransaction("Платіж по кредиту", payment, dbTransaction);
                    dbTransaction.Commit();
                    success = true;
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

            if (success)
            {
                MessageBox.Show("Транзакція успішно завершена.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
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
