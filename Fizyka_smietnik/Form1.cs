﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;

namespace Fizyka_smietnik
{

    public partial class Form1 : Form
    {
        Thread drawThread;
        Thread physicsThread;

        public bool drawingRunning = false;
        public bool physicsRuning = false;
        public bool physicsPause = false;

        public bool debugFPS = false;
        public bool debuginfo = false;


        public const int maxvel = 100;
        public const int K = 1;
        public long dt = Stopwatch.Frequency / (K* maxvel);
        public long fps = 0;
        public int tps = 0;
        public long ticksCount = 0;
        public long skippedTicksCount = 0;
        
        public int defaultRadius = 4;
        Size defaultFormSize;
        Size box = new Size(574, 384);

        Font drawFont = new Font("Arial", 8);

        SolidBrush blackBrush = new SolidBrush(Color.Black);

        Particle[] particles;

        public Form1()
        {
            InitializeComponent();
            defaultFormSize = Size;
        }

            //NARYSOWANIE GRANIC
        public void drawBorders(Graphics drawing)
        {
            Pen borderPen = new Pen(Color.Black);
            drawing.DrawLine(borderPen, 0, 0, box.Width - 1, 0);
            drawing.DrawLine(borderPen, 0, box.Height - 1, box.Width - 1, box.Height - 1);
            drawing.DrawLine(borderPen, 0, 0, 0, box.Height - 1);
            drawing.DrawLine(borderPen, box.Width - 1, 0, box.Width - 1, box.Height - 1);
        }

        public void drawParticles(Graphics drawing)
        {
            SolidBrush particleBrush = new SolidBrush(Color.Black);
            Pen blackPen = new Pen(Color.Black);
            for (int i = 0; i < particles.Length; i++)
            {
                particleBrush.Color = particles[i].color;
                drawing.FillEllipse(particleBrush, (float)((particles[i].X - particles[i].Radius) / 1), (float)((particles[i].Y - particles[i].Radius) / 1), 2 * particles[i].Radius, 2 * particles[i].Radius);
                drawing.DrawEllipse(blackPen, (float)(particles[i].X - particles[i].Radius), (float)(particles[i].Y - particles[i].Radius), 2 * particles[i].Radius, 2 * particles[i].Radius);
                blackPen.Color = Color.Black;
            }
        }

            //NARYSOWANIE CALEJ KLATKI NA OKNIE APLIKACJI

        public void drawFrame(Graphics drawing ,Bitmap bmg)
        {
            Rectangle box = new Rectangle(0, 0, bmg.Width, bmg.Height);
            if (drawingRunning)
            {
                if (pictureBox1.InvokeRequired)         //NIE MAM POJECIA DLACZEGO ALE MUSIALEM TO W TEN SPOSOB ZROBIC
                {
                    pictureBox1.Invoke(new MethodInvoker(
                         delegate ()
                         {
                             if (pictureBox1.Image!=null)
                             pictureBox1.Image.Dispose();
                             pictureBox1.Image = bmg.Clone( box, bmg.PixelFormat);
                         }));
                }
                else
                    pictureBox1.Image = bmg.Clone(box, bmg.PixelFormat);
            }
            drawing.Clear(Color.White);
        }

        public void showFPS(Graphics drawing)
        {
            label5.Invoke(new MethodInvoker(
                         delegate ()
                         {
                             label5.Text = ("FPS: " + fps.ToString());
                         }));
            label6.Invoke(new MethodInvoker(
                         delegate ()
                         {
                             label6.Text = (("TPS: " + tps.ToString() + " / " + (Stopwatch.Frequency / dt).ToString() + "\nSimulation time: " + (1000 * ticksCount * dt / Stopwatch.Frequency).ToString() + "ms \nTicks: " + ticksCount.ToString() + "\nSkipped ticks: " + skippedTicksCount.ToString()));
                         }));
            //drawing.DrawString(("FPS: " + fps.ToString()), drawFont, blackBrush, 5 , 5);
        }

        public void showTPS(Graphics drawing)
        {
            drawing.DrawString(("TPS: " + tps.ToString() + " / " + (Stopwatch.Frequency / dt).ToString() + " Simulation time: " + (1000*ticksCount*dt/Stopwatch.Frequency).ToString() + "ms Ticks: " + ticksCount.ToString() + " Skipped ticks: " + skippedTicksCount.ToString()), drawFont, blackBrush, 5, 15);
        }

        public void showParticleInfo (Graphics drawing)
        {
            for (int i = 0; i < particles.Length && i < 30; i++)
                drawing.DrawString(i.ToString() + ") X: " + particles[i].X.ToString() + " Y: " + particles[i].Y.ToString() + " VelX: " + particles[i].velX.ToString() + " VelY: " + particles[i].velY.ToString(), drawFont , blackBrush, 100 , 10 + 10*(i + 2));
        }

