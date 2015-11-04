using System;
using System.IO;
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
            RechthoekVorm achtergrond = new RechthoekVorm(Brushes.White, new Point(0, 0), new Point(1, 1), 1);
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
            foreach (PuntVorm vorm in Vormen)
            {
                vorm.Teken(gr);
            }
        }
        public void Schoon()
        {
            Vormen = new List<PuntVorm>();
            var bg = new RechthoekVorm(Brushes.White, new Point(0,0), new Point(grootte.Width, grootte.Height), 1); 
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
                PuntVorm beginVorm = Vormen[nieuwste];      // Zodat het aantal in Vormen kan veranderen
                RedoStack.Add(beginVorm);
                if (Vormen[nieuwste].VerzamelingNummer != 0)
                {
                    // Er zijn waarschijnlijk gelinkte vormen aanwezig, die moeten er ook uit:
                    // De volgende methode geeft ons een lijst van alle vormen met hetzelfde VerzamelingNummer als beginVorm:
                    List<PuntVorm> gelinkteVormen = Vormen.FindAll(zoekVorm => zoekVorm.VerzamelingNummer == beginVorm.VerzamelingNummer);

                    // Al deze vormen gooien we in de redoStack en daarna uit Vormen:
                    foreach (PuntVorm gelinkteVorm in gelinkteVormen)
                    {
                        RedoStack.Add(gelinkteVorm);
                        Vormen.Remove(gelinkteVorm);
                    }
                }
                Vormen.Remove(beginVorm);
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
                PuntVorm beginVorm = RedoStack[nieuwste];
                Vormen.Add(beginVorm);
                if (beginVorm.VerzamelingNummer != 0)
                {
                    List<PuntVorm> gelinkteVormen = RedoStack.FindAll(zoekVorm => zoekVorm.VerzamelingNummer == beginVorm.VerzamelingNummer);

                    // Al deze vormen gooien we in de redoStack en daarna uit Vormen:
                    foreach (PuntVorm gelinkteVorm in gelinkteVormen)
                    {
                        Vormen.Add(gelinkteVorm);
                        RedoStack.Remove(gelinkteVorm);
                    }
                }
                RedoStack.Remove(beginVorm);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("There is no action to redo.", "Error");
            }
        }
        // opdracht 2: opslaan en openen
        public void NaarBestand(string bestandsnaam, int formaat = 1) // sla bitmap op
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
            Point groottePunt = ((TweePuntVorm)Vormen[0]).Eindpunt;
            Bitmap tijdelijkeAfbeelding = new Bitmap(groottePunt.X, groottePunt.Y);
            this.Teken(Graphics.FromImage(tijdelijkeAfbeelding));
            tijdelijkeAfbeelding.Save(bestandsnaam, opslagformaat);
        }
        public void NaarSchetsBestand(string bestandsnaam)
        {
            StreamWriter bestand = new StreamWriter(bestandsnaam);
            foreach (PuntVorm vorm in Vormen)
            {
                bestand.WriteLine(vorm.ToString());
            }
            bestand.Close();
        }

        public void VanBestand(string bestandsnaam)
        {
            Bitmap geladen = new Bitmap(bestandsnaam);
            bitmap = geladen;
        }
        public void LeesBestand(string bestandsnaam)
        {
            StreamReader bestand = new StreamReader(bestandsnaam);
            string inhoud;
            PuntVorm gelezenVorm = null;
            string[] parameters;
            while ((inhoud = bestand.ReadLine()) != null)
            {
                char[] separators = new char[1] { ' ' };
                parameters = inhoud.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                switch (parameters.Length)
                {
                    case 8:
                        gelezenVorm = TekstVorm.VanString(inhoud);
                        break;
                    case 10:
                        gelezenVorm = TweePuntVorm.VanString(inhoud);
                        break;
                }
                if (gelezenVorm != null)
                    this.Vormen.Add(gelezenVorm);
            }
        }
    }
}
