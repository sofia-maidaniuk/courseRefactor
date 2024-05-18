using System;
using System.Collections.Generic;
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
using System.Data.SqlClient;
using System.Data;
using Xceed.Wpf.Toolkit;
using System.Text.RegularExpressions;
using System.Drawing;

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
