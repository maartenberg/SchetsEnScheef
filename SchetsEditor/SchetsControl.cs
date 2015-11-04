using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

namespace SchetsEditor
{   public class SchetsControl : UserControl
    {   private Schets schets;
        private ColorDialog kleurenKiezer;
        private Color penkleur;
        private int pendikte = 3;

        public Color PenKleur
        { get { return penkleur; }
        }
        public int PenDikte
        { get { return pendikte; }
        }
        public Schets Schets
        { get { return schets;   }
        }
        public SchetsControl()
        {   this.BorderStyle = BorderStyle.Fixed3D;
            this.DoubleBuffered = true;
            this.schets = new Schets();
            this.kleurenKiezer = new ColorDialog();
            this.Paint += this.teken;
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
        }
        private void teken(object o, PaintEventArgs pea)
        {   schets.Teken(pea.Graphics);
        }
        private void veranderAfmeting(object o, EventArgs ea)
        {   schets.VeranderAfmeting(this.ClientSize);
            this.Invalidate();
        }
        public Graphics MaakBitmapGraphics()
        {   Graphics g = schets.BitmapGraphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            return g;
        }
        public void Schoon(object o, EventArgs ea)
        {   schets.Schoon();
            this.Invalidate();
        }
        public void Roteer(object o, EventArgs ea)
        {   schets.VeranderAfmeting(new Size(this.ClientSize.Height, this.ClientSize.Width));
            schets.Roteer();
            this.Invalidate();
        }

        public void Undo(object o, EventArgs ea)
        {
            schets.Undo();
            this.Invalidate();
        }

        public void Redo(object o, EventArgs ea)
        {
            schets.Redo();
            this.Invalidate();
        }

        public void VeranderKleur(object obj, EventArgs ea)
        {   string kleurNaam = ((ComboBox)obj).Text;
            if (kleurNaam != "Aangepast")
            {
                penkleur = Color.FromName(kleurNaam);
            }
            else
            {
                DialogResult resultaat = kleurenKiezer.ShowDialog();
                if (resultaat == DialogResult.OK)
                    penkleur = kleurenKiezer.Color;
            }
        }
        public void VeranderKleurViaMenu(object obj, EventArgs ea)
        {   string kleurNaam = ((ToolStripMenuItem)obj).Text;
            if (kleurNaam != "Aangepast")
            {
                penkleur = Color.FromName(kleurNaam);
                return;
            }
            else
            {
                DialogResult resultaat = kleurenKiezer.ShowDialog();
                if (resultaat == DialogResult.OK)
                    penkleur = kleurenKiezer.Color;
            }
            
        }
        public void VeranderDikte(object ob, EventArgs ea)
        {
            int nieuweDikte = (int)((NumericUpDown)ob).Value;
            this.pendikte = nieuweDikte;
        }
        public void VanBestand(string bestandsnaam)
        {
            schets.LeesBestand(bestandsnaam);
            Point grootte = ((RechthoekVorm)schets.Vormen[0]).Eindpunt;
            this.Invalidate();
        }
    }
}
