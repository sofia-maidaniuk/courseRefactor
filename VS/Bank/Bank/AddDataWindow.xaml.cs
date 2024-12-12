using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Bank
{
    public partial class AddDataWindow : Window
    {
        public List<string> CollectedData { get; private set; } = new List<string>();
        private readonly List<string> _parameters;
        private readonly Dictionary<string, TextBox> _inputFields = new Dictionary<string, TextBox>();

        public AddDataWindow(List<string> parameters)
        {
            InitializeComponent();
            _parameters = parameters;
            Loaded += Window_Loaded; // Додаємо обробник для завантаження
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateInputFields();
        }

        private void GenerateInputFields()
        {
            foreach (var parameter in _parameters)
            {
                StackPanel fieldPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(0, 5, 0, 5) };

                // Додаємо мітку
                Label label = new Label
                {
                    Content = parameter.Replace("@", ""), // Видаляємо символ '@'
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };
                fieldPanel.Children.Add(label);

                // Додаємо текстове поле
                TextBox textBox = new TextBox
                {
                    Width = 300,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                fieldPanel.Children.Add(textBox);

                // Зберігаємо текстове поле у словник для подальшого доступу
                _inputFields[parameter] = textBox;

                DataPanel.Children.Add(fieldPanel);
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            CollectedData.Clear();

            foreach (var parameter in _parameters)
            {
                if (_inputFields.TryGetValue(parameter, out var textBox))
                {
                    string value = textBox.Text.Trim();
                    CollectedData.Add(value);
                }
            }

            // Перевірка, чи всі поля заповнені
            if (CollectedData.Contains(string.Empty))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true; // Закриваємо вікно і вказуємо успішне заповнення
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Закриваємо вікно без змін
            Close();
        }
    }
}
