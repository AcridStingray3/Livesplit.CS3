using System;
using System.Diagnostics;
using System.IO;
using System.Text;

// ReSharper disable file UnusedMember.Global

namespace Livesplit.CS3
{

    // Logs to a file in Temp rather than being forced to constantly use Debug.Print as that's annoying
    
    public static class Logger
    {
        private const string FILE_NAME = "****ToCS3_Livesplit_Log";
        private static readonly StreamWriter Writer;

        static Logger()
        {
            Debug.Print("Creating Logger");

            string logName = Path.Combine(Path.GetTempPath(), FILE_NAME + ".txt");
            if (File.Exists(logName))
                File.Delete(logName);
            

            FileStream fileStream = new FileStream(logName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            Writer = new StreamWriter(fileStream, Encoding.UTF8) {AutoFlush = true};

            File.SetCreationTimeUtc(logName, DateTime.UtcNow);
        }
        
        public static void Log(string message)
        {
            Writer.Write(message);
        }
        
    }
}