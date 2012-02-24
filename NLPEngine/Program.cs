using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace NLPEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            Process wordNet = new Process();

            wordNet.StartInfo.FileName = "C:\\Program Files (x86)\\WordNet\\2.1\\bin\\wn";
            wordNet.StartInfo.Arguments = "grab -synsv";
            wordNet.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            wordNet.StartInfo.RedirectStandardOutput = true;
            wordNet.StartInfo.UseShellExecute = false;
            wordNet.Start();

            System.IO.StreamReader myOutput = wordNet.StandardOutput;
            wordNet.WaitForExit(3000);

            if (wordNet.HasExited)
            {
                while (!myOutput.EndOfStream)
                {
                    String line = myOutput.ReadLine();
                    if (line.Contains("Sense 1"))
                    {
                        //grab the next two lines, process them, then break
                        String syns1 = myOutput.ReadLine();
                        String syns2 = myOutput.ReadLine();
                        List<String> syns = new List<String>();
                        Console.WriteLine(syns1);
                        Console.WriteLine(syns2);
                        break;
                    }
                }
            }
        }
    }
}
