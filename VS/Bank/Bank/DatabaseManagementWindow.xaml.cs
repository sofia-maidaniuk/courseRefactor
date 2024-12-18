using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bank
{
    public partial class DatabaseManagementWindow : Window
    {
        DataBase dataBase = new DataBase();

        public DatabaseManagementWindow()
        {
            InitializeComponent();
            LoadTables(); // Завантажуємо список таблиць у `TabControl`
        }

        private void LoadTables()
        {
            try
            {
                dataBase.openConnection();

                // Отримуємо список таблиць з бази даних
                using (SqlCommand command = new SqlCommand(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'",
                    dataBase.getSqlConnection()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<string> tableNames = new List<string>();

                        // Зчитуємо всі таблиці та зберігаємо їх у списку
                        while (reader.Read())
                        {
                            string tableName = reader["TABLE_NAME"].ToString();
                            if (tableName == "Klient" || tableName == "BankingCard" ||
                                tableName == "Transactions" || tableName == "Credits" ||
                                tableName == "Deposits" || tableName == "Services")
                            {
                                tableNames.Add(tableName);
                            }
                        }

                        reader.Close(); // Закриваємо DataReader, щоб звільнити з'єднання

                        // Тепер завантажуємо дані для кожної таблиці
                        foreach (var tableName in tableNames)
                        {
                            // Створюємо DataGrid
                            DataGrid dataGrid = new DataGrid
                            {
                                AutoGenerateColumns = true,
                                SelectionMode = DataGridSelectionMode.Single,
                                SelectionUnit = DataGridSelectionUnit.FullRow,
                                IsReadOnly = true,
                                Name = $"DataGrid_{tableName}"
                            };

                            dataGrid.SelectionChanged += (s, e) =>
                            {
                                
                            };


                            // Завантажуємо дані для DataGrid
                            LoadTableData(tableName, dataGrid);

                            // Створюємо вкладку для таблиці
                            TabItem tabItem = new TabItem
                            {
                                Header = tableName,
                                Content = dataGrid
                            };

                            tabs.Items.Add(tabItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні таблиць: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dataBase.closeConnection(); // Закриваємо з'єднання
            }
        }


        private void LoadTableData(string tableName, DataGrid dataGrid)
        {
            try
            {
                dataBase.openConnection();

                string query = $"SELECT * FROM {tableName}";
                SqlDataAdapter adapter = new SqlDataAdapter(query, dataBase.getSqlConnection());
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                dataGrid.ItemsSource = null; // Очищаємо попередні дані
                dataGrid.ItemsSource = dataTable.DefaultView; // Завантажуємо нові дані
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при завантаженні даних таблиці {tableName}: {ex.Message}",
                                "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dataBase.closeConnection();
            }
        }

        private void Add_Record(object sender, RoutedEventArgs e)
        {
            TabItem selectedTab = (TabItem)tabs.SelectedItem;
            if (selectedTab == null)
            {
                MessageBox.Show("Оберіть таблицю для додавання запису.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string tableName = selectedTab.Header.ToString();
            switch (tableName.ToLower())
            {
                case "klient":
                    Add_Klient();
                    break;
                case "bankingcard":
                    Add_BankingCard();
                    break;
                case "transactions":
                    Add_Transaction();
                    break;
                case "services":
                    Add_Service();
                    break;
                case "credits":
                    Add_Credit();
                    break;
                case "deposits":
                    Add_Deposit();
                    break;
                default:
                    MessageBox.Show("Ця таблиця не підтримує додавання записів.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    break;
            }
        }

        private void Add_Klient()
        {
            // Дані вводяться через додаткове вікно
            AddDataWindow addDataWindow = new AddDataWindow(new List<string> { "@last_Name", "@first_Name", "@surname", "@passport_Number", "@phone_Number", "@id_kod", "@birthday", "@password_user", "@registration_Date", "@photoData" });
            if (addDataWindow.ShowDialog() == true)
            {
                List<string> inputData = addDataWindow.CollectedData;
                ExecuteStoredProcedure("insert_klient", new Dictionary<string, object>
                {
                    { "@last_Name", inputData[0] },
                    { "@first_Name", inputData[1] },
                    { "@surname", inputData[2] },
                    { "@passport_Number", inputData[3] },
                    { "@phone_Number", inputData[4] },
                    { "@id_kod", inputData[5] },
                    { "@birthday", DateTime.Parse(inputData[6]).Date },
                    { "@password_user", inputData[7] },
                    { "@registration_Date", DateTime.Parse(inputData[8]).Date },
                    { "@photoData", DBNull.Value } // PhotoData поки NULL
                });
            }
        }

        private void Add_BankingCard()
        {
            AddDataWindow addDataWindow = new AddDataWindow(new List<string> { "@cardType", "@cardNumber", "@cvvCode", "@balance", "@currency", "@paySystem", "@cardDate", "@pin", "@ID_Klient" });
            if (addDataWindow.ShowDialog() == true)
            {
                List<string> inputData = addDataWindow.CollectedData;
                ExecuteStoredProcedure("insert_bankingCard", new Dictionary<string, object>
                {
                    { "@cardType", inputData[0] },
                    { "@cardNumber", inputData[1] },
                    { "@cvvCode", inputData[2] },
                    { "@balance", decimal.Parse(inputData[3]) },
                    { "@currency", inputData[4] },
                    { "@paySystem", inputData[5] },
                    { "@cardDate", DateTime.Parse(inputData[6]).Date },
                    { "@pin", int.Parse(inputData[7]) },
                    { "@ID_Klient", int.Parse(inputData[8]) }
                });
            }
        }

        private void Add_Transaction()
        {
            AddDataWindow addDataWindow = new AddDataWindow(new List<string> { "@transactionType", "@transactionDestination", "@transactionDate", "@transactionNumber", "@transactionValue", "@ID_Card" });
            if (addDataWindow.ShowDialog() == true)
            {
                List<string> inputData = addDataWindow.CollectedData;
                ExecuteStoredProcedure("insert_transaction", new Dictionary<string, object>
                {
                    { "@transactionType", inputData[0] },
                    { "@transactionDestination", inputData[1] },
                    { "@transactionDate", DateTime.Parse(inputData[2]).Date  },
                    { "@transactionNumber", inputData[3] },
                    { "@transactionValue", decimal.Parse(inputData[4]) },
                    { "@ID_Card", int.Parse(inputData[5]) }
                });
            }
        }

        private void Add_Service()
        {
            AddDataWindow addDataWindow = new AddDataWindow(new List<string> { "@serviceName", "@serviceBalance", "@serviceType" });
            if (addDataWindow.ShowDialog() == true)
            {
                List<string> inputData = addDataWindow.CollectedData;
                ExecuteStoredProcedure("insert_service", new Dictionary<string, object>
                {
                    { "@serviceName", inputData[0] },
                    { "@serviceBalance", decimal.Parse(inputData[1]) },
                    { "@serviceType", inputData[2] }
                });
            }
        }

        private void Add_Credit()
        {
            AddDataWindow addDataWindow = new AddDataWindow(new List<string>
            {
                "@creditTotalSum",
                "@creditSum",
                "@creditDate",
                "@creditStatus",
                "@repaymentDate",
                "@repaymentSum",
                "@ID_Card"
            });

            if (addDataWindow.ShowDialog() == true)
            {
                List<string> inputData = addDataWindow.CollectedData;

                // Формування параметрів для збереження
                var parameters = new Dictionary<string, object>
                {
                    { "@creditTotalSum", decimal.Parse(inputData[0]) },
                    { "@creditSum", decimal.Parse(inputData[1]) },
                    { "@creditDate", DateTime.Parse(inputData[2]).Date },
                    { "@creditStatus", bool.Parse(inputData[3]) },
                    { "@ID_Card", int.Parse(inputData[6]) }
                };

                // Перевіряємо, чи repaymentDate вказано
                if (string.IsNullOrEmpty(inputData[4])) parameters.Add("@repaymentDate", DBNull.Value);
                else parameters.Add("@repaymentDate", DateTime.Parse(inputData[4]).Date);

                // Перевіряємо, чи repaymentSum вказано
                if (string.IsNullOrEmpty(inputData[5])) parameters.Add("@repaymentSum", DBNull.Value);
                else parameters.Add("@repaymentSum", decimal.Parse(inputData[5]));


                // Виклик збереженої процедури
                ExecuteStoredProcedure("insert_credit", parameters);
            }
        }


        private void Add_Deposit()
        {
            AddDataWindow addDataWindow = new AddDataWindow(new List<string> { "@depositAmount", "@depositDate", "@interestRate", "@termInMonths", "@isProcessed", "@ID_Klient" });
            if (addDataWindow.ShowDialog() == true)
            {
                List<string> inputData = addDataWindow.CollectedData;
                ExecuteStoredProcedure("insert_deposit", new Dictionary<string, object>
                {
                    { "@depositAmount", decimal.Parse(inputData[0]) },
                    { "@depositDate", DateTime.Parse(inputData[1]).Date },
                    { "@interestRate", decimal.Parse(inputData[2]) },
                    { "@termInMonths", int.Parse(inputData[3]) },
                    { "@isProcessed", bool.Parse(inputData[4]) },
                    { "@ID_Klient", int.Parse(inputData[5]) }
                });
            }
        }

        private int ExecuteStoredProcedure(string procedureName, Dictionary<string, object> parameters)
        {
            int rowsAffected = 0;

            try
            {
                dataBase.openConnection();
                using (SqlCommand command = new SqlCommand(procedureName, dataBase.getSqlConnection()))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    rowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка під час виконання процедури: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                dataBase.closeConnection();
            }

            return rowsAffected; // Повертаємо кількість змінених рядків
        }



        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            AdminWindow adminWindow = new AdminWindow();
            adminWindow.Show();
            this.Close();
        }

        private void Upd_Record(object sender, RoutedEventArgs e)
        {
            // Перевіряємо, чи обрано вкладку та рядок
            if (tabs.SelectedItem is TabItem selectedTab && selectedTab.Content is DataGrid dataGrid)
            {
                if (dataGrid.SelectedItem is DataRowView selectedRow)
                {
                    // Словник первинних ключів для таблиць
                    var primaryKeys = new Dictionary<string, string>
            {
                { "Klient", "ID_Klient" },
                { "BankingCard", "ID_Card" },
                { "Transactions", "ID_transaction" },
                { "Credits", "ID_Credit" },
                { "Deposits", "ID_Deposit" },
                { "Services", "ID_Service" }
            };

                    string tableName = selectedTab.Header.ToString();

                    if (primaryKeys.TryGetValue(tableName, out string primaryKey))
                    {
                        // Отримуємо значення первинного ключа
                        int id = Convert.ToInt32(selectedRow[primaryKey]);

                        // Формуємо список параметрів для вікна редагування
                        List<string> parameters = new List<string>();
                        Dictionary<string, string> initialValues = new Dictionary<string, string>();

                        foreach (DataColumn column in selectedRow.DataView.Table.Columns)
                        {
                            string columnName = column.ColumnName;
                            string columnValue = selectedRow[columnName]?.ToString() ?? string.Empty;

                            if (columnName != primaryKey) // Виключаємо первинний ключ
                            {
                                parameters.Add($"@{columnName}");
                                initialValues[$"@{columnName}"] = columnValue;
                            }
                        }

                        // Відкриваємо вікно редагування даних
                        AddDataWindow editWindow = new AddDataWindow(parameters);

                        // Передаємо початкові значення у текстові поля
                        editWindow.Loaded += (s, args) =>
                        {
                            foreach (var field in initialValues)
                            {
                                if (editWindow.DataPanel.Children
                                    .OfType<StackPanel>()
                                    .FirstOrDefault(sp => sp.Children.OfType<Label>().FirstOrDefault()?.Content.ToString() == field.Key.Replace("@", "")) is StackPanel fieldPanel)
                                {
                                    if (fieldPanel.Children.OfType<TextBox>().FirstOrDefault() is TextBox textBox)
                                    {
                                        textBox.Text = field.Value;
                                    }
                                }
                            }
                        };

                        // Якщо користувач натиснув "Зберегти"
                        if (editWindow.ShowDialog() == true)
                        {
                            // Збираємо оновлені дані
                            var updatedData = new Dictionary<string, object>
                    {
                        { $"@{primaryKey}", id } // Додаємо первинний ключ для ідентифікації запису
                    };

                            for (int i = 0; i < parameters.Count; i++)
                            {
                                updatedData[parameters[i]] = editWindow.CollectedData[i];
                            }

                            // Викликаємо збережену процедуру для оновлення
                            ExecuteStoredProcedure($"update_{tableName.ToLower()}", updatedData);

                            // Оновлюємо таблицю після редагування
                            LoadTableData(tableName, dataGrid);
                            MessageBox.Show("Запис успішно оновлено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Первинний ключ не знайдено для таблиці.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Оберіть рядок для редагування.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Оберіть вкладку таблиці.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void Delete_Record(object sender, RoutedEventArgs e)
        {
            if (tabs.SelectedItem is TabItem selectedTab && selectedTab.Content is DataGrid dataGrid)
            {
                if (dataGrid.SelectedItem is DataRowView selectedRow)
                {
                    // Словник первинних ключів для таблиць
                    var primaryKeys = new Dictionary<string, string>
                {
                    { "Klient", "ID_Klient" },
                    { "BankingCard", "ID_BankingCard" },
                    { "Transactions", "ID_Transactions" },
                    { "Credits", "ID_Credits" },
                    { "Deposits", "ID_Deposit" },
                    { "Services", "ID_Service" }
                };

                    string tableName = selectedTab.Header.ToString();

                    if (primaryKeys.TryGetValue(tableName, out string primaryKey))
                    {
                        try
                        {
                            // Отримуємо значення первинного ключа
                            int id = Convert.ToInt32(selectedRow[primaryKey]);

                            int rowsAffected = ExecuteStoredProcedure($"delete_{tableName.ToLower()}", new Dictionary<string, object> { { "@id", id } });

                            if (rowsAffected > 0)
                            {
                                // Оновлюємо дані в таблиці після видалення
                                LoadTableData(tableName, dataGrid);
                                MessageBox.Show("Запис успішно видалено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Запис не було видалено. Можливо, він не існує або є помилка у базі даних.", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Помилка при видаленні запису: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Первинний ключ не знайдено для таблиці.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Оберіть рядок для видалення.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Оберіть вкладку таблиці.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void Upd_Table(object sender, RoutedEventArgs e)
        {
            if (tabs.SelectedItem is TabItem selectedTab && selectedTab.Content is DataGrid dataGrid)
            {
                string tableName = selectedTab.Header.ToString(); // Отримуємо назву таблиці
                LoadTableData(tableName, dataGrid); // Перезавантажуємо дані у DataGrid
                MessageBox.Show("Дані успішно оновлено!", "Оновлення", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Оберіть вкладку для оновлення даних.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
