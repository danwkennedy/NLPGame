﻿using System;
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
    public class Program
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
            List<String> vsyns = GetVerbSynonyms(users.verb);
            List<String> nsyns = GetNounSynonyms(users.noun);
            foreach (NounVerbPair pair in game)
            {
                String pnoun = pair.noun;
                String pverb = pair.verb;
                foreach (String vsyn in vsyns)
                {
                    //TODO log this to a log file
                    //Console.WriteLine(pverb + " " + vsyn);
                    if (pverb.IndexOf(vsyn, StringComparison.OrdinalIgnoreCase) >= 0 || vsyn.IndexOf(pverb, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        //TODO log this to a log file
                        //Console.WriteLine("matched verb");
                        foreach (String nsyn in nsyns)
                        {
                            //TODO log this to a log file
                            //Console.WriteLine(pnoun + " " + nsyn);
                            if (pnoun.IndexOf(nsyn, StringComparison.OrdinalIgnoreCase) >= 0 || nsyn.IndexOf(pnoun, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                //TODO log this to a log file
                                //Console.WriteLine("matched noun");
                                return pair;
                            }
                        }
                    }
                }
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