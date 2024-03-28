using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ToDoWPFApp {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        static string ExePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Substring(6);
        string Filename = ExePath + "\\toDoList.txt";

        List<ToDoItem> ListOfToDoItems = new List<ToDoItem>();

        private string textboxTitleDefault = "Title of To-Do";
        private string textboxContentDefault = "Content of To-Do";
        private string textboxFinishedDefault = "Status";
        private string textboxHelpDefault = "Extra Information and Notification will show up here.";

        private void Window_Loaded( object sender, RoutedEventArgs e ) {
            ReadFromFile();
            if (ListOfToDoItems.Count == 0) {
                buttonModify.IsEnabled = false;
                buttonRemove.IsEnabled = false;
                buttonRemoveAll.IsEnabled = false;
            }
        }
        private void textboxTitle_GotFocus( object sender, RoutedEventArgs e ) {
            textboxHelp.Text = "Enter the Title of the To-Do here.\n" +
                               "The Title of selected To-Do also shows up here.";
            if (textboxTitle.Text == textboxTitleDefault)
                textboxTitle.Text = String.Empty;
            textboxTitle.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void textboxTitle_LostFocus( object sender, RoutedEventArgs e ) {
            if (textboxTitle.Text == String.Empty) {
                textboxTitle.Text = textboxTitleDefault;
                textboxTitle.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void textboxContent_GotFocus( object sender, RoutedEventArgs e ) {
            textboxHelp.Text = "The content of the To-Do.\n" +
                               "Enter the details of what you need to do here.";
            if (textboxContent.Text == textboxContentDefault)
                textboxContent.Text = String.Empty;
            textboxContent.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void textboxContent_LostFocus( object sender, RoutedEventArgs e ) {
            if (textboxContent.Text == String.Empty) {
                textboxContent.Text = textboxContentDefault;
                textboxContent.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
        private void textboxFinished_GotFocus( object sender, RoutedEventArgs e ) {
            textboxHelp.Text = "The status of the selected To-Do item.";
        }

        //private void textboxFinished_LostFocus( object sender, RoutedEventArgs e ) {
        //    if (textboxFinished.Text == String.Empty) {
        //        textboxFinished.Text = textboxFinishedDefault;
        //        textboxFinished.Foreground = new SolidColorBrush(Colors.Gray);
        //    }
        //}

        private void buttonAdd_Click( object sender, RoutedEventArgs e ) {
            if (textboxTitle.Text == "" || textboxTitle.Text == textboxTitleDefault) {
                textboxTitle.Focus();
                return;
            }
            if (textboxContent.Text == "" || textboxContent.Text == textboxContentDefault) {
                textboxContent.Focus();
                return;
            }

            try {
                SetToDoHighestId();
                ToDoItem td = new ToDoItem(
                    textboxTitle.Text,
                    textboxContent.Text,
                    false
                );
                bool CanAdd = true;
                foreach (ToDoItem item in ListOfToDoItems)
                    if (CompareToDo(item, td)) {
                        CanAdd = !CompareToDo(item, td);
                        break;
                    }
                if (!CanAdd) { return; }
                ListOfToDoItems.Add(td);
                CheckBox cb = new CheckBox {
                    Content = "(" + td.Id + ") " + td.Title,
                    Name = "toDoItem",
                };
                cb.Checked += toDo_Checked;
                cb.Unchecked += toDo_Unchecked;
                listboxToDo.Items.Add(cb);
                WriteToFile();
                textboxTitle.Text = textboxTitleDefault;
                textboxTitle.Foreground = new SolidColorBrush(Colors.Gray);
                textboxContent.Text = textboxContentDefault;
                textboxContent.Foreground = new SolidColorBrush(Colors.Gray);

                buttonModify.IsEnabled = true;
                buttonRemove.IsEnabled = true;
                buttonRemoveAll.IsEnabled = true;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonRemove_Click( object sender, RoutedEventArgs e ) {
            if (listboxToDo.SelectedIndex == -1)
                return;
            if (MessageBox.Show("Remove This To-Do Item?", "Are you sure?", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;
            try {
                ListOfToDoItems.RemoveAt(listboxToDo.SelectedIndex);
                WriteToFile();
                listboxToDo.Items.Remove(listboxToDo.SelectedItem);
                if (ListOfToDoItems.Count == 0) {
                    buttonModify.IsEnabled = false;
                    buttonRemove.IsEnabled = false;
                    buttonRemoveAll.IsEnabled = false;
                }
                textboxHelp.Text = textboxHelpDefault;
                textboxFinished.FontWeight = FontWeights.Normal;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void buttonRemoveAll_Click( object sender, RoutedEventArgs e ) {
            if (MessageBox.Show("Remove All To-Do Items?", "Are you sure?", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
                return;
            listboxToDo.Items.Clear();
            ListOfToDoItems.Clear();
            WriteToFile();
            buttonModify.IsEnabled = false;
            buttonRemove.IsEnabled = false;
            buttonRemoveAll.IsEnabled = false;
            textboxHelp.Text = textboxHelpDefault;
            textboxFinished.FontWeight = FontWeights.Normal;
        }

        private void buttonModify_Click( object sender, RoutedEventArgs e ) {
            int CurrentIndex = listboxToDo.SelectedIndex;
            if (CurrentIndex <= -1)
                return;
            ToDoItem tdModified = new ToDoItem(
                textboxTitle.Text,
                textboxContent.Text,
                ListOfToDoItems[CurrentIndex].Checked
            );
            tdModified.SetId(ListOfToDoItems[CurrentIndex].Id);
            if (CompareToDo(tdModified, ListOfToDoItems[CurrentIndex]))
                return;

            CheckBox cb = new CheckBox {
                Content = "(" + tdModified.Id + ") " + tdModified.Title,
                Name = "toDoItem",
                IsChecked = tdModified.Checked,
            };
            cb.Checked += toDo_Checked;
            cb.Unchecked += toDo_Unchecked;
            if ((bool) cb.IsChecked) {
                cb.Foreground = new SolidColorBrush(Colors.Green);
                cb.FontWeight = FontWeights.DemiBold;
            }
            listboxToDo.Items.Remove(listboxToDo.SelectedItem);
            ListOfToDoItems.Remove(ListOfToDoItems[CurrentIndex]);
            listboxToDo.Items.Insert(CurrentIndex, cb);
            ListOfToDoItems.Insert(CurrentIndex, tdModified);
            textboxTitle.Foreground = new SolidColorBrush(Colors.Black);
            textboxContent.Foreground = new SolidColorBrush(Colors.Black);
            textboxTitle.Text = tdModified.Title;
            textboxContent.Text = tdModified.Content;
            if (tdModified.Checked) {
                textboxFinished.Text = "Finished";
                textboxFinished.Foreground = new SolidColorBrush(Colors.Green);
                textboxFinished.FontWeight = FontWeights.DemiBold;
            }
            else {
                textboxFinished.Text = "Unfinished";
                textboxFinished.Foreground = new SolidColorBrush(Colors.Black);
                textboxFinished.FontWeight = FontWeights.Normal;
            }
            WriteToFile();
        }

        private void buttonReload_Click( object sender, RoutedEventArgs e ) {
            ReadFromFile();
            textboxHelp.Text = textboxHelpDefault;
            textboxFinished.FontWeight = FontWeights.Normal;
        }

        private void listboxToDo_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
            if (listboxToDo.SelectedIndex == -1) {
                textboxTitle.Text = textboxTitleDefault;
                textboxTitle.Foreground = new SolidColorBrush(Colors.Gray);
                textboxContent.Text = textboxContentDefault;
                textboxContent.Foreground = new SolidColorBrush(Colors.Gray);
                textboxFinished.Text = textboxFinishedDefault;
                textboxFinished.Foreground = new SolidColorBrush(Colors.Gray);
                return;
            }
            textboxHelp.Text = "This is a To-Do item.";
            CheckBox selected = listboxToDo.SelectedItem as CheckBox;
            string[] selectedSplit = selected.Content.ToString().Trim().Split(')');
            int idSelected = int.Parse(selectedSplit[0].Substring(1));
            textboxTitle.Foreground = new SolidColorBrush(Colors.Black);
            textboxContent.Foreground = new SolidColorBrush(Colors.Black);
            textboxFinished.Foreground = new SolidColorBrush(Colors.Black);
            foreach (ToDoItem item in ListOfToDoItems) {
                if (item.Id == idSelected) {
                    textboxTitle.Text = item.Title;
                    textboxContent.Text = item.Content;
                    if (item.Checked) {
                        textboxFinished.Text = "Finished";
                        textboxFinished.Foreground = new SolidColorBrush(Colors.Green);
                        textboxFinished.FontWeight = FontWeights.DemiBold;
                    }
                    else {
                        textboxFinished.Text = "Unfinished";
                        textboxFinished.Foreground = new SolidColorBrush(Colors.Black);
                        textboxFinished.FontWeight = FontWeights.Normal;
                    }
                    break;
                }
            }
        }

        private void toDo_Checked( object sender, RoutedEventArgs e ) {
            CheckBox cb = sender as CheckBox;
            listboxToDo.SelectedItem = cb;
            try {
                int id = int.Parse(cb.Content.ToString().Trim().Split(')')[0].Substring(1));
                ToDoItem tdToAdd = null, tdToRemove = null;
                foreach (ToDoItem item in ListOfToDoItems) {
                    if (item.Id == id) {
                        tdToAdd = new ToDoItem(item.Title, item.Content, !item.Checked);
                        tdToAdd.SetId(id);
                        tdToRemove = item;
                        break;
                    }
                }
                if (tdToAdd != null) {
                    ListOfToDoItems.Insert(listboxToDo.SelectedIndex, tdToAdd);
                    ListOfToDoItems.Remove(tdToRemove);
                    WriteToFile();
                }
                textboxFinished.Text = "Finished";
                textboxFinished.Foreground = new SolidColorBrush(Colors.Green);
                textboxFinished.FontWeight = FontWeights.DemiBold;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
            cb.FontWeight = FontWeights.DemiBold;
            cb.Foreground = new SolidColorBrush(Colors.Green);
        }

        private void toDo_Unchecked( object sender, RoutedEventArgs e ) {
            CheckBox cb = sender as CheckBox;
            listboxToDo.SelectedItem = cb;
            try {
                int id = int.Parse(cb.Content.ToString().Trim().Split(')')[0].Substring(1));
                ToDoItem tdToAdd = null, tdToRemove = null;
                foreach (ToDoItem item in ListOfToDoItems) {
                    if (item.Id == id) {
                        tdToAdd = new ToDoItem(item.Title, item.Content, !item.Checked);
                        tdToAdd.SetId(id);
                        tdToRemove = item;
                        break;
                    }
                }
                if (tdToAdd != null) {
                    ListOfToDoItems.Insert(listboxToDo.SelectedIndex, tdToAdd);
                    ListOfToDoItems.Remove(tdToRemove);
                    WriteToFile();
                }
                textboxFinished.Text = "Unfinished";
                textboxFinished.Foreground = new SolidColorBrush(Colors.Black);
                textboxFinished.FontWeight = FontWeights.Normal;
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
            cb.FontWeight = FontWeights.Normal;
            cb.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void WriteToFile() {
            try {
                StreamWriter sw = new StreamWriter(Filename);
                foreach (ToDoItem item in ListOfToDoItems) {
                    string toDoData = String.Empty;
                    toDoData += item.Id + "|" + item.Title + "|" + item.Content + "|" + item.Checked + "";
                    sw.WriteLine(toDoData);
                }
                sw.Close();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }
        private void ReadFromFile() {
            if (!File.Exists(Filename))
                return;
            try {
                listboxToDo.Items.Clear();
                ListOfToDoItems.Clear();
                StreamReader streamReader = new StreamReader(Filename);
                string line = streamReader.ReadLine();
                while (line != null) {
                    string[] param = line.Split('|');
                    ToDoItem td = new ToDoItem(param[1], param[2], param[3] == "True" ? true : false);
                    td.SetId(int.Parse(param[0]));
                    ListOfToDoItems.Add(td);
                    CheckBox cb = new CheckBox {
                        Content = "(" + td.Id + ") " + td.Title,
                        Name = "toDoItem",
                    };
                    cb.IsChecked = td.Checked;
                    if (cb.IsChecked == true) {
                        cb.Foreground = new SolidColorBrush(Colors.Green);
                        cb.FontWeight = FontWeights.DemiBold;
                    }
                    cb.Checked += toDo_Checked;
                    cb.Unchecked += toDo_Unchecked;
                    listboxToDo.Items.Add(cb);
                    line = streamReader.ReadLine();
                }
                SetToDoHighestId();
                streamReader.Close();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SetToDoHighestId() {
            ToDoItem[] toDoItemsArray = ListOfToDoItems.ToArray();
            if (toDoItemsArray.Length == 0)
                ToDoItem.HighestId = -1;
            else
                ToDoItem.HighestId = toDoItemsArray[toDoItemsArray.Length - 1].Id;
        }

        private bool CompareToDo( ToDoItem left, ToDoItem right ) 
            => left.Title == right.Title &&
               left.Content == right.Content;
    }
}
