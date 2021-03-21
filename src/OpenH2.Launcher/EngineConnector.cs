using System;
using System.Diagnostics;
using System.IO;

namespace OpenH2.Launcher
{
    public static class EngineConnector
    {
        private static Process? runningProcess;

        private static string LocateEngine()
        {
            var enginePath = Environment.GetEnvironmentVariable("openh2_engine");

            if (enginePath != null && File.Exists(enginePath))
            {
                return enginePath;
            }

            enginePath = Path.Combine(Directory.GetCurrentDirectory(), "OpenH2.Engine.exe");

            if(File.Exists(enginePath))
            {
                return enginePath;
            }

            enginePath = Path.Combine(Directory.GetCurrentDirectory(), "engine", "OpenH2.Engine.exe");

            if (File.Exists(enginePath))
            {
                return enginePath;
            }

            throw new Exception("Cannot find OpenH2.Engine executable");
        }

        public static void Start(string mapPath)
        {
            if(runningProcess != null && !runningProcess.HasExited)
            {
                runningProcess.Kill();
            }

            var enginePath = LocateEngine();
            var startInfo = new ProcessStartInfo(enginePath, @$"""{mapPath}""");
            startInfo.WorkingDirectory = Path.GetDirectoryName(enginePath);
            startInfo.EnvironmentVariables["openh2_configroot"] = "Configs";

            runningProcess = Process.Start(startInfo);
        }
    }
}
