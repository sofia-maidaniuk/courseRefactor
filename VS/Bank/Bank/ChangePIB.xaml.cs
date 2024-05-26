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
    public partial class ChangePIB : Window
    {
        DataBase dataBase = new DataBase();
        private readonly int _clientId;
        public ChangePIB(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
        }

        private void ChangePIB_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxButton btn = MessageBoxButton.OK;
            MessageBoxImage img = MessageBoxImage.Information;

            string caption = "Дата збереження";

            if (!Regex.IsMatch(textBox_name.Text, "^[A-ZА-ЯІЇЄ][a-zа-яіїє]{1,50}$") ||
                               !Regex.IsMatch(textBox_lastname.Text, "^[A-ZА-ЯІЇЄ][a-zа-яіїє]{1,50}$") ||
                                              !Regex.IsMatch(textBox_surname.Text, "^[A-ZА-ЯІЇЄ][a-zа-яіїє]{1,50}$"))
            {
                System.Windows.MessageBox.Show("Прізвище, ім'я та по-батькові повинні починатися з великої літери та містити від 2 до 50 символів", caption, btn, img);
                textBox_name.Focus();
                textBox_lastname.Focus();
                textBox_surname.Focus();
                return;
            }

            var lastName = textBox_lastname.Text;
            var name = textBox_name.Text;
            var surname = textBox_surname.Text;

            string queryCheckPIB = "SELECT COUNT(*) FROM Klient WHERE last_Name = @lastName AND first_Name = @name AND surname = @surname";
            SqlCommand checkPIBCommand = new SqlCommand(queryCheckPIB, dataBase.getSqlConnection());
            checkPIBCommand.Parameters.AddWithValue("@lastName", lastName);
            checkPIBCommand.Parameters.AddWithValue("@name", name);
            checkPIBCommand.Parameters.AddWithValue("@surname", surname);

            dataBase.openConnection();
            int pibCount = (int)checkPIBCommand.ExecuteScalar();

            if (pibCount > 0)
            {
                MessageBox.Show("Користувач з таким ПІБ вже існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                dataBase.closeConnection();
                return;
            }

            var querystringPIB = $"update Klient set last_Name = '{lastName}', first_Name = '{name}', surname = '{surname}' where ID_Klient = {_clientId}";
            var command = new SqlCommand(querystringPIB, dataBase.getSqlConnection());
            dataBase.openConnection();
            if (command.ExecuteNonQuery() == 1)
            {
                System.Windows.MessageBox.Show("ПІБ успішно змінено", caption, btn, img);
                Close();
            }
            else
                System.Windows.MessageBox.Show("Помилка зміни ПІБ", caption, btn, img);

            dataBase.closeConnection();
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}
