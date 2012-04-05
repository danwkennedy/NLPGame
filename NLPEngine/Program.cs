﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

/*
 * @author Jessica Forrester - jwforres@ncsu.edu
 * @author Daniel Kennedy - dwkenned@ncsu.edu
 */
namespace NLPEngine
{
    public class Program
    {
        static public StreamWriter wnFileWriter;
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
            Console.WriteLine();

            Console.WriteLine("Matching user verb 'grab' to game verbs 'catch, talk'");
            List<String> gameVerbs = new List<String>();
            gameVerbs.Add("catch");
            gameVerbs.Add("talk");
            Console.WriteLine("Matched verb: " + MatchUserVerbToGameVerb("grab", gameVerbs));
            Console.WriteLine();

            Console.WriteLine("Matching user noun 'cart' to game nouns 'wagon, sheep'");
            List<String> gameNouns = new List<String>();
            gameNouns.Add("wagon");
            gameNouns.Add("sheep");
            Console.WriteLine("Matched noun: " + MatchUserNounToGameNoun("cart", gameNouns));
        }

        public static NounVerbPair MatchUserToGameVerbNounPair(NounVerbPair users, List<NounVerbPair> game)
        {
            
            List<String> vsyns = new List<String>();
            string delimStr = " \t";
            char [] delimiter = delimStr.ToCharArray();
            foreach(String userv in users.verb.Split(delimiter))
            {
                if (userv.Trim().Length > 0)
                    vsyns.AddRange(GetVerbSynonyms(userv));
            }
            List<String> nsyns = new List<String>();
            foreach (String usern in users.noun.Split(delimiter))
            {
                if (usern.Trim().Length > 0)
                    nsyns.AddRange(GetNounSynonyms(usern));
            }
            foreach (NounVerbPair pair in game)
            {
                String pnoun = pair.noun;
                String pverb = pair.verb;
                foreach (String vsyn in vsyns)
                {
                    if (pverb.IndexOf(vsyn, StringComparison.OrdinalIgnoreCase) >= 0 || vsyn.IndexOf(pverb, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foreach (String nsyn in nsyns)
                        {
                            if (pnoun.IndexOf(nsyn, StringComparison.OrdinalIgnoreCase) >= 0 || nsyn.IndexOf(pnoun, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (wnFileWriter != null)
                                    wnFileWriter.WriteLine("MATCHED user verb \""+users.verb+"\" to game verb \"" + pverb + "\" and user noun \""+users.noun + "\" to game noun \"" + pnoun + "\"");
                                return pair;
                            }
                        }
                    }
                }
            }
            if (wnFileWriter != null) {
                wnFileWriter.Write("FAILED to match user verb \"" + users.verb + "\" or its synonyms " );
                foreach (String vsyn in vsyns)
                    wnFileWriter.Write(vsyn + ", ");
                wnFileWriter.Write(" or user noun \"" + users.noun + "\" and its synonyms ");
                foreach (String nsyn in nsyns)
                    wnFileWriter.Write(nsyn + ", ");
                wnFileWriter.WriteLine("");
            }
            return null;
        }

        public static String MatchUserVerbToGameVerb(String verb, List<String> gameVerbs)
        {
            List<String> syns = GetVerbSynonyms(verb);
            foreach (String syn in syns)
            {
                if (gameVerbs.Contains(syn))
                {
                    return syn;
                }
            }
            return null;
        }

        public static String MatchUserNounToGameNoun(String noun, List<String> gameNouns)
        {
            List<String> syns = GetNounSynonyms(noun);
            foreach (String syn in syns)
            {
                if (gameNouns.Contains(syn))
                {
                    return syn;
                }
            }
            return null;
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
            String fileName = System.IO.Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\..\..\..\WordNet\2.1\bin\wn";
            if (fileName.StartsWith("file:"))
                fileName = fileName.Substring(6);
            System.IO.StreamReader outstream = RunHiddenProcess(fileName, word + (isVerb ? " -synsv" : " -synsn"));

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
                        String trimmedSyn = syn.Trim();
                        if (trimmedSyn.Length > 0)
                            syns.Add(trimmedSyn);
                    }
                    foreach (String syn in syns2.Split(delimiter))
                    {
                        String trimmedSyn = syn.Trim();
                        if (trimmedSyn.Length > 0)
                            syns.Add(trimmedSyn);
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

        public static void SetupDataGathering(int run)
        {
            string baseDirectory = Directory.GetCurrentDirectory() + @"\..\..\..\Logs\" + run;
            DirectoryInfo info = new DirectoryInfo(baseDirectory);

            // Check to see if the directory exists, if not, create it
            if (!info.Exists)
            {
                info.Create();
            }

            string filepath = baseDirectory + @"\Wordnet.txt";

            wnFileWriter = File.CreateText(filepath);
            wnFileWriter.AutoFlush = true;
            wnFileWriter.WriteLine("NLP Game run #{0}", run);
            wnFileWriter.WriteLine("Logging the WordNet output");
            wnFileWriter.WriteLine("Logging started at: {0}, {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
            wnFileWriter.WriteLine(" ");
        }
        public static void EndDataGathering()
        {
            wnFileWriter.WriteLine("Shutting Down");
            wnFileWriter.Close();
        }
    }
    public class NounVerbPair
    {
        public String noun;
        public String verb;
        public Action<String> action;
        public NounVerbPair(String theNoun, String theVerb)
        {
            noun = theNoun;
            verb = theVerb;
        }
        public NounVerbPair(String theNoun, String theVerb, Action<String> theAction)
        {
            noun = theNoun;
            verb = theVerb;
            action = theAction;
        }
    }
}