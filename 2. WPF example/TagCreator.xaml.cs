using Incas.Core.Classes;
using Incas.Core.Views.Windows;
using Incas.CustomDatabases.Views.Windows;
using Incas.Templates.Components;
using Incas.Templates.Models;
using Incas.Templates.ViewModels;
using Incas.Templates.Views.Windows;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Incas.Templates.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для TagCreator.xaml
    /// </summary>
    public partial class TagCreator : UserControl
    {
        public delegate void MethodContainer(TagCreator t);
        public event MethodContainer onDelete;
        public Tag tag;
        private TagViewModel vm;
        public TagCreator(Tag t, bool isNew = false)
        {
            this.InitializeComponent();
            this.tag = t;
            this.vm = new(t);
            this.DataContext = this.vm;
            this.ExpanderButton.IsChecked = true;
        }

        public bool Check()
        {
            if (string.IsNullOrWhiteSpace(this.tag.visibleName))
            {
                return false;
            }
            switch (this.tag.type)
            {
                case TagType.Variable:
                case TagType.Text:
                default:
                    return true;
                case TagType.Relation:
                case TagType.LocalEnumeration:
                case TagType.Generator:
                case TagType.Table:
                    if (string.IsNullOrWhiteSpace(this.tag.value))
                    {
                        return false;
                    }
                    return true;
            }
        }

        public void SaveTag(int templ)
        {
            this.tag.template = templ;
            this.tag.UpdateTag();
        }
        public void Minimize()
        {
            this.ExpanderButton.IsChecked = false;
        }
        public void Maximize()
        {
            this.ExpanderButton.IsChecked = true;
        }
        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            this.MainBorder.Height = this.ContentPanel.Height + 40;
            this.NumberUp.Visibility = Visibility.Collapsed;
        }
        private void MinimizeClick(object sender, RoutedEventArgs e)
        {
            this.MainBorder.Height = 40;
            this.NumberUp.Visibility = Visibility.Visible;
        }

        private void RemoveClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            onDelete?.Invoke(this);
            this.tag.RemoveTag();
        }

        private void CopyAllClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Clipboard.SetText(this.tag.value);
        }

        private void ClearClick(object sender, RoutedEventArgs e)
        {
            this.vm.DefaultValue = "";
        }

        private void AddVirtualTagClick(object sender, RoutedEventArgs e)
        {
            int start = this.vm.DefaultValue is not null ? this.vm.DefaultValue.Length : 0;
            this.vm.DefaultValue += "[Новый]";
            this.MainTextBox.SelectionStart = start + 1;
            this.MainTextBox.SelectionLength = 5;
        }

        private void DefineRelationClick(object sender, RoutedEventArgs e)
        {
            BindingSelector bs = new();
            bs.ShowDialog();
            if (bs.Result == DialogStatus.Yes)
            {
                this.vm.DefaultValue = $"{bs.SelectedDatabase}.{bs.SelectedTable}.{bs.SelectedField}";
            }
        }

        private void DefineGeneratorClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(this.vm.DefaultValue))
                {
                    if (DialogsManager.ShowQuestionDialog(
                    "Вы хотите использовать существующий генератор или создать новый?",
                    "Новый или существующий",
                    "Новый", "Существующий") == DialogStatus.Yes)
                    {
                        CreateTextTemplate ctt = new();
                        ctt.Show();
                        if (ctt.template.id > 0)
                        {
                            this.vm.DefaultValue = ctt.template.id.ToString();
                        }                
                    }
                    else
                    {
                        Template t = DialogsManager.ShowTemplateSelector(TemplateType.Text, "Выберите генератор для использования");
                        if (t.id != 0)
                        {
                            this.vm.DefaultValue = t.id.ToString();
                        }
                    }
                }
                else
                {
                    using Template t = new();
                    CreateTextTemplate ctt = new(t.GetTemplateById(int.Parse(this.vm.DefaultValue)));
                    ctt.Show();
                }
            }
            catch (Exception ex)
            {
                DialogsManager.ShowErrorDialog("При попытке открытия генератора возникла ошибка:\n" + ex.Message);
            }
        }

        private void EditScriptClick(object sender, RoutedEventArgs e)
        {
            CreateTagCommand ct = new(this.tag.GetCommand());
            ct.ShowDialog();
            if (ct.Result == DialogStatus.Yes)
            {
                this.tag.SaveCommand(ct.Command);
            }
            else if (ct.Result == DialogStatus.No)
            {
                this.tag.command = "";
            }
        }
        public void SetOrderNumber(int orderNumber)
        {
            this.vm.OrderNumber = orderNumber;
        }
        private void UpClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.vm.IncrementOrder();
        }

        private void DownClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.vm.DecrementOrder();
        }

        private void CopyNameToVisible(object sender, RoutedEventArgs e)
        {
            this.DublicateName();
        }
        public void DublicateName() // публичный потому что в родительском контейнере есть функция, позволяющая вызывать у всех дочерних элементов этого типа эту функцию
        {
            this.vm.VisibleName = this.vm.NameOfTag;
        }
    }
}
