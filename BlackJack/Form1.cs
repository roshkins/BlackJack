using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections;
using System.Resources;
namespace BlackJack
{
    public partial class Form1 : Form
    {
        private Bitmap BACK_OF_CARD = BlackJack.Properties.Resources.back;
        private float ASPECT_RATIO_OF_CARDS;
        private Bitmap drawingSurface = null;
        List<string> userCards, dealerCards, deck;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            isShowingSecondCard = false;
            this.DoubleBuffered = true;
            ASPECT_RATIO_OF_CARDS = (float)getCard("ac").Height / (float)getCard("ac").Width;
            deck = shuffledDeck();

            userCards = new List<string>();
            dealerCards = new List<string>();

            dealCards(ref userCards, ref dealerCards, ref deck);

            redrawScreen();

            if (checkIfUserBlackjacked())
            {
                displayMessage("Congrats! You blackjacked!");
                lblWins.Text = (int.Parse(lblWins.Text) + 1).ToString();
                Form1_Load(sender, e);
            }

        }

        private bool checkIfUserBlackjacked()
        {
            if (userCards.Count == 2)
            {
                int cardValues = 0;
                foreach (string card in userCards)
                {
                    int[] possibleVals = getCardValue(card);
                    if (possibleVals.Length == 2)
                    {
                        cardValues += 11;
                    }
                    else
                    {
                        cardValues += possibleVals[0];
                    }
                }
                return cardValues == 21;
            }
            return false;
        }



        private Bitmap drawCards(List<string> userCards, List<string> dealerCards)
        {
            Bitmap table = new Bitmap(this.Width, this.Height);
            Graphics g = Graphics.FromImage(table);
            g.FillRectangle(Brushes.DarkGreen, 0, 0, table.Width, table.Height);
            g.DrawRectangle(Pens.Black, 0, 0, table.Width, table.Height);
            int xOffset = 0;
            int yOffset = 0;
            int interCardDistance = 5;
            g.DrawString("Dealer:", DefaultFont, Brushes.White, new Point(xOffset, yOffset));

            int cardWidth = getCardWidth((int)g.MeasureString("User:", DefaultFont).Width, (int)g.MeasureString("Dealer:", DefaultFont).Width, 5);
            int cardHeight = (int)(cardWidth * ASPECT_RATIO_OF_CARDS);

            xOffset = (int)g.MeasureString("Dealer:", DefaultFont).Width;
            drawCard(ref g, xOffset, 0, cardWidth, cardHeight, dealerCards[0]);
            xOffset += cardWidth + interCardDistance;

            if (!isShowingSecondCard)
                drawCard(ref g, xOffset, 0, cardWidth, cardHeight, "back");
            else
                drawCard(ref g, xOffset, 0, cardWidth, cardHeight, dealerCards[1]);
            xOffset += cardWidth + interCardDistance;

            for (int i = 2; i < dealerCards.Count; i++)
            {
                drawCard(ref g, xOffset, 0, cardWidth, cardHeight, dealerCards[i]);
                xOffset += cardWidth + 5;
            }

            xOffset = 0;
            yOffset = cardHeight + 10;

            g.DrawString("User:", DefaultFont, Brushes.White, new Point(xOffset, yOffset));

            xOffset += (int)g.MeasureString("User:", DefaultFont).Width;

            foreach (string card in userCards)
            {
                drawCard(ref g, xOffset, yOffset, cardWidth, cardHeight, card);
                xOffset += cardWidth + 5;
            }
            return table;
        }

        private int getCardWidth(int userStringWidth, int dealerStringWidth, int interCardDistance = 0)
        {
            int possibleCardWidth = (int)(((this.Height - statusStrip.Height) / 2.3) / ASPECT_RATIO_OF_CARDS);
            int leftMargin = ((userStringWidth > dealerStringWidth) ? userStringWidth : dealerStringWidth);
            if (userCards.Count * possibleCardWidth < this.Width - leftMargin)
                return possibleCardWidth;
            else
                return (this.Width - (leftMargin + interCardDistance * userCards.Count)) / (userCards.Count);
        }

