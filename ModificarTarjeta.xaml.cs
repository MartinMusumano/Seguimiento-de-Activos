using System;
using System.Windows;
using System.Windows.Media;

namespace Seguimiento_de_Activos
{
    /// <summary>
    /// Modifica el alias/color de la tarjeta seleccionada, directamente en la lista de origen
    /// </summary>
    public partial class ModificarTarjeta : Window
    {
        Tarjeta tarjetaMod;

        public ModificarTarjeta(Tarjeta tarjetaSel)
        {
            InitializeComponent();
            tarjetaMod = tarjetaSel;    //Por referencia (tarjetaMod modifica tambien la Tarjeta en la lista original)
            Alias.Text = tarjetaSel.alias;
            cbox.SelectedIndex = (int)tarjetaSel.tipo;
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Alias.Text))
            {
                MessageBox.Show("El alias no puede estar vacío");
                return;
            }
            tarjetaMod.alias = Alias.Text;
            tarjetaMod.tipo = (Sustancia.Tipo)cbox.SelectedIndex;
            tarjetaMod.color = Sustancia.ToColor(tarjetaMod.tipo);
            this.DialogResult = true;
        }
    }
}
