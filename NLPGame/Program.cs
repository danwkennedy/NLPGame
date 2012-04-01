using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        //Global state variables
        static int gamePosition = 0;
        static bool gameOver = false;

        // the list of valid noun/verb pairs for a given point in the game, i.e. at the very beginning you can only do one action, 
        // at the next step you can only do one action, but at the third point there are many valid actions
        static List<NounVerbPair>[] validActionsByGamePosition;

        // Main function, repeatedly lets the user play the game until they choose not to
        static void Main(string[] args)
        {
            setupGame();
            bool playAgain = true;
            while (playAgain)
            {
                Console.Clear();
                resetGameState();
                playGame();
                Console.WriteLine("\nWould you like to play again? (Yes or No)\n");
                String yesOrNo = Console.ReadLine();
                playAgain = yesOrNo.Equals("Yes") || yesOrNo.Equals("yes") || yesOrNo.Equals("Y") || yesOrNo.Equals("y");
            }
        }

        static void resetGameState()
        {
            isNearLevers = false;
            isNearDoor = false;
            isNearWheel = false;
            isSunLeverPulled = false;
            isWheelTurned = false;
            gamePosition = 0;
            gameOver = false;
        }

        static void setupGame()
        {
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
            validActionsByGamePosition[2].Add(new NounVerbPair("lever", "pull", PullLever));
            validActionsByGamePosition[2].Add(new NounVerbPair("rain", "pull", PullRain));
            validActionsByGamePosition[2].Add(new NounVerbPair("lightning", "pull", PullLightning));
            validActionsByGamePosition[2].Add(new NounVerbPair("sun", "pull", PullSun));
            validActionsByGamePosition[2].Add(new NounVerbPair("door", "open", OpenDoor));
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
            String userInput = Console.ReadLine();

            //Make calls into NLP engine here to get the noun and verb matching the user's input,  we should repeat prompting until the user provides a valid input

            //TODO temporaryily expect an integer for testing purposes
            int option = Convert.ToInt32(userInput);
            Console.Clear();
            validPairs[option].action("dummy");
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
            Console.WriteLine("There is a large symbol in the center of the wheel that looks like the sun.\n");
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
                
                // NEXT ROOM if we implement another one, otherwise gameOver = true and we should tell them they reached the inner sanctum
                gameOver = true;
            }
        }
    }

    class NounVerbPair
    {
        public String noun;
        public String verb;
        public Action<String> action;
        public NounVerbPair(String theNoun, String theVerb, Action<String> theAction)
        {
            noun = theNoun;
            verb = theVerb;
            action = theAction;
        }
    }
}
