using System;
using System.Data.SqlClient;
using System.Data;
using System.Windows;
using System.Windows.Input;

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
            try
            {
                var loginUser = textBox_login.Text.Trim();
                var passUser = passwordBox_password.Password.Trim();

                dataBase.openConnection();

                // Перевірка ролі sysadmin
                string roleQuery = "SELECT IS_SRVROLEMEMBER('sysadmin')";
                SqlCommand roleCommand = new SqlCommand(roleQuery, dataBase.getSqlConnection());
                int isAdmin = (int)roleCommand.ExecuteScalar();

                if (isAdmin == 1 && loginUser == "admin_bank" && passUser == "admin1234")
                {
                    MessageBox.Show("Ви успішно увійшли як адміністратор!", "Успішно!", MessageBoxButton.OK, MessageBoxImage.Information);
                    AdminWindow adminWindow = new AdminWindow();
                    adminWindow.Show();
                    this.Close();
                }
                else
                {
                    // Перевірка звичайного користувача
                    string userQuery = "SELECT ID_Klient FROM Klient WHERE phone_Number = @login AND password_user = @password";
                    SqlCommand userCommand = new SqlCommand(userQuery, dataBase.getSqlConnection());
                    userCommand.Parameters.AddWithValue("@login", loginUser);
                    userCommand.Parameters.AddWithValue("@password", passUser);

                    object result = userCommand.ExecuteScalar();

                    if (result != null)
                    {
                        int idKlient = Convert.ToInt32(result);
                        MessageBox.Show("Ви успішно увійшли!", "Успішно!", MessageBoxButton.OK, MessageBoxImage.Information);
                        MainWindow mainWindow = new MainWindow(idKlient);
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Такого акаунту не існує або \nНе правильний пароль!", "Акаунту не існує!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dataBase.closeConnection();
            }
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
