using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLPEngine;
using System.IO;

namespace NLPGame
{
    class Program
    {
        //State variables for Room #1
        static bool isNearLevers = false;
        static bool isNearDoor = false;
        static bool isNearWheel = false;
        static bool isSunLeverPulled = false;
        static bool isWheelTurned = false;

        //State variables for Room #2
        static bool isNearKeys = false;
        static bool hasHawkKey = false;
        static bool hasLionKey = false;
        static bool hasSharkKey = false;
        static bool isHawkKeySuccess = false;
        static bool isSharkKeySuccess = false;
        static bool isNearDoorTwo = false;


        //Global state variables
        static int gamePosition = 0;
        static bool gameOver = false;

        static int runNumber;
        //static TimeSpan playTime;
        static DateTime startTime;

        static LinkGrammarParser linkGrammar;

        static StreamWriter fileWriter;

        static int sentencesNotParsed = 0;
        static int sentencesNotMatched = 0;
        static int sentencesCorrect = 0;

        // the list of valid noun/verb pairs for a given point in the game, i.e. at the very beginning you can only do one action, 
        // at the next step you can only do one action, but at the third point there are many valid actions
        static List<NounVerbPair>[] validActionsByGamePosition;

        // Main function, repeatedly lets the user play the game until they choose not to
        static void Main(string[] args)
        {
            SetupDataGathering();
            setupGame();
            bool playAgain = true;
            while (playAgain)
            {
                Console.Clear();
                resetGameState();
                playGame();

                fileWriter.WriteLine("Game ended at {0}", DateTime.Now.ToShortTimeString());
                fileWriter.WriteLine("Play time (seconds): {0}", DateTime.Now.Subtract(startTime).TotalSeconds);
                fileWriter.WriteLine("Number of sentences not parsed: {0}", sentencesNotParsed);
                fileWriter.WriteLine("Number of sentences not matched: {0}", sentencesNotMatched);
                fileWriter.WriteLine("Total number of missed sentences: {0}", (sentencesNotParsed + sentencesNotMatched));
                fileWriter.WriteLine("Total number of sentences passed: {0}", sentencesCorrect);
                fileWriter.WriteLine("Ratio of sentences passed over sentences not passed: {0}",  (float)sentencesCorrect / ((float) sentencesNotParsed + (float) sentencesNotMatched));

                Console.WriteLine("\nWould you like to play again? (Yes or No)\n");
                String yesOrNo = Console.ReadLine();
                playAgain = yesOrNo.Equals("Yes") || yesOrNo.Equals("yes") || yesOrNo.Equals("Y") || yesOrNo.Equals("y");
            }

            fileWriter.Close();
            NLPEngine.Program.EndDataGathering();
            linkGrammar.EndDataGathering();
        }

        static void resetGameState()
        {
            isNearLevers = false;
            isNearDoor = false;
            isNearWheel = false;
            isSunLeverPulled = false;
            isWheelTurned = false;
            isNearKeys = false;
            hasHawkKey = false;
            hasLionKey = false;
            hasSharkKey = false;
            isHawkKeySuccess = false;
            isSharkKeySuccess = false;
            isNearDoorTwo = false;
            gamePosition = 0;
            gameOver = false;
        }

