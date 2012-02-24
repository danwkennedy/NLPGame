using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

/*
 * @author Jessica Forrester - jwforres@ncsu.edu
 * @author Daniel Kennedy - dwkenned@ncsu.edu
 */

namespace NLPEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            //Sample code testing the get synonym functions
            Console.WriteLine("Getting verb synonyms for 'grab'");
            List<String> sampleSynonyms = GetVerbSynonyms("grab");
            foreach (String syn in sampleSynonyms)
            {
                Console.WriteLine(syn);
            }
            Console.WriteLine();
            Console.WriteLine("Getting noun synonyms for 'cart'");
            List<String> sampleSynonyms2 = GetNounSynonyms("cart");
            foreach (String syn in sampleSynonyms2)
            {
                Console.WriteLine(syn);
            }
        }

        static List<String> GetVerbSynonyms(String verb)
        {
            return GetSynonyms(verb, true);
        }

        static List<String> GetNounSynonyms(String noun)
        {
            return GetSynonyms(noun, false);
        }

        static List<String> GetSynonyms(String word, bool isVerb)
        {
            List<String> syns = new List<String>();
            //TODO we will need to handle WordNet being in a different location
            System.IO.StreamReader outstream = RunHiddenProcess("C:\\Program Files (x86)\\WordNet\\2.1\\bin\\wn", word + (isVerb ? " -synsv" : " -synsn"));

            while (!outstream.EndOfStream)
            {
                String line = outstream.ReadLine();
                if (line.Contains("Sense 1"))
                {
                    //grab the next two lines, process them, then break
                    String syns1 = outstream.ReadLine();
                    String syns2 = outstream.ReadLine();
                    int arrowInd = syns2.IndexOf("=>");
                    if (arrowInd >= 0)
                        syns2 = syns2.Substring(arrowInd + 2);
                    String delimStr = ",";
                    char[] delimiter = delimStr.ToCharArray();
                    foreach (String syn in syns1.Split(delimiter))
                    {
                        syns.Add(syn.Trim());
                    }
                    foreach (String syn in syns2.Split(delimiter))
                    {
                        syns.Add(syn.Trim());
                    }
                    break;
                }
            }
            return syns;
        }

        static System.IO.StreamReader RunHiddenProcess(String program, String arguments)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = program;
            proc.StartInfo.Arguments = arguments;
            proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();

            System.IO.StreamReader outstream = proc.StandardOutput;
            while (!proc.HasExited)
                proc.WaitForExit(500);

            return outstream;
        }
    }
}
