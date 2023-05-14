using AHP_Method.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Xml.Linq;

namespace AHP_Method
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Parameter rootParameter = new Parameter("Problem odločanja");
        List<Parameter> parametri;

         static bool HasChild(Parameter parameter)
        {
            if (parameter.Children.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            var rootCollection = new ObservableCollection<Parameter> { rootParameter };
            DataContext = rootCollection;
        }


        private void dodajParameter_Click(object sender, RoutedEventArgs e)
        {
            string newParameterName = newParameterTextBox.Text;
            if (!String.IsNullOrEmpty(newParameterName))
            {
                Parameter newChild = new Parameter(newParameterName);
                if (treeView.SelectedItem != null)
                {
                    Parameter selectedParameter = treeView.SelectedItem as Parameter;
                    if (selectedParameter != null)
                    {
                        if (selectedParameter.Children.Any(p => p.Name.ToLower() == newParameterName.ToLower()))
                        {
                            MessageBox.Show($"Parameter z imenom '{newParameterName}' že obstaja!");
                            return;
                        }
                        selectedParameter.Children.Add(newChild);
                        newChild.Parent = selectedParameter;
                    }
                }
                else
                {
                    if (rootParameter.Children.Any(p => p.Name.ToLower() == newParameterName.ToLower()))
                    {
                        MessageBox.Show($"Parameter z imenom '{newParameterName}' že obstaja!");
                        return;
                    }
                    rootParameter.Children.Add(newChild);
                    newChild.Parent = rootParameter;
                }
            }
            else
            {
                MessageBox.Show("Vnesite ime parametra!");
            }
            newParameterTextBox.Text = "";
        }

        private void odstraniParameter_Click(object sender, RoutedEventArgs e)
        {
            Parameter selectedParameter = treeView.SelectedItem as Parameter;
            if (selectedParameter != null && selectedParameter != rootParameter)
            {
                if (selectedParameter.Parent != null)
                {
                    selectedParameter.Parent.Children.Remove(selectedParameter);
                }
                else
                {
                    rootParameter.Children.Remove(selectedParameter);
                }

                foreach (Parameter child in selectedParameter.Children)
                {
                    child.Parent = null;
                }

                ((ObservableCollection<Parameter>)DataContext).Remove(selectedParameter);
            }
        }

        private List<Parameter> GetParameterList(ObservableCollection<Parameter> rootCollection)
        {
            var parameterList = new List<Parameter>();

            foreach (var parameter in rootCollection)
            {
                parameterList.Add(parameter);
                parameterList.AddRange(GetChildParameters(parameter));
            }

            return parameterList;
        }

        private List<Parameter> GetChildParameters(Parameter parameter)
        {
            var parameterList = new List<Parameter>();

            foreach (var child in parameter.Children)
            {
                parameterList.Add(child);
                parameterList.AddRange(GetChildParameters(child));
            }

            return parameterList;
        }

        private void naprejNaParametre_Click(object sender, RoutedEventArgs e)
        {
             var parameterList = GetParameterList((ObservableCollection<Parameter>)DataContext);
            parametri = parameterList;
            PairwiseComparison();
            myTabControl.SelectedIndex = 1;
        }


        private void PairwiseComparison() //Iteracija skozi parametre kjer nato starše dodam v nov list
        {
            List<Parameter> parents = new List<Parameter>();
            foreach (Parameter p in parametri)
            {
                if(HasChild(p))
                {
                    parents.Add(p);
                }
            }    
        }
        //private void primerjavaParametrovPoParih()
        //{
        //    var parameterList = GetParameterList((ObservableCollection<Parameter>)DataContext);
        //    parametri = parameterList;
        //    var parameterPairs = new List<ParameterPair>();
        //    for (int i = 0; i < parameterList.Count; i++)
        //    {
        //        for (int j = i + 1; j < parameterList.Count; j++)
        //        {
        //            parameterPairs.Add(new ParameterPair(parameterList[i], parameterList[j]));
        //        }
        //    }
        //    myTabControl.SelectedIndex = 2;
        //    DataContext = parameterPairs;
        //}

        //neke za graf
        //private void generateGraf_Click(object sender, RoutedEventArgs e)
        //{
        //    string dotSource = GenerateDotSource(); // Generate the DOT source code
        //    byte[] graphImageBytes = GenerateGraphImage(dotSource); // Generate the graph image

        //    // Display the graph image
        //    BitmapImage bitmapImage = new BitmapImage();
        //    bitmapImage.BeginInit();
        //    bitmapImage.StreamSource = new MemoryStream(graphImageBytes);
        //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        //    bitmapImage.EndInit();
        //    graphImage.Source = bitmapImage;
        //}




        //private double[,] GeneratePairwiseComparisonMatrix(Parameter nodeParameter)
        //{
        //    int size = nodeParameter.GetSubtreeSize();
        //    double[,] matrix = new double[size, size];
        //    List<Parameter> parameters = nodeParameter.GetSubtree();

        //    for (int i = 0; i < size; i++)
        //    {
        //        matrix[i, i] = 1;
        //        for (int j = i + 1; j < size; j++)
        //        {
        //            double comparison = 0;
        //            if (parameters[i] == nodeParameter || parameters[j] == nodeParameter) // Compare the node parameter with its immediate children
        //            {
        //                comparison = GetComparisonFromUser(parameters[i], parameters[j]);
        //            }
        //            else // Recursively compare the subtree of each child
        //            {
        //                double[,] childMatrix = GeneratePairwiseComparisonMatrix(parameters[i]);
        //                double[,] subchildMatrix = GeneratePairwiseComparisonMatrix(parameters[j]);
        //                comparison = CalculateComparisonFromMatrices(childMatrix, subchildMatrix);
        //            }
        //            matrix[i, j] = comparison;
        //            matrix[j, i] = 1.0 / comparison;
        //        }
        //    }

        //    return matrix;
        //}

    }
}
