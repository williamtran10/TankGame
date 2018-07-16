using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

//William Tran
//30 May 2018
//Tank Battle
//This game has two tanks shooting at each other over a wall

namespace TankBattle
{
    //set enums controlling if power and angle are going up or down
    enum PowerState
    {
        Up,
        Down,
        None
    }
    enum AngleState
    {
        Up,
        Down,
        None
    }

    public partial class Form1 : Form
    {
        //initialize power and angle states at none
        PowerState Power1State = PowerState.None;
        PowerState Power2State = PowerState.None;
        AngleState Angle1State = AngleState.None;
        AngleState Angle2State = AngleState.None;

        //declare global variables
        Vector2 Position;
        Vector2 Velocity;
        PointF RedCannonStart;
        PointF GreenCannonStart;
        PointF RedCannonEnd;
        PointF GreenCannonEnd;
        const float Gravity = 0.08f;
        const float VelocityMultiplier = 0.125f;
        const float WindStrengthMultiplier = 0.004f;
        float WindStrength = 0;
        int P1Power = 0;
        int P1AngleInt = 0;
        int P2Power = 0;
        int P2AngleInt = 0;
        int RandWind = 0;
        bool P1Ready = false;
        bool P2Ready = false;
        bool IsP1Shooting = false;

        //intitialize random for use later
        Random r = new Random();

        public Form1()
        {
            InitializeComponent();
        }

        private void Reset()
        {
            //reset all values to 0
            P1Power = 0;
            P1AngleInt = 0;
            P2Power = 0;
            P2AngleInt = 0;
            //set all labels to how they are at the start
            lblCurrentStage.Text = "Planning Stage";
            lblP1Angle.Text = "Angle = 0 degrees";
            lblP2Angle.Text = "Angle = 0 degrees";
            lblP1Power.Text = "Power = 0%";
            lblP2Power.Text = "Power = 0%";
            //reset health bars to full health
            picP1Health.Width = 156;
            picP2Health.Width = 156;
            //make bullet invisible
            picBullet.Visible = false;

            //call randomize who has high ground and where the tanks are and start planning method
            RandomizeSides();
            RandomizeTankPlacement();
            StartPlanning();

            //redraw tank cannons
            this.Refresh();
        }

        private void RandomizeSides()
        {
            //pick random number from 1 to 2
            int CoinFlip = r.Next(2);

            //if random number is 1, red tank has the high ground
            if (CoinFlip == 1)
            {
                //RED HAS HIGH GROUND
                //move the ground picture boxes so that red has high ground
                picHighGround.Left = 0;
                picHighGround.Top = 650;
                picLowGround.Left = 645;
                picLowGround.Top = 750;
                //move the tanks so that they are on their respective ground
                picRedTankTop.Top = 628;
                picRedTankBottom.Top = 637;
                picGreenTankTop.Top = 728;
                picGreenTankBottom.Top = 737;
                //move power and angle labels so that they are right under the ground
                lblP1Angle.Top = 664;
                lblP1Power.Top = 691;
                lblP2Angle.Top = 765;
                lblP2Power.Top = 792;
            }
            //else, the random number is 2: green tank has the high ground
            else
            {
                //GREEN HAS HIGH GROUND
                //move the ground picture boxes so that green has high ground
                picHighGround.Left = 535;
                picHighGround.Top = 650;
                picLowGround.Left = 0;
                picLowGround.Top = 750;
                //move the tanks so that they are on their respective ground
                picRedTankTop.Top = 728;
                picRedTankBottom.Top = 737;
                picGreenTankTop.Top = 628;
                picGreenTankBottom.Top = 637;
                //move power and angle labels so that they are right under the ground
                lblP1Angle.Top = 765;
                lblP1Power.Top = 792;
                lblP2Angle.Top = 664;
                lblP2Power.Top = 691;
            }
        }

