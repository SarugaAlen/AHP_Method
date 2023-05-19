using AHP_Method.Model;
using LiveCharts.Wpf.Charts.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        List<Parameter> parametriBrezRoot;


        private int currentIndex = 0;
        private int currentWeightIndex = 0;

        private DataTable tableParameters;
        private DataTable tableWeights;
        private List<List<double>> savedTable;
        
        ObservableCollection<Alternativa> alternative { get; set; }

        private int currentIndexAlternative = 0;
        private DataTable tableAlternative;


        /// <summary>
        /// Preveri, ali parameter vsebuje otroke.
        /// </summary>
        /// <param name="parameter">Parameter, ki ga želimo preveriti.</param>
        /// <returns>
        /// <c>true</c>, če parameter vsebuje otroke; <c>false</c>, če parameter nima otrok.
        /// </returns>
        /// <remarks>
        /// Metoda preveri, ali dani parameter vsebuje vsaj enega otroka.
        /// Če ima parameter vsaj enega otroka, vrne vrednost <c>true</c>, sicer vrne vrednost <c>false</c>.
        /// </remarks>
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



        /// <summary>
        /// Ob koncu urejanja celice v podatkovni mreži parametrov, preveri in posodobi vrednosti celic.
        /// </summary>
        /// <param name="sender">Objekt, ki sproži dogodek.</param>
        /// <param name="e">Podatki o urejanju celice v podatkovni mreži.</param>
        /// <remarks>
        /// Metoda preveri, ali urejena celica vsebuje pravilno vrednost. Če je urejena celica številčnega tipa,
        /// zamenja vejico s piko kot decimalni separator. Nato preveri, ali je nova vrednost število med 0 in 9.
        /// Če ni, prikaže opozorilo in prekine urejanje celice. V nasprotnem primeru zaokroži novo vrednost na dve decimalki
        /// in posodobi vrednost v povezanem DataRowView objektu.
        /// </remarks>

        private void GridParameters_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column is DataGridTextColumn textColumn && textColumn.Binding is Binding binding && binding.Path.Path == ".")
            {
                TextBox textBox = e.EditingElement as TextBox;
                if (textBox != null)
                {
                    string newText = textBox.Text.Replace(',', '.');

                    if (!double.TryParse(newText, NumberStyles.Float, CultureInfo.InvariantCulture, out double newValue) || newValue <= 0 || newValue > 9)
                    {
                        MessageBox.Show("Napačen vnos! Vnesite število večje od 0 in manjše ali enako 9.");
                        e.Cancel = true;
                        return;
                    }

                    newValue = Math.Round(newValue, 2);

                    var rowView = e.Row.Item as DataRowView;
                    if (rowView != null)
                    {
                        rowView[binding.Path.Path] = newValue;
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            var rootCollection = new ObservableCollection<Parameter> { rootParameter };
            alternative = new ObservableCollection<Alternativa>();

            DataContext = rootCollection;
            alternativeListBox.ItemsSource = alternative;
        }

        /// <summary>
        /// Preveri, ali ime parametra že obstaja znotraj njega in njegovih otrok.
        /// </summary>
        /// <param name="parameterName">Ime parametra, ki ga želimo preveriti glede na podvajanje.</param>
        /// <param name="parameter">Korenski objekt Parameter, od katerega želimo začeti iskanje.</param>
        /// <returns>True, če je ime parametra podvojeno, sicer false.</returns>
        private bool IsParameterNameDuplicate(string parameterName, Parameter parameter)
        {
            if (parameter.Name.ToLower() == parameterName.ToLower())
            {
                return true;
            }

            foreach (Parameter child in parameter.Children)
            {
                if (IsParameterNameDuplicate(parameterName, child))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Dodaja parameter v hierarhijo.
        /// </summary>
        /// <param name="sender">Objekt, ki sproži dogodek.</param>
        /// <param name="e">Podatki o dogodku.</param>
        /// <remarks>
        /// Metoda omogoča dodajanje novega parametra v hierarhijo.
        /// Preveri, ali je vnešeno ime novega parametra prazno ali nično.
        /// Če je ime veljavno, ustvari nov Parameter objekt z vnešenim imenom in ga doda kot otroka izbranemu parametru v drevesni strukturi (če je izbran parameter) ali kot otroka korenskega parametra (če ni izbran noben parameter).
        /// V primeru, da parameter z enakim imenom že obstaja, se prikaže ustrezno sporočilo o napaki.
        /// Po dodajanju se tekstovno polje za ime parametra izprazni.
        /// </remarks>
        private void dodajParameter_Click(object sender, RoutedEventArgs e)    //Dodajanje parametrov v hierarhijo
        {       
            string newParameterName = newParameterTextBox.Text;
            if (!String.IsNullOrEmpty(newParameterName))
            {
                if (IsParameterNameDuplicate(newParameterName, rootParameter))
                {
                    MessageBox.Show($"Parameter z imenom '{newParameterName}' že obstaja!");
                    return;
                }

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

        /// <summary>
        /// Odstrani parameter iz hierarhije.
        /// </summary>
        /// <param name="sender">Objekt, ki sproži dogodek.</param>
        /// <param name="e">Podatki o dogodku.</param>
        /// <remarks>
        /// Metoda omogoča odstranjevanje izbranega parametra iz hierarhije.
        /// Preveri, ali je izbran parameter veljaven (ni null) in ni korenski parameter.
        /// Če je izbran parameter veljaven, ga odstrani iz seznama otrok njegovega starša ali iz seznama otrok korenskega parametra (če nima starša).
        /// Prav tako posodobi starševstvo otrok odstranjenega parametra tako, da jih nastavi na null.
        /// Poleg tega odstrani izbrani parameter iz konteksta podatkov.
        /// </remarks>
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


        /// <summary>
        /// Obdela dogodek  za gumb "DodajAlternativo". Dodaja novo Alternativo v alternativeCollection.
        /// </summary>
        /// <param name="sender">Objekt, ki je sprožil dogodek.</param>
        /// <param name="e">Podatki o dogodku.</param>
        private void DodajAlternativo_Click(object sender, RoutedEventArgs e)
        {
            string newAlternativaName = newAlternativaBox.Text;
            if (!String.IsNullOrEmpty(newAlternativaName))
            {
                if (alternative.Any(alternativa => alternativa.Name == newAlternativaName))
                {
                    MessageBox.Show("Alternativa z tem imenom že obstaja!");
                    return;
                }

                Alternativa alternativa = new Alternativa(newAlternativaName);
                alternative.Add(alternativa);
                newAlternativaBox.Text = "";
            }
        }

        /// <summary>
        /// Obdela dogodek  za gumb "OdstraniAlternativo". Odstrani izbrano Alternativo iz ListBox-a.
        /// </summary>
        /// <param name="sender">Objekt, ki je sprožil dogodek.</param>
        /// <param name="e">Podatki o dogodku.</param>

        private void OdstraniAlternativo_Click(object sender, RoutedEventArgs e)
        {
            if (alternativeListBox.SelectedItem != null)
            {
                Alternativa selectedAlternativa = alternativeListBox.SelectedItem as Alternativa;

                alternative.Remove(selectedAlternativa);
            }
        }

        /// <summary>
        /// Pridobi seznam parametrov iz hierarhije.
        /// </summary>
        /// <param name="rootCollection">Korenja kolekcija parametrov.</param>
        /// <returns>Seznam parametrov.</returns>
        /// <remarks>
        /// Metoda rekurzivno pregleda hierarhijo parametrov, začenši z dano korenjo kolekcije.
        /// Vsak parameter v korenski kolekciji se doda na seznam parametrov, nato pa se rekurzivno preverijo in dodajo tudi vsi otroci parametrov.
        /// Na koncu se vrne seznam vseh parametrov.
        /// </remarks>
        private List<Parameter> GetParameterList(ObservableCollection<Parameter> rootCollection)  //Shranjevanje parametrov v List
        {
            var parameterList = new List<Parameter>();

            foreach (var parameter in rootCollection)
            {
                parameterList.Add(parameter);
                parameterList.AddRange(GetChildParameters(parameter));
            }

            return parameterList;
        }

        /// <summary>
        /// Pridobivanje otrok
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns> List otrok </returns>
        /// <remarks>
        /// Metoda rekurzivno preverja vse otroke podanega parametra in jih dodaja v seznam.
        /// Za vsakega otroka izvede rekurzivni klic, da pridobi vse njegove otroke in jih doda v seznam.
        /// Na koncu vrne seznam vseh otrok parametrov.
        /// </remarks>
        private List<Parameter> GetChildParameters(Parameter parameter)
        {
            var childList = new List<Parameter>();

            foreach (var child in parameter.Children)
            {
                childList.Add(child);
                childList.AddRange(GetChildParameters(child));
            }

            return childList;
        }

        /// <summary>
        /// Navigacija na naslednji tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Metoda pridobi seznam parametrov iz DataContexta in ga shrani v list parametrov.
        /// Nato uredi starše parametrov in naloži tabelo parametrov.
        /// Na koncu spremeni indeks izbranega zavihka na 1, kar prikaže naslednji zaslon z parametri.
        /// </remarks>

        private void naprejNaParametre_Click(object sender, RoutedEventArgs e)
        {
            var parameterList = GetParameterList((ObservableCollection<Parameter>)DataContext);
            parametri = parameterList;
            SortParents();
            NaloziTabelo();
            myTabControl.SelectedIndex = 1;
        }

        /// <summary>
        /// Funkcija iterira skozi hierarhijo parametrov in iz nje poišče starše in jih vstavi v nov list
        /// </summary>
        private void SortParents()
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


        /// <summary>
        /// Naloži tabelo parametrov.
        /// </summary>
        /// <remarks>
        /// Metoda naloži tabelo parametrov v data grid.
        /// Najprej preveri, ali so bili dodani starši parametrov.
        /// Če ni dodanih staršev, se prikaže sporočilo in preklopi na prvi zavihek.
        /// Nato se izbere trenutni staršni parameter in pridobi njegove otroke.
        /// Ustvari se nova podatkovna tabela in dodajo se stolpci za staršnega parametra in otroke.
        /// Vrstice tabele se napolnijo z vrednostmi, kjer so po diagonali vrednosti 1 ostale pa 0.
        /// Končna tabela se poveže z vmesnikom podatkovne mreže in se prikaže v mreži.
        /// </remarks>
        private void NaloziTabelo()
        {
            dataGridParameters.CellEditEnding += GridParameters_CellEditEnding;

            if (parents.Count == 0)
            {
                MessageBox.Show("Najprej morate dodati parametre!");
                myTabControl.SelectedIndex = 0;
            }

            Parameter parent = parents[currentIndex];
            ObservableCollection<Parameter> children = parent.Children;

            tableParameters = new DataTable();

            if (currentIndex < parents.Count)
            {
                dataGridParameters.Columns.Clear();
                dataGridParameters.CanUserAddRows = false;
                dataGridParameters.CanUserResizeColumns = false;
                dataGridParameters.AutoGenerateColumns = true;

                tableParameters.Columns.Add(parent.Name);

                foreach (Parameter child in children)
                {
                    tableParameters.Columns.Add(child.Name, typeof(double));
                }

                for (int i = 0; i < children.Count; i++)
                {
                    DataRow row = tableParameters.NewRow();
                    row[0] = children[i].Name;
                    for (int j = 1; j <= children.Count; j++)
                    {
                        if (i == j - 1)
                        {
                            row[j] = 1;
                        }
                        else
                        {
                            row[j] = 0;
                        }
                    }
                    tableParameters.Rows.Add(row);
                }
                dataGridParameters.ItemsSource = tableParameters.DefaultView;
                currentIndex++;
            }
        }

        /// <summary>
        /// Shrani podatke iz tabele.
        /// </summary>
        /// <remarks>
        /// Metoda shrani vse podatke iz tabele parametrov v seznam.
        /// Vsaka vrstica tabele se pretvori v seznam števil, pri čemer se preskoči prvi stolpec kateri je namenjen kot row header.
        /// Pretvorjene podatke se dodajo v seznam savedTable.
        /// </remarks>
        private void SaveTable()
        {
            savedTable = new List<List<double>>();
            foreach (DataRow row in tableParameters.Rows)
            {
                List<double> rowData = new List<double>();

                for (int i = 1; i < row.ItemArray.Length; i++)
                {
                    rowData.Add(Convert.ToDouble(row[i]));
                }

                savedTable.Add(rowData);
            }
        }

        /// <summary>
        /// Izračunaj normalizirano tabelo.
        /// </summary>
        /// <returns>Normalizirana tabela vrednosti.</returns>
        /// <remarks>
        /// Metoda izračuna normalizirano tabelo iz shranjene tabele vrednosti.
        /// Najprej pridobi število vrstic in stolpcev v shranjeni tabeli.
        /// Nato izračuna vsoto vrednosti za vsak stolpec.
        /// Za vsako vrednost v tabeli izračuna normalizirano vrednost, ki je razmerje med vrednostjo in vsoto vrednosti stolpca.
        /// Normalizirane vrednosti zaokroži na tri decimalna mesta.
        /// Rezultat je seznam seznamov z normaliziranimi vrednostmi za vsak stolpec.
        /// </remarks>
        private List<List<double>> CalculateNormalizedTable()
        {
            int rowCount = savedTable.Count;
            int columnCount = savedTable[0].Count;

            List<List<double>> normalizedTable = new List<List<double>>();

            for (int j = 0; j < columnCount; j++)
            {
                double sum = 0.0;
                for (int i = 0; i < rowCount; i++)
                {
                    sum += savedTable[i][j];
                }

                List<double> normalizedColumn = new List<double>();
                for (int i = 0; i < rowCount; i++)
                {
                    double normalizedValue = savedTable[i][j] / sum;
                    normalizedValue = Math.Round(normalizedValue, 3);
                    normalizedColumn.Add(normalizedValue);
                }

                normalizedTable.Add(normalizedColumn);
            }
            return normalizedTable;
        }

        /// <summary>
        /// Prikaz teže v tabeli.
        /// </summary>
        /// <param name="tableRows">Seznam seznamov z normaliziranimi vrednostmi.</param>
        /// <remarks>
        /// Metoda prikaže težo parametrov v tabeli.
        /// Najprej preveri, ali obstaja naslednji parameter za prikaz teže.
        /// Nato pridobi starša in otroke za določen parameter.
        /// Ustvari novo tabelo za teže.
        /// Počisti stolpce v datagridu.
        /// Nastavi možnost dodajanja vrstic in spreminjanja velikosti stolpcev na false.
        /// Samodejno generira stolpce glede na število otrok.
        /// Dodaja stolpce v tabelo tež.
        /// Napolni vrstice tabele tež z vrednostmi iz seznama normaliziranih vrednosti.
        /// Izračuna in doda stolpec za uteži.
        /// Za vsako vrstico izračuna vsoto vrednosti za vse stolpce razen zadnjega.
        /// Izračuna utež za vrstico kot razmerje med vsoto in številom stolpcev.
        /// Zaokroži utež na tri decimalna mesta.
        /// Posodobi težo otroka s pridobljeno utežjo.
        /// Nastavi vir podatkov za datagrid na tabelo tež.
        /// Poveča indeks za prikaz naslednjega parametra.
        /// </remarks>
        private void DisplayWeightTable(List<List<double>> tableRows)
        {
            if (currentWeightIndex < parents.Count)
            {
                Parameter parent = parents[currentWeightIndex];
                ObservableCollection<Parameter> children = parent.Children;
                tableWeights = new DataTable();

                dataGridWeights.Columns.Clear();

                dataGridWeights.CanUserAddRows = false;
                dataGridWeights.CanUserResizeColumns = false;
                dataGridWeights.AutoGenerateColumns = true;

                tableWeights.Columns.Add(parent.Name);

                foreach (Parameter child in children)
                {
                    tableWeights.Columns.Add(child.Name, typeof(double));
                }

                for (int i = 0; i < children.Count; i++)
                {
                    DataRow row = tableWeights.NewRow();
                    row[0] = children[i].Name;

                    for (int j = 0; j < children.Count; j++)
                    {
                        row[j + 1] = tableRows[j][i];
                    }

                    tableWeights.Rows.Add(row);
                }

                DataColumn uteziColumn = tableWeights.Columns.Add("Uteži", typeof(double));

                for (int rowIndex = 0; rowIndex < tableWeights.Rows.Count; rowIndex++)
                {
                    DataRow row = tableWeights.Rows[rowIndex];
                    double sum = 0.0;
                    for (int columnIndex = 1; columnIndex < tableWeights.Columns.Count - 1; columnIndex++)
                    {
                        sum += Convert.ToDouble(row[columnIndex]);
                    }
                    double weight = sum / (tableWeights.Columns.Count - 2);

                    row[uteziColumn] = Math.Round(weight, 3);
                    Parameter child = children[rowIndex];
                    child.Weight = weight;
                }
                dataGridWeights.ItemsSource = tableWeights.DefaultView;
                currentWeightIndex++;
            }
        }

        /// <summary>
        /// Izračun teže parametrov.
        /// </summary>
        /// <param name="sender">Objekt, ki sproži dogodek.</param>
        /// <param name="e">Argumenti dogodka.</param>
        /// <remarks>
        /// Metoda izračuna težo parametrov.
        /// Najprej shrani trenutno tabelo.
        /// Izračuna normalizirano tabelo.
        /// Nato prikaže tabelo z utežmi parametrov.
        /// </remarks>
        private void Izracunaj_Click(object sender, RoutedEventArgs e)
        {
            SaveTable();
            List<List<double>> tableRows = CalculateNormalizedTable();
            DisplayWeightTable(tableRows);
        }


        /// <summary>
        /// Premik na naslednjo matriko parametrov.
        /// </summary>
        /// <param name="sender">Objekt, ki sproži dogodek.</param>
        /// <param name="e">Argumenti dogodka.</param>
        /// <remarks>
        /// Metoda omogoča premik na naslednjo matriko parametrov.
        /// Če še niso prikazane vse matrike parametrov, počisti trenutne matrike, tabelo parametrov in shranjeno tabelo ter naloži naslednjo matriko parametrov.
        /// Če so prikazane že vse matrike parametrov, prikaže opozorilno sporočilo ob koncu primerjave parametrov po parih.
        /// </remarks>
        private void nextGrid_Click(object sender, RoutedEventArgs e)
        {
            if (currentIndex < parents.Count && currentWeightIndex < parents.Count)
            {
                dataGridParameters.Columns.Clear();
                dataGridParameters.ItemsSource = null;
                dataGridWeights.Columns.Clear();
                dataGridWeights.ItemsSource = null;

                tableParameters.Clear();
                tableParameters.Columns.Clear();
                tableParameters.Rows.Clear();
                tableParameters.Reset();

                savedTable.Clear();
                tableWeights.Clear();
                tableWeights.Reset();
                tableWeights.Columns.Clear();
                tableWeights.Rows.Clear();

                NaloziTabelo();
            }
            else if (currentIndex == parents.Count)
            {
                naprejAlternative.Visibility = Visibility.Visible;
                MessageBox.Show("Konec primerjave parametrov po parih.");
                return;
            }
        }

        private void naprejAlternative_Click(object sender, RoutedEventArgs e)
        {
            parametriBrezRoot = parametri.GetRange(1, parametri.Count - 1);
            NaloziTabeloAlternativ();
            myTabControl.SelectedIndex = 2;     
        }

        private void NaloziTabeloAlternativ()
        {
            if(alternative.Count == 0)
            {
                MessageBox.Show("Najprej morate vnesti alternative!");
                myTabControl.SelectedIndex = 0;
            }

            Parameter parameter = parametriBrezRoot[currentIndexAlternative];   //tu mors samo vse liste dat??? preveri

            
            tableAlternative = new DataTable();

            if(currentIndexAlternative < parametriBrezRoot.Count)
            {
                dataGridAlternative.Columns.Clear();
                dataGridAlternative.CanUserAddRows = false;
                dataGridAlternative.CanUserResizeColumns = false;
                dataGridAlternative.AutoGenerateColumns = true;

                tableAlternative.Columns.Add(parameter.Name);

                foreach (Alternativa alternative in alternative)
                {
                    tableAlternative.Columns.Add(alternative.Name, typeof(double));
                }

                for (int i = 0; i < alternative.Count; i++)
                {
                    DataRow row = tableAlternative.NewRow();
                    row[0] = alternative[i].Name;

                    for (int j = 0; j < alternative.Count; j++)
                    {
                        if (i == j)
                        {
                            row[j + 1] = 1; 
                        }
                        else
                        {
                            row[j + 1] = 0;
                        }
                    }

                    tableAlternative.Rows.Add(row);
                }
                dataGridAlternative.ItemsSource = tableAlternative.DefaultView;
                currentIndexAlternative++;
            } 
        }
        private void izracunajAlternative_Click(object sender, RoutedEventArgs e)
        {

        }

        private void naprejIzracun_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
