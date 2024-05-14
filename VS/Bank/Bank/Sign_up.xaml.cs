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
using System.Text.RegularExpressions;

namespace Bank
{

    public partial class Sign_up : Window
    {
        DataBase dataBase = new DataBase();
        public Sign_up()
        {
            InitializeComponent();
        }

        private void button_Sign_up_Click(object sender, RoutedEventArgs e)
        {
            var lastName = textBox_lastName.Text;
            var firstName = textBox_firstName.Text;
            var surname = textBox_surname.Text;
            var passportNumber = textBox_passportNumber.Text;
            var phoneNumber = textBox_phoneNumber.Text;
            var idKod = textBox_idKod.Text;
            var password = textBox_password.Text;

            DateTime? selectedDate = datePicker_birthDate.SelectedDate;


            if (string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(surname) ||
                string.IsNullOrWhiteSpace(passportNumber) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(idKod) ||
                string.IsNullOrWhiteSpace(password) ||
                selectedDate == null ||
                !selectedDate.HasValue)
            {
                MessageBox.Show("Будь ласка, заповніть всі поля.", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (phoneNumber.Length > 50 || password.Length > 50)
            {
                MessageBox.Show("Довжина логіну або пароля перевищує 50 символів!", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var birthday = selectedDate.Value.ToString("yyyy-MM-dd");
            DateTime currentDate = DateTime.Now;
            string formattedCurrentDate = currentDate.ToString("yyyy-MM-dd");

            DateTime minDateOfBirth = currentDate.AddYears(-14);
            if (selectedDate > minDateOfBirth)
            {
                MessageBox.Show("Ви повинні бути старше 14 років для реєстрації.", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Regex.IsMatch(password, @"^(?=.*[a-zA-Z])(?=.*\d).{4,}$"))
            {
                MessageBox.Show("Пароль повинен містити мінімум 1 латинську літеру і 1 цифру і бути довжиною щонайменше 4 символи.", "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (checkUser())
            {
                return;
            }

            string querystring = $"insert into Klient(last_Name, first_Name, surname, passport_Number, phone_Number, id_kod, birthday, password_user, registration_Date) " +
                $"values('{lastName}', '{firstName}', '{surname}', '{passportNumber}', '{phoneNumber}', '{idKod}', '{birthday}', '{password}', '{formattedCurrentDate}')";

            SqlCommand command = new SqlCommand(querystring, dataBase.getSqlConnection());
            dataBase.openConnection();

            if (command.ExecuteNonQuery() == 1)
            {
                MessageBox.Show("Ви успішно зареєстровані!", "Успішно!", MessageBoxButton.OK, MessageBoxImage.Information);
                Log_in log_in = new Log_in();
                log_in.Show();
                this.Close();
            }
            else
                MessageBox.Show("Не зареєстровано! Перевірте введення!", "Реєстрація не вдалась!", MessageBoxButton.OK, MessageBoxImage.Error);

            dataBase.closeConnection();
        }

        private Boolean checkUser()
        {
            string loginUser = textBox_phoneNumber.Text;
            string passUser = textBox_password.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string queryString = "SELECT ID_Klient, phone_Number, password_user FROM Klient WHERE phone_Number = @loginUser AND password_user = @passUser";

            using (SqlCommand command = new SqlCommand(queryString, dataBase.getSqlConnection()))
            {
                command.Parameters.AddWithValue("@loginUser", loginUser);
                command.Parameters.AddWithValue("@passUser", passUser);

                adapter.SelectCommand = command;
                adapter.Fill(table);
            }

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Користувач вже існує!", "Користувач існує!", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LogInLabel_Click(object sender, MouseButtonEventArgs e)
        {
            Log_in logInWindow = new Log_in();
            logInWindow.Show();
            this.Close();
        }
    }
}
