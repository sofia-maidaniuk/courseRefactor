using System;
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
using Bank.DataClasses;

namespace Bank
{
    public partial class Send_to : Window
    {
        private readonly int _clientId;
        public string BankCard { get; set; }
        public string CardNumber { get; set; }
        public string CVV { get; set; }
        public string ExpiryDate { get; set; }

        DataBase dataBase = new DataBase();
        Random random = new Random();
        SqlDataAdapter adapter = new SqlDataAdapter();
        DataTable table = new DataTable();
        public Send_to(int clientId, string bankCard, string cardNumber, string cvvCode, string expiryDate)
        {
            InitializeComponent();
            _clientId = clientId;
            BankCard = bankCard;
            CardNumber = cardNumber;
            CVV = cvvCode;
            ExpiryDate = expiryDate;
            DataContext = this;
        }

        private void Send_to_Loaded(object sender, RoutedEventArgs e)
        {
            textBox_cardNumber_to.Text = DataStorage.bankCard;
            textBox_cardNumber.Text = DataStorage.cardNumber;
        }

        private void button_send_to_Click(object sender, RoutedEventArgs e)
        {
            double dolar = 39.4;
            double euro = 42.3;

            var cardNumber = textBox_cardNumber.Text.ToString();
            var cvvCode = textBox_cvvCode.Text.ToString();
            var cardDate = textBox_date.Text.ToString();

            var cardNumber_to = textBox_cardNumber_to.Text.ToString();

            double sum = Convert.ToDouble(textBox_sum.Text);

            var currency = "";
            var currency_to = "";
            var cvvCodeCheck = "";
            var cardDateCheck = "";
            double cardBalanceCheck = 0;
            bool error = false;

            string queryCheckCard = $"select cvvCode, CONCAT(FORMAT(cardDate, '%M'),'/', FORMAT(cardDate, '%y')), " +
                $"balance, currency from BankingCard where cardNumber = '{cardNumber}'";
            SqlCommand commandCheckCard = new SqlCommand(queryCheckCard, dataBase.getSqlConnection());
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

            var queryCheckCard_to = $"select ID_Card, currency from BankingCard where cardNumber = '{cardNumber_to}'";
            SqlCommand commandCheckCard_to = new SqlCommand(queryCheckCard_to, dataBase.getSqlConnection());

            adapter.SelectCommand = commandCheckCard_to;
            adapter.Fill(table);

            SqlDataReader reader1 = commandCheckCard_to.ExecuteReader();
            while (reader1.Read())
            {
                currency_to = reader1[1].ToString();
            }
            reader1.Close();

            if(table.Rows.Count == 0)
            {
                MessageBox.Show("Картка одержувача не знайдена.", "Відміна", MessageBoxButton.OK, MessageBoxImage.Information);
                error = true;
            }

            if(Convert.ToDouble(sum) < 1.00)
            {
                MessageBox.Show("Мінімальна сума переказу 1.00 UAH", "Відміна", MessageBoxButton.OK, MessageBoxImage.Information);
                error = true;
            }

            if(cardNumber == cardNumber_to)
            {
                MessageBox.Show("Ви не можете переказати кошти на ту ж саму карту.", "Відміна", MessageBoxButton.OK, MessageBoxImage.Information);
                error = true;
            }

            if (sum > cardBalanceCheck)
            {
                MessageBox.Show("Недостатньо коштів на картці.", "Відміна", MessageBoxButton.OK, MessageBoxImage.Information);
                error = true;
            }

            if (error == false)
            {
                DataStorage.bankCard = textBox_cardNumber.Text.ToString();
                Validation validation = new Validation();
                validation.ShowDialog();

                if(DataStorage.attempts > 0)
                {
                    DateTime transactionDate = DateTime.Now;
                    var transactionNumber = "P";
                    for(int i = 0; i < 10; i++)
                    {
                        transactionNumber += Convert.ToString(random.Next(0, 10));
                    }
                    var queryTransiction1 = $"";
                    var queryTransiction2 = $"";

                    if(currency == "UAH" && currency_to == "USD")
                    {
                        queryTransiction1 = $"update BankingCard set balance = balance - {sum} where cardNumber = '{cardNumber}'";
                        queryTransiction2 = $"update BankingCard set balance = balance + {sum /= dolar} where cardNumber = '{cardNumber_to}'";
                    }
                    else if(currency == "UAH" && currency_to == "EUR"  )
                    {
                        queryTransiction1 = $"update BankingCard set balance = balance - {sum} where cardNumber = '{cardNumber}'";
                        queryTransiction2 = $"update BankingCard set balance = balance + {sum /= euro} where cardNumber = '{cardNumber_to}'";
                    }
                    else if(currency == "USD" && currency_to == "UAH")
                    {
                        queryTransiction1 = $"update BankingCard set balance = balance - {sum} where cardNumber = '{cardNumber}'";
                        queryTransiction2 = $"update BankingCard set balance = balance + {sum *= dolar} where cardNumber = '{cardNumber_to}'";
                    }
                    else if(currency == "USD" && currency_to == "EUR")
                    {
                        queryTransiction1 = $"update BankingCard set balance = balance - {sum} where cardNumber = '{cardNumber}'";
                        queryTransiction2 = $"update BankingCard set balance = balance + {sum *= 0.91} where cardNumber = '{cardNumber_to}'";
                    }
                    else if(currency == "EUR" && currency_to == "UAH")
                    {
                        queryTransiction1 = $"update BankingCard set balance = balance - {sum} where cardNumber = '{cardNumber}'";
                        queryTransiction2 = $"update BankingCard set balance = balance + {sum *= euro} where cardNumber = '{cardNumber_to}'";
                    }
                    else if(currency == "EUR" && currency_to == "USD")
                    {
                        queryTransiction1 = $"update BankingCard set balance = balance - {sum} where cardNumber = '{cardNumber}'";
                        queryTransiction2 = $"update BankingCard set balance = balance + {sum*=1.06} where cardNumber = '{cardNumber_to}'";
                    }
                    else
                    {
                        queryTransiction1 = $"update BankingCard set balance = balance - {sum} where cardNumber = '{cardNumber}'";
                        queryTransiction2 = $"update BankingCard set balance = balance + {sum} where cardNumber = '{cardNumber_to}'";
                    }

                   var command1 = new SqlCommand(queryTransiction1, dataBase.getSqlConnection());
                    var command2 = new SqlCommand(queryTransiction2, dataBase.getSqlConnection());

                    dataBase.openConnection();
                    command1.ExecuteNonQuery();
                    command2.ExecuteNonQuery();
                    dataBase.closeConnection();

                    Close();
                }
            }
        }
        private void label_cvvCode_click(object sender, MouseButtonEventArgs e)
        {
            if (textBox_cvvCode.Text.ToString() == "***")
                textBox_cvvCode.Text = DataStorage.cvvCode;
            else
                textBox_cvvCode.Text = "***";
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
