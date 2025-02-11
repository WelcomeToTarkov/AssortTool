using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media;
using AssortEditor.Models;
using Newtonsoft.Json;  // Install Newtonsoft.Json via NuGet for JSON handling


namespace AssortEditor
{
    public partial class MainWindow : Window
    {
        private const string SettingsFilePath = "AssortEditor_settings.json";
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            LoadThemePreference();
        }

        private void LoadThemePreference()
        {
            if (File.Exists(SettingsFilePath))
            {
                // Read the settings file and deserialize the theme preference
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFilePath));
                if (settings != null && settings.IsDarkMode)
                {
                    LoadDarkTheme();
                    DarkModeToggleButton.IsChecked = true; // Set the checkbox for dark mode
                }
                else
                {
                    LoadLightTheme();
                    DarkModeToggleButton.IsChecked = false; // Set the checkbox for light mode
                }
            }
            else
            {
                // Default to light mode if settings file doesn't exist
                LoadLightTheme();
            }
        }

        private void SaveThemePreference(bool isDarkMode)
        {
            var settings = new Settings { IsDarkMode = isDarkMode };
            var json = JsonConvert.SerializeObject(settings);
            File.WriteAllText(SettingsFilePath, json);
        }

        private void LoadDarkTheme()
        {
            var darkTheme = new ResourceDictionary
            {
                Source = new Uri("DarkTheme.xaml", UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(darkTheme);

            // Manually update logo for dark theme
            LogoImage.Source = (ImageSource)Application.Current.Resources["LogoImage"];
        }

        private void LoadLightTheme()
        {
            var lightTheme = new ResourceDictionary
            {
                Source = new Uri("LightTheme.xaml", UriKind.Relative)
            };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(lightTheme);

            // Manually update logo for light theme
            LogoImage.Source = (ImageSource)Application.Current.Resources["LogoImage"];
        }

        private void DarkModeToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            LoadDarkTheme();
            SaveThemePreference(true); // Save dark mode preference
        }

        private void DarkModeToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            LoadLightTheme();
            SaveThemePreference(false); // Save light mode preference
        }

        private void LoadAssortButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to select the JSON file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Select Assort File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                // Read the JSON content from the selected file
                string jsonContent = File.ReadAllText(filePath);

                // Get the ViewModel from DataContext
                var vm = DataContext as MainViewModel;
                if (vm != null)
                {
                    // Call the method to load items and barters
                    vm.LoadItemsAndBarters(jsonContent);
                }
            }
        }
        private void AddItemButton_Click(object sender, RoutedEventArgs e)
        {
            var addItemWindow = new AddItemWindow();
            var result = addItemWindow.ShowDialog();

            if (result == true)  // If the user pressed OK
            {
                var itemTpl = addItemWindow.CurrentItem._tpl;
                var barters = addItemWindow.CurrentItem.Barters;
                var upd = addItemWindow.CurrentItem.upd;  // Get 'upd' values from the addItemWindow

                var vm = DataContext as MainViewModel;
                if (vm != null)
                {
                    var newItem = new Item
                    {
                        _id = Helpers.GenerateMongoObjectId(),  // MongoDB-like ObjectId
                        _tpl = itemTpl,
                        Barters = barters,
                        parentid = "hideout",  // Default parent
                        slotid = "hideout",    // Default slot
                        upd = upd              // Assign the 'upd' values to the new item
                    };

                    var selectedTab = TabControl.SelectedItem as TabItem;

                    if (selectedTab != null)
                    {
                        var loyaltyLevel = selectedTab.Header.ToString();
                        vm.AddItem(newItem, loyaltyLevel);
                    }
                }
            }
        }

        private void AddChildItem_ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Item selectedItem)
            {
                // Open window to add a child item
                var addChildWindow = new AddChildWindow();
                var result = addChildWindow.ShowDialog();

                if (result == true)
                {
                    Item newItem;

                    if (addChildWindow.NewSlotId == "cartridges")
                    {
                        // Handle "cartridges" slot case
                        string input = Microsoft.VisualBasic.Interaction.InputBox(
                            "Enter the StackObjectsCount for the item:",
                            "StackObjectsCount Required",
                            "10" // Default value
                        );

                        if (int.TryParse(input, out int parsedStackCount))
                        {
                            newItem = new Item
                            {
                                _id = Helpers.GenerateMongoObjectId(),
                                _tpl = addChildWindow.NewTpl,
                                slotid = addChildWindow.NewSlotId,
                                parentid = selectedItem._id,
                                Location = 0,
                                upd = new Upd
                                {
                                    stackobjectscount = parsedStackCount
                                }
                            };
                        }
                        else
                        {
                            MessageBox.Show("Invalid input for StackObjectsCount. Please enter a valid number.");
                            return;
                        }
                    }
                    else
                    {
                        // Create a basic child item
                        newItem = new Item
                        {
                            _id = Helpers.GenerateMongoObjectId(),
                            _tpl = addChildWindow.NewTpl,
                            slotid = addChildWindow.NewSlotId,
                            parentid = selectedItem._id
                        };
                    }

                    // Add the new item to the parent's Children collection
                    selectedItem.Children.Add(newItem);
                }
            }
        }



        private void RemoveItem_ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Item selectedNode)
            {
                var vm = DataContext as MainViewModel;

                // Search and remove recursively
                foreach (var loyaltyLevel in new[] { vm.LoyaltyLevel1Items, vm.LoyaltyLevel2Items, vm.LoyaltyLevel3Items, vm.LoyaltyLevel4Items })
                {
                    if (RemoveNodeFromCollection(loyaltyLevel, selectedNode))
                    {
                        return; // Node found and removed
                    }
                }

                MessageBox.Show("Item could not be found.");
            }
            else
            {
                MessageBox.Show("Please select an item to remove.");
            }
        }
        
        private bool RemoveNodeFromCollection(ObservableCollection<Item> collection, Item nodeToRemove)
        {
            if (collection.Contains(nodeToRemove))
            {
                collection.Remove(nodeToRemove);
                return true;
            }

            // Search in children
            foreach (var item in collection)
            {
                if (RemoveNodeFromCollection(item.Children, nodeToRemove))
                {
                    return true;
                }
            }

            return false; // Node not found in this collection
        }
                    
        private void EditItem_ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Item selectedItem)
            {
                // Validate selectedItem before proceeding
                if (selectedItem == null)
                {
                    MessageBox.Show("No item selected to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var editItemWindow = new EditItemWindow(selectedItem);
                var result = editItemWindow.ShowDialog();

                if (result == true)
                {
                    // Update the selected item with new values
                    selectedItem._tpl = editItemWindow.CurrentItem._tpl ?? selectedItem._tpl;
                    selectedItem.Barters = new ObservableCollection<Barter>(editItemWindow.CurrentItem.Barters ?? selectedItem.Barters);

                    // Update slotId only if the item is a child (has a parentid)
                    if (selectedItem.slotid != "hideout")
                    {
                        selectedItem.slotid = editItemWindow.CurrentItem.slotid ?? selectedItem.slotid;
                    }

                    // Update 'upd' property
                    if (editItemWindow.CurrentItem.upd != null)
                    {
                        selectedItem.upd = new Upd
                        {
                            unlimitedcount = editItemWindow.CurrentItem.upd.unlimitedcount,
                            stackobjectscount = editItemWindow.CurrentItem.upd.stackobjectscount,
                            buyrestrictioncurrent = editItemWindow.CurrentItem.upd.buyrestrictioncurrent,
                            buyrestrictionmax = editItemWindow.CurrentItem.upd.buyrestrictionmax,
                            repairable = editItemWindow.CurrentItem.upd.repairable ?? null,
                            firemode = editItemWindow.CurrentItem.upd.firemode ?? null
                        };
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a valid item to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        
        private void SaveAssortButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to specify where to save the JSON file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Save Assort File"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                // Get the ViewModel from DataContext
                var vm = DataContext as MainViewModel;
                if (vm != null)
                {
                    // Serialize the items and barters into JSON format
                    string jsonContent = vm.SaveItemsAndBarters();

                    // Write the JSON content to the specified file
                    File.WriteAllText(filePath, jsonContent);

                    MessageBox.Show("Assort file saved successfully!", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }
    // Settings class to hold the theme preference
    public class Settings
    {
        public bool IsDarkMode { get; set; }
    }
}
