using Bank.DataClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bank
{
    public partial class AddBankCard : Window
    {
        private readonly int _clientId;
        private readonly DataBase dataBase = new DataBase();
        private readonly Random random = new Random();

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
            string pin;
            if (!ValidatePasswords(out pin)) return;

            var typeCard = ((ComboBoxItem)typeCardComboBox.SelectedItem).Content.ToString();
            var currency = ((ComboBoxItem)currencyComboBox.SelectedItem).Content.ToString();
            var paySystem = ((ComboBoxItem)paySystemComboBox.SelectedItem).Content.ToString();

            (string cardNumber, string cvvCode) = GenerateCardDetails(paySystem);

            if (!IsCardNumberUnique(cardNumber))
            {
                MessageBox.Show("Не вдалося згенерувати унікальний номер картки. Спробуйте ще раз.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (InsertBankCard(typeCard, cardNumber, cvvCode, currency, paySystem, pin))
            {
                MessageBox.Show("Карта успішно додана!", "Успіх!", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            else
            {
                MessageBox.Show("Карту не додано! Помилка!", "Помилка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidatePasswords(out string pin)
        {
            pin = textBox_password.Text;
            string password2 = textBox2_password.Text;

            if (pin.Length != 4 || password2.Length != 4 || pin != password2)
            {
                MessageBox.Show("Паролі не співпадають або не складаються з 4 символів. Будь ласка, введіть однакові паролі.",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private (string cardNumber, string cvvCode) GenerateCardDetails(string paySystem)
        {
            string cardNumber = paySystem == "Visa" ? "4" : "5";
            for (int i = 1; i < 16; i++)
            {
                cardNumber += random.Next(0, 10).ToString();
            }

            string cvvCode = "";
            for (int i = 0; i < 3; i++)
            {
                cvvCode += random.Next(0, 10).ToString();
            }

            return (cardNumber, cvvCode);
        }

        private bool IsCardNumberUnique(string cardNumber)
        {
            string query = "SELECT COUNT(*) FROM BankingCard WHERE CardNumber = @cardNumber";
            using (var command = new SqlCommand(query, dataBase.getSqlConnection()))
            {
                command.Parameters.AddWithValue("@cardNumber", cardNumber);
                dataBase.openConnection();
                int count = (int)command.ExecuteScalar();
                dataBase.closeConnection();
                return count == 0;
            }
        }

        private bool InsertBankCard(string typeCard, string cardNumber, string cvvCode, string currency, string paySystem, string pin)
        {
            string query = "INSERT INTO BankingCard(cardType, cardNumber, cvvCode, balance, currency, paySystem, cardDate, pin, ID_Klient) " +
                           "VALUES (@typeCard, @cardNumber, @cvvCode, 0, @currency, @paySystem, @cardDate, @pin, @clientId)";

            using (var command = new SqlCommand(query, dataBase.getSqlConnection()))
            {
                command.Parameters.AddWithValue("@typeCard", typeCard);
                command.Parameters.AddWithValue("@cardNumber", cardNumber);
                command.Parameters.AddWithValue("@cvvCode", cvvCode);
                command.Parameters.AddWithValue("@currency", currency);
                command.Parameters.AddWithValue("@paySystem", paySystem);
                command.Parameters.AddWithValue("@cardDate", DateTime.Now.AddYears(4).ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@pin", pin);
                command.Parameters.AddWithValue("@clientId", _clientId);

                dataBase.openConnection();
                int result = command.ExecuteNonQuery();
                dataBase.closeConnection();
                return result == 1;
            }
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
