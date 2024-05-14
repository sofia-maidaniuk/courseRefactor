using Bank.DataClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    public partial class AddBankCard : Window
    {
        private readonly int _clientId;

        DataBase dataBase = new DataBase();
        Random random = new Random();
        SqlDataAdapter adapter = new SqlDataAdapter();
        DataTable table = new DataTable();
        public AddBankCard(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
        }

        private void AddBankCard_Loaded(object sender, RoutedEventArgs e)
        {
            typeCardComboBox.SelectedIndex = 0;
            currencyComboBox.SelectedIndex = 0;
            paySystemComboBox.SelectedIndex = 0;
        }
        private void button_createCard_Click(object sender, RoutedEventArgs e)
        {
            var typeCardItem = typeCardComboBox.SelectedItem as ComboBoxItem;
            var typeCard = typeCardItem.Content.ToString();

            var currencyItem = currencyComboBox.SelectedItem as ComboBoxItem;
            var currency = currencyItem.Content.ToString();

            var paySystemItem = paySystemComboBox.SelectedItem as ComboBoxItem;
            var paySystem = paySystemItem.Content.ToString();

            var cardNumber = "";
            var cvvCode = "";
            var pin = "";

            string password1 = textBox_password.Text;
            string password2 = textBox2_password.Text;

            if (password1.Length != 4 || password2.Length != 4 || password1 != password2)
            {
                MessageBox.Show("Паролі не співпадають або не складаються з 4 символів. Будь ласка, введіть однакові паролі, кожен довжиною 4 символи.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                pin = password1;


                bool isCardFree = false;
                DateTime currentDate = DateTime.Now;
                string formattedCurrentDate = currentDate.ToString("yyyy-MM-dd");
                var cardDate = DateTime.Now.AddYears(4);
                string formattedCardDate = cardDate.ToString("yyyy-MM-dd");

                for (int i = 0; i < 3; i++)
                {
                    cvvCode += Convert.ToString(random.Next(0, 10));
                }

                do
                {
                    if (paySystem == "Visa")
                    {
                        cardNumber += "4";
                        for (int i = 0; i < 15; i++)
                        {
                            cardNumber += Convert.ToString(random.Next(0, 10));
                        }
                    }
                    else
                    {
                        cardNumber += "5";
                        for (int i = 0; i < 15; i++)
                        {
                            cardNumber += Convert.ToString(random.Next(0, 10));
                        }
                    }

                    var querystring_CheckBankCard = $"select * from BankingCard where CardNumber = '{cardNumber}'";

                    SqlCommand command_CheckBankCard = new SqlCommand(querystring_CheckBankCard, dataBase.getSqlConnection());
                    adapter.SelectCommand = command_CheckBankCard;
                    adapter.Fill(table);
                    if (table.Rows.Count == 0)
                    {
                        isCardFree = true;
                    }
                } while (isCardFree == false);

                var querystring_AddBankCard = $"insert into BankingCard(cardType, cardNumber, cvvCode, balance, currency, paySystem, cardDate, pin, ID_Klient) " +
                $"values ('{typeCard}', '{cardNumber}', '{cvvCode}', '{0}', '{currency}', '{paySystem}', '{formattedCardDate}', '{pin}', '{_clientId}' )";

                SqlCommand command_AddBankCard = new SqlCommand(querystring_AddBankCard, dataBase.getSqlConnection());
                dataBase.openConnection();

                if (command_AddBankCard.ExecuteNonQuery() == 1)
                {
                    MessageBox.Show("Карта успішно додана!", "Успішно!", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                    MessageBox.Show("Карту не додано! Помилка!", "Додавання не вдалось!", MessageBoxButton.OK, MessageBoxImage.Error);

                dataBase.closeConnection();
            }
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
