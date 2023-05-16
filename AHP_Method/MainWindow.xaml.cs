using AHP_Method.Model;
using LiveCharts.Wpf.Charts.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        List<Parameter> parents;

        private int currentIndex = 0;


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
            SortParents();
            myTabControl.SelectedIndex = 1;
        }


        private void SortParents() //Iteracija skozi parametre kjer nato starše dodam v nov list
        {
            parents = new List<Parameter>();
            foreach (Parameter p in parametri)
            {
                if (HasChild(p))
                {
                    parents.Add(p);
                }
            }

            parents.Reverse();
        }
  

        //private void pairwiseComparison2() //Funckija gre skozi vse starse in za vsakega pripravi primerjavo po parih njegovih otrok in ga doda v nov list premerjav
        //                                   // z indexcount bi visal vsakega starsa da bi funckija trajala dokler ni enaka stevila starsev?? in potem uporabnik veca index?
        //{
        //    foreach (Parameter parent in parents)
        //    {
        //        indexCount++;
        //        var parameterPairs = new List<ParameterPair>();
        //        for (int i = 0; i < parent.Children.Count; i++)
        //        {
        //            for (int j = i + 1; j < parent.Children.Count; j++)
        //            {
        //                var pair = new ParameterPair(parent.Children[i], parent.Children[j]);
        //                parameterPairs.Add(pair);
        //            }
        //        }
        //    }
        //}

        //private void pairwiseComparison()
        //{
        //    var parameterPairs = new List<ParameterPair>();
        //    for (int i = 0; i < parents.Count; i++)
        //    {
        //        for (int j = i + 1; j < parents.Count; j++)
        //        {
        //            var pair = new ParameterPair(parents[i], parents[j]);
        //            parameterPairs.Add(pair);
        //        }

        //        var childrenPairs = new List<ParameterPair>();
        //        foreach (var child in parents[i].Children)
        //        {
        //            for (int k = 0; k < parents.Count; k++)
        //            {
        //                if (parents[k] != parents[i] && parents[k].IsAncestor(child))
        //                {
        //                    var pair = new ParameterPair(child, parents[k]);
        //                    childrenPairs.Add(pair);
        //                }
        //            }
        //        }

        //        parameterPairs.AddRange(childrenPairs);
        //    }

        //    DataContext = parameterPairs;
        //    myTabControl.SelectedIndex = 2;
        //}

        private void Nalozi_Click(object sender, RoutedEventArgs e)
        {
            if (parents.Count == 0)
            {
                MessageBox.Show("Najprej morate dodati parametre!");
                myTabControl.SelectedIndex = 0;
            }
            if (currentIndex < parents.Count)
            {
                Parameter parent = parents[currentIndex];
                ObservableCollection<Parameter> children = parent.Children;
                DataTable table = new DataTable();

                dataGridParameters.Columns.Clear();
                dataGridParameters.CanUserAddRows = false;
                dataGridParameters.CanUserResizeColumns = false;
                dataGridParameters.AutoGenerateColumns = true;

                // Add the first column for the row headers
                table.Columns.Add(parent.Name);

                // Add columns for the child parameters
                foreach (Parameter child in children)
                {
                    table.Columns.Add(child.Name, typeof(double));
                }

                // Add a row for each child parameter
                for (int i = 0; i < children.Count; i++)
                {
                    DataRow row = table.NewRow();
                    row[0] = children[i].Name; // Set the value of the row header column
                    for (int j = 1; j <= children.Count; j++)
                    {
                        if (i == j - 1)
                        {
                            row[j] = 1; // Set diagonal cell values to 1
                        }
                        else
                        {
                            row[j] = 0; // Initialize the values of the other cells to 0
                        }
                    }
                    table.Rows.Add(row);
                }

                dataGridParameters.ItemsSource = table.DefaultView;
                currentIndex++; 
            }
        }

        private void Izracunaj_Click(object sender, RoutedEventArgs e)
        {

        }


        private void nextGrid_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex < parents.Count)
            {
                dataGridParameters.Columns.Clear();
                //currentIndex++;
                Nalozi_Click(sender, e);
            }
            else if (currentIndex == parents.Count)
            {
                MessageBox.Show("Konec primerjave parametrov po parih.");
                return;
            }
        }


        //private void ShowParametersInGrid()
        //{
        //    // Get all parameters
        //    var parameterList = GetParameterList((ObservableCollection<Parameter>)DataContext);

        //    // Sort parents
        //    SortParents();

        //    // Loop through parents and create a new tab for each
        //    for (int i = 0; i < parents.Count; i++)
        //    {
        //        var parent = parents[i];
        //        var dataGrid = new DataGrid();

        //        // Add columns to the data grid
        //        dataGrid.Columns.Add(new DataGridTextColumn
        //        {
        //            Header = parent.Name,
        //            Binding = new Binding($"PairwiseComparisonValues[{i},{i}]"),
        //            IsReadOnly = true
        //        });

        //        foreach (var child in parent.Children)
        //        {
        //            dataGrid.Columns.Add(new DataGridTextColumn
        //            {
        //                Header = child.Name,
        //                Binding = new Binding($"PairwiseComparisonValues[{parent.Index},{child.Index}]")
        //            });
        //        }

        //        // Add rows to the data grid
        //        for (int row = 0; row < parent.Children.Count; row++)
        //        {
        //            var child = parent.Children[row];
        //            var rowData = new List<double>();
        //            rowData.Add(1.0 / parameterList.Count); // Add the diagonal element
        //            for (int col = 0; col < parent.Children.Count; col++)
        //            {
        //                rowData.Add(1.0);
        //            }
        //            dataGrid.Items.Add(rowData);
        //        }

        //        // Add the data grid to a new tab
        //    }

        //    // Select the first tab
        //}





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
