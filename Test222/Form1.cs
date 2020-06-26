using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Fizyka_smietnik
{

    public partial class Form1 : Form
    {
        Thread drawThread;
        public bool drawingRunning = false;
        Thread physicsThread;
        long physicsFPS = 0;
        public bool physicsRuning = false;
        public bool physicsPause = false;
        string Texttest = "Test1";
        public int grawity = 10;
        Particle[] particles;
        public Form1()
        {
            InitializeComponent();
        }
        public void draw()
        {
            long drawFPS = 0;
            Font drawFont = new Font("Arial", 8);
            Font DWDfont = new Font("Comic_Sans", 20);
            SolidBrush blackBrush = new SolidBrush(Color.Black);
            Graphics SimDrawing;
            float moveY;
            Bitmap bm = new Bitmap(574, 384);
            Bitmap bmg = new Bitmap(574, 384);
            SimDrawing = Graphics.FromImage(bmg);
            Console.WriteLine("wlaczonno thread");
            Texttest = "Uruchomiono Thread";
            Stopwatch drawWatch = new Stopwatch();
            drawWatch.Start();
            while (drawingRunning)
            {
                drawWatch.Restart();
                if (drawingRunning)
                {
                    bm = bmg.Clone(new Rectangle(0,0,bmg.Width ,bmg.Height),bmg.PixelFormat);
                    if (pictureBox1.InvokeRequired)
                    {
                        pictureBox1.Invoke(new MethodInvoker(
                             delegate ()
                             {
                                 pictureBox1.Image = bm;
                             }));
                    }
                    else
                        pictureBox1.Image = bm;
                }
                SimDrawing.Clear(Color.White);
                Pen blackPen = new Pen(Color.Black);
                SimDrawing.DrawString(("Draw FPS: " + drawFPS.ToString() + " Physics FPS: " + physicsFPS.ToString()), drawFont, blackBrush, new PointF(100, 80));
                SimDrawing.DrawString(("0) X: " + particles[0].X.ToString() + " Y: " + particles[0].Y.ToString() + " Rad: " + particles[0].Radius.ToString()), drawFont, blackBrush, new PointF(100, 100)) ;
                SimDrawing.DrawString(("1) X: " + particles[1].X.ToString() + " Y: " + particles[1].Y.ToString() + " Rad: " + particles[1].Radius.ToString()), drawFont, blackBrush, new PointF(100, 130));
                //SimDrawing.DrawString("DWD", DWDfont, blackBrush, new PointF(particles[0].X, particles[0].Y));
                for (int i = 0; i < particles.Length; i++)
                {
                    SimDrawing.FillEllipse(new SolidBrush(particles[i].color), new Rectangle((int)((particles[i].X - particles[i].Radius) / 1), (int)((particles[i].Y - particles[i].Radius) / 1), (int)(2 * particles[i].Radius), 2 * particles[i].Radius));
                    SimDrawing.DrawEllipse(blackPen, (particles[i].X - particles[i].Radius), particles[i].Y - particles[i].Radius, 2 * particles[i].Radius, 2 * particles[i].Radius);
                    blackPen.Color = Color.Black;
                }
                    //BORDERS
                SimDrawing.DrawLine(blackPen, 0, 0, pictureBox1.Size.Width-1, 0 );
                SimDrawing.DrawLine(blackPen, 0, pictureBox1.Size.Height-1, pictureBox1.Size.Width-1, pictureBox1.Size.Height-1);
                SimDrawing.DrawLine(blackPen, 0, 0, 0, pictureBox1.Size.Height-1);
                SimDrawing.DrawLine(blackPen, pictureBox1.Size.Width-1, 0, pictureBox1.Size.Width-1, pictureBox1.Size.Height-1);
                drawWatch.Stop();
                drawFPS = Stopwatch.Frequency/ drawWatch.ElapsedTicks;
            }
            Texttest = "Zakonczono";
            return;
        }

        public void drawImage(Bitmap image)
        {
            pictureBox1.Image = image;
        }
        public void physics()
        {
            long PhysycsTicks = 0;
            Stopwatch PhysicsTimer = new Stopwatch();

            PhysicsTimer.Start();
            while (physicsRuning)
            {
                if (particles != null)
                {
                    while (physicsPause)
                    {
                        System.Threading.Thread.Sleep(500);
                        PhysicsTimer.Restart();
                    }
                    for (int i = 0; i < particles.Length; i++)
                    {
                        particles[i].updateparticle(PhysycsTicks,particles);
                    }
                    PhysicsTimer.Stop();
                    PhysycsTicks = PhysicsTimer.ElapsedTicks;
                    PhysicsTimer.Restart();
                    Texttest = ((float)PhysycsTicks/(float)Stopwatch.Frequency).ToString();
                    physicsFPS =Stopwatch.Frequency/ PhysycsTicks;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random rng = new Random();
            if (physicsThread != null)
                physicsThread.Abort();
            int numberofparticles = 5;
            int maxvel = 100;
            if (textBox1.Text!="")
                numberofparticles = Convert.ToInt32(textBox1.Text);
            if (textBox2.Text != "")
                maxvel = Convert.ToInt32(textBox2.Text);
            particles = new Particle[numberofparticles];
            for (int i = 0; i < numberofparticles; i++)
                particles[i] = new Particle(10, pictureBox1.Width , pictureBox1.Height, maxvel , rng);
            if (drawThread == null)
            {
                drawingRunning = true;
                drawThread = new Thread(draw);
                drawThread.Start();
            }
            System.Threading.Thread.Sleep(1000);

            //Pen blackPen = new Pen ( Color.Black );
            //SimDrawing.DrawLine(blackPen, 0, 0, pictureBox1.Size.Width-1, 0 );
            //SimDrawing.DrawLine(blackPen, 0, pictureBox1.Size.Height-1, pictureBox1.Size.Width-1, pictureBox1.Size.Height-1);
            //SimDrawing.DrawLine(blackPen, 0, 0, 0, pictureBox1.Size.Height-1);
            //SimDrawing.DrawLine(blackPen, pictureBox1.Size.Width-1, 0, pictureBox1.Size.Width-1, pictureBox1.Size.Height-1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (physicsThread == null)
            {
                physicsRuning = true;
                physicsPause = true;
                button2.Text = "Utworzono physics Thread";
                physicsThread = new Thread(physics);
                physicsThread.Start();
            }
            else
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (drawThread!=null)
            drawThread.Abort();
            if (physicsThread!=null)
            physicsThread.Abort();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

    public class Particle
    {

        geometry geomath = new geometry();
        public float X;
        public float Y;
        public int Radius;
        public float velX;
        public float velY;
        public float movY = 0;
        public Color color = Color.Transparent;

        public Particle(int x, int y, int radius, int velx = 0, int vely = 0)
        {
            X = x;
            Y = y;
            Radius = radius;
            velX = velx;
            velY = vely;
        }

        public Particle(int radius, int width, int height, int maxvel, Random rng)
        {
            Radius = radius;
            X = 10 + rng.Next() % (width - radius - 1);
            Y = 10 + rng.Next() % (height - radius - 1);
            if ((1 + rng.Next()) % 2 == 1)
                velX = rng.Next() % (maxvel);
            else
                velX = -rng.Next() % (maxvel);
            if ((1 + rng.Next()) % 2 == 1)
                velY = rng.Next() % (maxvel);
            else
                velY = -rng.Next() % (maxvel);
        }

        public void updateparticle(long ticks , Particle[] particles)
        {
            movY = (((float)ticks / Stopwatch.Frequency) * velY);
            X += ((float)ticks / Stopwatch.Frequency) * velX;
            Y += ((float)ticks / Stopwatch.Frequency) * velY;
            velY += ((float)ticks / Stopwatch.Frequency) * 1000;
            bordercollision();
            multipleparticlescollisions(particles);
        }

        public bool particlecollision(Particle secondparticle)
        {
            if (secondparticle != this)
            {    if (geomath.distance(X, Y, secondparticle.X, secondparticle.Y) < Radius + secondparticle.Radius)
                {
                    color = Color.Red;
                    secondparticle.color = Color.Red;
                    return true;
                }
            }
            return false;
        }
        public bool multipleparticlescollisions(Particle[] particles)
        {
            bool touched = false;
            for (int i = 0; i<particles.Length;i++)
            {
                if (particlecollision(particles[i]))
                    touched = true;
            }
            if (!touched)
            {
                color = Color.Transparent;
                return false;
            }
            else
                return true;
        }
        public void bordercollision()
        {
            if (X < Radius && velX < 0)
                velX = -velX*(float)0.7;
            if (X > 574 - Radius - 1 && velX>0)
                velX = -velX*(float)0.7;
            if (Y > 384- Radius-1 && velY > 0)
                velY = (-velY)*(float)0.9;
            if (Y < Radius && velY < 0)
                velY = (-velY)*(float)0.90;
        }
        public void resolvecollision( Particle collidedParticle)
        {

        }
    }
    public class PhysicGod
    {
        public int grawity = 10;

        public PhysicGod() { }

    }

    public class geometry
    {
        public geometry() { }
        public float distance(float x1 ,float y1 ,float x2, float y2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return (float)Math.Sqrt((dy * dy + dx * dx));
        }

    }
}
