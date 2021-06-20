using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ex2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileManager sourcefile;
        private FileManager grammarfile;
        private Analyser analyser;
        public ObservableCollection<Action> Actions { get; set; }
        public MainWindow()
        {
            Actions = new ObservableCollection<Action>();
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择源文件",
                Filter = "所有文件(*.*)|*.*",
                FileName = "选择文件夹.",
                FilterIndex = 1,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = false,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            bool? result = openFileDialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            else
            {
                sourcefile = new FileManager(openFileDialog.FileName);
                foreach (var str in sourcefile.getFileContent())
                {
                    rawFile.Text += (str + "\n");
                }
            }
        }

        private void startAnalysis_Click(object sender, RoutedEventArgs e)
        {

            analyser = new Analyser(grammarfile.getFileContent());

            for (int i = 0; i < analyser.grammar.count; i++)
            {
                rawFile.Text += "FIRST" + "(" + analyser.first.nonTerminal[i] + ")" + "=";
                foreach (var ch in analyser.first.First[i])
                {
                    rawFile.Text += ch + " ";
                }
                rawFile.Text += "\n";
            }

            for (int i = 0; i < analyser.grammar.count; i++)
            {
                rawFile.Text += "FOLLOW" + "(" + analyser.follow.nonTerminal[i] + ")" + "=";
                foreach (var ch in analyser.follow.Follow[i])
                {
                    rawFile.Text += ch + " ";
                }
                rawFile.Text += "\n";
            }

            //循环输出每位终结符
            int a = 0;
            rawFile.Text += string.Format("{0,-10}", "  ");
            for (int i = 0; i < analyser.grammar.terNum; i++)
            {
                rawFile.Text += string.Format("{0,-10}", analyser.grammar.terminalChar[i]);
            }
            rawFile.Text += "\n";
            //输出每行
            for (int i = 0; i < analyser.grammar.count; i++)
            {
                //输出非终结字符
                //输出相应的产生式
                rawFile.Text += string.Format("{0,-10}", analyser.grammar.grammarTable[i, 0][0]);
                for (int j = 0; j < analyser.grammar.terNum; j++)
                {
                    rawFile.Text += string.Format("{0,-10}", analyser.analyseTable[i, j]);
                }
                rawFile.Text += "\n";
            }
        }

        private void selectGrammar_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择语法文件",
                Filter = "所有文件(*.*)|*.*",
                FileName = "选择文件夹.",
                FilterIndex = 1,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = false,
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            bool? result = openFileDialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            else
            {
                grammarfile = new FileManager(openFileDialog.FileName);
                foreach (var str in grammarfile.getFileContent())
                {
                    rawFile.Text += (str + "\n");
                }
            }
        }

        private void show_Click(object sender, RoutedEventArgs e)
        {
            Actions.Clear();
            foreach (var str in sourcefile.getFileContent())
            {
                analyser.analysis(str);
                foreach (var action in analyser.getResult())
                {
                    Actions.Add(action);
                }
            }
            //Result.SetBinding(Actions);
            (this.FindName("Result") as DataGrid).ItemsSource = Actions;
        }
    }
}
