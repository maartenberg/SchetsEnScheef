using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace SchetsEditor
{
    public class Schets
    {
        private Bitmap bitmap;
        private Size grootte;
        public List<PuntVorm> Vormen;
        public static List<PuntVorm> RedoStack = new List<PuntVorm>();
        
        public Schets()
        {
            bitmap = new Bitmap(1, 1);
            Vormen = new List<PuntVorm>();
            RechthoekVorm achtergrond = new RechthoekVorm(Brushes.White, new Point(0, 0), new Point(1, 1));
            Vormen.Add(achtergrond);
        }
        public Graphics BitmapGraphics
        {
            get { return Graphics.FromImage(bitmap); }
        }
        public void VeranderAfmeting(Size sz)
        {
            this.grootte = sz;
            RechthoekVorm achtergrond = (RechthoekVorm)Vormen[0];
            achtergrond.Eindpunt = new Point(sz.Width, sz.Height);
            achtergrond.HerberekenRechthoek();
        }
        public void Teken(Graphics gr)
        {
            //gr.DrawImage(bitmap, 0, 0);
            foreach (PuntVorm vorm in Vormen)
            {
                vorm.Teken(gr);
            }
        }
        public void Schoon()
        {
            //Graphics gr = Graphics.FromImage(bitmap);
            //gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
            Vormen = new List<PuntVorm>();
            var bg = new RechthoekVorm(Brushes.White, new Point(0,0), new Point(grootte.Width, grootte.Height)); // kan beter?
            Vormen.Add(bg);
        }
        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        public void Undo()
        {
            int nieuwste = Vormen.Count - 1;
           // RedoStack.Add(nieuwste);
            if (nieuwste > 0)
            {
                RedoStack.Add(Vormen[nieuwste]);
                Vormen.Remove(Vormen[nieuwste]);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("There is no action to undo.", "Error");
            }
        }

        public void Redo()
        {
            int nieuwste = RedoStack.Count - 1;
            if (nieuwste >= 0)
            {
                Vormen.Add(RedoStack[nieuwste]);
                RedoStack.Remove(RedoStack[nieuwste]);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("There is no action to redo.", "Error");
            }
        }
        // opdracht 2: opslaan en openen
        public void NaarBestand(string bestandsnaam, int formaat = 1)
        {
            ImageFormat opslagformaat;
            switch (formaat)
            {
                case 1:
                    opslagformaat = ImageFormat.Png;
                    break;
                case 2:
                    opslagformaat = ImageFormat.Jpeg;
                    break;
                case 3:
                    opslagformaat = ImageFormat.Bmp;
                    break;
                default:
                    opslagformaat = ImageFormat.Png;
                    break;
            }
            bitmap.Save(bestandsnaam, opslagformaat);
        }

        public void VanBestand(string bestandsnaam)
        {
            Bitmap geladen = new Bitmap(bestandsnaam);
            bitmap = geladen;
        }
    }
}
