using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Automatico
{
    public class Program
    {

        static DateTime lastModified;
        static bool isWaitingForInput = true;
        static void Main(string[] args)
        {


            string script, scriptDirectory, geoDirectory, file, output, command;

            Console.WriteLine("Drag and drop the directory to monitor and press enter.");
            script = Console.ReadLine();

            if (!File.Exists(script))
            {
                Console.WriteLine("Invalid directory path.");
                return;
            }

            scriptDirectory = Path.GetDirectoryName(script);

            lastModified = File.GetLastWriteTime(script);


            Console.WriteLine("Drag and drop the directory that the bin file goes.");
            output = Console.ReadLine();


            if (!Directory.Exists(output))
            {
                Console.WriteLine("Invalid directory path.");
                return;
            }

            file = output;

            output = output + "\\" + Path.Combine(Path.GetFileNameWithoutExtension(script) + ".bin");

            //command = "python \"BBTAG_Script_Rebuilder.py\" \"" + script + "\" \"" + output + "\"";
            command = "python BBTAG_Script_Rebuilder.py " + script + " " + output;

            Console.WriteLine("Drag and drop the directory of GeoArcSysAIOCLITool.exe");
            geoDirectory = Console.ReadLine();


            if (!Directory.Exists(geoDirectory))
            {
                Console.WriteLine("Invalid directory path.");
                return;
            }



            Thread loadingThread = new Thread(DisplayLoadingMessage);
            loadingThread.Start();

            while (true)
            {
                // Check if the file has been modified
                DateTime currentModified = File.GetLastWriteTime(script);
                if (currentModified > lastModified)
                {
                    lastModified = currentModified;
                    Console.WriteLine("File has been modified. Executing command...");
                    isWaitingForInput = false;
                    (bool commandSuccess, string errorOutput) = RunCommand(command, scriptDirectory);
                    if (commandSuccess)
                    {
                        Console.WriteLine(".Bin Created!");
                    }
                    else
                    {
                        Console.WriteLine("failed with the following error output:");
                        Console.WriteLine(errorOutput);
                        isWaitingForInput = true;
                        continue;
                    }
                    (commandSuccess, errorOutput) = RunCommand("GeoArcSysAIOCLITool.exe PAC " + file + " -om Overwrite", geoDirectory);
                    if (commandSuccess)
                    {
                        Console.WriteLine("Pacced");
                    }
                    else
                    {
                        Console.WriteLine("failed with the following error output:");
                        Console.WriteLine(errorOutput);
                        isWaitingForInput = true;
                        continue;
                    }
                    isWaitingForInput = true;
                    continue;
                }

                // Wait for a specific interval before checking again
                Thread.Sleep(1000); // Wait for 1 second
            }
        }

        static void DisplayLoadingMessage()
        {
            string[] loadingSymbols = { "|", "/", "-", "\\" };
            int symbolIndex = 0;

            while (true)
            {
                if (isWaitingForInput)
                {
                    try
                    {
                        Console.Write(loadingSymbols[symbolIndex]);
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

                        symbolIndex = (symbolIndex + 1) % loadingSymbols.Length;
                        Thread.Sleep(1000);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                else
                {
                    Console.WriteLine();
                    Thread.Sleep(1000);
                }
            }
        }


        static (bool success, string errorOutput) RunCommand(string command2, string workingDirectory)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C " + command2;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = workingDirectory; // Set the working directory

            if (command2.Contains("GeoArcSysAIOCLITool.exe PAC "))
            {
                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                    process.Kill();
                    process.StandardOutput.ReadToEnd();

                    int exitCode = process.ExitCode;
                    string errorOutput = process.StandardError.ReadToEnd();

                    return (exitCode == 0, errorOutput);
                }
            }

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                process.Kill();

                int exitCode = process.ExitCode;
                string errorOutput = process.StandardError.ReadToEnd();

                return (exitCode == 0, errorOutput);
            }
        }
    }

}