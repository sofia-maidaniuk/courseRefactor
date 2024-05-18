using Microsoft.Win32;
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
    public partial class Profil : Window
    {
        DataBase DataBase = new DataBase();
        private readonly int _clientId;
        
        public Profil(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
            Loaded += Profil_Loaded;
        }

        private void UpdateInfo()
        {
            var querystring = $"select last_Name, first_Name, surname, phone_Number from Klient where ID_Klient = {_clientId}";
            SqlCommand command = new SqlCommand(querystring, DataBase.getSqlConnection());
            DataBase.openConnection();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                label_firstname.Content = reader[0].ToString();
                label_name.Content = reader[1].ToString();
                label_surname.Content = reader[2].ToString();
                label_phone.Content = reader[3].ToString();
            }
            reader.Close();

        }

        private void ClearFields()
        {
            label_name.Content = string.Empty;
            label_phone.Content = string.Empty;
        }


        private void Profil_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
        }

        private void updateImage_click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            UpdateInfo();
        }

        private void button_changeAvatar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                avatarImage.Source = bitmap;
            }
        }
        private void button_changePhone_Click(object sender, RoutedEventArgs e)
        {
            ChangePhone changePhone = new ChangePhone(_clientId);
            changePhone.Show();
        }

        private void button_changePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePassword changePassword = new ChangePassword(_clientId);
            changePassword.Show();
        }

        private void button_changePIB_Click(object sender, RoutedEventArgs e)
        {
            ChangePIB changePIB = new ChangePIB(_clientId);
            changePIB.Show();
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            Close();        
        }
    }
}
