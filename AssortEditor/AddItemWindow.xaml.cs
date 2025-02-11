using System.Windows;
using AssortEditor.Models;

namespace AssortEditor
{
    public partial class AddItemWindow : Window
    {
        public Item CurrentItem { get; set; }

        public AddItemWindow()
        {
            InitializeComponent();
            CurrentItem = new Item
            {
                upd = new Upd
                {
                    repairable = new Repairable(),
                    foldable = new Foldable(),
                    firemode = new FireMode()
                }
            };
            BarterListView.ItemsSource = CurrentItem.Barters;
        }



        private void AddBarterButton_Click(object sender, RoutedEventArgs e)
        {
            string barterTpl = BarterTplTextBox.Text;
            int barterCount;

            if (int.TryParse(BarterCountTextBox.Text, out barterCount))
            {
                var newBarter = new Barter
                {
                    _tpl = barterTpl,
                    Count = barterCount
                };

                if (barterTpl == "59f32c3b86f77472a31742f0" || barterTpl == "59f32bb586f774757e1e8442")
                {
                    // Show the input dialog
                    DogtagBarterInputs dialog = new DogtagBarterInputs();
                    if (dialog.ShowDialog() == true)
                    {
                        // Get the level and side
                        newBarter.Level = dialog.Level;
                        newBarter.Side = dialog.Side;
                    }
                }

                CurrentItem.Barters.Add(newBarter);
                BarterTplTextBox.Clear();
                BarterCountTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a valid number for the count.");
            }
        }


        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentItem._tpl = ItemTplTextBox.Text;

            // Add upd properties
            CurrentItem.upd.unlimitedcount = UnlimitedCountCheckBox.IsChecked == true;
            if (int.TryParse(StackObjectsCountTextBox.Text, out int stackCount))
                CurrentItem.upd.stackobjectscount = stackCount;
            if (int.TryParse(BuyRestrictionMaxTextBox.Text, out int buyMax))
                CurrentItem.upd.buyrestrictionmax = buyMax;
            if (int.TryParse(BuyRestrictionCurrentTextBox.Text, out int buyCurrent))
                CurrentItem.upd.buyrestrictioncurrent = buyCurrent;

            // Add Repairable if applicable
            if (!string.IsNullOrWhiteSpace(DurabilityTextBox.Text) && 
                int.TryParse(DurabilityTextBox.Text, out int durability))
            {
                CurrentItem.upd.repairable.durability = durability;
            }
            if (!string.IsNullOrWhiteSpace(MaxDurabilityTextBox.Text) && 
                int.TryParse(MaxDurabilityTextBox.Text, out int maxDurability))
            {
                CurrentItem.upd.repairable.maxdurability = maxDurability;
            }

            if (!string.IsNullOrWhiteSpace(FireModeTextBox.Text))
            {
                CurrentItem.upd.firemode.firemode = FireModeTextBox.Text;
                Console.WriteLine($"FireMode set to: {FireModeTextBox.Text}");
            }

            if (FoldableCheckBox.IsChecked == true)
            {
                CurrentItem.upd.foldable.foldable = true;
                CurrentItem.upd.foldable.folded = FoldedCheckBox.IsChecked == true;
                Console.WriteLine($"Foldable set to: {CurrentItem.upd.foldable.foldable}, Folded: {CurrentItem.upd.foldable.folded}");
            }


            this.DialogResult = true;
            Close();
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void EditBarter_Click(object sender, RoutedEventArgs e)
        {
            var selectedBarter = BarterListView.SelectedItem as Barter;
            if (selectedBarter != null)
            {
                // Open the EditBarterWindow with the selected barter
                var editWindow = new EditBarterWindow(selectedBarter);
                if (editWindow.ShowDialog() == true)
                {
                    // If the user successfully edited the barter, update the list
                    BarterListView.Items.Refresh();
                }
            }
        }

        private void RemoveBarter_Click(object sender, RoutedEventArgs e)
        {
            var selectedBarter = BarterListView.SelectedItem as Barter;
            if (selectedBarter != null)
            {
                // Remove the selected barter from the list
                CurrentItem.Barters.Remove(selectedBarter);
            }
        }
    }
}
