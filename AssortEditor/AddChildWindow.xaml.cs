using System.Windows;

namespace AssortEditor
{
    public partial class AddChildWindow : Window
    {
        public string NewTpl { get; set; }
        public string NewSlotId { get; set; }

        public AddChildWindow()
        {
            InitializeComponent();
            DataContext = this;  // Set the DataContext to the current instance (AddChildWindow)
        }

        // Button click event to confirm adding the new item
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate the inputs
            if (string.IsNullOrWhiteSpace(NewTpl) || string.IsNullOrWhiteSpace(NewSlotId))
            {
                MessageBox.Show("Please provide both Tpl and SlotId.");
                return;
            }

            DialogResult = true; // Close the window and return true to indicate success
            Close();
        }
    }
}