        public void draw()
        {
            Bitmap bmg = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics SimDrawing = Graphics.FromImage(bmg);
            Stopwatch drawWatch = new Stopwatch();
            drawWatch.Start();
            while (drawingRunning)
            {
                drawWatch.Restart();
                drawFrame(SimDrawing , bmg);
                if (debugFPS) showFPS(SimDrawing);
                //if (debugFPS) showTPS(SimDrawing);
                if (debuginfo) showParticleInfo(SimDrawing);
                drawParticles(SimDrawing);
                drawBorders(SimDrawing);
                drawWatch.Stop();
                fps = Stopwatch.Frequency/ drawWatch.ElapsedTicks;
            }
        }

        public void physics()
        {
            Stopwatch PhysicsTimer = new Stopwatch();
            long nextTick = 0;
            int skippedTicksPackage = 10;
            long nextTpsCount = Stopwatch.Frequency;
            int tps = 0;

            PhysicsTimer.Start();
            while (physicsRuning && particles != null)
            {
                //zatrzymywanie fizyki
                if(physicsPause)
                {
                    PhysicsTimer.Stop();
                    while (physicsPause)
                        Thread.Sleep(10);
                    PhysicsTimer.Start();
                }
                //czekanie na kolejny tick
                while(PhysicsTimer.ElapsedTicks < nextTick)
                    Thread.Sleep(1);
                //tick
                ticksCount++;
                tps++;
                physicsTick();
                //oblicznie czasu kolejnego tick'a
                nextTick += dt;
                //sprawdzanie różnicy między teraz i nextTick
                while(PhysicsTimer.ElapsedTicks - nextTick > skippedTicksPackage * dt)
                {
                    //pomijanie pakietu tick'ow
                    nextTick += skippedTicksPackage * dt;
                    skippedTicksCount += skippedTicksPackage;
                }
                //tps
                if(PhysicsTimer.ElapsedTicks > nextTpsCount)
                {
                    nextTpsCount += Stopwatch.Frequency;
                    this.tps = tps;
                    tps = 0;
                }
            }
        }

        public void physicsTick()
        {
            for (int i = 0; i < particles.Length; i++)
                particles[i].updateparticle(dt, particles,box);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AutoSize = false;
            Size = defaultFormSize;
            Random rng = new Random(); //UTWORZENIE SEEDA RNG
            box.Width = Convert.ToInt32(numericUpDown3.Value) * defaultRadius;
            box.Height = Convert.ToInt32(numericUpDown4.Value) * defaultRadius;
            pictureBox1.Size=box;
            AutoSize = true;
            if (physicsThread != null)
            {
                physicsThread.Abort();
            }
            if (drawThread != null)
            {
                drawThread.Abort();
            }

            int numberofparticles = 5;          //WCZYTANIE WARTOSCI Z OKNA
            int maxvel = 100;
            numberofparticles = Convert.ToInt32(numericUpDown1.Value);
            maxvel =  Convert.ToInt32(numericUpDown2.Value);
            dt = Stopwatch.Frequency / (K * maxvel);

            //UTWORZENIE TABLICY CZASTEK

            particles = new Particle[numberofparticles];
            for (int i = 0; i < numberofparticles; i++)
                particles[i] = new Particle(defaultRadius, box.Width , pictureBox1.Height, maxvel , rng);

                //UTWORZENIE THREADA DLA RYSOWANIA ORAZ FIZYKI

                drawingRunning = true;
                drawThread = new Thread(draw);
                drawThread.Start();

                physicsRuning = true;
                physicsPause = true;
                button2.Text = "Physics Paused";
                physicsThread = new Thread(physics);
                physicsThread.Start();
                physicsRuning = true;
            System.Threading.Thread.Sleep(1000);
        }

                        // PAUZOWANIE FIZYKI
        private void button2_Click(object sender, EventArgs e)
        {
            if (physicsRuning)
            {
                if (physicsPause)
                {
                    physicsPause = false;
                    button2.Text = "Physics Unpaused";
                }
                else
                {
                    physicsPause = true;
                    button2.Text = "Physics Paused";
                }
            }
        }

                    // ZAKONCZENIE WATKOW PRZED ZAMKNIECIEM OKNA
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (drawThread!=null)
            drawThread.Abort();
            if (physicsThread!=null)
            physicsThread.Abort();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (debugFPS)
            {
                debugFPS = false;
                label5.Visible = false;
                label6.Visible = false;
            }
            else
            {
                debugFPS = true;
                label5.Visible = true;
                label6.Visible = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (debuginfo) debuginfo = false;
            else debuginfo = true;
        }

