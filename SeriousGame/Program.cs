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

            // Wyb�r poziomu trudno�ci
            string difficulty = Microsoft.VisualBasic.Interaction.InputBox("Wybierz poziom trudno�ci: easy, medium, hard", "Wyb�r trudno�ci", "easy");
            Application.Run(new Form1(difficulty));
        }
    }
}
