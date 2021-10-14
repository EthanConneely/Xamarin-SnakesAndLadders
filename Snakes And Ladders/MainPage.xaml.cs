using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Snakes_And_Ladders
{
    public partial class MainPage : ContentPage
    {
        // Finish Cell
        const int FinishCell = 99;

        // The time in milliseconds to animate over a single cell 1000 = 1 second
        const int AnimateTime = 150;

        // random number generator for used as the dice roll
        Random rng;

        // can be 2 3 or 4
        // 1 player snake is depressing
        int playersPlaying;

        // selects which int to use in the playersPoition array
        int currentPlayer;

        // Stores the current cell that each player is on the board
        int[] playersPoition;

        // A dictonary is like an array but it can contain a key(int, string etc) which will return a value (int in our case but can be anything)
        // the way this works is if we want to know where the first snake goes to we do this
        // snakes[7] this will return 3
        // if we want to know where the ladder at cell 16 goes do this
        // snakes[48] this will return 66
        // in a dictonary the keys have to be unique meaning you can have 2 50s but values can be the same
        readonly Dictionary<int, int> snakes = new Dictionary<int, int>()
        {
            {7 , 3 },// our example
            {17, 0 },
            {25, 9},
            {38, 4 },
            {50, 5 },
            {53, 35},
            {55, 0 },
            {59, 22},
            {74, 27},
            {82, 44},
            {84, 58},
            {89, 47},
            {91, 24},// Mean
            {96, 86},
            {98, 62},
        };

        // The numbers are the cell number -1 as we start from zero not 1
        // eg cell 1 is actually 0 cell 100 is 99 etc
        readonly Dictionary<int, int> ladders = new Dictionary<int, int>()
        {
            {2,  19},
            {5,  13},
            {10, 27},
            {14, 33},
            {16, 73},// Awesome
            {21, 36},
            {37, 58},
            {48, 66},// our other example
            {56, 75},
            {60, 77},
            {72, 85},
            {80, 97},
            {87, 90},
        };

        // this is run when the page is created
        public MainPage()
        {
            InitializeComponent();
        }

        // This is a method that is called when the page is loaded
        protected override void OnAppearing()
        {
            ResetGame();
        }

        private async void ResetGame()
        {
            rng = new Random();

            // Reset the players positions
            playersPoition = new int[4] { 0, 0, 0, 0 };

            // Make player 1 start first again
            currentPlayer = 0;

            // make sure the dice is enabled
            diceButton.IsEnabled = true;

            // display which players turn it is on the label remember agian current player goes from 0-3
            currentTurnText.Text = $"Player {currentPlayer + 1}'s Turn";

            string action;

            // We ask the user how many people are playing in a popup
            do
            {
                action = await DisplayActionSheet("How many players?", null, null, "One", "Two", "Three", "Four");
                switch (action)
                {
                    case "One":
                        playersPlaying = 1;
                        break;
                    case "Two":
                        playersPlaying = 2;
                        break;
                    case "Three":
                        playersPlaying = 3;
                        break;
                    case "Four":
                        playersPlaying = 4;
                        break;
                }
            } while (string.IsNullOrEmpty(action));

            // Hide all players and then only enable the ones playing
            player1.IsVisible = false;
            player2.IsVisible = false;
            player3.IsVisible = false;
            player4.IsVisible = false;

            player1.TranslationX = 0;
            player1.TranslationY = 0;
            player2.TranslationX = 0;
            player2.TranslationY = 0;
            player3.TranslationX = 0;
            player3.TranslationY = 0;
            player4.TranslationX = 0;
            player4.TranslationY = 0;

            // make the players visable
            if (playersPlaying >= 1)
            {
                player1.IsVisible = true;
            }

            if (playersPlaying >= 2)
            {
                player2.IsVisible = true;
            }

            if (playersPlaying >= 3)
            {
                player3.IsVisible = true;
            }

            if (playersPlaying >= 4)
            {
                player4.IsVisible = true;
            }
        }

        private async void OnDiceClicked(object sender, EventArgs e)
        {
            // stop being able to press the button again 
            // until all the animations and movement is done
            diceButton.IsEnabled = false;

            // Cast the button to an image button
            ImageButton button = (ImageButton)sender;

            // We use the rng to get the number on the dice we rolled the number starts at 1 and goes up to 7 because 7 isnt included in the rng
            int roll = rng.Next(1, 7);

            // this could be done with a normal button but i think it looks better clicking on a dice plus
            // the value on the dice is then displayed for what you rolled

            // this is because android cannot allow numbers in its name it would be alot easier if it did
            // this is just an array of strings in order so when we roll a one it is passed and return the string "one"
            string[] numbersInTextForm = new string[] { "one", "two", "three", "four", "five", "six" };
            button.Source = "dice" + numbersInTextForm[roll - 1] + ".png";


            // Handle the main logic of snakes and ladders
            await HandlePlayersTurn(roll);

            // display which players turn it is on the label remember agian current player goes from 0-3
            currentTurnText.Text = $"Player {currentPlayer + 1}'s Turn";

            // enable the dice again
            diceButton.IsEnabled = true;
        }

        private async Task HandlePlayersTurn(int roll)
        {
            bool bounceBack = false;

            for (int i = 0; i < roll; i++)
            {
                // Bounce back off end of the board
                if (playersPoition[0] == FinishCell && bounceBack == false)
                {
                    bounceBack = true;
                }
                playersPoition[currentPlayer] += bounceBack ? -1 : 1;
                Debug.WriteLine("Moved to index " + playersPoition[currentPlayer]);
                // Animate moving the player
                await MovePlayer();
            }

            // Check if the player landed on a snake or a ladder
            if (ladders.ContainsKey(playersPoition[currentPlayer]))// On a ladder :)
            {
                playersPoition[currentPlayer] = ladders[playersPoition[currentPlayer]];

                // Animate Moving Up the ladder seperately to just a normal move other wise
                // the piece would move directly to the top of the ladder and look like
                // its cheating 
                await MovePlayer();
                Debug.WriteLine("Done ladders");
            }
            else if (snakes.ContainsKey(playersPoition[currentPlayer]))// On a snake :'(
            {
                playersPoition[currentPlayer] = snakes[playersPoition[currentPlayer]];

                // Animate Moving down the snakes for the same reason as the ladder
                await MovePlayer();
                Debug.WriteLine("Done snakes");
            }

            // Check if the play has landed exactly on the finishing cell to win
            if (playersPoition[currentPlayer] == FinishCell)
            {
                string title = $"Player {currentPlayer + 1} won!";
                string message = $"Player {currentPlayer + 1} has won the game\nDo you want to play another game";
                await DisplayAlert(title, message, "Play Again");

                ResetGame();

                // Return so we keep whos turn it was when they won
                return;
            }

            // Next player and loop back around
            currentPlayer += 1;
            if (currentPlayer >= playersPlaying)
            {
                currentPlayer = 0;
            }
        }

        // TASK is needed so you can call an await from within an await function
        async Task MovePlayer()
        {
            int cellIndex = playersPoition[currentPlayer];

            // this is the equivalent to his weird move horizontal and vertical methods

            // Calculate grids x position from the cell index
            int x = cellIndex % 10;

            // this checks to see if we are on a row the goes from right to left and flips the x position
            // because % 10 would make the x position jump back to the far left when we want it to start from the right
            if (cellIndex % 20 >= 10)
            {
                x = 9 - x;// Snake back and forth on the board
            }

            // calculates the grid y position
            int y = cellIndex / 10;

            // this converts the board x and y position to relative positions to translate to and from
            double hDistance = gameBoard.X + (gameBoard.Width / 10 * x);
            double vDistance = gameBoard.Y - (gameBoard.Height / 10 * y);

            var player = gameBoard.FindByName<Image>("player" + (currentPlayer + 1));

            // Change the visuals depending on which 
            await player.TranslateTo(hDistance, vDistance, AnimateTime);
        }
    }
}