        private void changedL(object sender, EventArgs e)
        {
            // 5L <= H
            while (5*Convert.ToInt32(numericUpDown3.Value) > Convert.ToInt32(numericUpDown4.Value))
                numericUpDown4.Value+= 5 * Convert.ToInt32(numericUpDown3.Value)-Convert.ToInt32(numericUpDown4.Value); 
        }

        private void changeH(object sender, EventArgs e)
        {
            // H >= 5L
            while (5 * Convert.ToInt32(numericUpDown3.Value) > Convert.ToInt32(numericUpDown4.Value))
                numericUpDown3.Value-= 5 * Convert.ToInt32(numericUpDown3.Value) - Convert.ToInt32(numericUpDown4.Value);
        }
    }

    public class Particle
    {

        geometry geomath = new geometry();

        public double X;
        public double Y;
        public int Radius;
        public double velX;
        public double velY;
        public Color color = Color.Transparent;

        public Particle(int x, int y, int radius, int velx = 0, int vely = 0)
        {
            X = x;
            Y = y;
            Radius = radius;
            velX = velx;
            velY = vely;
        }

        public Particle( int radius, int width, int height, int maxvel, Random rng)          //LOSOWE UTWORZENIE czastki
        {
            Radius = radius;
            X = Radius + rng.Next() % (width - 2*Radius - 1);
            Y = Radius + rng.Next() % (height - 2*Radius - 1);
            if (maxvel == 0)
            {
                velX = 0;
                velY = 0;
                return;
            }
            if ((1 + rng.Next()) % 2 == 1)
                velX = rng.Next() % (maxvel);
            else
                velX = -rng.Next() % (maxvel);
            if ((1 + rng.Next()) % 2 == 1)
                velY = rng.Next() % (maxvel);
            else
                velY = -rng.Next() % (maxvel);
        }

        public void updateparticle(long ticks , Particle[] particles, Size box)
        {
            X += ((double)ticks / Stopwatch.Frequency) * velX;
            Y += ((double)ticks / Stopwatch.Frequency) * velY;
            velY += ((double)ticks / Stopwatch.Frequency) * 1000;
            bordercollision(box);
            multipleparticlescollisions(particles);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool particlecollision(Particle secondparticle)
        {
            if (secondparticle != this)
            {
                double dx = X - secondparticle.X;
                double dy = Y - secondparticle.Y;
                if (dy * dy + dx * dx <= (Radius + secondparticle.Radius) * (Radius + secondparticle.Radius))
                //if (geomath.distance(X, Y, secondparticle.X, secondparticle.Y) <= (Radius + secondparticle.Radius))
                {
                    color = Color.Red;
                    return true;
                }
            }
            return false;
        }
        public bool particlecollision(ref double X1,ref double Y2 ,ref int R2)
        {
                double dx = X - X1;
                double dy = Y - Y2;
                if (dy * dy + dx * dx <= (Radius + R2) * (Radius + R2))
                //if (geomath.distance(X, Y, secondparticle.X, secondparticle.Y) <= (Radius + secondparticle.Radius))
                {
                    color = Color.Red;
                    return true;
                }
            return false;
        }
        public bool multipleparticlescollisions(Particle[] particles)
        {
            bool touched = false;
            for (int i = 0; i<particles.Length;i++)
            {
                /*if (particles[i] != this)
                 {
                     double dx = X - particles[i].X;
                     double dy = Y - particles[i].Y;
                     if (dy * dy + dx * dx <= (Radius + particles[i].Radius) * (Radius + particles[i].Radius))
                     //if (geomath.distance(X, Y, secondparticle.X, secondparticle.Y) <= (Radius + secondparticle.Radius))
                     {
                         color = Color.Red;
                         touched = true;
                     }
                 }*/

                if (particles[i] != this)
                {
                    if (particlecollision(ref particles[i].X,ref particles[i].Y,ref particles[i].Radius))
                    {
                        touched = true;
                    }
                }
            }
            if (!touched)
            {
                color = Color.Transparent;
                return false;
            }
            else
                return true;
        }
        public void bordercollision(Size box)
        {
            if (X < Radius && velX < 0)
                velX = -velX*0.7;
            if (X > box.Width - Radius - 1 && velX>0)
                velX = -velX*0.7;
            if (Y > box.Height- Radius-1 && velY > 0)
                velY = (-velY)*0.85;
            if (Y < Radius && velY < 0)
                velY = (-velY)*0.85;
        }
        public void resolvecollision( Particle collidedParticle)
        {
            //TODO
        }
    }

    public class geometry
    {
        public geometry() { }
        public double distance(double x1 ,double y1 ,double x2, double y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return (double)Math.Sqrt((dy * dy + dx * dx));
        }
        public double distance_nosqrt(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return (dy * dy + dx * dx);
        }

    }
}
