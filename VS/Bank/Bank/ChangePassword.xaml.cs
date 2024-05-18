using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class ChangePassword : Window
    {
        DataBase dataBase = new DataBase();
        private readonly int _clientId;
        public ChangePassword(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
        }
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxButton btn = MessageBoxButton.OK;
            MessageBoxImage img = MessageBoxImage.Information;

            string caption = "Дата збереження";

            if (!Regex.IsMatch(textBox_password.Text, "^(?=.*[a-zA-Z])(?=.*\\d).{8,}$"))
            {
                System.Windows.MessageBox.Show("Пароль повинен містити мінімум 1 латинську літеру і 1 цифру і бути довжиною щонайменше 8 символи.", caption, btn, img);
                textBox_password.Focus();
                return;
            }

            var password = textBox_password.Text;
            var querystringPassword = $"update Klient set password_user = '{password}' where ID_Klient = {_clientId}";
            var command = new SqlCommand(querystringPassword, dataBase.getSqlConnection());
            dataBase.openConnection();
            if (command.ExecuteNonQuery() == 1)
            {
                System.Windows.MessageBox.Show("Пароль успішно змінено", caption, btn, img);
                Close();
            }
            else
                System.Windows.MessageBox.Show("Помилка під час зміни паролю", caption, btn, img);

            dataBase.closeConnection();
        }
        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