        private void RandomizeTankPlacement()
        {
            //pick random number from 1 to 350
            int RandPlace = r.Next(50, 400);
            //set position of red tank bottom to the random place with the top of the tank following accordingly 
            picRedTankTop.Left = RandPlace + 8;
            picRedTankBottom.Left = RandPlace;

            //pick random number from 650 to 1100
            RandPlace = r.Next(780, 1120);
            //set position of red tank bottom to the random place with the top of the tank following accordingly 
            picGreenTankTop.Left = RandPlace + 16;
            picGreenTankBottom.Left = RandPlace;

            //set the point for where the cannon starts to make it inside of the top of the tank
            RedCannonStart = new PointF(picRedTankTop.Right - 4, picRedTankTop.Top + 4);
            GreenCannonStart = new PointF(picGreenTankTop.Left + 4, picGreenTankTop.Top + 4);
        }

        private void StartPlanning()
        {
            //when planning stage starts:
            //reset ready bools and ready pictures
            P1Ready = false;
            P2Ready = false;
            picP1Ready.Image = Resource1.EmptyCheck;
            picP2Ready.Image = Resource1.EmptyCheck;
            //make all labels and pictures neccessary for the Planning Stage visible
            picP1Ready.Visible = true;
            picP2Ready.Visible = true;
            lblReady1.Visible = true;
            lblReady2.Visible = true;
            lblP1Angle.Visible = true;
            lblP2Angle.Visible = true;
            lblP1Power.Visible = true;
            lblP2Power.Visible = true;

            //enable planning timer
            tmrPlanning.Enabled = true;

            //randomize wind
            RandomizeWind();
        }

        private void RandomizeWind()
        {
            //generate random number from -125 to +125 for wind
            RandWind = r.Next(-125, 125);

            //set the wind strength to be used when calculating shooting trajectory
            //wind strength is a fraction of the RandWind
            WindStrength = (float)RandWind * WindStrengthMultiplier;

            //
            //SETTING WIND ARROWS
            //

            //if there is positive wind: it is blowing to the right
            if (RandWind > 0)
            {
                //completely cover the left side and partially cover the right side to show the appropriate amount of wind below the covers
                picWindCoverLeft.Left = 468;
                picWindCoverLeft.Width = 125;
                picWindCoverRight.Left = 595 + RandWind;
                picWindCoverRight.Width = 125 - RandWind;
            }
            //if there is negative wind: it is blowing to the left
            else if (RandWind < 0)
            {
                //completely cover the right side and partially cover the left side to show the appropriate amount of wind below the covers
                picWindCoverLeft.Left = 468;
                picWindCoverLeft.Width = 125 + RandWind;
                picWindCoverRight.Left = 595;
                picWindCoverRight.Width = 125;
            }
            //if there is no wind then completely cover the arrows
            else
            {
                picWindCoverLeft.Left = 468;
                picWindCoverLeft.Width = 125;
                picWindCoverRight.Left = 595;
                picWindCoverRight.Width = 125;
            }
        }

        private void P1Shoot()
        {
            //make all Planning Stage labels and picture boxes invisible
            picP1Ready.Visible = false;
            picP2Ready.Visible = false;
            lblReady1.Visible = false;
            lblReady2.Visible = false;
            lblP1Angle.Visible = false;
            lblP2Angle.Visible = false;
            lblP1Power.Visible = false;
            lblP2Power.Visible = false;

            //set stage label to shoot
            lblCurrentStage.Text = "Shoot!";

            //reset power states for next planning stage
            Power1State = PowerState.None;
            Power2State = PowerState.None;
            Angle1State = AngleState.None;
            Angle2State = AngleState.None;
            
            //turn off planning timer, turn on shooting timer
            tmrPlanning.Enabled = false;

            //turn angle and power to float
            float AngleFloat = (float)P1AngleInt;
            float PowerFloat = (float)P1Power;

            //set shooter to P1
            IsP1Shooting = true;

            //do shooting calculations using given Power and Angle
            CalculateShooting(PowerFloat, AngleFloat);
        }

