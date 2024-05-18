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

namespace Bank
{
    public partial class TransactionList : Window
    {

        DataBase DataBase = new DataBase();
        private readonly int _clientId;

        public TransactionList(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
        }

        private void TransactionList_Loaded(object sender, RoutedEventArgs e)
        {
            var querystringTransaction = $"select transactionType, transactionDestination, transactionDate, transactionNumber, transactionValue from Transaction " +
                $"inner join BankingCard on Transaction.ID_transaction = BankingCard.ID_Card " +
                $"inner join Klient on Klient.ID_Klient = BankingCard.ID_Klient where Klient.ID_Klient = '{_clientId}'";
            SqlCommand command = new SqlCommand(querystringTransaction, DataBase.getSqlConnection());
            DataBase.openConnection();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                ListViewItem item = new ListViewItem(reader[0].ToString);
                item.SubItems.Add(reader[1].ToString);
                item.SubItems.Add(reader[2].ToString);
                item.SubItems.Add(reader[3].ToString);
                item.SubItems.Add(reader[4].ToString);
                ListView.Items.Add(item);
            }
            reader.Close();

            ListView.Sort()
        }
    }
}
