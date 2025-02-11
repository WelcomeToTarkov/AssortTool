using System.Windows;

namespace AssortEditor.Models;

public partial class DogtagBarterInputs : Window
{
    public int? Level { get; private set; }
    public string Side { get; private set; }

    public DogtagBarterInputs()
    {
        InitializeComponent();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(LevelTextBox.Text, out int level))
        {
            Level = level;
            Side = SideTextBox.Text;
            DialogResult = true;
        }
        else
        {
            MessageBox.Show("Please enter a valid number for the level.");
        }
    }
}