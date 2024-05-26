using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Bank.DataClasses;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Bank.Classes;

namespace Bank
{
    public partial class ChangePhone : Window
    {
        DataBase dataBase = new DataBase();
        private readonly int _clientId;
        public ChangePhone(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
        }

        private void ChangePhone_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxButton btn = MessageBoxButton.OK;
            MessageBoxImage img = MessageBoxImage.Information;

            string caption = "Дата збереження";

            if (!Regex.IsMatch(textBox_phone.Text, "^[+][3][8][0][0-9]{7,14}$"))
            {
                System.Windows.MessageBox.Show("Номер телефону повинен бути у форматі +380XXXXXXXXX", caption, btn, img);
                textBox_phone.Focus();
                return;
            }

            var phoneNumber = textBox_phone.Text;

            string queryCheckPhone = "SELECT COUNT(*) FROM Klient WHERE phone_Number = @phoneNumber";
            SqlCommand checkPhoneCommand = new SqlCommand(queryCheckPhone, dataBase.getSqlConnection());
            checkPhoneCommand.Parameters.AddWithValue("@phoneNumber", phoneNumber);

            dataBase.openConnection();
            int phoneCount = (int)checkPhoneCommand.ExecuteScalar();

            if (phoneCount > 0)
            {
                MessageBox.Show("Користувач з таким номером телефону вже існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                dataBase.closeConnection();
                return;
            }

            var querystringPhone = $"update Klient set phone_Number = '{phoneNumber}' where ID_Klient = {_clientId}";
            var command = new SqlCommand(querystringPhone, dataBase.getSqlConnection());
            dataBase.openConnection();
            if(command.ExecuteNonQuery() == 1)
            {
                System.Windows.MessageBox.Show("Номер телефону успішно змінено", caption, btn, img);
                Close();
            }
            else
                System.Windows.MessageBox.Show("Помилка зміни номеру телефону", caption, btn, img);
            
            dataBase.closeConnection();
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
