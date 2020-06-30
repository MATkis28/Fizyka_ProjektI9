using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fizyka_smietnik
{
    public partial class Form1 : Form
    {
        Thread drawThread;
        Thread physicsThread;
        Particle[] particles;
        Detector detector;// = new Detector(300,500); //TODO: jakoś to trzeba zrobić żeby można było zmieniać te wartosci w UI i może narysowac ten detektor jakos

        //symulacja
        private int nh;
        private int nl;
        private int numberofparticles = 5;
        private int defaultRadius = 3;
        private int maxVel = 100;
        private int K = 1;
        private const long g = 1000;
        private long dt;
        private int M = 5000; //liczba ticków co które oblicza sie ciśnienie

        //stany
        private bool drawingRunning = false;
        private bool physicsRuning = false;
        private bool physicsPause = false;

        //debug info
        private bool debugFPS = false;
        private bool debuginfo = false;
        private int fps = 0;
        private int tps = 0;
        private long ticksCount = 0;
        private long skippedTicksCount = 0;

        //okno
        private Size defaultFormSize;
        private Size box = new Size(574, 384);
        private Font drawFont = new Font("Arial", 8);
        private SolidBrush blackBrush = new SolidBrush(Color.Black);

        public Form1()
        {
            InitializeComponent();
            defaultFormSize = Size;
        }

        private void drawDetector(Graphics drawing)
        {
            Pen borderPen = new Pen(Color.Red);
            drawing.DrawLine(borderPen, box.Width - 1, (float)detector.end, box.Width - 1, (float)detector.begin);
            drawing.DrawLine(borderPen, box.Width - 2, (float)detector.end, box.Width - 2, (float)detector.begin);
            if (textBox1.InvokeRequired)         //NIE MAM POJECIA DLACZEGO ALE MUSIALEM TO W TEN SPOSOB ZROBIC
            {
                pictureBox1.Invoke(new MethodInvoker(
                     delegate ()
                     {
                         textBox1.Text = detector.p.ToString();
                     }));
            }
            else
                textBox1.Text = detector.p.ToString();

        }

        private void drawBorders(Graphics drawing)
        {
            Pen borderPen = new Pen(Color.Black);
            drawing.DrawLine(borderPen, 0, 0, box.Width - 1, 0);
            drawing.DrawLine(borderPen, 0, box.Height - 1, box.Width - 1, box.Height - 1);
            drawing.DrawLine(borderPen, 0, 0, 0, box.Height - 1);
            drawing.DrawLine(borderPen, box.Width - 1, 0, box.Width - 1, box.Height - 1);
        }

        private void drawParticles(Graphics drawing)
        {
            //SolidBrush particleBrush = new SolidBrush(Color.Black);
            Pen blackPen = new Pen(Color.Black);
            for (int i = 0; i < particles.Length; i++)
            {
                //particleBrush.Color = particles[i].color;
                //drawing.FillEllipse(particleBrush, (float)((particles[i].X - particles[i].Radius) / 1), (float)((particles[i].Y - particles[i].Radius) / 1), 2 * particles[i].Radius, 2 * particles[i].Radius);
                drawing.DrawEllipse(blackPen, (float)(particles[i].X - particles[i].Radius), (float)(particles[i].Y - particles[i].Radius), 2 * particles[i].Radius, 2 * particles[i].Radius);
                blackPen.Color = Color.Black;
            }
        }

        private void drawFrame(Graphics drawing ,Bitmap bmg)
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

        private void showFPS(Graphics drawing)
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

        private void showParticleInfo (Graphics drawing)
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
                if (debugFPS) showFPS(SimDrawing);
                if (debuginfo) showParticleInfo(SimDrawing);
                drawParticles(SimDrawing);
                drawBorders(SimDrawing);
                drawDetector(SimDrawing);
                drawFrame(SimDrawing, bmg);
                drawWatch.Stop();
                fps = (int)(Stopwatch.Frequency/ drawWatch.ElapsedTicks);
            }
        }

        public void physics()
        {
            Stopwatch PhysicsTimer = new Stopwatch();
            long nextTick = 0;
            int skippedTicksPackage = 10;
            long nextTpsCount = Stopwatch.Frequency;
            int tps = 0;
            int m = 0;

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
                m++;
                physicsTick();
                //liczenie ciśnienia
                if (m==M)
                {
                    detector.calculatePressure(M * dt);
                    m = 0;
                }
                //oblicznie czasu kolejnego tick'a
                nextTick += dt;
                //sprawdzanie różnicy między teraz i nextTick
                while(PhysicsTimer.ElapsedTicks - nextTick > skippedTicksPackage * dt)
                {
                    //pomijanie pakietu tick'ow
                    nextTick += skippedTicksPackage * dt;
                    skippedTicksCount += skippedTicksPackage;
                }
                //liczenie tps
                if(PhysicsTimer.ElapsedTicks > nextTpsCount)
                {
                    nextTpsCount += Stopwatch.Frequency;
                    this.tps = tps;
                    tps = 0;
                }
            }
        }

        private void physicsTick()
        {
            for (int i = 0; i < particles.Length; i++)
                particles[i].updateparticle(particles, detector, i);
        }

        //tworzenie symulacji
        private void button1_Click(object sender, EventArgs e)
        {
            defaultRadius = Convert.ToInt32(numericUpDown7.Value);
            AutoSize = false;
            Size = defaultFormSize;
            Random rng = new Random(); //UTWORZENIE SEEDA RNG
            nh = Convert.ToInt32(numericUpDown4.Value); //POBRANIE ROZMAIRU OKNA
            nl = Convert.ToInt32(numericUpDown3.Value);
            box.Width = nl * defaultRadius;
            box.Height = nh * defaultRadius;
            K = nl; //K = (nl < nh) ? nl : nh; 
            pictureBox1.Size=box;
            AutoSize = true;
            if (physicsThread != null)      //WYLACZENIE POPRZEDNICH SYMULACJI
            {
                physicsThread.Abort();
            }
            if (drawThread != null)
            {
                drawThread.Abort();
            }
            ticksCount = 0;
            skippedTicksCount = 0;

            //UTWORZENIE DETEKTORA
            double h = Convert.ToInt32(numericUpDown5.Value);
            double lambda = Convert.ToInt32(numericUpDown6.Value);
            detector = new Detector(box.Height -(h*defaultRadius) -(lambda*defaultRadius), box.Height- (h * defaultRadius));

            //WCZYTANIE WARTOSCI Z OKNA
            numberofparticles = Convert.ToInt32(numericUpDown1.Value);
            if (4 * numberofparticles > nh * nl)
            {
                numberofparticles = nh * nl / 4;
                numericUpDown1.Value = numberofparticles;
            }
            maxVel =  Convert.ToInt32(numericUpDown2.Value);
            dt = Stopwatch.Frequency / (K * maxVel);
            g = Convert.ToInt32(numericUpDown8.Value);

            //UTWORZENIE TABLICY CZASTEK
            double dts = (double)dt / Stopwatch.Frequency;
            particles = new Particle[numberofparticles];
            for (int i = 0; i < numberofparticles; i++)
                particles[i] = new Particle(defaultRadius, dts, g, box, maxVel, rng);

            //RESETOWANIE TPS
            fps = 0;
            tps = 0;
            ticksCount = 0;
            skippedTicksCount = 0;

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

        // pauzowanie fizyki
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

        //show fps
        private void button3_Click(object sender, EventArgs e)
        {
            if (debugFPS)
            {
                debugFPS = false;
                label5.Text = "";
                label6.Text = "";
            }
            else
            {
                debugFPS = true;
            }
        }

        //debuginfo
        private void button4_Click(object sender, EventArgs e)
        {
            if (debuginfo) debuginfo = false;
            else debuginfo = true;
        }

        // zakonczenie watkow przed zamknieciem okna
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (drawThread != null)
                drawThread.Abort();
            if (physicsThread != null)
                physicsThread.Abort();
        }

        private void changedL(object sender, EventArgs e)
        {
            // 5L <= H
            while (5*Convert.ToInt32(numericUpDown3.Value) > Convert.ToInt32(numericUpDown4.Value))
                numericUpDown4.Value= 5 * Convert.ToInt32(numericUpDown3.Value);
            changedh(sender, e);
            changedlambda(sender, e);
        }

        private void changeH(object sender, EventArgs e)
        {
            // H >= 5L
            while (5 * Convert.ToInt32(numericUpDown3.Value) > Convert.ToInt32(numericUpDown4.Value))
                numericUpDown3.Value= Convert.ToInt32(numericUpDown4.Value)/5;
            changedh(sender, e);
            changedlambda(sender, e);
        }

        private void changedlambda(object sender, EventArgs e)
        {
            if (numericUpDown6.Value + numericUpDown5.Value > numericUpDown4.Value)
                numericUpDown6.Value = numericUpDown4.Value - numericUpDown5.Value;
        }

        private void changedh(object sender, EventArgs e)
        {
            if (numericUpDown5.Value != 0)
            {
                if (numericUpDown6.Value + numericUpDown5.Value > numericUpDown4.Value)
                    numericUpDown5.Value = numericUpDown4.Value - numericUpDown6.Value;
            }
        }
    }

    public class Detector
    {
        public double begin;
        public double end;
        public double p;

        private double suma_p = 0;

        public Detector(double begin, double end)
        {
            this.begin = begin;
            this.end = end;
        }

        public void detect(double vel) { suma_p += 2 * vel; }

        public void calculatePressure(double delta_t)
        {
            p = suma_p / (delta_t * (end - begin));
            suma_p = 0;
        }
    }

    public class Particle
    {
        //stałe
        private const double d = 1.05 * 1.05;

        public double X;
        public double Y;
        public double velX;
        public double velY;
        public int Radius;
        public double dts;
        public long g;
        public Size box;
        public Color color = Color.Transparent;

        public Particle(int X, int Y, int Radius, double dts, long g, Size box, int velX = 0, int velY = 0)
        {
            this.X = X;
            this.Y = Y;
            this.Radius = Radius;
            this.velX = velX;
            this.velY = velY;
            this.dts = dts;
            this.g = g;
            this.box = box;
        }

        public Particle(int Radius, double dts, long g, Size box, int maxVel, Random rng)          //LOSOWE UTWORZENIE czastki
        {
            this.Radius = Radius;
            this.dts = dts;
            this.g = g;
            this.box = box;
            X = Radius + rng.Next() % (box.Width - 2*Radius - 1);
            Y = Radius + rng.Next() % (box.Height - 2*Radius - 1);
            if (maxVel == 0)
            {
                velX = 0;
                velY = 0;
                return;
            }
            if ((1 + rng.Next()) % 2 == 1)
                velX = rng.Next() % (maxVel);
            else
                velX = -rng.Next() % (maxVel);
            if ((1 + rng.Next()) % 2 == 1)
                velY = rng.Next() % (maxVel);
            else
                velY = -rng.Next() % (maxVel);
        }

        public void updateparticle(Particle[] particles, Detector detector, int currentIndex)
        {
            X += dts * velX;
            Y += dts * velY;
            velY += dts * g;
            bordercollision(detector);
            multipleparticlescollisions(particles, currentIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool particlecollision(Particle secondparticle)
        {
            if (secondparticle != this)
            {
                double dx = X - secondparticle.X;
                double dy = Y - secondparticle.Y;
                if (dy * dy + dx * dx <= (Radius + secondparticle.Radius) * (Radius + secondparticle.Radius) * d) 
                //if (geometry.distance(X, Y, secondparticle.X, secondparticle.Y) <= (Radius + secondparticle.Radius))
                {
                    //color = Color.Red;
                    return true;
                }
            }
            return false;
        }
        public bool particlecollision(ref double X1, ref double Y2, ref int R2)
        {
            double dx = X - X1;
            double dy = Y - Y2;
            if (dy * dy + dx * dx < (Radius + R2) * (Radius + R2) * d)
                {                     
                    //color = Color.Red;
                    return true;
                }
            return false;
        }
        public bool multipleparticlescollisions(Particle[] particles, int currentIndex)
        {
            bool touched = false;
            for (int i = currentIndex + 1; i < particles.Length; i++) 
            {
                /*if (particles[i] != this)
                 {
                     double dx = X - particles[i].X;
                     double dy = Y - particles[i].Y;
                     if (dy * dy + dx * dx <= (Radius + particles[i].Radius) * (Radius + particles[i].Radius))
                     //if (geometry.distance(X, Y, secondparticle.X, secondparticle.Y) <= (Radius + secondparticle.Radius))
                     {
                         color = Color.Red;
                         touched = true;
                     }
                 }*/
                if (particlecollision(ref particles[i].X,ref particles[i].Y,ref particles[i].Radius))
                {
                    touched = true;
                    resolvecollision(particles[i]);
                }
                
            }
            if (!touched)
            {
                //color = Color.Transparent;
                return false;
            }
            else
                return true;
        }
        //TODO: wywołac funkcje detekcji
        public void bordercollision(Detector detector)
        {
            if (X < Radius && velX < 0)
                velX = -velX;
            if (X > box.Width - Radius - 1 && velX > 0)
            {
                if (Y > detector.begin && Y < detector.end)
                    detector.detect(velX);
                velX = -velX;
            }
            if (Y > box.Height - Radius - 1 && velY > 0)
                velY = (-velY);
            if (Y < Radius && velY < 0)
                velY = (-velY);
        }
        public void resolvecollision( Particle collidedParticle)
        {
            //przeksztalcenie wspolrzednych polarnych
            double wektor1wartosc = Math.Sqrt(velX * velX + velY * velY);
            double wektor1kat;
            if (velY == 0)
                wektor1kat = velX > 0 ? 0 : 180;
            else if (velX == 0)
                wektor1kat = -velY > 0 ? -90 : 90;
            else
                wektor1kat = Math.Atan2(-velY, velX) * 180 / Math.PI;

            double wektor2wartosc = Math.Sqrt(collidedParticle.velX * collidedParticle.velX + collidedParticle.velY * collidedParticle.velY);
            double wektor2kat;
            if (velY == 0)
                wektor2kat = collidedParticle.velX > 0 ? 0 : 180;
            else if (velX == 0)
                wektor2kat = collidedParticle.velY < 0 ? -90 : 90;
            else
                wektor2kat = Math.Atan2(-collidedParticle.velY, collidedParticle.velX) * 180 / Math.PI;

            //"przesuniecie" ukladu wspolrzednych (aby oba atomy leżały na osi X - atom 1 po lewej, atom 2 po prawej)
            double polozenieX = collidedParticle.X - this.X;
            double polozenieY = -(collidedParticle.Y - this.Y);
            double polozenieKat;
            if (polozenieY == 0)
                polozenieKat = polozenieX > 0 ? 0 : 180;
            else if (polozenieX == 0)
                polozenieKat = polozenieY > 0 ? -90 : 90;
            else
                polozenieKat = Math.Atan2(polozenieY, polozenieX) * 180 / Math.PI;

            //ustalenie nowych kątów i rozklad skladowych
            double wektor1katRoboczy = wektor1kat - polozenieKat;
            if (wektor1katRoboczy > 180)
                wektor1katRoboczy -= 360;
            if (wektor1katRoboczy <= -180)
                wektor1katRoboczy += 360;
            double wektorP1wartosc = Math.Sin(wektor1katRoboczy) * wektor1wartosc;
            double wektorR1wartosc = Math.Cos(wektor1katRoboczy) * wektor1wartosc;

            double wektor2katRoboczy = wektor2kat - polozenieKat;
            if (wektor2katRoboczy > 180)
                wektor2katRoboczy -= 360;
            if (wektor2katRoboczy <= -180)
                wektor2katRoboczy += 360;
            double wektorP2wartosc = Math.Sin(wektor2katRoboczy) * wektor2wartosc;
            double wektorR2wartosc = Math.Cos(wektor2katRoboczy) * wektor2wartosc;

            //zapobieganie sklejaniu atomów
            if (!(wektor1katRoboczy < 90 && wektor1katRoboczy > -90) && (wektor2katRoboczy < 90 && wektor2katRoboczy > -90))
                return;

            //obliczenie wektorów wyjsciowych
            wektor1wartosc = Math.Sqrt(wektorP1wartosc * wektorP1wartosc + wektorR2wartosc * wektorR2wartosc);
            if (wektorP1wartosc == 0)
                wektor1katRoboczy = wektorR2wartosc > 0 ? 0 : 180;
            else if (wektorR2wartosc == 0)
                wektor1katRoboczy = wektorP1wartosc > 0 ? -90 : 90;
            else
                wektor1katRoboczy = Math.Atan2(wektorP1wartosc, wektorR2wartosc) * 180 / Math.PI;

            wektor2wartosc = Math.Sqrt(wektorP2wartosc * wektorP2wartosc + wektorR1wartosc * wektorR1wartosc);
            if (wektorP2wartosc == 0)
                wektor2katRoboczy = wektorR1wartosc > 0 ? 0 : 180;
            else if (wektorR1wartosc == 0)
                wektor2katRoboczy = wektorP2wartosc > 0 ? -90 : 90;
            else
                wektor2katRoboczy = Math.Atan2(wektorP2wartosc, wektorR1wartosc) * 180 / Math.PI;

            //powrót do orginalnego układu współrzędnych
            wektor1kat = wektor1katRoboczy + polozenieKat;
            wektor2kat = wektor2katRoboczy + polozenieKat;

            //powrót do wspolrzednych x-y
            this.velY = -Math.Sin(wektor1kat) * wektor1wartosc;
            this.velX = Math.Cos(wektor1kat) * wektor1wartosc;
            collidedParticle.velY = -Math.Sin(wektor2kat) * wektor2wartosc;
            collidedParticle.velX = Math.Cos(wektor2kat) * wektor2wartosc;
        }
    }

    public class geometry
    {
        public static double distance(double x1 ,double y1 ,double x2, double y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return (double)Math.Sqrt((dy * dy + dx * dx));
        }
        public static double distance_nosqrt(double x1, double y1, double x2, double y2)
        {
            double dx = x1 - x2;
            double dy = y1 - y2;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            return (dy * dy + dx * dx);
        }
    }
}
