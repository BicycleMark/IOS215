using System;
using UIKit;
using MyExpenses.Data;
using System.Collections.Generic;
using System.Linq;

namespace MyExpenses
{
	partial class ExpenseDetailViewController : UIViewController
	{
        Expense selectedExpense;
        public Expense SelectedExpense {
            get {
                return selectedExpense;
            }
            set {
                if (selectedExpense != value) {
                    selectedExpense = value;
                    OnUpdateDetails();
                }
            }
        }

        UIPickerView categoryPicker;
        UIToolbar pickerToolbar;
        UIBarButtonItem doneButton;
        TextFieldPickerData pickerData;

		public ExpenseDetailViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            saveButton.TouchUpInside += SaveExpense;

            // Use a picker view to select our category choices.
            categoryPicker = new UIPickerView();
            categoryPicker.Model = pickerData = new TextFieldPickerData(categoryText, 
                new[] { "Lodging", "Meal", "Transportation", "Other" });
            categoryText.InputView = categoryPicker;

            // Use a Toolbar to add a "Done" button to dismiss the picker.
            pickerToolbar = new UIToolbar {
                BarTintColor = UIColor.White,
                Translucent = false,
            };

            doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, 
                (s, e) => categoryText.ResignFirstResponder());

            var flexibleSpace = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null);
            pickerToolbar.Items = new [] { flexibleSpace, doneButton };
            pickerToolbar.SizeToFit();
            categoryText.InputAccessoryView = pickerToolbar;

            // Update the UI with the selected expense.
            OnUpdateDetails();
        }

        public override void ViewDidDisappear(bool animated)
        {
            // Get rid of things we created manually.
            categoryPicker.Dispose();
            categoryPicker = null;

            pickerToolbar.Dispose();
            pickerToolbar = null;

            doneButton.Dispose();
            doneButton = null;

            pickerData.Dispose();
            pickerData = null;
        }

        async void SaveExpense(object sender, EventArgs e)
        {
            // Save the expense back to the data store.
            if (SelectedExpense != null) {

                isBusyIndicator.StartAnimating();

                try {
                    string value = expenseName.Text;
                    SelectedExpense.Title = string.IsNullOrWhiteSpace(value) ? "New Expense" : value;
                    SelectedExpense.Billable = billableSwitch.On;
                    SelectedExpense.Category = categoryText.Text;
                    value = expenseAmount.Text; double amount;
                    if (string.IsNullOrWhiteSpace(value)
                        || !Double.TryParse(value, out amount))
                        SelectedExpense.Amount = 0;
                    else {
                        SelectedExpense.Amount = amount;
                    }

                    DataStore db = new DataStore();
                    await db.Update(SelectedExpense);
                }
                finally {
                    isBusyIndicator.StopAnimating();
                }
            }

            // Go back to prior screen.
            NavigationController.PopViewController(true);
        }

        void OnUpdateDetails()
        {
            if (!IsViewLoaded)
                return;

            if (SelectedExpense != null) {

                this.expenseName.Text = SelectedExpense.Title;
                this.expenseAmount.Text = SelectedExpense.Amount.ToString();
                this.billableSwitch.On = SelectedExpense.Billable;
                this.categoryText.Text = SelectedExpense.Category;
                pickerData.SelectItem(categoryPicker, SelectedExpense.Category);

            } else {
                this.expenseName.Text = string.Empty;
                this.expenseAmount.Text = "0.00";
                this.billableSwitch.On = false;
                this.categoryText.Text = string.Empty;
            }
        }

        class TextFieldPickerData : UIPickerViewModel
        {
            string[] data;
            WeakReference textFieldRef;

            public TextFieldPickerData(UITextField textField, IEnumerable<string> data)
            {
                this.textFieldRef = new WeakReference(textField);
                this.data = data.ToArray();
            }

            public bool SelectItem(UIPickerView pickerView, string item) 
            {
                int index = Array.IndexOf(data, item);
                if (index > 0) {
                    pickerView.Select(index, 0, false);
                    return true;
                }
                return false;
            }

            public override nint GetComponentCount(UIPickerView pickerView)
            {
                return 1;
            }

            public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
            {
                return data.Length;
            }

            public override string GetTitle(UIPickerView pickerView, nint row, nint component)
            {
                return data[row];
            }

            public override void Selected(UIPickerView pickerView, nint row, nint component)
            {
                UITextField textField = textFieldRef.Target as UITextField;
                if (textField != null) {
                    textField.Text = data[row];
                }
            }
        }
	}
}