        private void drawCard(ref Graphics g, int x, int y, int width, int height, string card)
        {
            g.DrawImage(getCard(card), new RectangleF(x, y, width, height));
        }

        private void dealCards(ref List<string> userCards, ref List<string> dealerCards, ref List<string> deck)
        {
            dealCard(ref userCards, ref deck);
            dealCard(ref userCards, ref deck);

            dealCard(ref dealerCards, ref deck);
            dealCard(ref dealerCards, ref deck);

        }

        private void dealCard(ref List<string> cardPile, ref List<string> deck)
        {
            if (deck.Count > 0)
            {
                cardPile.Add(deck[0]);
                deck.Remove(deck[0]);
            }
        }

        private List<string> shuffledDeck()
        {
            List<string> deckOrdered = getOrderedDeck();
            List<string> deckRandomized = new List<string>();
            Random r = new Random(DateTime.Now.Millisecond);
            while (deckOrdered.Count > 0)
            {
                int randNum = r.Next(deckOrdered.Count);
                deckRandomized.Add(deckOrdered[randNum]);
                deckOrdered.Remove(deckOrdered[randNum]);
            }
            return deckRandomized;
        }
        private List<string> getOrderedDeck()
        {
            string suits = "chsd";
            string[] cards = { "a", "2", "3", "4", "5", "6", "7", "8", "9", "10", "j", "q", "k" };
            List<string> orderedDeck = new List<string>();
            foreach (char suit in suits)
                foreach (string card in cards)
                {
                    orderedDeck.Add((string)card + suit);
                }
            return orderedDeck;
        }