        private void CalculateShooting(float Power, float Angle)
        {
            //
            //POSITION CALCULATIONS
            //

            //set bullet position based on whos shooting
            if (IsP1Shooting == true)
            {
                //if red is shooting then make the starting position at end of red tank's cannon
                Position = new Vector2(RedCannonEnd.X, RedCannonEnd.Y);
            }
            else 
            {
                //if green is shooting then make the starting position at end of green tank's cannon
                Position = new Vector2(GreenCannonEnd.X, GreenCannonEnd.Y);
            }

            //make the position where the bullet appears
            picBullet.Left = (int)Position.X;
            picBullet.Top = (int)Position.Y;

            //
            //VELOCITY CALCULATIONS
            //

            //convert angle to radians
            Angle = MathHelper.ToRadians(Angle);

            //calculate x and y initial velocity based on power and angle using Cos and Sin and multiply by the velocity multiplier constant
            float VelocityX = Power * (float)Math.Cos(Angle) * VelocityMultiplier;
            float VelocityY = Power * (float)Math.Sin(Angle) * VelocityMultiplier;

            //flip velocityY to make it arc upwards instead of downward
            VelocityY *= -1;

            //if P2 is shooting, flip the x veocity so it shoots the other way
            if (IsP1Shooting == false)
            {
                VelocityX *= -1;
            }

            //add wind to change the X velocity
            VelocityX += WindStrength;

            //make velocity into vector
            Velocity = new Vector2(VelocityX, VelocityY);

            //make bullet visible and start shooting timer
            picBullet.Visible = true;
            tmrShoot.Enabled = true;
        }

