using System;
using System.Data.SqlClient;
using System.Windows;
using Bank.DataClasses;

namespace Bank
{
    public partial class Validation : Window
    {
        DataBase dataBase = new DataBase();
        public delegate void ValidationCompletedEventHandler(bool isValid);
        public event ValidationCompletedEventHandler ValidationCompleted;

        public Validation()
        {
            InitializeComponent();
        }

        private void OnValidationCompleted(bool isValid)
        {
            ValidationCompleted?.Invoke(isValid);
        }

        private void Validation_Click(object sender, RoutedEventArgs e)
        {
            int attempts = 3;

            string password1 = textBox_password.Text;
            string password2 = textBox2_password.Text;

            if (password1.Length != 4 || password2.Length != 4 || password1 != password2)
            {
                MessageBox.Show("Паролі не співпадають або не складаються з 4 символів. Будь ласка, введіть однакові паролі, кожен довжиною 4 символи.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                OnValidationCompleted(false);
            }
            else
            {
                var cardPin = Convert.ToInt32(password1);
                var pin = 0;

                var queryCheckPin = $"SELECT pin FROM BankingCard WHERE cardNumber = '{DataStorage.cardNumber}'";
                SqlCommand command = new SqlCommand(queryCheckPin, dataBase.getSqlConnection());
                dataBase.openConnection();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    pin = Convert.ToInt32(reader[0]);
                }
                reader.Close();

                if (cardPin == pin)
                {
                    MessageBox.Show("Операцію підтверджено", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                    DataStorage.attempts = attempts;
                    OnValidationCompleted(true);
                    Close();
                }
                else
                {
                    MessageBox.Show("Невірний пін-код. Спробуйте ще раз.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (attempts > 0)
                    {
                        attempts--;
                        OnValidationCompleted(false);
                    }
                    else
                    {
                        DataStorage.attempts = attempts;
                        MessageBox.Show("Ви вичерпали всі спроби.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        OnValidationCompleted(false);
                        Close();
                    }
                }
                dataBase.closeConnection();
            }
        }
    }
}
