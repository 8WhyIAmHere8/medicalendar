using Microsoft.Maui.Controls;

namespace medi1.EntryComponents
{
    public partial class NumericSpinner : ContentView
    {
        public NumericSpinner()
        {
            InitializeComponent();
            ValueEntry.Text = Value.ToString();
        }

        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create(nameof(Value), typeof(int), typeof(NumericSpinner), 0, BindingMode.TwoWay);

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private void OnIncreaseClicked(object sender, EventArgs e)
        {
            Value++;
            ValueEntry.Text = Value.ToString();
        }

        private void OnDecreaseClicked(object sender, EventArgs e)
        {
            if (Value > 0)
                Value--;

            ValueEntry.Text = Value.ToString();
        }

        private void OnEntryChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(e.NewTextValue, out int newValue))
                Value = newValue;
        }
    }
}
