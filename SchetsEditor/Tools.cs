using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

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
        }
        public virtual void MuisLos(SchetsControl s, Point p)
        {   //kwast = new SolidBrush(s.PenKleur);
        }
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);
    }

    public class TekstTool : StartpuntTool
    {
        protected int verzamelingNummer = 1;
        public override string ToString() { return "tekst"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

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
            KaderVorm vorm = new KaderVorm((SolidBrush)kwast, startpunt, p);
            s.Schets.Vormen.Add(vorm);
            s.Refresh();
        }
    }
    
    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void MuisLos(SchetsControl s, Point p)
        {
            RechthoekVorm vorm = new RechthoekVorm(kwast, startpunt, p);
            s.Schets.Vormen.Add(vorm);
            s.Invalidate();
        }
    }

    public class LijnTool : TweepuntTool
    {
        protected bool herteken = false;     // overreden in pen-subklasse, stopt vervelend scherm
        protected int Verzamelingnummer = 0;
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawLine(previewPen, p1, p2);
        }

        public override void MuisLos(SchetsControl s, Point p)
        {
            kwast = new SolidBrush(s.PenKleur);
            LijnVorm lijn = new LijnVorm(kwast, startpunt, p, Verzamelingnummer);
            s.Schets.Vormen.Add(lijn);
            lijn.Teken(s.CreateGraphics());
            if (herteken) s.Invalidate();
        }
    }

    public class PenTool : LijnTool
    {
        public PenTool()
        {
            herteken = false;
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
    
    public class MepTool : StartpuntTool
    {
        public override void Letter(SchetsControl s, char c) { }
        public override void MuisVast(SchetsControl s, Point p)
        {
            for (int i = s.Schets.Vormen.Count - 1; i > 0; i--)     // foreach werkt hier niet omdat we de dan de onderste krijgen die reageert: de achtergrond
            {
                PuntVorm vorm = s.Schets.Vormen[i];
                if (vorm.Geklikt(p))
                {
                    MessageBox.Show(vorm.ToString(), "Au!");
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
                        var verzameling = s.Schets.Vormen.FindAll(puvorm => puvorm.VerzamelingNummer == vorm.VerzamelingNummer);
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
            EllipsVorm ellips = new EllipsVorm((SolidBrush)kwast, startpunt, p);
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
            VollipsVorm vollips = new VollipsVorm((SolidBrush)kwast, startpunt, p);
            s.Schets.Vormen.Add(vollips);
            s.Invalidate();
        }
    }

    // Vormen hieronder
    public abstract class PuntVorm
    {
        public SolidBrush Kwast;
        public Point Startpunt;
        public int VerzamelingNummer; // Momenteel alleen gebruikt voor pen-lijnen

        public abstract void Teken(Graphics gr);
        public abstract bool Geklikt(Point klik);
        public static bool InVerzameling(PuntVorm vorm, int nummer) {
            return vorm.VerzamelingNummer == nummer;
        }

        public abstract void VanString(string s);   // Voor laden

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
            string[] waarden = new string[5]
            {
                "letter", letter, this.Startpunt.X.ToString(), this.Startpunt.Y.ToString(), Kwast.Color.ToString()
            };
            return String.Join(" ", waarden);
        }
        public override void VanString(string s)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class TweePuntVorm : PuntVorm
    {
        public Point Eindpunt;
        protected Rectangle rect;
        protected Pen pen;

        public TweePuntVorm(Brush kwast, Point startpunt, Point eindpunt)
        {
            this.Kwast = (SolidBrush)kwast;
            this.pen = new Pen(kwast, 3);
            this.Startpunt = startpunt;
            this.Eindpunt = eindpunt;
            this.HerberekenRechthoek();
        }
        public virtual void HerberekenRechthoek()
        {
            this.rect = TweepuntTool.Punten2Rechthoek(Startpunt, Eindpunt);
        }
    }

    public class LijnVorm : TweePuntVorm
    {
        static int klikHulp = 5;
        public LijnVorm(Brush kwast, Point startpunt, Point eindpunt, int verzamelingnr = 0) : base(kwast, startpunt, eindpunt)
        {
            this.VerzamelingNummer = verzamelingnr;
        }
        public override void Teken(Graphics gr)
        {
            gr.DrawLine(pen, Startpunt, Eindpunt);
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
        public override void VanString(string s)
        {
            throw new NotImplementedException();
        }
        public override string ToString()
        {
            string[] parameters = new string[6] 
                { "lijn", VerzamelingNummer.ToString(),
                    Startpunt.X.ToString(), Startpunt.Y.ToString(),
                    Eindpunt.X.ToString(), Eindpunt.Y.ToString() };
            return String.Join(" ", parameters);
        }
    }

    public class RechthoekVorm : TweePuntVorm
    {
        public RechthoekVorm(Brush kwast, Point startpunt, Point eindpunt) : base(kwast, startpunt, eindpunt)
        {   // omdat het moet?
        }
        public override bool Geklikt(Point klik)
        {
            return rect.Contains(klik);
        }

        public override void VanString(string s)
        {
            throw new NotImplementedException();
        }
        public override void Teken(Graphics gr)
        {
            gr.FillRectangle(Kwast, rect);
        }
    }
    public class KaderVorm : RechthoekVorm
    {
        public KaderVorm(SolidBrush kwast, Point startpunt, Point eindpunt) : base(kwast, startpunt, eindpunt)
        {
        }
        public override bool Geklikt(Point klik)
        {
            bool xInBereik, yInBereik, xOpRand, yOpRand;
            // Het is 'raak' als een van twee condities waar is:
            //  1. X ligt richtbij een rand en Y ligt tussen de boven- en onderkant -> klik op linker- of rechterrand   (xOpRand && yInBereik)
            //  2. Y ligt dichtbij een rand en X ligt tussen de linker- en rechterkant -> klik op boven- of onderrand   (yOpRand && xInBereik)
            xInBereik = klik.X > rect.Left - 2.5 && klik.X < rect.Right + 2.5;
            yInBereik = klik.Y > rect.Top - 2.5 && klik.Y < rect.Bottom + 2.5;

            xOpRand = Math.Abs(klik.X - rect.Left) < 5 || Math.Abs(klik.X - rect.Right) < 5;      // ja dit moet echt een constante worden ooit
            yOpRand = Math.Abs(klik.Y - rect.Top) < 5 || Math.Abs(klik.Y - rect.Bottom) < 5;

            return (xOpRand && yInBereik) || (yOpRand && xInBereik);
        }
        public override void Teken(Graphics gr)
        {
            gr.DrawRectangle(pen, rect);
        }
    }

    public class EllipsVorm : KaderVorm
    {
        protected int halveHoogte, halveBreedte, middelX, middelY; // Gebruikt in berekenen of er in/op de ellips is geklikt
        public EllipsVorm(SolidBrush kwast, Point startpunt, Point eindpunt) : base(kwast, startpunt, eindpunt)
        {
            halveBreedte = rect.Width / 2;      //a
            halveHoogte = rect.Height / 2;      //b

            middelX = rect.Left + halveBreedte; //x0
            middelY = rect.Top + halveHoogte;   //y0
        }

        public override void Teken(Graphics gr)
        {
            gr.DrawEllipse(pen, rect);
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
            //double res = Math.Pow(((klik.X - middelX) / halveBreedte) ,2) + Math.Pow(((klik.Y - middelY) / halveHoogte) ,2);
            return res;
        }
    }
    public class VollipsVorm : EllipsVorm
    {
        public VollipsVorm(SolidBrush kwast, Point startpunt, Point eindpunt) : base(kwast, startpunt, eindpunt)
        {
            // todo hoe geen body
        }
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