        private void P2Shoot()
        {
            //this method only gets called after the P1 bullet hits something
            //turn P2 angle and power to float
            float AngleFloat = (float)P2AngleInt;
            float PowerFloat = (float)P2Power;

            //set shooter to P2
            IsP1Shooting = false;

            //do shooting calculations using P2's angle and power
            CalculateShooting(PowerFloat, AngleFloat);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //if game hasnt started yet, reset it when they hit enter
            if (e.KeyCode == Keys.Enter && tmrPlanning.Enabled == false && tmrShoot.Enabled == false)
            {
                //turn on reset button
                mnuGameReset.Enabled = true;
                Reset();
            }

            if (tmrPlanning.Enabled == true)
            {
                //THESE ONLY GET CHECKED WHEN IN PLANNING STAGE
                //when space is hit, toggle ready for P1
                if (e.KeyCode == Keys.Space)
                {
                    if (P1Ready == false)
                    {
                        //if not ready, make ready and flip picture
                        P1Ready = true;
                        picP1Ready.Image = Resource1.RedCheck;
                    }
                    else
                    {
                        //if ready, make not ready and flip picture
                        P1Ready = false;
                        picP1Ready.Image = Resource1.EmptyCheck;
                    }
                }
                //when NumPad0 is hit, toggle ready for P2
                else if (e.KeyCode == Keys.NumPad0)
                {
                    if (P2Ready == false)
                    {
                        //if not ready, make ready and flip picture
                        P2Ready = true;
                        picP2Ready.Image = Resource1.GreenCheck;
                    }
                    else
                    {
                        //if ready, make not ready and flip picture
                        P2Ready = false;
                        picP2Ready.Image = Resource1.EmptyCheck;
                    }

                }
                //while in staging: find what key was pressed and change its angle or power state accordingly
                //P1 Keys
                else if (e.KeyCode == Keys.W)
                {
                    Angle1State = AngleState.Up;
                }
                else if (e.KeyCode == Keys.S)
                {
                    Angle1State = AngleState.Down;
                }
                else if (e.KeyCode == Keys.D)
                {
                    Power1State = PowerState.Up;
                }
                else if (e.KeyCode == Keys.A)
                {
                    Power1State = PowerState.Down;
                }
                //P2 Keys
                else if (e.KeyCode == Keys.Up)
                {
                    Angle2State = AngleState.Up;
                }
                else if (e.KeyCode == Keys.Down)
                {
                    Angle2State = AngleState.Down;
                }
                else if (e.KeyCode == Keys.Left)
                {
                    Power2State = PowerState.Up;
                }
                else if (e.KeyCode == Keys.Right)
                {
                    Power2State = PowerState.Down;
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //if in staging: when a key controlling power or angle is let go, reset its angle or power state to none
            if (tmrPlanning.Enabled == true)
            {
                if (e.KeyCode == Keys.W || e.KeyCode == Keys.S)
                {
                    Angle1State = AngleState.None;
                }
                else if (e.KeyCode == Keys.A || e.KeyCode == Keys.D)
                {
                    Power1State = PowerState.None;
                }
                else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    Angle2State = AngleState.None;
                }
                else if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                {
                    Power2State = PowerState.None;
                }
            }
        }

        private void tmrShoot_Tick(object sender, EventArgs e)
        {
            //THIS TIMER CONTROLS THE SHOOTING STAGE
            //every tick, the velocity is added to the position and gravity is added to the Y of the velocity 
            Position += Velocity;
            Velocity.Y += Gravity;

            //cap velocity Y at 12.9 so that its impossible for it to fly over the tank hitbox (13px)
            if (Velocity.Y > 12.9)
            {
                Velocity.Y = 12.9f;
            }


            //set the bullet position to match the position vector
            picBullet.Left = (int)Position.X;
            picBullet.Top = (int)Position.Y;

            //on collision with ground or wall or flys out of sides of window
            if ((picBullet.Bounds.IntersectsWith(picHighGround.Bounds) || picBullet.Bounds.IntersectsWith(picLowGround.Bounds) || picBullet.Bounds.IntersectsWith(picWallHitBox.Bounds)) || picBullet.Left < -5 || picBullet.Left > ClientSize.Width)
            {
                //switch whos shooting or end stage if both players shot
                SwitchShooter();
            }
            //if red bullet hits green tank
            else if ((IsP1Shooting && picBullet.Bounds.IntersectsWith(picGreenTankBottom.Bounds)) || (IsP1Shooting && picBullet.Bounds.IntersectsWith(picGreenTankTop.Bounds)))
            {
                //shorten the green health bar by a third of it
                picP2Health.Width -= 52;

                //if green has no more health, red wins
                if (picP2Health.Width == 0)
                {
                    //end game and show message box
                    EndGame();
                    MessageBox.Show("Red wins!");
                }
                else
                {
                    //switch whos shooting or end stage if both players shot
                    SwitchShooter();
                }
            }
            //if green bullet hits red tank
            else if ((IsP1Shooting == false && picBullet.Bounds.IntersectsWith(picRedTankBottom.Bounds)) || (IsP1Shooting == false && picBullet.Bounds.IntersectsWith(picRedTankTop.Bounds)))
            {
                //shorten the red health bar by a third of it
                picP1Health.Width -= 52;

                //if red has no more health, green wins
                if (picP1Health.Width == 0)
                {
                    //end game and show message box
                    EndGame();
                    MessageBox.Show("Green wins!");
                }
                else
                {
                    //switch whos shooting or end stage if both players shot
                    SwitchShooter();
                }
            }
        }

        private void EndGame()
        {
            //when game ends, turn off all times and let user press enter to play again
            tmrPlanning.Enabled = false;
            tmrShoot.Enabled = false;
            //turn off reset button
            mnuGameReset.Enabled = false;
            lblCurrentStage.Text = "Press Enter to Play again";
        }

        private void SwitchShooter()
        {
            if (IsP1Shooting == true)
            {
                //let P2 shoot if P1 was shooting
                P2Shoot();
            }
            else
            {
                //end shooting if P2 shot
                EndShooting();
            }
        }

        private void EndShooting()
        {
            //disable shooting timer, make bullet invisible
            picBullet.Visible = false;
            tmrShoot.Enabled = false;
            //start the planning stage
            StartPlanning();
        }

        private void mnuGameReset_Click(object sender, EventArgs e)
        {
            //reset the game
            Reset();
        }

        private void mnuAboutHelp_Click(object sender, EventArgs e)
        {
            //show messagebox with all of the controls for the game
            MessageBox.Show("The point of this game is to hit the enemy tank 3 times by shooting at it over the tower.\nControls:\nFirst, press Enter to start the game\n\nPlayer 1 Controls:\nUse A and D to adjust power of shot\nUse W and S to adjust angle of shot\nUse Space to toggle Ready indicator\n\nPlayer 2 Controls:\nUse Left Arrow Key and Right Arrow Key to adjust power of shot\nUse Up Arrow Key and Down Arrow Key to adjust angle of shot\nUse 0 on the Numpad to toggle Ready indicator\n\nShooting will commence when both tanks are ready.\nThe game will continue to loop between planning and shooting until someone wins.\nYou can reset the game in the Game Menu.");
        }

        private void mnuAboutAboutGame_Click(object sender, EventArgs e)
        {
            //show message box saying who the game is by
            MessageBox.Show("This is a game by William Tran.");
        }

        private void mnuGameExit_Click(object sender, EventArgs e)
        {
            //exit the game
            Application.Exit();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            //THIS GETS CALLED WHENEVER FORM IS REFRESHED
            //set variable to conrol cannon length
            float CannonLength = 24;

            //convert angles to radians
            float P1AngleRad = MathHelper.ToRadians(P1AngleInt);
            float P2AngleRad = MathHelper.ToRadians(P2AngleInt);

            //create graphics objects and pens
            Graphics g = this.CreateGraphics();
            Pen RedPen = new Pen(System.Drawing.Color.Red, 6);
            Pen GreenPen = new Pen(System.Drawing.Color.Green, 6);

            //set the end point for where the cannon ends to make it reflect the angle that it is currently set to
            //the end point is the start point plus a multiple of the X and Y component of the angle
            //the y component is flipped to make the cannon point upwards and the x component is flipped for the green tank as it faces to the other way
            RedCannonEnd = new PointF(RedCannonStart.X + (float)Math.Cos(P1AngleRad) * CannonLength, (RedCannonStart.Y + -((float)Math.Sin(P1AngleRad) * CannonLength)));
            GreenCannonEnd = new PointF(GreenCannonStart.X + -((float)Math.Cos(P2AngleRad) * CannonLength), (GreenCannonStart.Y + -((float)Math.Sin(P2AngleRad) * CannonLength)));
            //draw a line for the cannon between each start and end point
            g.DrawLine(RedPen, RedCannonStart, RedCannonEnd);
            g.DrawLine(GreenPen, GreenCannonStart, GreenCannonEnd);

            //release drawing objects from memory
            RedPen.Dispose();
            GreenPen.Dispose();
            g.Dispose();
        }

        private void tmrPlanning_Tick(object sender, EventArgs e)
        {
            //every tick in planning stage, change angles based on what is input by user: max 90, min 0
            //P1 angle
            if (Angle1State == AngleState.Up && P1AngleInt < 90)
            {
                //add 1 to angle, change label for angle, and redraw cannons
                P1AngleInt++;
                lblP1Angle.Text = "Angle = " + P1AngleInt + " degrees";
                this.Refresh();
            }
            else if (Angle1State == AngleState.Down && P1AngleInt > 0)
            {
                //subtract 1 to angle, change label for angle, and redraw cannons
                P1AngleInt--;
                lblP1Angle.Text = "Angle = " + P1AngleInt + " degrees";
                this.Refresh();
            }
            //P2 angle
            if (Angle2State == AngleState.Up && P2AngleInt < 90)
            {
                //add 1 to angle, change label for angle, and redraw cannons
                P2AngleInt++;
                lblP2Angle.Text = "Angle = " + P2AngleInt + " degrees";
                this.Refresh();
            }
            else if (Angle2State == AngleState.Down && P2AngleInt > 0)
            {
                //subtract 1 to angle, change label for angle, and redraw cannons
                P2AngleInt--;
                lblP2Angle.Text = "Angle = " + P2AngleInt + " degrees";
                this.Refresh();
            }

            //every tick, change power based on what is input by user: max 100, min 0
            //P1 power
            if (Power1State == PowerState.Up && P1Power < 100)
            {
                //add 1 to power and change power label
                P1Power++;
                lblP1Power.Text = "Power = " + P1Power + "%"; ;
            }
            else if (Power1State == PowerState.Down && P1Power > 0)
            {
                //subtract 1 to power and change power label
                P1Power--;
                lblP1Power.Text = "Power = " + P1Power + "%"; ;
            }

            //P2 power
            if (Power2State == PowerState.Up && P2Power < 100)
            {
                //add 1 to power and change power label
                P2Power++;
                lblP2Power.Text = "Power = " + P2Power + "%"; ;
            }
            else if (Power2State == PowerState.Down && P2Power > 0)
            {
                //subtract 1 to power and change power label
                P2Power--;
                lblP2Power.Text = "Power = " + P2Power + "%"; ;
            }

            //if both players are ready ready then shoot
            if (P1Ready && P2Ready)
            {
                P1Shoot();
            }
        }
    }
}
