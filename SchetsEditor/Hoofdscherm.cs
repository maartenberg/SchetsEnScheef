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
            menu.DropDownItems.Add("New", null, this.nieuw);
            menu.DropDownItems.Add("Open sketch in window", null, this.openvenster);
            menu.DropDownItems.Add("Exit", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }
        private void maakHelpMenu()
        {   ToolStripDropDownItem menu;
            menu = new ToolStripMenuItem("Help");
            menu.DropDownItems.Add("About \"Schets\"", null, this.about);
            menuStrip.Items.Add(menu);
        }
        private void about(object o, EventArgs ea)
        {   MessageBox.Show("Schets versie 1.0\n(c) UU Informatica 2010 \n\nWith new features by Maarten van den Berg\n& Machteld Hamers"
                           , "About \"Schets\""
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

            openKiezer.Filter = "Schets-files (*.SCHETS)|*.SCHETS|" +
                                "Alle files (*.*)|*.*";
            openKiezer.Title = "Open Schets in new window";

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
        {
            this.Close();
        }
    }
}
