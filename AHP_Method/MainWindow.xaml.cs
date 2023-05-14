using AHP_Method.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AHP_Method
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Parameter rootParameter = new Parameter("Problem odločanja");
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
                        selectedParameter.Children.Add(newChild);
                        newChild.Parent = selectedParameter;
                    }
                }
                else
                {
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
