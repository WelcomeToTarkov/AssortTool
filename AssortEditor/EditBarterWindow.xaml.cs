using System.Windows;
using AssortEditor.Models;

namespace AssortEditor
{
    public partial class EditBarterWindow : Window
    {
        public Barter SelectedBarter { get; set; }

        public EditBarterWindow(Barter barter)
        {
            InitializeComponent();
            SelectedBarter = barter;
            BarterTplTextBox.Text = barter._tpl;
            BarterCountTextBox.Text = barter.Count.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Update the barter with the new values
            SelectedBarter._tpl = BarterTplTextBox.Text;
            int newCount;
            if (int.TryParse(BarterCountTextBox.Text, out newCount))
            {
                SelectedBarter.Count = newCount;

                // Check for specific tpl values
                if (SelectedBarter._tpl == "59f32c3b86f77472a31742f0" || SelectedBarter._tpl == "59f32bb586f774757e1e8442")
                {
                    // Open the InputDialog to edit Level and Side
                    DogtagBarterInputs dialog = new DogtagBarterInputs();

                    // Pre-fill with existing values if available
                    if (SelectedBarter.Level.HasValue)
                    {
                        dialog.LevelTextBox.Text = SelectedBarter.Level.Value.ToString();
                    }
                    if (!string.IsNullOrEmpty(SelectedBarter.Side))
                    {
                        dialog.SideTextBox.Text = SelectedBarter.Side;
                    }

                    if (dialog.ShowDialog() == true)
                    {
                        // Update Level and Side values
                        SelectedBarter.Level = dialog.Level;
                        SelectedBarter.Side = dialog.Side;
                    }
                }
                else
                {
                    // Pre-fill with existing values if available
                    if (SelectedBarter.Level.HasValue)
                    {
                        SelectedBarter.Level = null;
                    }
                    if (!string.IsNullOrEmpty(SelectedBarter.Side))
                    {
                        SelectedBarter.Side = null;
                    }
                }

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid count.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
