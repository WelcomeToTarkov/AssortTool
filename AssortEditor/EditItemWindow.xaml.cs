using System.Collections.ObjectModel;
using System.Windows;
using AssortEditor.Models;

namespace AssortEditor
{
    public partial class EditItemWindow : Window
    {
        public Item CurrentItem { get; set; }

        public EditItemWindow(Item itemToEdit)
        {
            InitializeComponent();

            // Ensure 'itemToEdit.upd' and its properties are not null
            CurrentItem = new Item
            {
                _id = itemToEdit._id,
                _tpl = itemToEdit._tpl,
                Barters = new ObservableCollection<Barter>(itemToEdit.Barters),
                parentid = itemToEdit.parentid,
                slotid = itemToEdit.slotid,
                upd = new Upd()
                {
                    unlimitedcount = itemToEdit.upd?.unlimitedcount ?? false, // Null-safe check
                    stackobjectscount = itemToEdit.upd?.stackobjectscount ?? 0,
                    buyrestrictionmax = itemToEdit.upd?.buyrestrictionmax ?? 0,
                    buyrestrictioncurrent = itemToEdit.upd?.buyrestrictioncurrent ?? 0,
                    repairable = itemToEdit.upd?.repairable != null 
                        ? new Repairable()
                        {
                            durability = itemToEdit.upd.repairable.durability,
                            maxdurability = itemToEdit.upd.repairable.maxdurability
                        }
                        : new Repairable(), // Fallback for null repairable
                    firemode = itemToEdit.upd?.firemode != null 
                        ? new FireMode() { firemode = itemToEdit.upd.firemode.firemode }
                        : new FireMode(), // Fallback for null firemode
                    foldable = itemToEdit.upd?.foldable != null 
                        ? new Foldable()
                        {
                            foldable = itemToEdit.upd.foldable.foldable,
                            folded = itemToEdit.upd.foldable.folded
                        }
                        : new Foldable(), // Fallback for null foldable
                }
            };

            // Initialize the UI fields with values from CurrentItem
            ItemTplTextBox.Text = CurrentItem._tpl;

            // Adjust UI for top-level or child item
            if (CurrentItem.parentid != "hideout")
            {
                SetupForChildItem();
            }
            else
            {
                SetupForTopLevelItem();
            }

            DataContext = CurrentItem;
        }



        private void SetupForTopLevelItem()
        {
            SlotIdTextBox.Visibility = Visibility.Collapsed;
            SlotIdTextBoxLabel.Visibility = Visibility.Collapsed;

            BarterListView.ItemsSource = CurrentItem.Barters;
            UnlimitedCountCheckBox.IsChecked = CurrentItem.upd.unlimitedcount;
            StackObjectsCountTextBox.Text = CurrentItem.upd.stackobjectscount.ToString();
            BuyRestrictionMaxTextBox.Text = CurrentItem.upd.buyrestrictionmax.ToString();
            BuyRestrictionCurrentTextBox.Text = CurrentItem.upd.buyrestrictioncurrent.ToString();

            // Repairable fields
            if (CurrentItem.upd.repairable != null)
            {
                DurabilityTextBox.Text = CurrentItem.upd.repairable.durability?.ToString();
                MaxDurabilityTextBox.Text = CurrentItem.upd.repairable.maxdurability?.ToString();
            }

            // FireMode field
            FireModeTextBox.Text = CurrentItem.upd.firemode?.firemode;

            // Foldable field
            if (CurrentItem.upd.foldable.foldable)
            {
                FoldableCheckBox.IsChecked = true;
                FoldedCheckBox.IsChecked = CurrentItem.upd.foldable.folded;
            }
        }

        private void SetupForChildItem()
        {
            SlotIdTextBox.Text = CurrentItem.slotid;
            
            BarterTplTextBoxLabel.Visibility = Visibility.Collapsed;
            BarterTplTextBox.Visibility = Visibility.Collapsed;
            BarterCountTextBoxLabel.Visibility = Visibility.Collapsed;
            BarterCountTextBox.Visibility = Visibility.Collapsed;
            AddBarterButton.Visibility = Visibility.Collapsed;
            BarterListView.Visibility = Visibility.Collapsed;
            BarterListViewLabel.Visibility = Visibility.Collapsed;
            ItemPropertiesTextBlock.Visibility = Visibility.Collapsed;
            UnlimitedCountCheckBox.Visibility = Visibility.Collapsed;
            StackObjectsTextBlock.Visibility = Visibility.Collapsed;
            StackObjectsCountTextBox.Visibility = Visibility.Collapsed;
            BuyRestrictionCurrentTextBlock.Visibility = Visibility.Collapsed;
            BuyRestrictionCurrentTextBox.Visibility = Visibility.Collapsed;
            BuyRestrictionMaxTextBlock.Visibility = Visibility.Collapsed;
            BuyRestrictionMaxTextBox.Visibility = Visibility.Collapsed;
            RepairableHeader.Visibility = Visibility.Collapsed;
            DurabilityTextBlock.Visibility = Visibility.Collapsed;
            DurabilityTextBox.Visibility = Visibility.Collapsed;
            MaxDurabilityTextBlock.Visibility = Visibility.Collapsed;
            MaxDurabilityTextBox.Visibility = Visibility.Collapsed;
            FireModeTextBoxLabel.Visibility = Visibility.Collapsed;
            FireModeTextBox.Visibility = Visibility.Collapsed;
            FoldableCheckBoxLabel.Visibility = Visibility.Collapsed;
            FoldedCheckBoxLabel.Visibility = Visibility.Collapsed;
            FoldedCheckBox.Visibility = Visibility.Collapsed;
            FiremodeFoldableHeader.Visibility = Visibility.Collapsed;
            FoldableCheckBox.Visibility = Visibility.Collapsed;
        }


    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Save the shared `tpl` property
        CurrentItem._tpl = ItemTplTextBox.Text;

