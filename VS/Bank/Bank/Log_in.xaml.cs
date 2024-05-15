using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;


namespace Bank
{
    public partial class Log_in : Window
    {
        DataBase dataBase = new DataBase();

        public Log_in()
        {
            InitializeComponent();
            KeyDown += Log_in_KeyDown;
            img_password_no.Visibility = Visibility.Visible;
            img_password_yes.Visibility = Visibility.Hidden;
        }

        private void button_Log_in_Click(object sender, RoutedEventArgs e)
        {
            var loginUser = textBox_login.Text;
            var passUser = passwordBox_password.Password;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            dataBase.openConnection();

            string querystring = $"select ID_Klient, phone_Number, password_user from Klient where phone_Number = '{loginUser}' and password_user = '{passUser}'";

            SqlCommand command = new SqlCommand(querystring, dataBase.getSqlConnection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            object result = command.ExecuteScalar();

            if (table.Rows.Count == 1)
            {
                int idKlient = (int)result;
                MessageBox.Show("Ви успішно увійшли!", "Успішно!", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow MainWindow = new MainWindow(idKlient);
                MainWindow.Show();
                dataBase.closeConnection();
                this.Close();
            }
            else
                MessageBox.Show("Такого акаунту не існує або \nНе правильний пароль!", "Акаунту не існує!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Log_in_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                button_Log_in_Click(sender, e);
            }
        }

        private void SignUpLabel_Click(object sender, MouseButtonEventArgs e)
        {
            Sign_up signUpWindow = new Sign_up();
            signUpWindow.Show();
            this.Close();
        }

        private void Img_password_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (passwordBox_password.Visibility == Visibility.Visible)
            {
                textBox_password.Text = passwordBox_password.Password;
                textBox_password.Visibility = Visibility.Visible;
                passwordBox_password.Visibility = Visibility.Hidden;
                img_password_yes.Visibility = Visibility.Visible;
                img_password_no.Visibility = Visibility.Hidden;
            }
            else
            {
                textBox_password.Visibility = Visibility.Hidden;
                passwordBox_password.Visibility = Visibility.Visible;
                img_password_yes.Visibility = Visibility.Hidden;
                img_password_no.Visibility = Visibility.Visible;
            }
        }
    }
}
