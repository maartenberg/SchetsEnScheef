using System;
using System.Drawing;
using System.Windows.Forms;

namespace SchetsEditor
{
    public class Hoofdscherm : Form
    {
        MenuStrip menuStrip;

        public Hoofdscherm()
        {   this.ClientSize = new Size(800, 600);
            menuStrip = new MenuStrip();
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakHelpMenu();
            this.Text = "Schetsy";
            this.IsMdiContainer = true;
            this.MainMenuStrip = menuStrip;
        }
        private void maakFileMenu()
        {   ToolStripDropDownItem menu;
            menu = new ToolStripMenuItem("File");
            menu.DropDownItems.Add("Nieuw", null, this.nieuw);
            menu.DropDownItems.Add("Open in venster", null, this.openvenster);
            menu.DropDownItems.Add("Exit", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }
        private void maakHelpMenu()
        {   ToolStripDropDownItem menu;
            menu = new ToolStripMenuItem("Help");
            menu.DropDownItems.Add("Over \"Schets\"", null, this.about);
            menuStrip.Items.Add(menu);
        }
        private void about(object o, EventArgs ea)
        {   MessageBox.Show("Schets versie 1.0\n(c) UU Informatica 2010"
                           , "Over \"Schets\""
                           , MessageBoxButtons.OK
                           , MessageBoxIcon.Information
                           );
        }

        private void nieuw(object sender, EventArgs e)
        {   SchetsWin s = new SchetsWin();
            s.MdiParent = this;
            s.Show();
        }
        private void openvenster(object sender, EventArgs e)
        {
            OpenFileDialog openKiezer = new OpenFileDialog();

            openKiezer.Filter = "Schetsbestanden (*.SCHETS)|*.SCHETS|" +
                                "Alle bestanden (*.*)|*.*";
            openKiezer.Title = "Schets openen in nieuw venster";

            DialogResult resultaat = openKiezer.ShowDialog();
            if (resultaat == DialogResult.OK)
            {
                string bestandsnaam = openKiezer.FileName;
                SchetsWin s = new SchetsWin();
                s.MdiParent = this;
                s.OpenBestand(bestandsnaam);
                s.Show();
            }
        }
        private void afsluiten(object sender, EventArgs e)
        {   this.Close();
        }
    }
}
