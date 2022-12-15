using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Seguimiento_de_Activos
{
    /// <summary>
    /// Ventana para modificar Alias y nodo/s asociado/s a un sector
    /// La respuesta se obtiene a partir de la propiedad Answer
    /// Recibe: bool CanAddSensors indicando si se pueden añadir sensores (nodos)
    ///         string Alias, alias inicial del sector
    ///         string[] EUIs, EUIs de los sensores iniciales
    /// </summary>
    public partial class ModificarSector : Window
    {
        public List<EUI> listaEUI = new List<EUI>();
        public List<string> rta = new List<string>();
        const int MAX_SENSORES = 5;

        public ModificarSector(bool CanAddSensors, string alias, string[] EUIs)
        {
            InitializeComponent();

            foreach (string s in EUIs)
                listaEUI.Add(new EUI(s));
            lista.ItemsSource = listaEUI;
            Alias.Text = alias;

            if (!CanAddSensors || listaEUI.Count >= MAX_SENSORES)
                Add_Button.Visibility = Visibility.Collapsed;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            short numEUIs=0;

            foreach (EUI eui in listaEUI)
            {
                if (!string.IsNullOrEmpty(eui.Value))
                { 
                    if (!(eui.Value.Length == 16))
                    {
                        MessageBox.Show("El campo \"EUI\" debe contener exactamente 16 caracteres");
                        return;
                    }
                    numEUIs++;
                }
            }
            if (numEUIs == 0)
            {
                MessageBox.Show("Debe haber al menos un nodo o sensor");
                return;
            }
            if (string.IsNullOrEmpty(Alias.Text))
            {
                MessageBox.Show("El campo \"Alias\" no puede estar vacío");
                return;
            }

            rta.Add(Alias.Text);
            for(int i = 0; i < listaEUI.Count; i++)
                if(!string.IsNullOrEmpty(listaEUI[i].Value))
                    rta.Add(listaEUI[i].Value.ToUpper());

            this.DialogResult = true;
        }

        public string[] Answer
        {
            get
            {
                return rta.ToArray();
            }
        }

        private void Add_click(object sender, RoutedEventArgs e)
        {
            lista.ItemsSource = null;
            listaEUI.Add(new EUI(""));
            lista.ItemsSource = listaEUI;
            if (listaEUI.Count == 5)
                Add_Button.Visibility = Visibility.Collapsed;
        }

        public class EUI
        {
            public EUI(string eui)
            {
                Value = eui;
            }
            public string Value { get; set; }

            public override string ToString()
            {
                return Value;
            }
        }
    }
}
