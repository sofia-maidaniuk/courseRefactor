using Bank.DataClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;


namespace Bank
{
    public partial class MainWindow : Window
    {
        DataBase dataBase = new DataBase();
        public int clientId;

        public MainWindow(int IdKlient)
        {
            InitializeComponent();
            clientId = IdKlient;
            MainWindow_Loaded();
        }

        private void button_createCard_Click(object sender, RoutedEventArgs e)
        {
            AddBankCard addBankCard = new AddBankCard(clientId);
            addBankCard.Show();
        }

        private void MainWindow_Loaded()
        {
            noCard.Visibility = Visibility.Visible;
            CardOk.Visibility = Visibility.Hidden;
            visaCardImage.Visibility = Visibility.Hidden;
            masterCardImage.Visibility = Visibility.Hidden;

            DataTable cards = new DataTable();

            string querystringMyCards = "SELECT ID_Card, cardNumber FROM BankingCard WHERE ID_Klient = @clientId";

            dataBase.openConnection();

            using (SqlCommand command = new SqlCommand(querystringMyCards, dataBase.getSqlConnection()))
            {
                command.Parameters.AddWithValue("@clientId", clientId);

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(cards);
                }
            }

            dataBase.closeConnection();

            if (cards.Rows.Count == 0) 
            {
                MessageBox.Show("У вас немає жодної карти.", "Повідомлення");
                return; 
            }

            cardsComboBox.Items.Clear();
            cardsComboBox.Items.Add("Виберіть карту");
            foreach (DataRow row in cards.Rows)
            {
                cardsComboBox.Items.Add(row["cardNumber"]);
            }

            cardsComboBox.SelectedIndex = 0;
        }

        private void cardsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cardsComboBox.SelectedIndex > 0)
            {
                string selectedCardNumber = cardsComboBox.SelectedItem.ToString();
                SelectBankCard(selectedCardNumber);
            }
        }

        private void SelectBankCard(string selectedCardNumber)
        {
            label_cardNumber.Content = "";
            string paySystem = "";

            string querystringSelectCard = $"select cardNumber, cvvCode, CONCAT(FORMAT(cardDate, '%M'),'/', FORMAT(cardDate, '%y')), " +
                $"paySystem, currency, balance from BankingCard where cardNumber = '{selectedCardNumber}'";

            SqlCommand commandSelectCard = new SqlCommand(querystringSelectCard, dataBase.getSqlConnection());
            dataBase.openConnection();
            SqlDataReader reader = commandSelectCard.ExecuteReader();

            while (reader.Read())
            {
                var cardNumber = reader[0].ToString();

                int tmp = 0;
                int tmp1 = 4;

                for (int m = 0; m < 4; m++)
                {
                    for (int n = tmp; n < tmp1; n++)
                    {
                        label_cardNumber.Content = label_cardNumber.Content.ToString() + cardNumber[n].ToString();
                    }
                    label_cardNumber.Content += " ";
                    tmp += 4;
                    tmp1 += 4;
                }

                label_cvvCode.Content = reader[1].ToString();
                label_date.Content = reader[2].ToString();
                DataStorage.expiryDate = label_date.Content.ToString();
                paySystem = reader[3].ToString();
                label_currency.Content = reader[4].ToString();
                label_balance.Content = reader[5].ToString();
                DataStorage.cvvCode = label_cvvCode.Content.ToString();
                label_cvvCode.Content = "***";
            }
            reader.Close();

            if (paySystem == "Visa")
            {
                CardOk.Visibility = Visibility.Visible;
                visaCardImage.Visibility = Visibility.Visible;
                masterCardImage.Visibility = Visibility.Hidden;
                noCard.Visibility = Visibility.Hidden;
            }
            else
            {
                CardOk.Visibility = Visibility.Visible;
                visaCardImage.Visibility = Visibility.Hidden;
                masterCardImage.Visibility = Visibility.Visible;
                noCard.Visibility = Visibility.Hidden;
            }
        }

        private void label_cvvCode_click(object sender, MouseButtonEventArgs e)
        {
            if (label_cvvCode.Content.ToString() == "***")
                label_cvvCode.Content = DataStorage.cvvCode;
            else
                label_cvvCode.Content = "***";
        }

        private void button_transfer_Click(object sender, RoutedEventArgs e)
        {
            if (cardsComboBox.SelectedItem != null)
            {
                DataStorage.bankCard = cardNumberTransaction_texBox.Text;
                DataStorage.cardNumber = cardsComboBox.SelectedItem.ToString();
                cardsComboBox.Text = "";

                Send_to sendToWindow = new Send_to(clientId, DataStorage.bankCard, DataStorage.cardNumber, DataStorage.cvvCode, DataStorage.expiryDate);
                sendToWindow.Show();
            }
            else
                MessageBox.Show("Будь ласка, виберіть карту перед тим, як продовжити.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void profilImage_click(object sender, MouseButtonEventArgs e)
        {
            Profil profil = new Profil(clientId);
            profil.Show();
        }

        private void transactionList_click(object sender, MouseButtonEventArgs e)
        {
            TransactionList transactionList = new TransactionList(clientId);
            transactionList.Show();
        }

        private void button_payPhone_Click(object sender, RoutedEventArgs e)
        {
            if (cardsComboBox.SelectedItem != null && cardsComboBox.SelectedItem != "Виберіть карту")
            {
                DataStorage.cardNumber = cardsComboBox.SelectedItem.ToString();
                DataStorage.phone_Number = payPhone_texBox.Text;

                Phone phone = new Phone(clientId, DataStorage.bankCard, DataStorage.cardNumber, DataStorage.cvvCode, DataStorage.expiryDate, DataStorage.phone_Number);
                payPhone_texBox.Text = string.Empty;
                phone.Show();
            }
            else
                MessageBox.Show("Будь ласка, виберіть карту перед тим, як продовжити.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void updateImage_click(object sender, RoutedEventArgs e)
        {
            MainWindow_Loaded();
        }


        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Log_in log_in = new Log_in();
            log_in.Show();
            this.Close();
        }

        private void button_credit_Click(object sender, RoutedEventArgs e)
        {
            if (cardsComboBox.SelectedItem != null && cardsComboBox.SelectedItem != "Виберіть карту")
            {
                DataStorage.cardNumber = cardsComboBox.SelectedItem.ToString();

                Credit credit = new Credit(clientId);
                credit.Show();
            }
            else
                MessageBox.Show("Будь ласка, виберіть карту перед тим, як продовжити.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void button_payment_Click(object sender, RoutedEventArgs e)
        {
            if (cardsComboBox.SelectedItem != null && cardsComboBox.SelectedItem != "Виберіть карту")
            {
                DataStorage.cardNumber = cardsComboBox.SelectedItem.ToString();
                Payment payment = new Payment(clientId, DataStorage.bankCard, DataStorage.cardNumber, DataStorage.cvvCode, DataStorage.expiryDate);
                payment.Show();
            }
            else
                MessageBox.Show("Будь ласка, виберіть карту перед тим, як продовжити.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void button_deposit_Click(object sender, RoutedEventArgs e)
        {
            if (cardsComboBox.SelectedItem != null && cardsComboBox.SelectedItem != "Виберіть карту")
            {
                DataStorage.cardNumber = cardsComboBox.SelectedItem.ToString();

                Deposit deposit = new Deposit(clientId);
                deposit.Show();
            }
            else
                MessageBox.Show("Будь ласка, виберіть карту перед тим, як продовжити.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}