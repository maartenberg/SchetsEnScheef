using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SchetsEditor
{
    public interface ISchetsTool
    {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p);
        void Letter(SchetsControl s, char c);
    }

    public abstract class StartpuntTool : ISchetsTool
    {
        protected Point startpunt;
        protected Brush kwast;

        public virtual void MuisVast(SchetsControl s, Point p)
        {   startpunt = p;
            Schets.RedoStack.Clear();
        }
        public virtual void MuisLos(SchetsControl s, Point p)
        {   
        }
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);
    }

    public class TekstTool : StartpuntTool
    {
        protected int verzamelingNummer = 1;
        public override string ToString() { return "tekst"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void MuisVast(SchetsControl s, Point p)
        {
            verzamelingNummer++;
            kwast = new SolidBrush(s.PenKleur);
            base.MuisVast(s, p);
            kwast = new SolidBrush(s.PenKleur);
        }

        public override void Letter(SchetsControl s, char c)
        {
            if (c > 32)
            {
                Graphics gr = s.MaakBitmapGraphics();
                var letter = new TekstVorm(c, kwast, startpunt);
                letter.VerzamelingNummer = verzamelingNummer;
                s.Schets.Vormen.Add(letter);
                letter.Teken(gr);   // Letter moet gemeten worden voor we weten hoever we op moeten schuiven
                startpunt.X += (int)letter.sz.Width;
                s.Invalidate();
            }
            if (c == 32) //spatie, nieuw woord
            {
                verzamelingNummer++;
                startpunt.X += 20;
        }
    }
    }

    public abstract class TweepuntTool : StartpuntTool
    {
        protected Pen previewPen;
        protected int penDikte;
        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {   return new Rectangle( new Point(Math.Min(p1.X,p2.X), Math.Min(p1.Y,p2.Y))
                                , new Size (Math.Abs(p1.X-p2.X), Math.Abs(p1.Y-p2.Y))
                                );
        }
        public static Pen MaakPen(Brush b, int dikte)
        {   Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }
        public override void MuisVast(SchetsControl s, Point p)
        {   base.MuisVast(s, p);
            previewPen = new Pen(s.PenKleur, 1);
            previewPen.DashStyle = DashStyle.Dash;
            penDikte = s.PenDikte;
            kwast = new SolidBrush(s.PenKleur);
        }
        public override void MuisDrag(SchetsControl s, Point p)
        {   s.Refresh();
            this.Bezig(s.CreateGraphics(), this.startpunt, p);
        }
        public override void MuisLos(SchetsControl s, Point p)
        {   base.MuisLos(s, p);
            this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p);
            s.Refresh();
        }
        public override void Letter(SchetsControl s, char c)
        {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2);
        
        public virtual void Compleet(Graphics g, Point p1, Point p2)
        {   this.Bezig(g, p1, p2);
        }
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {   g.DrawRectangle(previewPen, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
        public override void MuisLos(SchetsControl s, Point p)
        {
            KaderVorm vorm = new KaderVorm((SolidBrush)kwast, startpunt, p, s.PenDikte);
            s.Schets.Vormen.Add(vorm);
            s.Refresh();
        }
    }
    
    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void MuisLos(SchetsControl s, Point p)
        {
            RechthoekVorm vorm = new RechthoekVorm(kwast, startpunt, p, s.PenDikte);
            s.Schets.Vormen.Add(vorm);
            s.Invalidate();
        }
    }

    public class LijnTool : TweepuntTool
    {
        protected int Verzamelingnummer = 0;
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawLine(previewPen, p1, p2);
        }

        public override void MuisLos(SchetsControl s, Point p)
        {
            LijnVorm lijn = new LijnVorm(kwast, startpunt, p, s.PenDikte, Verzamelingnummer);
            s.Schets.Vormen.Add(lijn);
            lijn.Teken(s.CreateGraphics());
            s.Invalidate();
        }
    }

    public class PenTool : LijnTool
    {
        public PenTool()
        {
            Verzamelingnummer = 1;
        }
        public override string ToString() { return "pen"; }

        public override void MuisLos(SchetsControl s, Point p)
        {
            Verzamelingnummer++;
            base.MuisLos(s, p);
        }
        public override void MuisDrag(SchetsControl s, Point p)
        {   base.MuisLos(s, p);
            this.MuisVast(s, p);
        } 
    }
    public class RegenboogPenTool : PenTool
    {
        private int doorteller = 0;
        private static List<Color> kleuren = new List<Color>(
            new Color[8] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Cyan, Color.Blue, Color.Purple, Color.Violet }
            );

        public override string ToString() { return "regenboog"; }

        public override void MuisDrag(SchetsControl s, Point p)
        {
            this.kwast = new SolidBrush(kleuren[doorteller]);
            base.MuisDrag(s, p);
            doorteller = (doorteller + 1) % 8;
        }
    }
    
    public class EmmerTool : StartpuntTool
    {
        public override string ToString() { return "verf"; }
        public override void Letter(SchetsControl s, char c) { }
        public override void MuisVast(SchetsControl s, Point p)
        {
            for (int i = s.Schets.Vormen.Count - 1; i > 0; i--)     // foreach werkt hier niet omdat we de dan de onderste krijgen die reageert: de achtergrond
            {
                PuntVorm vorm = s.Schets.Vormen[i];
                if (vorm.Geklikt(p))
                {
                    vorm.Kwast = new SolidBrush(s.PenKleur);
                    vorm.TekenPen = new Pen(s.PenKleur, vorm.Dikte);

                    if (vorm.VerzamelingNummer != 0)
                    {
                        var verzameling = s.Schets.Vormen.FindAll(puvorm => puvorm.VerzamelingNummer == vorm.VerzamelingNummer && puvorm.GetType() == vorm.GetType());
                        foreach (var gelinktevorm in verzameling)
                        {
                            gelinktevorm.Kwast = new SolidBrush(s.PenKleur);
                            gelinktevorm.TekenPen = new Pen(s.PenKleur, gelinktevorm.Dikte);
                        }
                    }
                    s.Invalidate();
                    break;
                }
            }
        
            
        }

        public override void MuisDrag(SchetsControl s, Point p)
        {
            return;
        }
    }
    public class GumTool : StartpuntTool
    {
        public override string ToString() { return "gum"; }

        public override void MuisLos(SchetsControl s, Point p)
        {
            for (int i = s.Schets.Vormen.Count - 1; i > 0; i--) // 0 is de achtergrond!
            {
                PuntVorm vorm = s.Schets.Vormen[i];
                if (vorm.Geklikt(p)) {
                    s.Schets.Vormen.Remove(vorm);
                    if (vorm.VerzamelingNummer != 0)
                    {
                        var verzameling = s.Schets.Vormen.FindAll(puvorm => puvorm.VerzamelingNummer == vorm.VerzamelingNummer && puvorm.GetType() == vorm.GetType());
                        foreach (var gelinktevorm in verzameling) s.Schets.Vormen.Remove(gelinktevorm);
                    }
                    s.Invalidate();
                    break;
                }
            }
        }
        public override void Letter(SchetsControl s, char c) { }
        public override void MuisDrag(SchetsControl s, Point p)
        {
            MuisLos(s, p);
        }
    }

    public class OvaalTool : TweepuntTool
    {
        public override string ToString()
        {
            return "ellips";
        }
        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawEllipse(previewPen, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
        }
        public override void MuisLos(SchetsControl s, Point p)
        {
            kwast = new SolidBrush(s.PenKleur);
            EllipsVorm ellips = new EllipsVorm((SolidBrush)kwast, startpunt, p, s.PenDikte);
            s.Schets.Vormen.Add(ellips);
            s.Invalidate();
        }
    }
    public class VolOvaalTool : OvaalTool
    {
        public override string ToString()
        {
            return "vollips";
        }
        public override void MuisLos(SchetsControl s, Point p)
        {
            kwast = new SolidBrush(s.PenKleur);
            VollipsVorm vollips = new VollipsVorm((SolidBrush)kwast, startpunt, p, s.PenDikte);
            s.Schets.Vormen.Add(vollips);
            s.Invalidate();
        }
    }

    // Vormen hieronder
    public abstract class PuntVorm
    {
        public SolidBrush Kwast;
        public Point Startpunt;
        public int VerzamelingNummer; 
        public Pen TekenPen;
        public int Dikte; // Alleen gebruikt in tweepuntvorm maar nodig voor aanpassingen

        public abstract void Teken(Graphics gr);
        public abstract bool Geklikt(Point klik);
    }
    public class TekstVorm : PuntVorm
    {
        string letter;
        public SizeF sz;
        static Font font = new Font("Tahoma", 40);

        public TekstVorm(char letter, Brush kwast, Point startpunt)
        {
            this.letter = letter.ToString();
            this.Kwast = (SolidBrush)kwast;
            this.Startpunt = startpunt;
        }
        public override void Teken(Graphics gr)
        {
            if (sz.IsEmpty)
            this.sz = gr.MeasureString(letter, font, Startpunt, StringFormat.GenericTypographic);
            gr.DrawString(letter.ToString(), font, Kwast, Startpunt, StringFormat.GenericTypographic);
        }
        public override bool Geklikt(Point klik)
        {
            RectangleF rh = new RectangleF(Startpunt, sz);
            return rh.Contains(klik);
        }
        public override string ToString()
        {
            // Formaat: "letter [letter] [verzamelingnummer][x] [y] [r] [g] [b]
            string[] waarden = new string[8]
            {
                "letter", letter, this.VerzamelingNummer.ToString(),
                this.Startpunt.X.ToString(), this.Startpunt.Y.ToString(),
                Kwast.Color.R.ToString(), Kwast.Color.G.ToString(), Kwast.Color.B.ToString()
            };
            return String.Join(" ", waarden);
        }
        public static PuntVorm VanString(string s)
        {
            // Formaat: [vormtype] [letter] [verzamelingnummer] [x] [y] [r] [g] [b]
            PuntVorm resultaat;
            char[] separators = new char[1] { ' ' };
            string[] parameters = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            char gelezenLetter = char.Parse(parameters[1]);
            int gelezenVerzamelingNummer = int.Parse(parameters[2]);

            // Lees startpunt
            int gelezenX, gelezenY;
            gelezenX = int.Parse(parameters[3]);
            gelezenY = int.Parse(parameters[4]);
            Point gelezenStartpunt = new Point(gelezenX, gelezenY);

            // Lees kleur
            int r, g, b;
            r = int.Parse(parameters[5]);
            g = int.Parse(parameters[6]);
            b = int.Parse(parameters[7]);
            SolidBrush gelezenKwast = new SolidBrush(Color.FromArgb(r, g, b));

            // Maak vorm
            resultaat = new TekstVorm(gelezenLetter, gelezenKwast, gelezenStartpunt);
            resultaat.VerzamelingNummer = gelezenVerzamelingNummer;

            return resultaat;
        }
    }

    public abstract class TweePuntVorm : PuntVorm
    {
        public Point Eindpunt;
        protected Rectangle rect;
        
        protected abstract string vormType { get; }

        public TweePuntVorm(Brush kwast, Point startpunt, Point eindpunt, int dikte)
        {
            this.Kwast = (SolidBrush)kwast;
            this.TekenPen = new Pen(kwast, dikte);
            this.Dikte = dikte;
            this.Startpunt = startpunt;
            this.Eindpunt = eindpunt;
            this.HerberekenRechthoek();
        }
        public virtual void HerberekenRechthoek()
        {
            this.rect = TweepuntTool.Punten2Rechthoek(Startpunt, Eindpunt);
        }
        public static PuntVorm VanString(string s)
        {
            // Formaat: "[vormtype] [verzamelingnummer] [startx] [starty] [eindx] [eindy] [r] [g] [b] [dikte]"
            PuntVorm resultaat;
            char[] separators = { ' ' };
            string[] parameters = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (parameters.Length != 10)
            {
                //  fout!
                MessageBox.Show(s, "Unreadable line!"); 
                return null;
            }
            try {
            // Lees verzamelingnummer
            int geladenVerzamelingNummer = int.Parse(parameters[1]);

            // Lees startpunt, eindpunt
            Point geladenStartpunt, geladenEindpunt;
            int x1, y1, x2, y2;
            x1 = int.Parse(parameters[2]);
            y1 = int.Parse(parameters[3]);
            geladenStartpunt = new Point(x1, y1);
            x2 = int.Parse(parameters[4]);
            y2 = int.Parse(parameters[5]);
            geladenEindpunt = new Point(x2, y2);

            // Lees kleur
            int r, g, b;
            r = int.Parse(parameters[6]);
            g = int.Parse(parameters[7]);
            b = int.Parse(parameters[8]);
            Color kleur = Color.FromArgb(r, g, b);
            SolidBrush kwast = new SolidBrush(kleur);

            int geladenDikte = int.Parse(parameters[9]);

            switch (parameters[0])
            {
                case "Lijn":
                    resultaat = new LijnVorm(kwast, geladenStartpunt, geladenEindpunt, geladenDikte, geladenVerzamelingNummer);
                    break;
                case "Kader":
                    resultaat = new KaderVorm(kwast, geladenStartpunt, geladenEindpunt, geladenDikte);
                    break;
                case "Rechthoek":
                    resultaat = new RechthoekVorm(kwast, geladenStartpunt, geladenEindpunt, geladenDikte);
                    break;
                case "GevuldeEllips":
                    resultaat = new VollipsVorm(kwast, geladenStartpunt, geladenEindpunt, geladenDikte);
                    break;
                case "Ellips":
                    resultaat = new EllipsVorm(kwast, geladenStartpunt, geladenEindpunt, geladenDikte);
                    break;
                default:
                    MessageBox.Show(s, "Unreadable line - non-recognised shape!");
                    resultaat = null;
                    break;
            }
            }
            catch
            {
                MessageBox.Show(s, "Onleesbare regel in bestand!");
                resultaat = null;
            }
            return resultaat;
        }
        public override string ToString()
        {
            // Formaat: "[vormtype] [verzamelingnummer] [startx] [starty] [eindx] [eindy] [r] [g] [b] [dikte]"
            string[] parameters = 
                { vormType, VerzamelingNummer.ToString(),
                    Startpunt.X.ToString(), Startpunt.Y.ToString(),
                    Eindpunt.X.ToString(), Eindpunt.Y.ToString(),
                    Kwast.Color.R.ToString(), Kwast.Color.G.ToString(), Kwast.Color.B.ToString(),
                    Dikte.ToString()
                };
            return String.Join(" ", parameters);
    }
    }

    public class LijnVorm : TweePuntVorm
    {
        protected override string vormType
        {
            get
            {
                return "Lijn";
            }
        }
        static int klikHulp = 5;
        public LijnVorm(Brush kwast, Point startpunt, Point eindpunt, int dikte, int verzamelingnr = 0) : base(kwast, startpunt, eindpunt, dikte)
        {
            this.VerzamelingNummer = verzamelingnr;
        }
        public override void Teken(Graphics gr)
        {
            gr.DrawLine(TekenPen, Startpunt, Eindpunt);
        }
        public override void HerberekenRechthoek()
        {   // Omdat sommige met de pen gemaakte lijnen heel kort kunnen zijn moet de rechthoek iets groter zijn
            Point linksboven, rechtsonder;
            linksboven = new Point(Math.Min(Startpunt.X, Eindpunt.X) - klikHulp, Math.Min(Startpunt.Y, Eindpunt.Y) - klikHulp);
            rechtsonder = new Point(Math.Max(Startpunt.X, Eindpunt.X) + klikHulp, Math.Max(Startpunt.Y, Eindpunt.Y) + klikHulp);
            this.rect = RechthoekTool.Punten2Rechthoek(linksboven, rechtsonder);
        }
        public override bool Geklikt(Point klik)
        {
            return this.BerekenAfstand(klik) < 5 && rect.Contains(klik);    // Tweede is omdat de lijn doorloopt, door de rect worden we beperkt tot de buurt van de lijn.
        }
        public double BerekenAfstand(Point klik)
        {
            double res;
            // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
            res = Math.Abs((Eindpunt.Y - Startpunt.Y) * klik.X - (Eindpunt.X - Startpunt.X) * klik.Y + Eindpunt.X * Startpunt.Y - Eindpunt.Y * Startpunt.X) / 
                    Math.Sqrt(Math.Pow(Eindpunt.Y - Startpunt.Y, 2) + Math.Pow(Eindpunt.X - Startpunt.X, 2));
            return res;
        }
        }

    public class RechthoekVorm : TweePuntVorm
        {
        protected override string vormType
        {
            get
            {
                return "Rechthoek";
        }
    }
        public RechthoekVorm(Brush kwast, Point startpunt, Point eindpunt, int dikte) : base(kwast, startpunt, eindpunt, dikte)
        { }
        public override bool Geklikt(Point klik)
        {
            return rect.Contains(klik);
        }

        public override void Teken(Graphics gr)
        {
            gr.FillRectangle(Kwast, rect);
        }
    }
    public class KaderVorm : RechthoekVorm
    {
        protected override string vormType
        {
            get
            {
                return "Kader";
            }
        }
        public KaderVorm(SolidBrush kwast, Point startpunt, Point eindpunt, int dikte) : base(kwast, startpunt, eindpunt, dikte)
        {
        }
        public override bool Geklikt(Point klik)
        {
            bool xInBereik, yInBereik, xOpRand, yOpRand;
            // Het is 'raak' als een van twee condities waar is:
            //  1. X ligt richtbij een rand en Y ligt tussen de boven- en onderkant -> klik op linker- of rechterrand   (xOpRand && yInBereik)
            //  2. Y ligt dichtbij een rand en X ligt tussen de linker- en rechterkant -> klik op boven- of onderrand   (yOpRand && xInBereik)
            xInBereik = klik.X > rect.Left - 2.5 - Dikte && klik.X < rect.Right + 2.5 + Dikte;
            yInBereik = klik.Y > rect.Top - 2.5 - Dikte && klik.Y < rect.Bottom + 2.5 + Dikte;

            xOpRand = Math.Abs(klik.X - rect.Left) < 5 + 0.2 * Dikte || Math.Abs(klik.X - rect.Right) < 5 + 0.2 * Dikte;
            yOpRand = Math.Abs(klik.Y - rect.Top) < 5 + 0.2 * Dikte|| Math.Abs(klik.Y - rect.Bottom) < 5 + 0.2 * Dikte;

            return (xOpRand && yInBereik) || (yOpRand && xInBereik);
        }
        public override void Teken(Graphics gr)
        {
            gr.DrawRectangle(TekenPen, rect);
        }
    }

    public class EllipsVorm : TweePuntVorm
    {
        protected override string vormType
        {
            get
            {
                return "Ellips";
            }
        }
        protected int halveHoogte, halveBreedte, middelX, middelY; // Gebruikt in berekenen of er in/op de ellips is geklikt
        public EllipsVorm(SolidBrush kwast, Point startpunt, Point eindpunt, int dikte) : base(kwast, startpunt, eindpunt, dikte)
        {
            halveBreedte = rect.Width / 2;      //a
            halveHoogte = rect.Height / 2;      //b

            middelX = rect.Left + halveBreedte; //x0
            middelY = rect.Top + halveHoogte;   //y0
        }

        public override void Teken(Graphics gr)
        {
            gr.DrawEllipse(TekenPen, rect);
        }
        public override bool Geklikt(Point klik)
        {
            // Voor een ellips met middelpunt (midX,midY) en grootte vanaf middelpunt (breedte,hoogte) geldt:
            //      (x - midX / breedte)^2 + (y - midY / hoogte)^2 = 1
            //      Bron: wikipedia
            // Als deze afstand lager dan 1 is is het punt binnen de ellips, marge van 0,05 geeft:
            double afstand = berekenAfstand(klik);
            return afstand > 0.95 && afstand < 1.05;
        }
        protected double berekenAfstand(Point klik)
        {
            double x, y;
            x = klik.X;
            y = klik.Y;
            double helft1 = Math.Pow((x - middelX) / halveBreedte, 2);
            double helft2 = Math.Pow((y - middelY) / halveHoogte, 2);
            double res = helft1 + helft2;
            return res;
        }
    }
    public class VollipsVorm : EllipsVorm
    {
        protected override string vormType
        {
            get
            {
                return "GevuldeEllips";
            }
        }
        public VollipsVorm(SolidBrush kwast, Point startpunt, Point eindpunt, int dikte) : base(kwast, startpunt, eindpunt, dikte)
        { }
        public override void Teken(Graphics gr)
        {
            gr.FillEllipse(Kwast, rect);
        }

        public override bool Geklikt(Point klik)
        {
            double afstand = base.berekenAfstand(klik);
            return afstand < 1.05;
        }
    }
}
