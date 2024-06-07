using System.Diagnostics;

namespace BeeMobileApp.Classes
{
    internal static class Globals
    {
        internal static string ProjName { get; set; }        // Seleted project
        internal static string StrucMemName { get; set; }         // Seleted strutural member
        internal static string GameOrSite { get; set; }     // Variable to determine if the user is in the game or the real site 
        internal static string GameHostID { get; set; }     // The ID of the host of the game project 
        internal static string GameID { get; set; }         // The game ID that the user wants to play
        internal static Stopwatch GSW = new Stopwatch();
        internal static byte[] ProfileImage { get; set; }
        public static async Task Wait1MicSec() { await Task.Delay(1); }
        public static bool IsLoaded { get; set; } = true;       //True if the the xamarin page is loaded successfully
    }
}