        /// <summary>
        /// Gets the image associated to a given card.  
        /// </summary>
        /// <param name="cardString"> A card in \d\d\w (numbers) or \w\w (faces and aces) format.</param>
        /// <returns> A bitmap image of the card.</returns>
        private Bitmap getCard(string cardString)
        {
            Match numCard = Regex.Match(cardString, "^(\\d?\\d)(\\w)$");
            Match faceOrAceCard = Regex.Match(cardString, "^(\\w)(\\w)$");
            if (numCard.Success)
            {
                string resourceName = "_" + int.Parse(numCard.Groups[1].Value).ToString() + "_of_";
                return determineSuitAndResource(numCard.Groups[2].Value, resourceName);

            }
            else if (faceOrAceCard.Success)
            {
                Hashtable valueOfCard = new Hashtable();
                valueOfCard.Add("a", "ace");
                valueOfCard.Add("j", "jack");
                valueOfCard.Add("q", "queen");
                valueOfCard.Add("k", "king");

                string resourceName = valueOfCard[faceOrAceCard.Groups[1].Value] + "_of_";

                return determineSuitAndResource(faceOrAceCard.Groups[2].Value, resourceName, "2");
            }
            else if (cardString == "back")
            {
                return BACK_OF_CARD;
            }
            else
            {
                throw new Exception("Card format not recognized. Couldn't understand \"" + cardString + "\".");
            }
        }
        private int[] getCardValue(string cardString)
        {
            Match numCard = Regex.Match(cardString, "^(\\d?\\d)(\\w)$");
            Match faceOrAceCard = Regex.Match(cardString, "^(\\w)(\\w)$");
            if (numCard.Success)
            {
                int[] getCardValue = { int.Parse(numCard.Groups[1].Value) };
                return getCardValue;

            }
            else if (faceOrAceCard.Success)
            {
                if (faceOrAceCard.Groups[1].Value == "a")
                {
                    int[] retval = { 1, 11 };
                    return retval;
                }
                else
                {
                    int[] retval = { 10 };
                    return retval;
                }
            }
            else
            {
                throw new Exception("Card format not recognized. Couldn't understand \"" + cardString + "\".");
            }
        }
        private Bitmap determineSuitAndResource(string suit, string resourceName, string suffix = "")
        {
            if (suit.Length != 1) throw new Exception("Suit not one character.");
            Hashtable suits = new Hashtable();
            suits.Add("c", "clubs");
            suits.Add("h", "hearts");
            suits.Add("s", "spades");
            suits.Add("d", "diamonds");
            resourceName += suits[suit] + suffix;
            Bitmap retval = (Bitmap)BlackJack.Properties.Resources.ResourceManager.GetObject(resourceName);
            if (retval == null)
                throw new Exception("Card image is null!");
            else
                return retval;
        }
        private void displayMessage(string welcomeMessage)
        {
            MessageBox.Show(welcomeMessage);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (drawingSurface != null)
                e.Graphics.DrawImageUnscaledAndClipped(drawingSurface, new Rectangle(0, 0, drawingSurface.Width, drawingSurface.Height));
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (userCards != null && dealerCards != null)
                redrawScreen();

        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'h':
                    userHit();
                    break;
                case 's':
                    userStand();
                    break;
            }
        }

        private void userStand()
        {
            int userTotal = getCardVals(userCards);
            //displayMessage("Your total: " + userTotal);
            dealerPlay(userTotal);
        }
        bool isShowingSecondCard = false; //Toggles whether second card is shown.
        private void dealerPlay(int userTotal)
        {
            isShowingSecondCard = true;
            redrawScreen();
            do
            {
                int runningTotal = getCardVals(dealerCards);
                if (runningTotal > 21)
                {
                    displayMessage("Dealer busts!");
                    break;
                }
                else if (runningTotal < 17)
                {
                    dealCard(ref dealerCards, ref deck);
                    redrawScreen();
                }
                else
                    break;
            } while (true);

            int finalTotal = getCardVals(dealerCards);

            if (finalTotal < userTotal || finalTotal > 21)
            {
                displayMessage("You win! You have: " + userTotal + ".\nDealer had: " + finalTotal + ".");
                lblWins.Text = (int.Parse(lblWins.Text) + 1).ToString();
            }
            else if (finalTotal > userTotal || userTotal > 21)
            {
                displayMessage("Sorry, you lost. Try again. You had: " + userTotal + ".\nDealer had: " + finalTotal + ".");
                lblLoses.Text = (int.Parse(lblLoses.Text) + 1).ToString();
            }
            else
            {
                displayMessage("You tied! You both had: " + userTotal + ".");
                lblWins.Text = (int.Parse(lblWins.Text) + 1).ToString();
                lblLoses.Text = (int.Parse(lblLoses.Text) + 1).ToString();
            }


            Form1_Load(null, null);

        }


        private void redrawScreen()
        {
            drawingSurface = drawCards(userCards, dealerCards);
            this.Refresh();
        }
        private void userHit()
        {
            dealCard(ref userCards, ref deck);
            redrawScreen();

            int cardValues = getCardVals(userCards);

            if (cardValues > 21)
            {
                isShowingSecondCard = true;
                redrawScreen();
                displayMessage("Bust!");
                lblLoses.Text = (int.Parse(lblLoses.Text) + 1).ToString();
                Form1_Load(null, null);
            }
            else if (cardValues == 21)
                userStand();

        }
        private int getCardVals(List<string> cardList)
        {
            int aces = 0;
            int cardValues = 0;
            foreach (string card in cardList)
            {
                int[] possibleCardVals = getCardValue(card);
                if (possibleCardVals.Length == 1)
                {
                    //Not an ace.
                    cardValues += possibleCardVals[0];
                }
                else
                {
                    //Is an ace.
                    aces++;
                }
            }
            for (int i = 1; i <= aces; i++)
            {
                if (21 - cardValues >= 11)
                    cardValues += 11; //If greater then 11, count ace as 11.
                else
                    cardValues += 1; //Else we don't want to bust, so only add 1.
            }
            return cardValues;

        }
    }
}