        static void SetupDataGathering()
        {
            string baseDirectory = Directory.GetCurrentDirectory() + @"\..\..\..\Logs";
            DirectoryInfo info = new DirectoryInfo(baseDirectory);

            if (info.Exists)
            {
                FileSystemInfo[] files = info.GetFileSystemInfos();

                runNumber = 0;
                foreach (FileSystemInfo file in files)
                {
                    int index;
                    if (Int32.TryParse(file.Name, out index))
                    {
                        if (index >= runNumber)
                        {
                            runNumber = index + 1;
                        }
                    }
                }
            }
            else
            {
                info.Create();
                runNumber = 0;
            }

            DirectoryInfo parentInfo = Directory.CreateDirectory(baseDirectory + @"\" + runNumber);
            string filename = parentInfo.FullName + @"\GameLog.txt";
            
            fileWriter = File.CreateText(filename);
            fileWriter.AutoFlush = true;
            fileWriter.WriteLine("NLP Game run #{0}", runNumber);
            fileWriter.WriteLine("Logging game activity");
            fileWriter.WriteLine("Game started at: {0}, {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());

            NLPEngine.Program.SetupDataGathering(runNumber);
        }

        static void setupGame()
        {
            linkGrammar = new LinkGrammarParser(runNumber);

            validActionsByGamePosition = new List<NounVerbPair>[5];
            validActionsByGamePosition[0] = new List<NounVerbPair>();
            validActionsByGamePosition[0].Add(new NounVerbPair("pedestal", "go", GoToPedestal));
            
            validActionsByGamePosition[1] = new List<NounVerbPair>();
            validActionsByGamePosition[1].Add(new NounVerbPair("tablet", "read", ReadTablet));
            
            validActionsByGamePosition[2] = new List<NounVerbPair>();
            validActionsByGamePosition[2].Add(new NounVerbPair("wheel", "go", GoToWheel));
            validActionsByGamePosition[2].Add(new NounVerbPair("levers", "go", GoToLevers));
            validActionsByGamePosition[2].Add(new NounVerbPair("door", "go", GoToDoor));
            validActionsByGamePosition[2].Add(new NounVerbPair("wheel", "turn", TurnWheel));
            validActionsByGamePosition[2].Add(new NounVerbPair("rain", "pull", PullRain));
            validActionsByGamePosition[2].Add(new NounVerbPair("lightning", "pull", PullLightning));
            validActionsByGamePosition[2].Add(new NounVerbPair("sun", "pull", PullSun));
            validActionsByGamePosition[2].Add(new NounVerbPair("door", "open", OpenDoor));
            
            validActionsByGamePosition[3] = new List<NounVerbPair>();
            validActionsByGamePosition[3].Add(new NounVerbPair("hawk", "take", TakeHawkKey));
            validActionsByGamePosition[3].Add(new NounVerbPair("shark", "take", TakeSharkKey));
            validActionsByGamePosition[3].Add(new NounVerbPair("lion", "take", TakeLionKey));
            validActionsByGamePosition[3].Add(new NounVerbPair("hawk", "use", UseHawkKey));
            validActionsByGamePosition[3].Add(new NounVerbPair("shark", "use", UseSharkKey));
            validActionsByGamePosition[3].Add(new NounVerbPair("lion", "use", UseLionKey));
            validActionsByGamePosition[3].Add(new NounVerbPair("keys", "go", GoToKeys));
            validActionsByGamePosition[3].Add(new NounVerbPair("table", "go", GoToKeys));
            validActionsByGamePosition[3].Add(new NounVerbPair("door", "go", GoToDoorTwo));
            validActionsByGamePosition[3].Add(new NounVerbPair("door", "open", OpenDoorTwo));

            startTime = DateTime.Now;
        }

        // Core game function
        static void playGame()
        {
            Console.WriteLine("You are the famous explorer Digsby Longsworth.\n");
            Console.WriteLine("You find yourself deep inside a Mayan temple in the heart of the Yucatan peninsula.  A metal grate just slid shut behind you, blocking the only exit from the room.\n");
            Console.WriteLine("You take a look around and see a pedestal in the center of the room, it looks like there is something on it.\n\n");

            while (promptUserAndExecuteAction(validActionsByGamePosition[gamePosition])) { }
        }

        // Prompts user for input
        static bool promptUserAndExecuteAction(List<NounVerbPair> validPairs)
        {
            Console.WriteLine("What do you want to do?");
            Console.Write("> ");
            String userInput = Console.ReadLine();

            //Make calls into NLP engine here to get the noun and verb matching the user's input,  we should repeat prompting until the user provides a valid input

            // The verb and the noun pulled from the user input
            string verb = "Invalid";
            string noun = "Invalid";

            bool isValid = false;
           
            // Loop until the link grammar returns a valid verb/noun combination
            while (!isValid) {
                if (linkGrammar.GetVerbNounPair(userInput, out verb, out noun))
                {
                    NounVerbPair pair = NLPEngine.Program.MatchUserToGameVerbNounPair(new NounVerbPair(noun, verb), validPairs);
                    if (pair != null)
                    {
                        Console.WriteLine("\n");
                        pair.action("dummy");
                        isValid = true;

                        sentencesCorrect++;
                        break;
                    }

                    sentencesNotMatched++;
                    NLPEngine.Program.wnFileWriter.WriteLine("failure was at game position " + gamePosition);

                    //The noun/verb didn't match to any valid ones
                    Console.WriteLine("I don't quite follow you. Please try again.");
                    Console.Write("> ");
                    userInput = Console.ReadLine();
                }
                else
                {
                    sentencesNotParsed++;

                    Console.WriteLine("I don't quite follow you. Please try again.");
                    Console.Write("> ");
                    userInput = Console.ReadLine();
                }
            }

            return !gameOver;
        }

        static void GoToPedestal(String dummy)
        {
            Console.WriteLine("You walk over to the pedestal.\n");
            Console.WriteLine("You now see there is a tablet covered in ancient runes. Before your eyes you see the runes shift and change.  Amazingly the words are now in English.\n");
            gamePosition = 1;
        }

        static void ReadTablet(String dummy)
        {
            Console.WriteLine("The tablet reads \"You must complete the three challenges to earn the right to see the inner sanctum.  Failure will be met with certain death.\"\n");  
            Console.WriteLine("As you read the tablet the room around you begins to shimmer and change.  The pedestal slides down into the floor and disappears.\n");
            Console.WriteLine("To your left there now appears a row of levers.");
            Console.WriteLine("To your right there is now a large spoked wheel.");
            Console.WriteLine("In front of you there is now a closed door.");
            gamePosition = 2;
        }

        static void GoToLevers(String dummy)
        {
            if (isNearLevers)
            {
                Console.WriteLine("You are already at the row of levers.\n");
            }
            else
            {
                Console.WriteLine("You walk over to the row of levers.\n");
                isNearWheel = false;
                isNearDoor = false;
                isNearLevers = true;
            }
            Console.WriteLine("There are three levers with symbols above them.  One looks like lightning, one like rain, and another looks like the sun.\n");
        }

        static void PullLever(String dummy)
        {
            if (!isNearLevers)
            {
                Console.WriteLine("You can't pull any levers from over here.\n");
            }
            else
            {
                Console.WriteLine("I don't know which lever you are trying to pull.\n");
            }
        }

        static void PullLightning(String dummy)
        {
            if (!isNearLevers)
            {
                Console.WriteLine("You can't pull the lightning lever from over here.\n");
            }
            else
            {
                Console.WriteLine("You pull the lever with the lightning symbol.\n");
                Console.WriteLine("The hair on your arms stands on edge as you feel an electric charge building up inside the room...\n");
                Console.WriteLine("ZAAAAAAAAP! Uh oh, you were struck by a lightning bolt and died.\n");
                Console.WriteLine("Sorry, game over.\n");
                gameOver = true;
            }
        }

        static void PullRain(String dummy)
        {
            if (!isNearLevers)
            {
                Console.WriteLine("You can't pull the rain lever from over here.\n");
            }
            else
            {
                Console.WriteLine("You pull the lever with the rain symbol.\n");
                Console.WriteLine("You feel something wet drip onto your arms, and OUCH that burns!\n");
                Console.WriteLine("The liquid starts falling faster from above and you glance around frantically to find cover.  There isn’t any, and you die from acid burns\n");
                Console.WriteLine("Sorry, game over.\n");
                gameOver = true;
            }
        }

        static void PullSun(String dummy)
        {
            if (!isNearLevers)
            {
                Console.WriteLine("You can't pull the sun lever from over here.\n");
            }
            else
            {
                Console.WriteLine("You pull the lever with the sun symbol.\n");
                Console.WriteLine("The room suddenly seems brighter and you hear a latch releasing somewhere in the room\n");
                isSunLeverPulled = true;
            }
        }

        static void GoToWheel(String dummy)
        {
            if (isNearWheel)
            {
                Console.WriteLine("You are already at the wheel.\n");
            }
            else
            {
                Console.WriteLine("You walk over to the wheel.\n");
                isNearWheel = true;
                isNearDoor = false;
                isNearLevers = false;
            }
            Console.WriteLine("There is a large symbol in the center of the wheel that looks like the sun.\n");
        }

        static void TurnWheel(String dummy)
        {
            if (!isNearWheel)
            {
                Console.WriteLine("You can't turn the wheel from over here.\n");
            }
            else if (!isSunLeverPulled)
            {
                Console.WriteLine("The wheel doesn’t budge.  Something seems to be holding it in place.\n");
            }
            else
            {
                Console.WriteLine("The wheel is slow to turn at first but then it moves easily and you hear a loud click as of a latch releasing.\n");
                isWheelTurned = true;
            }
        }

        static void GoToDoor(String dummy)
        {
            if (isNearDoor)
            {
                Console.WriteLine("You are already at the door.\n");
            }
            else
            {
                Console.WriteLine("You walk over to the door.\n");
                isNearWheel = false;
                isNearDoor = true;
                isNearLevers = false;
            }
            Console.WriteLine("There is no handle or knob, it looks pretty solid.\n");
        }

        static void OpenDoor(String dummy)
        {
            if (!isNearDoor)
            {
                Console.WriteLine("You can't open the door from over here.\n");
            }
            else if (!isWheelTurned)
            {
                Console.WriteLine("You shove the door but it doesn’t budge.  Something seems to be holding it in place.\n");
            }
            else
            {
                Console.WriteLine("The door pushes open easily and you walk through.\n");
                Console.WriteLine("You find yourself in another room.  You see a table in the center of the room with several keys on it.\n");
                Console.WriteLine("On the far side of the room is a closed door.\n");
                gamePosition = 3;
            }
        }

        static void GoToKeys(String dummy)
        {
            if (isNearKeys)
            {
                Console.WriteLine("You are already at the table with the keys.\n");
            }
            else
            {
                Console.WriteLine("You walk over to the table with the keys.\n");
                isNearKeys = true;
                isNearDoorTwo = false;
            }
            Console.WriteLine("There are three keys on the table.\n");
            Console.WriteLine("Looking at them closer you can see one is engraved with the symbol of a hawk, one with a lion, and one with a shark.\n");
        }

        static void TakeHawkKey(String dummy)
        {
            if (!isNearKeys)
            {
                Console.WriteLine("You can't take the hawk key from over here.\n");
            }
            else
            {
                Console.WriteLine("You take the key with the hawk symbol and put it in your pocket.\n");
                hasHawkKey = true;
            }
        }

        static void TakeSharkKey(String dummy)
        {
            if (!isNearKeys)
            {
                Console.WriteLine("You can't take the shark key from over here.\n");
            }
            else
            {
                Console.WriteLine("You take the key with the shark symbol and put it in your pocket.\n");
                hasSharkKey = true;
            }
        }

        static void TakeLionKey(String dummy)
        {
            if (!isNearKeys)
            {
                Console.WriteLine("You can't take the lion key from over here.\n");
            }
            else
            {
                Console.WriteLine("You take the key with the lion symbol and put it in your pocket.\n");
                hasLionKey = true;
            }
        }

        static void UseHawkKey(String dummy)
        {
            if (!hasHawkKey)
            {
                Console.WriteLine("You can't use the hawk key, you don't have it.\n");
            }
            else if (!isNearDoorTwo)
            {
                Console.WriteLine("You can't use the hawk key from over here.\n");
            }
            else
            {
                Console.WriteLine("You take the key with the hawk symbol and put it in one of the door locks and turn it.\n");
                Console.WriteLine("You hear a click as of a latch releasing.\n");
                isHawkKeySuccess = true;
            }
        }

        static void UseSharkKey(String dummy)
        {
            if (!hasSharkKey)
            {
                Console.WriteLine("You can't use the shark key, you don't have it.\n");
            }
            else if (!isNearDoorTwo)
            {
                Console.WriteLine("You can't use the shark key from over here.\n");
            }
            else
            {
                Console.WriteLine("You take the key with the Shark symbol and put it in one of the door locks and turn it.\n");
                Console.WriteLine("You hear a click as of a latch releasing.\n");
                isSharkKeySuccess = true;
            }
        }

        static void UseLionKey(String dummy)
        {
            if (!hasLionKey)
            {
                Console.WriteLine("You can't use the lion key, you don't have it.\n");
            }
            else if (!isNearDoorTwo)
            {
                Console.WriteLine("You can't use the lion key from over here.\n");
            }
            else
            {
                Console.WriteLine("You take the key with the lion symbol and put it in one of the door locks and turn it.\n");
                Console.WriteLine("At first nothing happens.  Then you hear an enormous RRRROAR.\n");
                Console.WriteLine("You spin around only to be face to face with a gigantic lion!  CHOMP! He eats you.\n");
                Console.WriteLine("Sorry, game over.\n");
                gameOver = true;
            }
        }

        static void GoToDoorTwo(String dummy)
        {
            if (isNearDoorTwo)
            {
                Console.WriteLine("You are already at the door.\n");
            }
            else
            {
                Console.WriteLine("You walk over to the door.\n");
                isNearKeys = false;
                isNearDoorTwo = true;
            }
            Console.WriteLine("There are two large locks.\n");
            Console.WriteLine("There are two images on the door, one appears to be a mouse and the other a fish.\n");
        }

        static void OpenDoorTwo(String dummy)
        {
            if (!isNearDoorTwo)
            {
                Console.WriteLine("You can't open the door from over here.\n");
            }
            else if (!isHawkKeySuccess || !isSharkKeySuccess)
            {
                Console.WriteLine("You shove the door but it doesn’t budge.  Something seems to be holding it in place.\n");
            }
            else
            {
                Console.WriteLine("The door pushes open easily and you walk through.\n");
                Console.WriteLine("You find yourself in magnificent room plated in gold.\n");
                Console.WriteLine("Congratulations! You have reached the inner sanctum.\n");
                gameOver = true;
            }
        }
    }
}
