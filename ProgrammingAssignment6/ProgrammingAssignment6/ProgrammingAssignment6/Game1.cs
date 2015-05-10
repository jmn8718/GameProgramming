using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using XnaCards;

namespace ProgrammingAssignment6
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int WINDOW_WIDTH = 800;
        const int WINDOW_HEIGHT = 600;

        // max valid blackjack score for a hand
        const int MAX_HAND_VALUE = 21;

        // deck and hands
        Deck deck;
        List<Card> dealerHand = new List<Card>();
        List<Card> playerHand = new List<Card>();

        // hand placement
        const int TOP_CARD_OFFSET = 100;
        const int HORIZONTAL_CARD_OFFSET = 150;
        const int VERTICAL_CARD_SPACING = 125;

        // messages
        SpriteFont messageFont;
        const string SCORE_MESSAGE_PREFIX = "Score: ";
        Message playerScoreMessage;
        List<Message> messages = new List<Message>();

        // message placement
        const int SCORE_MESSAGE_TOP_OFFSET = 25;
        const int HORIZONTAL_MESSAGE_OFFSET = HORIZONTAL_CARD_OFFSET;
        Vector2 winnerMessageLocation = new Vector2(WINDOW_WIDTH / 2,
            WINDOW_HEIGHT / 2);

        // menu buttons
        Texture2D quitButtonSprite;
        List<MenuButton> menuButtons = new List<MenuButton>();

        // menu button placement
        const int TOP_MENU_BUTTON_OFFSET = TOP_CARD_OFFSET;
        const int QUIT_MENU_BUTTON_OFFSET = WINDOW_HEIGHT - TOP_CARD_OFFSET;
        const int HORIZONTAL_MENU_BUTTON_OFFSET = WINDOW_WIDTH / 2;
        const int VERTICAL_MENU_BUTTON_SPACING = 125;

        // use to detect hand over when player and dealer didn't hit
        bool playerHit = false;
        bool dealerHit = false;

        // game state tracking
        static GameState currentState = GameState.WaitingForPlayer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution and show mouse
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // create and shuffle deck
            deck = new Deck(Content, 0, 0);
            deck.Shuffle();
            // first player card
            Card card = deck.TakeTopCard();
            card.FlipOver();
            card.X = HORIZONTAL_CARD_OFFSET;
            card.Y = TOP_CARD_OFFSET + (VERTICAL_CARD_SPACING * playerHand.Count);
            playerHand.Add(card);

            // first dealer card
            card = deck.TakeTopCard();
            card.X = WINDOW_WIDTH - HORIZONTAL_CARD_OFFSET;
            card.Y = TOP_CARD_OFFSET + (VERTICAL_CARD_SPACING * dealerHand.Count);
            dealerHand.Add(card);

            // second player card
            card = deck.TakeTopCard();
            card.FlipOver();
            card.X = HORIZONTAL_CARD_OFFSET;
            card.Y = TOP_CARD_OFFSET + (VERTICAL_CARD_SPACING * playerHand.Count);
            playerHand.Add(card);

            // second dealer card
            card = deck.TakeTopCard();
            card.FlipOver();
            card.X = WINDOW_WIDTH - HORIZONTAL_CARD_OFFSET;
            card.Y = TOP_CARD_OFFSET + (VERTICAL_CARD_SPACING * dealerHand.Count);
            dealerHand.Add(card);

            // load sprite font, create message for player score and add to list
            messageFont = Content.Load<SpriteFont>("Arial24");
            playerScoreMessage = new Message(SCORE_MESSAGE_PREFIX + GetBlackjackScore(playerHand).ToString(),
                messageFont,
                new Vector2(HORIZONTAL_MESSAGE_OFFSET, SCORE_MESSAGE_TOP_OFFSET));
            messages.Add(playerScoreMessage);

            // load quit button sprite for later use
            quitButtonSprite = Content.Load<Texture2D>("quitbutton");

            // create hit button and add to list
            MenuButton hitButton = new MenuButton(
                Content.Load<Texture2D>("hitbutton"),
                new Vector2(HORIZONTAL_MENU_BUTTON_OFFSET, TOP_MENU_BUTTON_OFFSET),
                GameState.PlayerHitting);
            menuButtons.Add(hitButton);
            // create stand button and add to list
            MenuButton standButton = new MenuButton(
                Content.Load<Texture2D>("standbutton"),
                new Vector2(HORIZONTAL_MENU_BUTTON_OFFSET, TOP_MENU_BUTTON_OFFSET + VERTICAL_MENU_BUTTON_SPACING),
                GameState.WaitingForDealer);
            menuButtons.Add(standButton);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MouseState mouse = Mouse.GetState();
            // update menu buttons as appropriate
            if (currentState == GameState.WaitingForPlayer)
            {
                foreach (MenuButton button in menuButtons)
                {
                    button.Update(mouse);
                }
            }
            else if (currentState == GameState.DisplayingHandResults)
            {
                foreach (MenuButton button in menuButtons)
                {
                    button.Update(mouse);
                }
            }

            // game state-specific processing
            switch (currentState)
            {
                case GameState.PlayerHitting:
                    Card cardPlayer = deck.TakeTopCard();
                    cardPlayer.FlipOver();
                    cardPlayer.X = HORIZONTAL_CARD_OFFSET;
                    cardPlayer.Y = TOP_CARD_OFFSET + (VERTICAL_CARD_SPACING * playerHand.Count);
                    playerHand.Add(cardPlayer);
                    playerScoreMessage.Text = SCORE_MESSAGE_PREFIX + GetBlackjackScore(playerHand).ToString();
                    playerHit = true;
                    Game1.ChangeState(GameState.WaitingForDealer);
                    break;
                case GameState.WaitingForDealer:
                    if (GetBlackjackScore(dealerHand) <= 16)
                    {
                        Game1.ChangeState(GameState.DealerHitting);
                    }
                    else
                    {

                        Game1.ChangeState(GameState.CheckingHandOver);
                    }
                    break;
                case GameState.DealerHitting:
                    Card cardDealer = deck.TakeTopCard();
                    cardDealer.FlipOver();
                    cardDealer.X = WINDOW_WIDTH - HORIZONTAL_CARD_OFFSET;
                    cardDealer.Y = TOP_CARD_OFFSET + (VERTICAL_CARD_SPACING * dealerHand.Count);
                    dealerHand.Add(cardDealer);
                    dealerHit = true;
                    Game1.ChangeState(GameState.CheckingHandOver);
                    break;
                case GameState.CheckingHandOver:
                    int playerScore = GetBlackjackScore(playerHand);
                    int dealerScore =  GetBlackjackScore(dealerHand);
                    if ( playerScore > MAX_HAND_VALUE ||
                        dealerScore > MAX_HAND_VALUE ||
                        (!playerHit && !dealerHit))
                    {
                        String resultText; 
                        
                        //Player and dealer busted
                        if(playerScore > MAX_HAND_VALUE &&
                             dealerScore > MAX_HAND_VALUE)
                        {
                            resultText = "It's a tie";
                        }
                        //Dealer busted
                        else if (dealerScore > MAX_HAND_VALUE) 
                        {
                            resultText = "Player won";
                        }
                        //Player busted 
                        else if(playerScore > MAX_HAND_VALUE)
                        {
                            resultText = "Dealer won";
                        }
                        // No one was busted, so we check the scores
                        else if(playerScore > dealerScore)
                        {
                            resultText = "Player won";
                        }
                        else if(playerScore < dealerScore)
                        {
                            resultText = "Dealer won";
                        }
                        else //playerScore == dealerScore
                        {
                            resultText = "It's a tie";
                        }
                        messageFont = Content.Load<SpriteFont>("Arial24");
                        Message resultMessage = new Message(resultText,
                            messageFont,
                            winnerMessageLocation);
                        messages.Add(resultMessage);
                        //Flip over the dealer first card
                        dealerHand[0].FlipOver();
                        //Create message for dealer score
                        Message dealerScoreMessage = new Message(SCORE_MESSAGE_PREFIX + GetBlackjackScore(dealerHand).ToString(),
                            messageFont,
                            new Vector2(WINDOW_WIDTH - HORIZONTAL_MESSAGE_OFFSET, SCORE_MESSAGE_TOP_OFFSET));
                        messages.Add(dealerScoreMessage);
                        //remove hit and stand buttons
                        menuButtons.Clear();
                        //Add quit button
                        MenuButton hitButton = new MenuButton(
                            quitButtonSprite,
                            new Vector2(HORIZONTAL_MENU_BUTTON_OFFSET, QUIT_MENU_BUTTON_OFFSET),
                            GameState.Exiting);
                        menuButtons.Add(hitButton);
                        //change game state
                        Game1.ChangeState(GameState.DisplayingHandResults);
                    }
                    else
                    {
                        playerHit = false;
                        dealerHit = false;
                        Game1.ChangeState(GameState.WaitingForPlayer);
                    }
                    break;
                case GameState.Exiting:
                    this.Exit();
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Goldenrod);

            spriteBatch.Begin();
            // draw hands
            foreach (Card card in playerHand)
            {
                card.Draw(spriteBatch);
            }
            foreach (Card card in dealerHand)
            {
                card.Draw(spriteBatch);
            }
            // draw messages
            foreach (Message message in messages)
            {
                message.Draw(spriteBatch);
            }

            // draw menu buttons
            foreach (MenuButton button in menuButtons)
            {
                button.Draw(spriteBatch);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Calculates the Blackjack score for the given hand
        /// </summary>
        /// <param name="hand">the hand</param>
        /// <returns>the Blackjack score for the hand</returns>
        private int GetBlackjackScore(List<Card> hand)
        {
            // add up score excluding Aces
            int numAces = 0;
            int score = 0;
            foreach (Card card in hand)
            {
                if (card.Rank != Rank.Ace)
                {
                    score += GetBlackjackCardValue(card);
                }
                else
                {
                    numAces++;
                }
            }

            // if more than one ace, only one should ever be counted as 11
            if (numAces > 1)
            {
                // make all but the first ace count as 1
                score += numAces - 1;
                numAces = 1;
            }

            // if there's an Ace, score it the best way possible
            if (numAces > 0)
            {
                if (score + 11 <= MAX_HAND_VALUE)
                {
                    // counting Ace as 11 doesn't bust
                    score += 11;
                }
                else
                {
                    // count Ace as 1
                    score++;
                }
            }

            return score;
        }

        /// <summary>
        /// Gets the Blackjack value for the given card
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>the Blackjack value for the card</returns>
        private int GetBlackjackCardValue(Card card)
        {
            switch (card.Rank)
            {
                case Rank.Ace:
                    return 11;
                case Rank.King:
                case Rank.Queen:
                case Rank.Jack:
                case Rank.Ten:
                    return 10;
                case Rank.Nine:
                    return 9;
                case Rank.Eight:
                    return 8;
                case Rank.Seven:
                    return 7;
                case Rank.Six:
                    return 6;
                case Rank.Five:
                    return 5;
                case Rank.Four:
                    return 4;
                case Rank.Three:
                    return 3;
                case Rank.Two:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Changes the state of the game
        /// </summary>
        /// <param name="newState">the new game state</param>
        public static void ChangeState(GameState newState)
        {
            currentState = newState;
        }
    }
}