        // Check if the item is a child item

        if (CurrentItem.parentid != "hideout")
        {
            // Save only `tpl` and `slotid` for child items
            CurrentItem.slotid = SlotIdTextBox.Text;
            if (CurrentItem.slotid == "cartridges")
            {
                // Handle "cartridges" slot case
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter the StackObjectsCount for the item:",
                    "StackObjectsCount Required",
                    CurrentItem.upd.stackobjectscount.ToString() ?? "10" // Default value
                );

                if (int.TryParse(input, out int parsedStackCount))
                {

                    CurrentItem.Location = 0;
                    CurrentItem.upd = new Upd
                    {
                        stackobjectscount = parsedStackCount
                    };
                }
                else
                {
                    MessageBox.Show("Invalid input for StackObjectsCount. Please enter a valid number.");
                    return;
                }
            }
        }
        else
        {
            // Save `tpl` and all other properties for top-level items
            CurrentItem.upd.unlimitedcount = UnlimitedCountCheckBox.IsChecked == true;
            CurrentItem.upd.stackobjectscount = int.TryParse(StackObjectsCountTextBox.Text, out int stackCount) ? stackCount : 0;
            CurrentItem.upd.buyrestrictionmax = int.TryParse(BuyRestrictionMaxTextBox.Text, out int buyMax) ? buyMax : 0;
            CurrentItem.upd.buyrestrictioncurrent = int.TryParse(BuyRestrictionCurrentTextBox.Text, out int buyCurrent) ? buyCurrent : 0;

            // Validate repairable fields
            if (!string.IsNullOrWhiteSpace(DurabilityTextBox.Text) && !string.IsNullOrWhiteSpace(MaxDurabilityTextBox.Text))
            {
                CurrentItem.upd.repairable = CurrentItem.upd.repairable ?? new Repairable();
                CurrentItem.upd.repairable.durability = int.TryParse(DurabilityTextBox.Text, out int durability) ? durability : (int?)null;
                CurrentItem.upd.repairable.maxdurability = int.TryParse(MaxDurabilityTextBox.Text, out int maxDurability) ? maxDurability : (int?)null;
            }
            else
            {
                CurrentItem.upd.repairable = null;
            }

            // Save FireMode
            if (string.IsNullOrWhiteSpace(FireModeTextBox.Text))
            {
                CurrentItem.upd.firemode = null;
            }
            else
            {
                CurrentItem.upd.firemode.firemode = FireModeTextBox.Text;
            }

            // Save Foldable
            if (FoldableCheckBox.IsChecked == true)
            {
                if (CurrentItem.upd.foldable != null)
                {
                    CurrentItem.upd.foldable.foldable = true;
                    if (FoldedCheckBox.IsChecked != null)
                        CurrentItem.upd.foldable.folded = FoldedCheckBox.IsChecked.Value;
                }
            }
            else
            {
                CurrentItem.upd.foldable = null;
            }
        }

        // Set the dialog result and close the window
        this.DialogResult = true;
        Close();
    }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void AddBarterButton_Click(object sender, RoutedEventArgs e)
        {
            string barterTpl = BarterTplTextBox.Text;
            if (int.TryParse(BarterCountTextBox.Text, out int barterCount))
            {
                var newBarter = new Barter { _tpl = barterTpl, Count = barterCount };
                CurrentItem.Barters.Add(newBarter);
                BarterTplTextBox.Clear();
                BarterCountTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Please enter a valid number for the count.");
            }
        }

        private void EditBarterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedBarter = BarterListView.SelectedItem as Barter;
            if (selectedBarter != null)
            {
                var editWindow = new EditBarterWindow(selectedBarter);
                if (editWindow.ShowDialog() == true)
                {
                    BarterListView.Items.Refresh();
                }
            }
        }

        private void RemoveBarterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedBarter = BarterListView.SelectedItem as Barter;
            if (selectedBarter != null)
            {
                CurrentItem.Barters.Remove(selectedBarter);
            }
        }
    }
}
