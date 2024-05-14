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

        DataBase dataBase = new DataBase();
        Random random = new Random();
        SqlDataAdapter adapter = new SqlDataAdapter();
        DataTable table = new DataTable();
        public Send_to(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
        }
        private void label_cvvCode_click(object sender, MouseButtonEventArgs e)
        {
            if (label_cvvCode.Content.ToString() == "***")
            {
                label_cvvCode.Content = DataStorage.cvvCode;
            }
            else
            {
                label_cvvCode.Content = "****";
            }
        }

        private void Send_to_Loaded(object sender, RoutedEventArgs e)
        {
            label_cardNumber_to.Content = DataStorage.bankCard;
            label_cardNumber.Content = DataStorage.bankCard;
        }

        private void button_send_to_Click(object sender, RoutedEventArgs e)
        {
            double dolar = 39.4;
            double euro = 42.3;

            var cardNumber = label_cardNumber.Content.ToString();
            var cvvCode = label_cvvCode.Content.ToString();
            var cardDate = label_date.Content.ToString();

            var cardNumber_to = label_cardNumber_to.Content.ToString();

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

            if (cvvCode != cvvCodeCheck || cardDate != cardDateCheck)
            {
                MessageBox.Show("Невірний CVV код або термін дії картки.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                error = true;
            }

        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
