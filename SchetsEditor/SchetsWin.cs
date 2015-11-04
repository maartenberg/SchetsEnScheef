using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.IO;

namespace SchetsEditor
{
    public class SchetsWin : Form
    {   
        MenuStrip menuStrip;
        SchetsControl schetscontrol;
        ISchetsTool huidigeTool;
        Panel paneel;
        bool vast, veranderd;
        ResourceManager resourcemanager
            = new ResourceManager("SchetsEditor.Properties.Resources"
                                 , Assembly.GetExecutingAssembly()
                                 );

        private void veranderAfmeting(object o, EventArgs ea)
        {
            schetscontrol.Size = new Size ( this.ClientSize.Width  - 70
                                          , this.ClientSize.Height - 50);
            paneel.Location = new Point(64, this.ClientSize.Height - 30);
        }

        private void klikToolMenu(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((ToolStripMenuItem)obj).Tag;
        }

        private void klikToolButton(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((RadioButton)obj).Tag;
        }

        private void afsluiten(object obj, EventArgs ea)
        {
            this.Close();
        }

        // Onderdeel 2: opslaan en openen van schets
        private void naarbestand(object obj, EventArgs ea)
        {
            SaveFileDialog opslagKiezer = new SaveFileDialog();

            opslagKiezer.Filter = "PNG-bestand|*.png|JPG-bestand|*.jpg|BMP-bestand|*.bmp";
            opslagKiezer.Title = "Schets opslaan";

            DialogResult resultaat = opslagKiezer.ShowDialog();
            if (resultaat == DialogResult.OK)
            {
                string bestandsnaam = opslagKiezer.FileName;
                this.schetscontrol.Schets.NaarBestand(bestandsnaam, opslagKiezer.FilterIndex);
                // Hier wordt this.veranderd NIET gereset, omdat de afbeelding hieruit niet kan worden ingelezen
            }
        }
        private void naarschetsbestand(object obj, EventArgs ea)
        {
            SaveFileDialog opslagKiezer = new SaveFileDialog();

            opslagKiezer.Filter = "SCHETS-bestand|*.SCHETS";
            opslagKiezer.Title = "Schets opslaan";

            DialogResult resultaat = opslagKiezer.ShowDialog();
            if (resultaat == DialogResult.OK)
            {
                string bestandsnaam = opslagKiezer.FileName;
                this.schetscontrol.Schets.NaarSchetsBestand(bestandsnaam);
                veranderd = false;
            }
        }


        public void OpenBestand(string bestandsnaam)
        {
            schetscontrol.VanBestand(bestandsnaam);
        }
        private void vanbestand(object obj, EventArgs ea)
        {
            OpenFileDialog openKiezer = new OpenFileDialog();

            openKiezer.Filter = "Schetsbestanden (*.SCHETS)|*.SCHETS|" +
                                "Alle bestanden (*.*)|*.*";
            openKiezer.Title = "Schets openen in huidig venster";

            DialogResult resultaat = openKiezer.ShowDialog();
            if (resultaat == DialogResult.OK)
            {
                string bestandsnaam = openKiezer.FileName;
                schetscontrol.VanBestand(bestandsnaam);
            }
        }

