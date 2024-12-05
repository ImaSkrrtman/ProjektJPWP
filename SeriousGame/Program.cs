using System;
using System.Windows.Forms;

namespace SeriousGame
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Wybór poziomu trudnoœci
            string difficulty = Microsoft.VisualBasic.Interaction.InputBox("Wybierz poziom trudnoœci: easy, medium, hard", "Wybór trudnoœci", "easy");
            Application.Run(new Form1(difficulty));
        }
    }
}
