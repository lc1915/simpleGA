using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace simpleGA
{
    public partial class Form1 : Form
    {
        Bitmap image;
        Graphics formGraphics;
        double Max_ratio;//图像放大比率

        static int max = 101;
        static int maxqvehicle = 1024;
        static int maxdvehicle = 1024;
        Random ra;

        int K;//最多使用车数目
        int KK;//实际使用车数
        int L;//客户数目,染色体长度
        double Pw;// W1, W2, W3;//惩罚权重
        double Pc, Pm;//交叉概率和变异概率

        int scale;//种群规模
        int T;//进化代数
        int t;//当前代数

        int[] bestGh = new int[max];//所有代数中最好的染色体
        double[] timeGh = new double[max];//所有代数中最好的染色体
        double bestEvaluation;//所有代数中最好的染色体的适应度
        int bestT;//最好的染色体出现的代数

        double decodedEvaluation;//解码后所有车辆所走路程总和........................

        double[,] vehicle = new double[max, 3];//K下标从1开始到K，0列表示车的最大载重量，1列表示车行驶的最大距离，2列表示速度
        int[] decoded = new int[max];//染色体解码后表达的每辆车的服务的客户的顺序
        double[,] guest_distance = new double[max, max];//客户距离
        double[] guest_weight = new double[max];//客户需求量

        int[,] oldGroup = new int[max, max];//初始种群，父代种群，行数表示种群规模，一行代表一个个体，即染色体，列表示染色体基因片段
        int[,] newGroup = new int[max, max];//新的种群，子代种群
        double[] Fitness = new double[max];//种群适应度，表示种群中各个个体的适应度
        double[] Pi = new double[max];//种群中各个个体的累计概率

        double[] x1 = new double[max];
        double[] y1 = new double[max];

        //初始化函数
        void initdata()
        {
            int i, j;

            Max_ratio = 20;//图像显示比例
            decodedEvaluation = 0;//解码后所有车辆所走路程总和

            Pw = 300;//车辆超额惩罚权重
            L = 20;//客户数目,染色体长度
            K = 5;//最大车数目
            scale = 100;//种群规模
            Pc = 0.9;//交叉概率
            Pm = 0.9;//变异概率，实际为(1-Pc)*0.9=0.09
            T = 400;//进化代数

            bestEvaluation = 0;//所有代数中最好的染色体的适应度


	        //车辆最大载重和最大行驶
	        vehicle[1,0]=8.0;
	        vehicle[1,1]=50.0;
	        vehicle[2,0]=8.0;
	        vehicle[2,1]=50.0;
	        vehicle[3,0]=8.0;
	        vehicle[3,1]=50.0;
	        vehicle[4,0]=8.0;
	        vehicle[4,1]=50.0;
	        vehicle[5,0]=8.0;
	        vehicle[5,1]=50.0;

            vehicle[6, 0] = maxqvehicle;//限制最大
            vehicle[6, 1] = maxdvehicle;

            //客户坐标
	        x1[0]=14.5;y1[0]=13.0;guest_weight[0]=0.0;
	        x1[1]=12.8;y1[1]=8.5;guest_weight[1]=0.1;
	        x1[2]=18.4;y1[2]=3.4;guest_weight[2]=0.4;
	        x1[3]=15.4;y1[3]=16.6;guest_weight[3]=1.2;
	        x1[4]=18.9;y1[4]=15.2;guest_weight[4]=1.5;
	        x1[5]=15.5;y1[5]=11.6;guest_weight[5]=0.8;
	        x1[6]=3.9;y1[6]=10.6;guest_weight[6]=1.3;
	        x1[7]=10.6;y1[7]=7.6;guest_weight[7]=1.7;
	        x1[8]=8.6;y1[8]=8.4;guest_weight[8]=0.6;
	        x1[9]=12.5;y1[9]=2.1;guest_weight[9]=1.2;
	        x1[10]=13.8;y1[10]=15.2;guest_weight[10]=0.4;
	        x1[11]=6.7;y1[11]=16.9;guest_weight[11]=0.9;
	        x1[12]=14.8;y1[12]=7.6;guest_weight[12]=1.3;
	        x1[13]=1.8;y1[13]=8.7;guest_weight[13]=1.3;
	        x1[14]=17.1;y1[14]=11.0;guest_weight[14]=1.9;
	        x1[15]=7.4;y1[15]=1.0;guest_weight[15]=1.7;
	        x1[16]=0.2;y1[16]=2.8;guest_weight[16]=1.1;
	        x1[17]=11.9;y1[17]=19.8;guest_weight[17]=1.5;
	        x1[18]=13.2;y1[18]=15.1;guest_weight[18]=1.6;
	        x1[19]=6.4;y1[19]=5.6;guest_weight[19]=1.7;
	        x1[20]=9.6;y1[20]=14.8;guest_weight[20]=1.5;

            //客户之间距离
            for (i = 0; i <= L; i++)
            {
                for (j = 0; j <= L; j++)
                {
                    guest_distance[i, j] = Math.Pow((x1[i] - x1[j]) * (x1[i] - x1[j]) + (y1[i] - y1[j]) * (y1[i] - y1[j]), 0.5);
                }
            }

        }

        //染色体评价函数，输入一个染色体，得到该染色体评价值
        double Evaluate(int[] Gh)
        {
            //染色体从下标0开始到L-1；
            int i, j;//i车的编号，j客户编号
            int flag;//超额使用的车数
            double cur_d, cur_q, evaluation;//当前车辆行驶距离，载重量，评价值，即各车行驶总里程

            cur_d = guest_distance[0, Gh[0]];//Gh[0]表示第一个客户，
            cur_q = guest_weight[Gh[0]];

            i = 1;//从1号车开始，默认第一辆车能满足第一个客户的需求
            evaluation = 0;//评价值初始为0
            flag = 0;//表示车辆数未超额

            for (j = 1; j < L; j++)
            {
                cur_q = cur_q + guest_weight[Gh[j]];
                cur_d = cur_d + guest_distance[Gh[j], Gh[j - 1]];

                //如果当前客户需求大于车的最大载重，或者距离大于车行驶最大距离，调用下一辆车
                if (cur_q > vehicle[i, 0] || cur_d + guest_distance[Gh[j], 0] > vehicle[i, 1])//还得加上返回配送中心距离
                {
                    i = i + 1;//使用下一辆车
                    evaluation = evaluation + cur_d - guest_distance[Gh[j], Gh[j - 1]] + guest_distance[Gh[j - 1], 0];
                    cur_d = guest_distance[0, Gh[j]];//从配送中心到当前客户j距离
                    cur_q = guest_weight[Gh[j]];
                }
            }
            evaluation = evaluation + cur_d + guest_distance[Gh[L - 1], 0];//加上最后一辆车走的距离

            flag = i - K;//看车辆使用数目是否大于规定车数，最多只超一辆车
            if (flag < 0)
                flag = 0;//不大于则不惩罚

            evaluation = evaluation + flag * Pw;//超额车数乘以惩罚权重
            return 10 / evaluation;//压缩评价值

        }

        //染色体解码函数，输入一个染色体，得到该染色体表达的每辆车的服务的客户的顺序
        void Decoding(int[] Gh)
        {
            //染色体从下标0开始到L-1；
            int i, j;//i车的编号，j客户编号
            double cur_d, cur_q, evaluation;//当前车辆行驶距离，载重量，评价值，即各车行驶总里程


            cur_d = guest_distance[0, Gh[0]];//Gh[0]表示第一个客户，
            cur_q = guest_weight[Gh[0]];

            i = 1;//从1号车开始，默认第一辆车能满足第一个客户的需求
            decoded[i] = 1;
            evaluation = 0;
            
            for (j = 1; j < L; j++)
            {
                cur_q = cur_q + guest_weight[Gh[j]];
                cur_d = cur_d + guest_distance[Gh[j], Gh[j - 1]];
               
                //如果当前客户需求大于车的最大载重，或者距离大于车行驶最大距离，调用下一辆车
                if (cur_q > vehicle[i, 0] || cur_d + guest_distance[Gh[j], 0] > vehicle[i, 1])
                {
                    i = i + 1;//使用下一辆车
                    decoded[i] = decoded[i - 1] + 1;//
                    evaluation = evaluation + cur_d - guest_distance[Gh[j], Gh[j - 1]] + guest_distance[Gh[j - 1], 0];
                    cur_d = guest_distance[0, Gh[j]];//从配送中心到当前客户j距离
                    cur_q = guest_weight[Gh[j]];

                }
                else
                {
                    decoded[i] = decoded[i] + 1;//
                }
            }

            decodedEvaluation = evaluation + cur_d + guest_distance[Gh[L - 1], 0];//加上最后一辆车走的距离
            KK = i;

        }

        //初始化种群
        void initGroup()
        {
            int i, j, k;

            int minValue = 0;
            int maxValue = 65535;

            for (k = 0; k < scale; k++)//种群数
            {
                oldGroup[k, 0] = ra.Next(minValue, maxValue) % L + 1;
                for (i = 1; i < L; i++)//染色体长度
                {
                Lab1:
                    oldGroup[k, i] = ra.Next(minValue, maxValue) % L + 1;
                    for (j = 0; j < i; j++)
                    { if (oldGroup[k, i] == oldGroup[k, j]) goto Lab1; }
                }

            }

            ////////////////////////////////////////////////////////显示初始化种群
            string l = "";
            for (k = 0; k < scale; k++)
            {
                //printf("第%d个个体\t", k);
                l = "";
                for (i = 0; i < L; i++)
                {
                    l = l + " " + oldGroup[k, i].ToString();
                }
                listBox1.Items.Add(l);
            }
            ////////////////////////////////////////////////////////显示初始化种群
        }

        //计算种群中各个个体的累积概率，前提是已经计算出各个个体的适应度Fitness[max]，作为赌轮选择策略一部分，Pi[max]
        void Count_rate()
        {
            int k;
            double sumFitness = 0;//适应度总和

            for (k = 0; k < scale; k++)
            {
                sumFitness += Fitness[k];
            }

            //计算各个个体累计概率
            Pi[0] = Fitness[0] / sumFitness;
            for (k = 1; k < scale; k++)
            {
                Pi[k] = Fitness[k] / sumFitness + Pi[k - 1];
            }
        }

        //复制染色体，k表示新染色体在种群中的位置，kk表示旧的染色体在种群中的位置
        void copyGh(int k, int kk)
        {
            int i;
            for (i = 0; i < L; i++)
            {
                newGroup[k, i] = oldGroup[kk, i];
            }
        }

        //挑选某代种群中适应度最高的个体，直接复制到子代中，前提是已经计算出各个个体的适应度Fitness[max]
        void Select_bestGh()
        {
            int k, i, maxid;
            double maxevaluation;
            int[] tempGhh = new int[max];

            maxid = 0;
            maxevaluation = Fitness[0];
            for (k = 1; k < scale; k++)
            {
                if (maxevaluation < Fitness[k])
                {
                    maxevaluation = Fitness[k];
                    maxid = k;
                }
            }


            if (bestEvaluation < maxevaluation)
            {
                bestEvaluation = maxevaluation;
                bestT = t;//最好的染色体出现的代数;
                for (i = 0; i < L; i++)
                {
                    bestGh[i] = oldGroup[maxid, i];
                }

            }

            //复制染色体，k表示新染色体在种群中的位置，kk表示旧的染色体在种群中的位置
            copyGh(0, maxid);//将当代种群中适应度最高的染色体k复制到新种群中，排在第一位0
        }

        //产生随机数

        int select()
        {
            int k;

            int minValue = 0;
            int maxValue = 65535;

            double ran1;
            ran1 = ra.Next(minValue, maxValue) % 1000 / 1000.0;//////////////////////////////////////////////////////////////////////////////////产生方式
            //printf("随机产生：%f",ran1);
            for (k = 0; k < scale; k++)
            {
                if (ran1 <= Pi[k])
                {
                    break;
                }
            }

            return k;
        }

        //类OX交叉算子
        void OXCross(int k1, int k2)
        {
            int i, j, k, flag;
            int ran1, ran2, temp;
            int[] Gh1 = new int[max];
            int[] Gh2 = new int[max];

            int minValue = 0;
            int maxValue = 65535;


            ran1 = ra.Next(minValue, maxValue) % L;
        Lab2: ran2 = ra.Next(minValue, maxValue) % L;
            if (ran1 == ran2)
                goto Lab2;

            if (ran1 > ran2)//确保ran1<ran2
            {
                temp = ran1;
                ran1 = ran2;
                ran2 = temp;
            }

            flag = ran2 - ran1 + 1;//删除重复基因前染色体长度

            for (i = 0, j = ran1; i < flag; i++, j++)
            {
                Gh1[i] = newGroup[k2, j];
                Gh2[i] = newGroup[k1, j];
            }
            //已近赋值i=ran2-ran1个基因


            for (k = 0, j = flag; j < L; j++)//染色体长度
            {
            Lab3:
                Gh1[j] = newGroup[k1, k++];
                for (i = 0; i < flag; i++)
                { if (Gh1[i] == Gh1[j]) goto Lab3; }
            }

            for (k = 0, j = flag; j < L; j++)//染色体长度
            {
            Lab4:
                Gh2[j] = newGroup[k2, k++];
                for (i = 0; i < flag; i++)
                { if (Gh2[i] == Gh2[j]) goto Lab4; }
            }

            for (i = 0; i < L; i++)
            {
                newGroup[k1, i] = Gh1[i];//交叉完毕放回种群
                newGroup[k2, i] = Gh2[i];//交叉完毕放回种群
            }
        }

        //多次对换变异算子
        void OnCVariation(int k)
        {
            //k表示种群中第k个染色体
            int i;
            int ran1, ran2, temp;
            int count;//对换次数

            //count=5;
            int minValue = 0;
            int maxValue = 65535;

            count = ra.Next(minValue, maxValue) % L;

            for (i = 0; i < count; i++)
            {

                ran1 = ra.Next(minValue, maxValue) % L;
            Lab8: ran2 = ra.Next(minValue, maxValue) % L;
                if (ran1 == ran2)
                    goto Lab8;
                temp = newGroup[k, ran1];
                newGroup[k, ran1] = newGroup[k, ran2];
                newGroup[k, ran2] = temp;
            }

        }

        //进化函数，保留最优
        void Evolution()
        {
            int k, selectId;

            double r;//大于0小于1的随机数

            //挑选某代种群中适应度最高的个体
            Select_bestGh();
           
            //赌轮选择策略挑选scale-1个下一代个体
            for (k = 1; k < scale; k++)
            {
                selectId = select();
                copyGh(k, selectId);
            }

            ////////////////////////////////////////////////////////////////////////////////交叉方法
            for (k = 1; k + 1 < scale / 2; k = k + 2)
            {
                r = ra.Next(0, 65535) % 1000 / 1000.0;//////产生概率
                if (r < Pc)
                {
                    OXCross(k, k + 1);//进行交叉
                }
                else
                {
                    r = ra.Next(0, 65535) % 1000 / 1000.0;
                    if (r < Pm)
                    {
                        OnCVariation(k);
                    }
                    r = ra.Next(0, 65535) % 1000 / 1000.0;
                    if (r < Pm)
                    {
                        OnCVariation(k + 1);
                    }
                }
            }
            if (k == scale / 2 - 1)//剩最后一个染色体没有交叉L-1
            {
                r = ra.Next(0, 65535) % 1000 / 1000.0;
                if (r < Pm)
                {
                    OnCVariation(k);
                }
            }

        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        void paintLine(int[] line, int color)
        {
            int i, xa, ya, xb, yb;

            Pen p = new Pen(Color.Blue, 1);//画笔


            int type = color % 10;
            switch (type)
            {
                case 0:
                    p.Color = Color.Black;
                    break;
                case 1:
                    p.Color = Color.Gray;
                    break;
                case 2:
                    p.Color = Color.Yellow;
                    break;
                case 3:
                    p.Color = Color.Green;
                    break;
                case 4:
                    p.Color = Color.Orange;
                    break;
                case 5:
                    p.Color = Color.YellowGreen;
                    break;
                case 6:
                    p.Color = Color.Brown;
                    break;
                case 7:
                    p.Color = Color.Purple;
                    break;
                case 8:
                    p.Color = Color.SaddleBrown;
                    break;
                case 9:
                    p.Color = Color.MidnightBlue;
                    break;
            }

            for (i = 2; i <= line[0]; i++)
            {
                xa = Convert.ToInt32(x1[line[i - 1]] * Max_ratio);
                ya = Convert.ToInt32(y1[line[i - 1]] * Max_ratio);

                xb = Convert.ToInt32(x1[line[i]] * Max_ratio);
                yb = Convert.ToInt32(y1[line[i]] * Max_ratio);
                formGraphics.DrawLine(p, new Point(xa, ya), new Point(xb, yb));//画线
            }

            paintPoint();
        }

        void paintPoint()
        {
            int i, x, y;

            System.Drawing.SolidBrush my0 = new System.Drawing.SolidBrush(System.Drawing.Color.Blue);//画刷
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);//画刷

            x = Convert.ToInt32(x1[0] * Max_ratio);
            y = Convert.ToInt32(y1[0] * Max_ratio);
            formGraphics.FillEllipse(my0, new Rectangle(x, y, 5, 5));//画实心椭圆

            Brush blackBrush = Brushes.Black;
            Font myFont1 = new Font("Hacttenschweiler", 7);
            formGraphics.DrawString("0", myFont1, blackBrush, new Point(x, y));//写字


            for (i = 1; i <= L; i++)
            {
                x = Convert.ToInt32(x1[i] * Max_ratio);
                y = Convert.ToInt32(y1[i] * Max_ratio);
                formGraphics.FillEllipse(myBrush, new Rectangle(x, y, 5, 5));//画实心椭圆
                formGraphics.DrawString(i.ToString(), myFont1, blackBrush, new Point(x, y));//写字
            }
            pictureBox1.Image = (Image)image;

        }

        void inti()
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();

            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";

            pictureBox1.Image = null;
            image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            formGraphics = Graphics.FromImage(image);
            formGraphics.Clear(panel1.BackColor);
        }


        private void button1_Click(object sender, EventArgs e)
        {

            inti();
            progressBar1.Value = 0;
            ra = new Random(unchecked((int)DateTime.Now.Ticks));//时间种子

            int i, j, k;

            //初始化数据，不同问题初始化数据不一样
            initdata();

            //初始化种群
            initGroup();
            paintPoint();//画点*************************************************************
            int[] tempGA = new int[L];

            //计算初始化种群适应度，Fitness[max]
            for (k = 0; k < scale; k++)
            {
                for (i = 0; i < L; i++)
                {
                    tempGA[i] = oldGroup[k, i];
                }

                Fitness[k] = Evaluate(tempGA);
            }

            //计算初始化种群中各个个体的累积概率，Pi[max]
            Count_rate();

            TimeSpan ts1 = new TimeSpan(DateTime.Now.Ticks);
            for (t = 0; t < T; t++)
            {
                Evolution();//进化函数，保留最优

                for (k = 0; k < scale; k++)//将新种群newGroup复制到旧种群oldGroup中，准备下一代进化
                {
                    for (i = 0; i < L; i++)
                    {
                        oldGroup[k, i] = newGroup[k, i];
                    }
                }

                //计算种群适应度，Fitness[max]
                for (k = 0; k < scale; k++)
                {
                    for (i = 0; i < L; i++)
                    {
                        tempGA[i] = oldGroup[k, i];
                    }
                    Fitness[k] = Evaluate(tempGA);
                }
                //计算种群中各个个体的累积概率，Pi[max]
                Count_rate();
                //进度条
                progressBar1.Value = t % 100;
            }
            progressBar1.Value = 100;

            TimeSpan ts2 = new TimeSpan(DateTime.Now.Ticks);
            TimeSpan ts = ts2.Subtract(ts1).Duration();
            //时间差的绝对值 
            labTime.Text = "运行时间：" + ts.TotalMilliseconds.ToString();

            //最后种群
            string ll = "";
            for (k = 0; k < scale; k++)
            {
                ll = "";
                for (i = 0; i < L; i++)
                {
                    ll = ll + " " + oldGroup[k, i].ToString();
                }
                listBox2.Items.Add(ll);
            }

            /*
            //最后种群适应度，Fitness[max]
            for (k = 0; k < scale; k++)
            {
                listBox5.Items.Add(Fitness[k]);

            }

            //最后种群中各个个体的累积概率，Pi[max]
            for (k = 0; k < scale; k++)
            {
                listBox6.Items.Add(Pi[k]);
            }
             * */

            //出现代数
            textBox1.Text = bestT.ToString() + " 代";
            //染色体评价值
            textBox2.Text = bestEvaluation.ToString() + " 或 " + (10 / bestEvaluation).ToString();

            //最好的染色体
            string s11 = "";
            for (i = 0; i < L; i++)
            {
                s11 = s11 + " " + bestGh[i].ToString();
            }
            textBox3.Text = s11;

            //最好的染色体解码
            Decoding(bestGh);
            //使用车数
            textBox4.Text = KK.ToString() + "辆";
            //解码
            string s22 = "";
            for (i = 0; i <= KK; i++)
            {
                s22 = s22 + " " + decoded[i].ToString();
            }
            textBox5.Text = s22;

            textBox6.Text = decodedEvaluation.ToString();

            string tefa = "";
            int tek;
            int[] templ = new int[max];

            for (i = 1; i <= KK; i++)
            {

                templ[1] = 0;
                tefa = "0-";
                tek = decoded[i - 1];
                for (j = tek, k = 2; j < decoded[i]; j++, k++)
                {
                    tefa = tefa + bestGh[j].ToString() + "-";
                    templ[k] = bestGh[j];
                }
                templ[k] = 0;
                templ[0] = k;
                tefa = k.ToString() + "-" + tefa + "0";
                listBox3.Items.Add(tefa);
                paintLine(templ, i);///////////////////////////////////////画线

            }
        }
    }
}