        public SchetsWin()
        {
            ISchetsTool[] deTools = { new PenTool()         
                                    , new RegenboogPenTool()
                                    , new EmmerTool()
                                    , new LijnTool()
                                    , new RechthoekTool()
                                    , new VolRechthoekTool()
                                    , new OvaalTool()
                                    , new VolOvaalTool()
                                    , new TekstTool()
                                    , new GumTool()
                                    };
            String[] deKleuren = { "Black", "Red", "Green", "Blue"
                                 , "Yellow", "Magenta", "Cyan", "White"
                                 , "Aangepast"
                                 };

            this.ClientSize = new Size(700, 650);
            huidigeTool = deTools[0];

            schetscontrol = new SchetsControl();
            schetscontrol.Location = new Point(64, 10);
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       {   vast=true; veranderd = true; 
                                           huidigeTool.MuisVast(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisDrag(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisLos (schetscontrol, mea.Location);
                                           vast = false; 
                                       };
            schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                       {   huidigeTool.Letter  (schetscontrol, kpea.KeyChar); 
                                       };
            this.Controls.Add(schetscontrol);

            menuStrip = new MenuStrip();
            menuStrip.Visible = false;
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakToolMenu(deTools);
            this.maakAktieMenu(deKleuren);
            this.maakToolButtons(deTools);
            this.maakAktieButtons(deKleuren);
            this.Resize += this.veranderAfmeting;
            this.FormClosing += this.AfsluitVrager;
            this.veranderAfmeting(null, null);
        }

        private void maakFileMenu()
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("File");
            menu.MergeAction = MergeAction.MatchOnly;
            menu.DropDownItems.Add(new ToolStripSeparator());
            menu.DropDownItems.Add("Opslaan", null, naarschetsbestand);
            menu.DropDownItems.Add("Openen", null, this.vanbestand);
            menu.DropDownItems.Add("Exporteren", null, this.naarbestand);
            menu.DropDownItems.Add("Sluiten", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }

        private void maakToolMenu(ICollection<ISchetsTool> tools)
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
            foreach (ISchetsTool tool in tools)
            {   ToolStripItem item = new ToolStripMenuItem();
                item.Tag = tool;
                item.Text = tool.ToString();
                item.Image = (Image)resourcemanager.GetObject(tool.ToString());
                item.Click += this.klikToolMenu;
                menu.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menu);
        }

        private void maakAktieMenu(String[] kleuren)
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Aktie");
            menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon );
            menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer );
            menu.DropDownItems.Add("Undo", null, schetscontrol.Undo);
            menu.DropDownItems.Add("Redo", null, schetscontrol.Redo);
            ToolStripMenuItem submenu = new ToolStripMenuItem("Kies kleur");
            foreach (string k in kleuren)
                submenu.DropDownItems.Add(k, null, schetscontrol.VeranderKleurViaMenu);
            menu.DropDownItems.Add(submenu);
            menuStrip.Items.Add(menu);
        }

        private void maakToolButtons(ICollection<ISchetsTool> tools)
        {
            int t = 0;
            foreach (ISchetsTool tool in tools)
            {
                RadioButton b = new RadioButton();
                b.Appearance = Appearance.Button;
                b.Size = new Size(45, 62);
                b.Location = new Point(10, 10 + t * 62);
                b.Tag = tool;
                b.Text = tool.ToString();
                b.Image = (Image)resourcemanager.GetObject(tool.ToString());
                b.TextAlign = ContentAlignment.TopCenter;
                b.ImageAlign = ContentAlignment.BottomCenter;
                b.Click += this.klikToolButton;
                this.Controls.Add(b);
                if (t == 0) b.Select();
                t++;
            }
        }

        private void maakAktieButtons(String[] kleuren)
        {   
            paneel = new Panel();
            paneel.Size = new Size(625, 24);
            this.Controls.Add(paneel);
            
            Button b; Label l; ComboBox cbb; NumericUpDown num;
            b = new Button(); 
            b.Text = "Clear";  
            b.Location = new Point(  0, 0); 
            b.Click += schetscontrol.Schoon; 
            paneel.Controls.Add(b);
            
            b = new Button(); 
            b.Text = "Rotate"; 
            b.Location = new Point( 80, 0); 
            b.Click += schetscontrol.Roteer; 
            paneel.Controls.Add(b);
            
            l = new Label();  
            l.Text = "Penkleur:"; 
            l.Location = new Point(160, 3); 
            l.AutoSize = true;               
            paneel.Controls.Add(l);
            
            cbb = new ComboBox(); cbb.Location = new Point(220, 0); 
            cbb.DropDownStyle = ComboBoxStyle.DropDownList; 
            cbb.SelectionChangeCommitted += schetscontrol.VeranderKleur;
            foreach (string k in kleuren)
                cbb.Items.Add(k);
            cbb.SelectedIndex = 0;
            schetscontrol.VeranderKleur(cbb, null);  // zodat bovenste kleur ook standaard wordt ingesteld
            paneel.Controls.Add(cbb);

            b = new Button();
            b.Text = "Undo";
            b.Location = new Point(350, 0);
            b.AutoSize = true;
            b.Click += schetscontrol.Undo;
            paneel.Controls.Add(b);

            b = new Button();
            b.Text = "Redo";
            b.Location = new Point(430, 0);
            b.AutoSize = true;
            b.Click += schetscontrol.Redo;
            paneel.Controls.Add(b);

            l = new Label();
            l.Text = "Pendikte:";
            l.Location = new Point(510, 3);
            l.AutoSize = true;
            paneel.Controls.Add(l);

            num = new NumericUpDown();
            num.Location = new Point(570, 0);
            num.Minimum = 1;
            num.Maximum = 50;
            num.Width = 50;
            num.ValueChanged += schetscontrol.VeranderDikte;
            paneel.Controls.Add(num);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SchetsWin
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "SchetsWin";
            this.Text = "T";
            this.ResumeLayout(false);
        }
        private void AfsluitVrager(object o, FormClosingEventArgs fcea)
        {
            if (!veranderd) return;
            DialogResult zeker = MessageBox.Show("Weet je zeker dat je wilt sluiten?", 
                "Onopgeslagen wijzigingen", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (zeker != DialogResult.Yes)
            {
                fcea.Cancel = true;
            }
        }
    }
}
