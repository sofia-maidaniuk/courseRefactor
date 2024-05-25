using Microsoft.Win32;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bank.DataClasses;

namespace Bank
{
    public partial class Profil : Window
    {
        DataBase DataBase = new DataBase();
        private readonly int _clientId;
        private byte[] selectedImageData;

        public Profil(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
            Loaded += Profil_Loaded;
        }

        private void button_changeAvatar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;
                selectedImageData = File.ReadAllBytes(imagePath);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = bitmap;
                avatarImageBrush.ImageSource = bitmap;

                SaveImageToDatabase();
            }
        }

        private void SaveImageToDatabase()
        {
            var query = "UPDATE Klient SET photoData = @photoData WHERE ID_Klient = @clientId";
            SqlCommand command = new SqlCommand(query, DataBase.getSqlConnection());
            command.Parameters.AddWithValue("@photoData", selectedImageData);
            command.Parameters.AddWithValue("@clientId", _clientId);

            try
            {
                DataBase.openConnection();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при збереженні зображення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                DataBase.closeConnection();
            }
        }

        private void UpdateInfo()
        {
            var querystring = $"SELECT last_Name, first_Name, surname, phone_Number, photoData FROM Klient WHERE ID_Klient = {_clientId}";
            SqlCommand command = new SqlCommand(querystring, DataBase.getSqlConnection());
            DataBase.openConnection();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                label_name.Content = reader["last_Name"].ToString();
                label_firstname.Content = reader["first_Name"].ToString();
                label_surname.Content = reader["surname"].ToString();
                label_phone.Content = reader["phone_Number"].ToString();

                if (reader["photoData"] != DBNull.Value)
                {
                    byte[] imageBytes = (byte[])reader["photoData"];
                    BitmapImage bitmap = new BitmapImage();
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                    }
                    avatarImageBrush.ImageSource = bitmap;
                }
            }
            reader.Close();
            DataBase.closeConnection();
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
