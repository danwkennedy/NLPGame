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


        public LinkGrammarParser()
        {
            pipe = new NamedPipeServerStream("testPipe", PipeDirection.InOut);

            Process linkGrammarProcess = new Process();
            String fileName = System.IO.Path.GetDirectoryName(
      System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\..\\..\\..\\Resources\\LinkGrammar.exe";
            if (fileName.StartsWith("file:"))
                linkGrammarProcess.StartInfo.FileName = fileName.Substring(6);
            else
                linkGrammarProcess.StartInfo.FileName = fileName;
            //linkGrammarProcess.StartInfo.FileName = @"C:\Users\Daniel\Desktop\link-grammar-4.7.4\msvc9\Debug\LinkGrammarDextor.exe";
            linkGrammarProcess.StartInfo.Arguments += "testPipe";

            //linkGrammarProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            //linkGrammarProcess.StartInfo.RedirectStandardOutput = true;
            //linkGrammarProcess.StartInfo.UseShellExecute = false;

            Console.WriteLine("[LinkGrammarPipe] Starting Link Grammar");
            linkGrammarProcess.Start();
            Console.WriteLine("[LinkGrammarPipe] Waiting for pipe connection");
            pipe.WaitForConnection();
            Console.WriteLine("[LinkGrammarPipe] Client has connected");

            writer = new StreamWriter(pipe);
            reader = new StreamReader(pipe);
        }

        public bool GetVerbNounPair(string sentence, out string verb, out string noun)
        {
            try
            {
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

            //Console.WriteLine("Number of matches: {0}", matches.Count);

            if (matches.Count > 0)
            {
                verb = matches[0].Groups[1].Value;
                noun = matches[0].Groups[2].Value;
                return true;
            }
            else
            {
                verb = "Invalid";
                noun = "Invalid";
                return false;
            }

            
        }
    }
}
