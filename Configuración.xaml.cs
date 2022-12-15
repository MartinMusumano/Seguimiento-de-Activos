using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Seguimiento_de_Activos
{
    /// <summary>
    /// Permite modificar campos static MAX_MEDICIONES y MAX_INTERVALO de
    /// la clase Visible Payload.
    /// </summary>
    public partial class Configuración : Window
    {
        public Configuración()
        {
            InitializeComponent();
            cboxBuffer.SelectedIndex = VisiblePayload.MAX_MEDICIONES-1;
            cboxIntervalo.SelectedIndex = IndexWithTimeSpan(VisiblePayload.MAX_INTERVALO);
        }

        private void btnDialogOk_Click(object sender, RoutedEventArgs e)
        {
            int sel = (cboxBuffer.SelectedIndex + 1);
            VisiblePayload.MAX_MEDICIONES = sel;

            sel = cboxIntervalo.SelectedIndex;
            VisiblePayload.MAX_INTERVALO = TimeSpanWithIndex(sel);

            this.DialogResult = true;
        }

        private void quest_buffer_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Indica la máxima cantidad de mediciones que\n" +
                            "podrán almacenarse para estimar la distancia.\n\n" +
                            "Se calcula un promedio a partir de los valores\n" +
                            "almacenados en el buffer. Cuando éste se\n" +
                            "llene, se descartará la medición más antigua.\n");
        }

        private void quest_Interval_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Indica el máximo intervalo que puede transcurrir\n" +
                            "entre dos mediciones consecutivas. Siempre que\n" +
                            "no se exceda, se promediarán las mediciones. De lo\n" +
                            "contrario, se descartarán la/s medicion/es antiguas.");
        }

        private int IndexWithTimeSpan(TimeSpan ts)
        {
            if (ts == TimeSpan.FromSeconds(20))
                return 0;
            else if(ts == TimeSpan.FromSeconds(30))
                return 1;
            else if (ts == TimeSpan.FromSeconds(45))
                return 2;
            else if (ts == TimeSpan.FromSeconds(60))
                return 3;
            else if (ts == TimeSpan.FromSeconds(90))
                return 4;
            else
                return 5;
        }

        private TimeSpan TimeSpanWithIndex(int index)
        {
            switch (index)
            {
                case 0: return TimeSpan.FromSeconds(20);
                case 1: return TimeSpan.FromSeconds(30);
                case 2: return TimeSpan.FromSeconds(45);
                case 3: return TimeSpan.FromSeconds(60);
                case 4: return TimeSpan.FromSeconds(90);
                case 5: return TimeSpan.FromSeconds(120);
                default: return TimeSpan.FromSeconds(45);
            }
        }
    }
}
