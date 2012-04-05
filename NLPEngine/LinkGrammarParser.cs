using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NLPEngine
{
    public class LinkGrammarParser
    {

        NamedPipeServerStream pipe;
        StreamWriter writer;
        StreamReader reader;

        string regexPattern = @"\[VP (\w+)(?: \[PP \w+)* \[NP (?:the |a )*([\w+|\s]+)* NP\]";


        StreamWriter fileWriter;

        public LinkGrammarParser(int runNumber)
        {
            pipe = new NamedPipeServerStream("testPipe", PipeDirection.InOut);

            Process linkGrammarProcess = new Process();
            String fileName = Directory.GetCurrentDirectory() + @"\..\..\..\Resources\LinkGrammar.exe";

            linkGrammarProcess.StartInfo.FileName = fileName;

            linkGrammarProcess.StartInfo.Arguments += "testPipe";

            linkGrammarProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Console.WriteLine("[LinkGrammarPipe] Starting Link Grammar");
            linkGrammarProcess.Start();
            Console.WriteLine("[LinkGrammarPipe] Waiting for pipe connection");
            pipe.WaitForConnection();
            Console.WriteLine("[LinkGrammarPipe] Client has connected");

            writer = new StreamWriter(pipe);
            reader = new StreamReader(pipe);

            SetupDataGathering(runNumber);
        }

        ~LinkGrammarParser()
        {
            pipe.Close();
        }

        private void SetupDataGathering(int run)
        {
            string baseDirectory = Directory.GetCurrentDirectory() + @"\..\..\..\Logs\" + run;
            DirectoryInfo info = new DirectoryInfo(baseDirectory);

            // Check to see if the directory exists, if not, create it
            if (!info.Exists)
            {
                info.Create();
            }

            string filepath = baseDirectory + @"\Linkgrammar.txt";

            fileWriter = File.CreateText(filepath);
            fileWriter.AutoFlush = true;
            fileWriter.WriteLine("NLP Game run #{0}", run);
            fileWriter.WriteLine("Logging the Link Grammar Parser's output");
            fileWriter.WriteLine("Logging started at: {0}, {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
            fileWriter.WriteLine(" ");
        }

        public void EndDataGathering()
        {
            fileWriter.WriteLine("Shutting Down");
            fileWriter.Close();
        }

        public bool GetVerbNounPair(string sentence, out string verb, out string noun)
        {
            try
            {
                fileWriter.WriteLine("Sending: {0}", sentence);

                writer.WriteLine(sentence);
                writer.Flush();
                string response = "";

                bool ready = false;
                while (!ready)
                {
                    response = reader.ReadLine();

                    if (!String.IsNullOrWhiteSpace(response))
                    {
                        ready = true;
                    }
                }

                fileWriter.WriteLine("Recieved: {0}", response);
                
                return ParseTree(response, out verb, out noun);
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        private bool ParseTree(string sentence, out string verb, out string noun)
        {
            if (sentence.Equals("Invalid"))
            {
                verb = "Invalid";
                noun = "Invalid";
                return false;
            }

            MatchCollection matches = Regex.Matches(sentence, regexPattern);

            if (matches.Count > 0)
            {
                verb = matches[0].Groups[1].Value;
                noun = matches[0].Groups[2].Value;

                fileWriter.WriteLine("Parsed into: {0}(verb), {1}(noun)", verb, noun);
                fileWriter.WriteLine(" ");

                return true;
            }
            else
            {
                verb = "Invalid";
                noun = "Invalid";

                fileWriter.WriteLine("Parsed into: {0}(verb), {1}(noun)", verb, noun);
                fileWriter.WriteLine(" ");

                return false;
            }
        }
    }
}
