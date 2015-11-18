using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Emotiv;
using System.Net.Sockets;
using System.Net;
using System.IO;  

namespace graduation_project
{
    public partial class main : Form
    {
        EEG_Logger p;
        bool isWifiConnected;
        Thread t;
        Thread c;
        Thread d;
        Thread f;
        Socket socket;
        bool fly_unlock;
        bool ear_CraftConnected;
        int numOfStu = 0;
        EdkDll.EE_CognitivAction_t actions;
        static Profile profile;
        string cogDataFileName;
        string cogNumFileName;
        string cogNum;
        int    hardLevel = 6; // 难度系数
        bool fll = true;
        string ac;
        string serNo;

        // 主方法体
        public main(string account, string serialno)
        {
            InitializeComponent();
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.main_FormClosing); 
            // 初始化
            ac = account;
            serNo = serialno;
            Init();
            p = new EEG_Logger();
           
            /*while (p.judgeOnContact() != true)
            {
                for (int i=0; i < 1000000; i++) ;
            }
            contactStatus.Text = "已连接";*/

            p.judgeOnContact();

            t = new Thread(judge);
            t.Start();

            c = new Thread(Cognitiv);
            c.Start();

            d = new Thread(follow);
            d.Start();

            Thread.Sleep(1000);
        }

        // 认知界面线程方法
        void follow()
        {
            int cog = 0;
            bool cogLift  = false;
            bool cogDrop  = false;
            bool cogLeft  = false;
            bool cogRight = false;
            bool cogFront = false;
            bool cogBack  = false;
            while (true)
            {
                cog = p.getCog();
                // 上
                if (cog == 1)
                {
                    if (!cogLift)
                    {
                        followCogStatusLiftDelegateMethod();
                        p.setCog(0);
                        cog = 0;
                        // 防止混乱
                        cogLift  = true;
                        cogDrop  = false;
                        cogLeft  = false;
                        cogRight = false;
                        cogFront = false;
                        cogBack  = false;
                    }
                }
                // 下
                if (cog == 2)
                {
                    if (!cogDrop)
                    {
                        followCogStatusDropDelegateMethod();
                        p.setCog(0);
                        cog = 0;
                        // 防止混乱
                        cogLift = false;
                        cogDrop = true;
                        cogLeft = false;
                        cogRight = false;
                        cogFront = false;
                        cogBack = false;
                    }
                }
                // 前
                if (cog == 3)
                {
                    if (!cogFront)
                    {
                        followCogStatusPushDelegateMethod();
                        p.setCog(0);
                        cog = 0;
                        // 防止混乱
                        cogLift = false;
                        cogDrop = false;
                        cogLeft = false;
                        cogRight = false;
                        cogFront = true;
                        cogBack = false;
                    }
                }
                // 后
                if (cog == 4)
                {
                    if (!cogBack)
                    {
                        followCogStatusPullDelegateMethod();
                        p.setCog(0);
                        cog = 0;
                        // 防止混乱
                        cogLift = false;
                        cogDrop = false;
                        cogLeft = false;
                        cogRight = false;
                        cogFront = false;
                        cogBack = true;
                    }
                }
                // 左
                else if (cog == 5)
                {
                    if (!cogLeft)
                    {
                        followCogStatusLeftDelegateMethod();
                        p.setCog(0);
                        cog = 0;
                        // 防止混乱
                        cogLift = false;
                        cogDrop = false;
                        cogLeft = true;
                        cogRight = false;
                        cogFront = false;
                        cogBack = false;
                        if (ear_CraftConnected == true)
                        {
                            zuofei();
                            richBox2DelegateMethod("左");
                            Thread.Sleep(2000); //  停顿两秒
                        }

                    }
                }
                // 右
                else if (cog == 6)
                {
                    if (!cogRight)
                    {
                        followCogStatusRightDelegateMethod();
                        p.setCog(0);
                        cog = 0;
                        // 防止混乱
                        cogLift = false;
                        cogDrop = false;
                        cogLeft = false;
                        cogRight = true;
                        cogFront = false;
                        cogBack = false;
                        if (ear_CraftConnected == true)
                        {
                            youfei();
                            richBox2DelegateMethod("右");
                            Thread.Sleep(3000); // 停顿两秒
                        }
                    }
                }
                // 空闲
                else
                {
                    followCogStatusResetDelegateMethod();
                    freeCogStatusDelegateMethod();
                    // 防止混乱
                    cogLift = false;
                    cogDrop = false;
                    cogLeft = false;
                    cogRight = false;
                    cogFront = false;
                    cogBack = false;
                }
            }
        }

        // 学习界面线程方法
        void Cognitiv()
        {
            int num = 0;  
            while (true)
            {
                if (p.getIsCognitivTrainingCompleted() == true)
                {
                    cogStatusDelegateMethod("学习完毕");
                    Thread.Sleep(1000);
                    num = 0;
                    cogBarStatusDelegateMethod(0, 10);
                    p.setIsCognitivTrainingCompleted();
                    if (numOfStu == 1)
                    {
                        if (!this.correct1.Visible)
                        {
                            cogNum += "N";
                            cogNumFileWrite(this.cogNumFileName, this.cogNum);
                        }
                        cogDataFileWrite(this.cogDataFileName);
                        cogStatusOk_1DelegateMethod();
                        
                    }
                    else if (numOfStu == 2)
                    {
                        if (!this.correct2.Visible)
                        {
                             cogNum += "U";
                             cogNumFileWrite(this.cogNumFileName, this.cogNum);
                        }
                        cogDataFileWrite(this.cogDataFileName);
                        cogStatusOk_2DelegateMethod();
                        
                    }
                    else if (numOfStu == 3)
                    {
                        
                            
                       if (!this.correct3.Visible)
                       {
                             cogNum += "D";
                             cogNumFileWrite(this.cogNumFileName, this.cogNum);
                       }
                       cogDataFileWrite(this.cogDataFileName);
                       cogStatusOk_3DelegateMethod();
                    }
                    else if (numOfStu == 4)
                    {
                        if (!this.correct4.Visible)
                        {
                             cogNum += "L";
                             cogNumFileWrite(this.cogNumFileName, this.cogNum);
                        }
                        cogDataFileWrite(this.cogDataFileName);
                        cogStatusOk_4DelegateMethod();
                    }
                    else if (numOfStu == 5)
                    {
                        if (!this.correct5.Visible)
                        {
                            cogNum += "R";
                            cogNumFileWrite(this.cogNumFileName, this.cogNum);
                        }
                        cogDataFileWrite(this.cogDataFileName);
                        cogStatusOk_5DelegateMethod();
                    }
                    else if(numOfStu == 6)
                    {
                        if (!this.correct6.Visible)
                        {
                            cogNum += "F";
                            cogNumFileWrite(this.cogNumFileName, this.cogNum);
                         }
                         cogDataFileWrite(this.cogDataFileName);
                         cogStatusOk_6DelegateMethod();
                    }
                    else if (numOfStu == 7)
                    {
                         if (!this.correct7.Visible)
                         {
                            cogNum += "B";
                            cogNumFileWrite(this.cogNumFileName, this.cogNum);
                         }
                         cogDataFileWrite(this.cogDataFileName);
                         cogStatusOk_7DelegateMethod();
                    }
                }
                else if (p.getIsCognitivTrainingSucceeded() == true)
                {
                    Thread.Sleep(500);
                    cogStatusDelegateMethod("学习成功");
                    num = 0;
                    cogBarStatusDelegateMethod(0, 10);
                }
                else if (p.getIsCognitivTrainingStarted() == true)
                {
                    cogStatusDelegateMethod("正在学习");
                    if (num < 10) num++;
                    cogBarStatusDelegateMethod(num, 10);
                    Thread.Sleep(700);
                }
                else if (p.getIsCognitivTrainingRejected() == true)
                {
                    cogStatusDelegateMethod("学习失败");
                }
                else
                {
                    cogStatusDelegateMethod("状态栏");
                }
                Thread.Sleep(100);
            }
        }

        // 状态界面线程方法
        void judge()
        {
            Thread.Sleep(1000);
            while (true)
            {
                //执行 使得状态改变
                p.judgeOnContact();

                //连接状态
                if (p.getConnectedStatus() == true)
                {
                    string str = "已连接";
                    this.contactStatusDelegateMethod(str);
                }
                else
                {
                    string str = "未连入";
                    this.contactStatusDelegateMethod(str);
                    electricPowerDelegateMethod(0, 5); //图形显示
                    electricPowerStatusDelegateMethod("0%");      //比例显示
                    this.starttimeDelegateMethod("00.00 s");
                }
                //电量的显示
                if (p.getConnectedStatus() == true)
                {
                    //开始时间的显示
                    double timeFromStart = ((double)((int)((p.getTimeFromStart() + 0.005) * 100))) / 100;
                    this.starttimeDelegateMethod(timeFromStart.ToString() + " s");
                }

                //电量的显示
                if (p.getConnectedStatus() == true)
                {
                    Int32 chargeLevel = p.getChargeLevel();
                    Int32 maxChargeLevel = p.getMaxChargeLevel();
                    if (chargeLevel < 0) chargeLevel = 0;
                    if (maxChargeLevel < 0) chargeLevel = 5;
                    String chargePercentage = ((Int32)((chargeLevel * 1.0 / maxChargeLevel) * 100)).ToString() + "%";
                    electricPowerDelegateMethod(chargeLevel, maxChargeLevel); //图形显示
                    electricPowerStatusDelegateMethod(chargePercentage);      //比例显示
                }

                int signalStrength = p.getSignalStrength();
                if (signalStrength == 2)
                {
                    signalStrengthGoodDelegateMethod();
                    signalStrengthStatusDelegateMethod("强");
                }
                else if (signalStrength == 1)
                {
                    signalStrengthBadDelegateMethod();
                    signalStrengthStatusDelegateMethod("弱");
                }
                else
                {
                    signalStrengthNoSignalDelegateMethod();
                    signalStrengthStatusDelegateMethod("无信号");
                    p.disconnected();
                }

                //信号可用性的显示
                if (p.getConnectedStatus() == true)
                {
                    int numCqChan = p.getNumContactQualityChannelsChargeLevel();
                    EdkDll.EE_EEG_ContactQuality_t[] cqfc = p.getContactQualityFromAllChannels();
                    bool flagP3 = false;
                    bool flagP4 = false;
                    for (Int32 i = 0; i < numCqChan; ++i)
                    {
                        //信号差
                        if (cqfc[i] == EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_VERY_BAD)
                        {
                            // P3信号
                            if (i == 0)
                            {
                                this.p3BadDelegateMethod();
                                flagP3 = false;
                            }
                            // P4信号
                            else if (i == 1)
                            {
                                this.p4BadDelegateMethod();
                                flagP4 = false;
                            }
                            // AF3信号
                            else if (i == 2 && flagP3 == true && flagP4 == true)
                            {
                                this.af3BadDelegateMethod();
                            }
                            // F7信号
                            else if (i == 3 && flagP3 == true && flagP4 == true)
                            {
                                this.f7BadDelegateMethod();
                            }
                            // F3信号
                            else if (i == 4 && flagP3 == true && flagP4 == true)
                            {
                                this.f3BadDelegateMethod();
                            }
                            // FC5信号
                            else if (i == 5 && flagP3 == true && flagP4 == true)
                            {
                                this.fc5BadDelegateMethod();
                            }
                            // T7信号
                            else if (i == 6 && flagP3 == true && flagP4 == true)
                            {
                                this.t7BadDelegateMethod();
                            }
                            // T7信号
                            else if (i == 7 && flagP3 == true && flagP4 == true)
                            {
                                this.p7BadDelegateMethod();
                            }
                            // O1信号
                            else if (i == 8 && flagP3 == true && flagP4 == true)
                            {
                                this.o1BadDelegateMethod();
                            }
                            // O2信号
                            else if (i == 9 && flagP3 == true && flagP4 == true)
                            {
                                this.o2BadDelegateMethod();
                            }
                            // P8信号
                            else if (i == 10 && flagP3 == true && flagP4 == true)
                            {
                                this.p8BadDelegateMethod();
                            }
                            // T8信号
                            else if (i == 11 && flagP3 == true && flagP4 == true)
                            {
                                this.t8BadDelegateMethod();
                            }
                            // FC6信号
                            else if (i == 12 && flagP3 == true && flagP4 == true)
                            {
                                this.fc6BadDelegateMethod();
                            }
                            // F4信号
                            else if (i == 13 && flagP3 == true && flagP4 == true)
                            {
                                this.f4BadDelegateMethod();
                            }
                            // F8信号
                            else if (i == 14 && flagP3 == true && flagP4 == true)
                            {
                                this.f8BadDelegateMethod();
                            }
                            // AF4信号
                            else if (i == 15 && flagP3 == true && flagP4 == true)
                            {
                                this.af4BadDelegateMethod();
                            }
                        }
                        //信号不好
                        else if (cqfc[i] == EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_POOR)
                        {
                            // P3信号
                            if (i == 0)
                            {
                                this.p3PoorDelegateMethod();
                                flagP3 = false;
                            }
                            // P4信号
                            else if (i == 1)
                            {
                                this.p4PoorDelegateMethod();
                                flagP4 = false;
                            }
                            // AF3信号
                            else if (i == 2 && flagP3 == true && flagP4 == true)
                            {
                                this.af3PoorDelegateMethod();
                            }
                            // F7信号
                            else if (i == 3 && flagP3 == true && flagP4 == true)
                            {
                                this.f7PoorDelegateMethod();
                            }
                            // F3信号
                            else if (i == 4 && flagP3 == true && flagP4 == true)
                            {
                                this.f3PoorDelegateMethod();
                            }
                            // FC5信号
                            else if (i == 5 && flagP3 == true && flagP4 == true)
                            {
                                this.fc5PoorDelegateMethod();
                            }
                            // T7信号
                            else if (i == 6 && flagP3 == true && flagP4 == true)
                            {
                                this.t7PoorDelegateMethod();
                            }
                            // T7信号
                            else if (i == 7 && flagP3 == true && flagP4 == true)
                            {
                                this.p7PoorDelegateMethod();
                            }
                            // O1信号
                            else if (i == 8 && flagP3 == true && flagP4 == true)
                            {
                                this.o1PoorDelegateMethod();
                            }
                            // O2信号
                            else if (i == 9 && flagP3 == true && flagP4 == true)
                            {
                                this.o2PoorDelegateMethod();
                            }
                            // P8信号
                            else if (i == 10 && flagP3 == true && flagP4 == true)
                            {
                                this.p8PoorDelegateMethod();
                            }
                            // T8信号
                            else if (i == 11 && flagP3 == true && flagP4 == true)
                            {
                                this.t8PoorDelegateMethod();
                            }
                            // FC6信号
                            else if (i == 12 && flagP3 == true && flagP4 == true)
                            {
                                this.fc6PoorDelegateMethod();
                            }
                            // F4信号
                            else if (i == 13 && flagP3 == true && flagP4 == true)
                            {
                                this.f4PoorDelegateMethod();
                            }
                            // F8信号
                            else if (i == 14 && flagP3 == true && flagP4 == true)
                            {
                                this.f8PoorDelegateMethod();
                            }
                            // AF4信号
                            else if (i == 15 && flagP3 == true && flagP4 == true)
                            {
                                this.af4PoorDelegateMethod();
                            }
                        }
                        //信号良好
                        else if (cqfc[i] == EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_FAIR)
                        {
                            // P3信号
                            if (i == 0)
                            {
                                this.p3FairDelegateMethod();
                                flagP3 = false;
                            }
                            // P4信号
                            else if (i == 1)
                            {
                                this.p4FairDelegateMethod();
                                flagP4 = false;
                            }
                            // AF3信号
                            else if (i == 2 && flagP3 == true && flagP4 == true)
                            {
                                this.af3FairDelegateMethod();
                            }
                            // F7信号
                            else if (i == 3 && flagP3 == true && flagP4 == true)
                            {
                                this.f7FairDelegateMethod();
                            }
                            // F3信号
                            else if (i == 4 && flagP3 == true && flagP4 == true)
                            {
                                this.f3FairDelegateMethod();
                            }
                            // FC5信号
                            else if (i == 5 && flagP3 == true && flagP4 == true)
                            {
                                this.fc5FairDelegateMethod();
                            }
                            // T7信号
                            else if (i == 6 && flagP3 == true && flagP4 == true)
                            {
                                this.t7FairDelegateMethod();
                            }
                            // T7信号
                            else if (i == 7 && flagP3 == true && flagP4 == true)
                            {
                                this.p7FairDelegateMethod();
                            }
                            // O1信号
                            else if (i == 8 && flagP3 == true && flagP4 == true)
                            {
                                this.o1FairDelegateMethod();
                            }
                            // O2信号
                            else if (i == 9 && flagP3 == true && flagP4 == true)
                            {
                                this.o2FairDelegateMethod();
                            }
                            // P8信号
                            else if (i == 10 && flagP3 == true && flagP4 == true)
                            {
                                this.p8FairDelegateMethod();
                            }
                            // T8信号
                            else if (i == 11 && flagP3 == true && flagP4 == true)
                            {
                                this.t8FairDelegateMethod();
                            }
                            // FC6信号
                            else if (i == 12 && flagP3 == true && flagP4 == true)
                            {
                                this.fc6FairDelegateMethod();
                            }
                            // F4信号
                            else if (i == 13 && flagP3 == true && flagP4 == true)
                            {
                                this.f4FairDelegateMethod();
                            }
                            // F8信号
                            else if (i == 14 && flagP3 == true && flagP4 == true)
                            {
                                this.f8FairDelegateMethod();
                            }
                            // AF4信号
                            else if (i == 15 && flagP3 == true && flagP4 == true)
                            {
                                this.af4FairDelegateMethod();
                            }
                        }
                        //信号好
                        else if (cqfc[i] == EdkDll.EE_EEG_ContactQuality_t.EEG_CQ_GOOD)
                        {
                            // P3信号
                            if (i == 0)
                            {
                                this.p3GoodDelegateMethod();
                                flagP3 = true;
                            }
                            // P4信号
                            else if (i == 1)
                            {
                                this.p4GoodDelegateMethod();
                                flagP4 = true;
                            }
                            // AF3信号
                            else if (i == 2 && flagP3 == true && flagP4 == true)
                            {
                                this.af3GoodDelegateMethod();
                            }
                            // F7信号
                            else if (i == 3 && flagP3 == true && flagP4 == true)
                            {
                                this.f7GoodDelegateMethod();
                            }
                            // F3信号
                            else if (i == 4 && flagP3 == true && flagP4 == true)
                            {
                                this.f3GoodDelegateMethod();
                            }
                            // FC5信号
                            else if (i == 5 && flagP3 == true && flagP4 == true)
                            {
                                this.fc5GoodDelegateMethod();
                            }
                            // T7信号
                            else if (i == 6 && flagP3 == true && flagP4 == true)
                            {
                                this.t7GoodDelegateMethod();
                            }
                            // T7信号
                            else if (i == 7 && flagP3 == true && flagP4 == true)
                            {
                                this.p7GoodDelegateMethod();
                            }
                            // O1信号
                            else if (i == 8 && flagP3 == true && flagP4 == true)
                            {
                                this.o1GoodDelegateMethod();
                            }
                            // O2信号
                            else if (i == 9 && flagP3 == true && flagP4 == true)
                            {
                                this.o2GoodDelegateMethod();
                            }
                            // P8信号
                            else if (i == 10 && flagP3 == true && flagP4 == true)
                            {
                                this.p8GoodDelegateMethod();
                            }
                            // T8信号
                            else if (i == 11 && flagP3 == true && flagP4 == true)
                            {
                                this.t8GoodDelegateMethod();
                            }
                            // FC6信号
                            else if (i == 12 && flagP3 == true && flagP4 == true)
                            {
                                this.fc6GoodDelegateMethod();
                            }
                            // F4信号
                            else if (i == 13 && flagP3 == true && flagP4 == true)
                            {
                                this.f4GoodDelegateMethod();
                            }
                            // F8信号
                            else if (i == 14 && flagP3 == true && flagP4 == true)
                            {
                                this.f8GoodDelegateMethod();
                            }
                            // AF4信号
                            else if (i == 15 && flagP3 == true && flagP4 == true)
                            {
                                this.af4GoodDelegateMethod();
                            }
                        }
                        //没信号
                        else
                        {
                            // P3信号
                            if (i == 0)
                            {
                                this.p3NoSignalDelegateMethod();
                                flagP3 = false;
                            }
                            // P4信号
                            else if (i == 1)
                            {
                                this.p4NoSignalDelegateMethod();
                                flagP4 = false;
                            }
                            if (flagP4 == false && flagP3 == false)
                            {
                                // 显示改为没信号
                                this.af3NoSignalDelegateMethod();
                                this.f7NoSignalDelegateMethod();
                                this.f3NoSignalDelegateMethod();
                                this.fc5NoSignalDelegateMethod();
                                this.t7NoSignalDelegateMethod();
                                this.p7NoSignalDelegateMethod();
                                this.o1NoSignalDelegateMethod();
                                this.o2NoSignalDelegateMethod();
                                this.p8NoSignalDelegateMethod();
                                this.t8NoSignalDelegateMethod();
                                this.fc6NoSignalDelegateMethod();
                                this.f4NoSignalDelegateMethod();
                                this.f8NoSignalDelegateMethod();
                                this.af4NoSignalDelegateMethod();
                            }
                        }
                    }
                }
                else
                {
                    // 显示改为没信号
                    this.p3NoSignalDelegateMethod();
                    this.p4NoSignalDelegateMethod();
                    this.af3NoSignalDelegateMethod();
                    this.f7NoSignalDelegateMethod();
                    this.f3NoSignalDelegateMethod();
                    this.fc5NoSignalDelegateMethod();
                    this.t7NoSignalDelegateMethod();
                    this.p7NoSignalDelegateMethod();
                    this.o1NoSignalDelegateMethod();
                    this.o2NoSignalDelegateMethod();
                    this.p8NoSignalDelegateMethod();
                    this.t8NoSignalDelegateMethod();
                    this.fc6NoSignalDelegateMethod();
                    this.f4NoSignalDelegateMethod();
                    this.f8NoSignalDelegateMethod();
                    this.af4NoSignalDelegateMethod();
                }
                Thread.Sleep(100);

                if (p.getConnectedStatus() == true && fll == true)
                {
                    fll = false;
                    // 读取本地脑波数据
                    if (File.Exists(cogDataFileName))
                    {
                        EmoEngine.Instance.LoadUserProfile(0, cogDataFileName);
                        profile = EmoEngine.Instance.GetUserProfile(0);
                        EmoEngine.Instance.SetUserProfile(0, profile);
                        Console.WriteLine("load profile");
                    }

                    // 读取本地脑波已存在的数据数量
                    // 读取本地脑波数据
                    if (File.Exists(cogNumFileName))
                    {
                        cogNum = cogNumFileRead(cogNumFileName);
                        for (int i = 0; i < cogNum.Length; i++)
                        {
                            if (cogNum[i].ToString() == "N")
                            {
                                cogStatusOk_1DelegateMethod();
                            }
                            else if (cogNum[i].ToString() == "U")
                            {
                                cogStatusOk_2DelegateMethod();
                            }
                            else if (cogNum[i].ToString() == "D")
                            {
                                cogStatusOk_3DelegateMethod();
                            }
                            else if (cogNum[i].ToString() == "L")
                            {
                                cogStatusOk_4DelegateMethod();
                            }
                            else if (cogNum[i].ToString() == "R")
                            {
                                cogStatusOk_5DelegateMethod();
                            }
                            else if (cogNum[i].ToString() == "F")
                            {
                                cogStatusOk_6DelegateMethod();
                            }
                            else if (cogNum[i].ToString() == "B")
                            {
                                cogStatusOk_7DelegateMethod();
                            }
                        }
                    }
                }
            }
        }

        // 初始化界面
        public void Init()
        {

            /* 信号点设计控件（透明化）
             * Start:2015/3/31 14:02 
             */
            this.cogDataFileName = @"emo\" + ac + ".emo";
            this.cogNumFileName = @"emo\" + ac + ".num";
            this.label20.Text = ac;
            this.label21.Text = serNo;

            // 1
            pb1_red.Parent = pictureBox;
            pb1_orange.Parent = pictureBox;
            pb1_yellow.Parent = pictureBox;
            pb1_green.Parent = pictureBox;

            // 2
            pb2_red.Parent = pictureBox;
            pb2_orange.Parent = pictureBox;
            pb2_yellow.Parent = pictureBox;
            pb2_green.Parent = pictureBox;

            // 3
            pb3_red.Parent = pictureBox;
            pb3_orange.Parent = pictureBox;
            pb3_yellow.Parent = pictureBox;
            pb3_green.Parent = pictureBox;

            // 4
            pb4_red.Parent = pictureBox;
            pb4_orange.Parent = pictureBox;
            pb4_yellow.Parent = pictureBox;
            pb4_green.Parent = pictureBox;

            // 5
            pb5_red.Parent = pictureBox;
            pb5_orange.Parent = pictureBox;
            pb5_yellow.Parent = pictureBox;
            pb5_green.Parent = pictureBox;

            // 6
            pb6_red.Parent = pictureBox;
            pb6_orange.Parent = pictureBox;
            pb6_yellow.Parent = pictureBox;
            pb6_green.Parent = pictureBox;

            // 7
            pb7_red.Parent = pictureBox;
            pb7_orange.Parent = pictureBox;
            pb7_yellow.Parent = pictureBox;
            pb7_green.Parent = pictureBox;

            // 8
            pb8_red.Parent = pictureBox;
            pb8_orange.Parent = pictureBox;
            pb8_yellow.Parent = pictureBox;
            pb8_green.Parent = pictureBox;

            // 9
            pb9_red.Parent = pictureBox;
            pb9_orange.Parent = pictureBox;
            pb9_yellow.Parent = pictureBox;
            pb9_green.Parent = pictureBox;

            // 10
            pb10_red.Parent = pictureBox;
            pb10_orange.Parent = pictureBox;
            pb10_yellow.Parent = pictureBox;
            pb10_green.Parent = pictureBox;

            // 11
            pb11_red.Parent = pictureBox;
            pb11_orange.Parent = pictureBox;
            pb11_yellow.Parent = pictureBox;
            pb11_green.Parent = pictureBox;

            // 12
            pb12_red.Parent = pictureBox;
            pb12_orange.Parent = pictureBox;
            pb12_yellow.Parent = pictureBox;
            pb12_green.Parent = pictureBox;

            // 13
            pb13_red.Parent = pictureBox;
            pb13_orange.Parent = pictureBox;
            pb13_yellow.Parent = pictureBox;
            pb13_green.Parent = pictureBox;

            // 14
            pb14_red.Parent = pictureBox;
            pb14_orange.Parent = pictureBox;
            pb14_yellow.Parent = pictureBox;
            pb14_green.Parent = pictureBox;

            // 15
            pb15_red.Parent = pictureBox;
            pb15_orange.Parent = pictureBox;
            pb15_yellow.Parent = pictureBox;
            pb15_green.Parent = pictureBox;

            // 16
            pb16_red.Parent = pictureBox;
            pb16_orange.Parent = pictureBox;
            pb16_yellow.Parent = pictureBox;
            pb16_green.Parent = pictureBox;

            // box
            box.Parent = back;

            /* 
             * 耳机状态说明（使用说明，固定）
             * Start:2015/3/31 15:40 
             */
            textBox1.Text = "步骤一：  在佩戴脑电波耳机之前请确保16个电子凹槽每一个都装好涂有溶液的垫。如果垫没涂上溶液将会接收不到信号，另外如果装好垫再涂溶液时请小心填涂。" + Environment.NewLine + Environment.NewLine
                          + "步骤二：  开关在Emotiv耳机确认内置电池充电提供电源通过寻找附近的蓝色LED电源开关，耳机的背面。如果耳机电池需要充电将电源开关拨至OFF位置和插入耳机电池充电器的Emotiv使用装有耳机迷你USB电缆。让耳机电池之前再次尝试至少充电15分钟。" + Environment.NewLine + Environment.NewLine
                          + "步骤三：  检验无线信号接收报告为好，在控制面板上的发动机状态盒公司。如果不是，请确保Emotiv加密狗插入计算机上的USB端口和单个LED对加密狗的上半连续或闪烁的很快。如果LED闪烁缓慢或不发光，然后将适配器从计算机，重新插入，并再次尝试。删除任何金属或密集的物理障碍物靠近器或耳机，并远离任何强大的电磁干扰源，如微波炉或大功率无线电发射机。" + Environment.NewLine + Environment.NewLine
                          + "步骤四：  轻轻拉开头巾和降低传感器臂到你的头上，从上往下放在Emotiv耳机，靠近头骨的后部。下一步，将耳机向前直到最靠近耳机的枢轴点的传感器直接位于上方的耳朵靠近你的发际线尽可能。调整适应，在前面的矩形车厢两端的头带，坐在舒适的上面和耳朵后面。倾斜的耳机使两个最低，最前面的传感器是对称的在你的额头和眉毛上方2到2.5英寸以上。最后，检查所有的传感器都摸你的头，如果不是，轻轻滑动耳机小到合适已经达到适合然后微调耳机。" + Environment.NewLine + Environment.NewLine
                          + "步骤五：  在控制面板的设置选项，Emotiv耳机显示的Emotiv耳机传感器阵列的自上而下的视角。每个传感器的接触质量是由颜色代码表示：" + Environment.NewLine + Environment.NewLine
                          + "      黑色      没信号" + Environment.NewLine + Environment.NewLine
                          + "      红色      非常差的信号" + Environment.NewLine + Environment.NewLine
                          + "      橙色      差的信号" + Environment.NewLine + Environment.NewLine
                          + "      黄色      清楚的信号" + Environment.NewLine + Environment.NewLine
                          + "      绿色      很好的信号" + Environment.NewLine + Environment.NewLine
                          + "步骤六：   从两个传感器的正上方，耳朵后面（这些都是参考传感器与头皮接触良好是必不可少的），调整传感器使它们成为你的头皮适当的接触（即显示在接触质量显示绿色）。如果指标：" + Environment.NewLine + Environment.NewLine
                          + "黑色：检查传感器有毛毡垫装。检查毛毡垫压坚决反对你的头皮。然后尝试重新滋润的毛毡垫。如果此问题仍然存在，这可能表明在Emotiv的耳机问题。" + Environment.NewLine + Environment.NewLine
                          + "黄色，橙色或红色：传感器不与头皮充分接触。检查一下，觉得垫舒适但坚定的与头皮接触。如果接触是足够的，确保毛毡垫是湿的。如果传感器的指示剂颜色变浅，信号质量的提高。如果传感器的指示剂颜色变暗，信号质量恶化。如果问题依然存在，尝试离别的头发在附近的电极，所以觉得垫使你的头皮更好的接触。" + Environment.NewLine + Environment.NewLine
                          + "步骤七：   重复步骤6直到所有其余的每个电极的传感器有足够的接触质量。虽然多数指标应显示绿色，通常会容忍的Emotiv检测到多个传感器的输入指标显示黄色。" + Environment.NewLine + Environment.NewLine
                          + "如果在任何时间（参考传感器位于正上方，耳朵后面）不再有一个良好的连接（即不显示绿色），立即恢复这些传感器的绿色在继续之前。" + Environment.NewLine;

            /* 表格列明设计控件（透明化）
             * Start:2015/4/1 14:02 
             */
            lb_t1.Parent = pb_table;
            lb_t2.Parent = pb_table;
            lb_t3.Parent = pb_table;
            //pictureBox1.Parent = pb_table;
            back.Parent = tabPage2;

            pb1_yellow.Visible = false;
            pb1_orange.Visible = false;
            pb1_green.Visible = false;
            pb1_red.Visible = false;

            pb2_yellow.Visible = false;
            pb2_orange.Visible = false;
            pb2_green.Visible = false;
            pb2_red.Visible = false;

            pb3_yellow.Visible = false;
            pb3_orange.Visible = false;
            pb3_green.Visible = false;
            pb3_red.Visible = false;

            pb4_yellow.Visible = false;
            pb4_orange.Visible = false;
            pb4_green.Visible = false;
            pb4_red.Visible = false;

            pb5_yellow.Visible = false;
            pb5_orange.Visible = false;
            pb5_green.Visible = false;
            pb5_red.Visible = false;

            pb6_yellow.Visible = false;
            pb6_orange.Visible = false;
            pb6_green.Visible = false;
            pb6_red.Visible = false;

            pb7_yellow.Visible = false;
            pb7_orange.Visible = false;
            pb7_green.Visible = false;
            pb7_red.Visible = false;

            pb8_yellow.Visible = false;
            pb8_orange.Visible = false;
            pb8_green.Visible = false;
            pb8_red.Visible = false;

            pb9_yellow.Visible = false;
            pb9_orange.Visible = false;
            pb9_green.Visible = false;
            pb9_red.Visible = false;

            pb10_yellow.Visible = false;
            pb10_orange.Visible = false;
            pb10_green.Visible = false;
            pb10_red.Visible = false;

            pb11_yellow.Visible = false;
            pb11_orange.Visible = false;
            pb11_green.Visible = false;
            pb11_red.Visible = false;

            pb12_yellow.Visible = false;
            pb12_orange.Visible = false;
            pb12_green.Visible = false;
            pb12_red.Visible = false;

            pb13_yellow.Visible = false;
            pb13_orange.Visible = false;
            pb13_green.Visible = false;
            pb13_red.Visible = false;

            pb14_yellow.Visible = false;
            pb14_orange.Visible = false;
            pb14_green.Visible = false;
            pb14_red.Visible = false;

            pb15_yellow.Visible = false;
            pb15_orange.Visible = false;
            pb15_green.Visible = false;
            pb15_red.Visible = false;

            pb16_yellow.Visible = false;
            pb16_orange.Visible = false;
            pb16_green.Visible = false;
            pb16_red.Visible = false;

            //电量初始化
            electricPower.Maximum = 5;
            electricPower.Value = 0;
            electricPowerStatus.Text = "0%";

            //信号强度初始化
            signalStrength1_red.Visible   = true;
            signalStrength2_red.Visible   = true;
            signalStrength1_green.Visible = false;
            signalStrength2_green.Visible = false;

            //学习状态对错初始化
            correct1.Visible = false;
            correct2.Visible = false;
            correct3.Visible = false;
            correct4.Visible = false;
            correct5.Visible = false;
            correct6.Visible = false;
            correct7.Visible = false;

            wrong1.Visible   = true;
            wrong2.Visible   = true;
            wrong3.Visible   = true;
            wrong4.Visible   = true;
            wrong5.Visible   = true;
            wrong6.Visible   = true;
            wrong7.Visible   = true;

            // 设置初试WIFI没连接
            isWifiConnected = false;

            // socket初始化
            socket = null;

            // 电机状态
            fly_unlock = false;

            // 耳机和四轴飞行器连接状态
            ear_CraftConnected = false;

            // 已存在的数据数量
            cogNum = "";
        }

        // 主窗体关闭事件
        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 线程释放
            t.Abort();
            c.Abort();
            d.Abort();
            
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {
          
        }

        private void main_Load(object sender, EventArgs e)
        {
            
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void pb1_yellow_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pb_logo_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void pb_table_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.uploadFileByHttp("http://localhost/emotive/index.php/Upload/upload.html", this.cogNumFileName);
            this.uploadFileByHttp("http://localhost/emotive/index.php/Upload/upload.html", this.cogDataFileName);
        }

        // 正常状态学习按钮
        private void button2_Click(object sender, EventArgs e)
        {
            EmoEngine.Instance.CognitivSetActivationLevel(0, 5);
            EmoEngine.Instance.CognitivSetTrainingAction(0, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            EmoEngine.Instance.CognitivSetTrainingControl(0, EdkDll.EE_CognitivTrainingControl_t.COG_START);
            numOfStu = 1;
        }

        // 向上状态学习按钮
        private void button3_Click(object sender, EventArgs e)
        {
            EmoEngine.Instance.CognitivSetActivationLevel(0, 5);
            actions = actions | EdkDll.EE_CognitivAction_t.COG_LIFT;
            EmoEngine.Instance.CognitivSetActiveActions(0, (UInt32)actions);
            EmoEngine.Instance.CognitivSetTrainingAction(0, EdkDll.EE_CognitivAction_t.COG_LIFT);
            EmoEngine.Instance.CognitivSetTrainingControl(0, EdkDll.EE_CognitivTrainingControl_t.COG_START);
            numOfStu = 2;
        }

        // 向下状态学习按钮
        private void button4_Click(object sender, EventArgs e)
        {
            EmoEngine.Instance.CognitivSetActivationLevel(0, 5);
            actions = actions | EdkDll.EE_CognitivAction_t.COG_DROP;
            EmoEngine.Instance.CognitivSetActiveActions(0, (UInt32)actions);
            EmoEngine.Instance.CognitivSetTrainingAction(0, EdkDll.EE_CognitivAction_t.COG_DROP);
            EmoEngine.Instance.CognitivSetTrainingControl(0, EdkDll.EE_CognitivTrainingControl_t.COG_START);
            numOfStu = 3;
        }

        // 向左状态学习按钮
        private void button5_Click(object sender, EventArgs e)
        {
            EmoEngine.Instance.CognitivSetActivationLevel(0, 5);
            actions = actions | EdkDll.EE_CognitivAction_t.COG_LEFT;
            EmoEngine.Instance.CognitivSetActiveActions(0, (UInt32)actions);
            EmoEngine.Instance.CognitivSetTrainingAction(0, EdkDll.EE_CognitivAction_t.COG_LEFT);
            EmoEngine.Instance.CognitivSetTrainingControl(0, EdkDll.EE_CognitivTrainingControl_t.COG_START);
            numOfStu = 4;
        }

        // 向右状态学习按钮
        private void button6_Click(object sender, EventArgs e)
        {
            EmoEngine.Instance.CognitivSetActivationLevel(0, 5);
            actions = actions | EdkDll.EE_CognitivAction_t.COG_RIGHT;
            EmoEngine.Instance.CognitivSetActiveActions(0, (UInt32)actions);
            EmoEngine.Instance.CognitivSetTrainingAction(0, EdkDll.EE_CognitivAction_t.COG_RIGHT);
            EmoEngine.Instance.CognitivSetTrainingControl(0, EdkDll.EE_CognitivTrainingControl_t.COG_START);
            numOfStu = 5;
        }

        // 向前状态学习按钮
        private void button7_Click(object sender, EventArgs e)
        {
            EmoEngine.Instance.CognitivSetActivationLevel(0, 5);
            actions = actions | EdkDll.EE_CognitivAction_t.COG_PUSH;
            EmoEngine.Instance.CognitivSetActiveActions(0, (UInt32)actions);
            EmoEngine.Instance.CognitivSetTrainingAction(0, EdkDll.EE_CognitivAction_t.COG_PUSH);
            EmoEngine.Instance.CognitivSetTrainingControl( 0, EdkDll.EE_CognitivTrainingControl_t.COG_START);
            numOfStu = 6;
        }

        // 向后状态学习按钮
        private void button8_Click(object sender, EventArgs e)
        {
            EmoEngine.Instance.CognitivSetActivationLevel(0, 5);
            actions = actions | EdkDll.EE_CognitivAction_t.COG_PULL;
            EmoEngine.Instance.CognitivSetActiveActions(0, (UInt32)actions);
            EmoEngine.Instance.CognitivSetTrainingAction(0, EdkDll.EE_CognitivAction_t.COG_PULL);
            EmoEngine.Instance.CognitivSetTrainingControl(0, EdkDll.EE_CognitivTrainingControl_t.COG_START);
            numOfStu = 7;
        }


        /////////////////////////////////////////////////////////////////////////////////////
        //
        //                            学习点击委托   2015/4/26  16:56
        //
        /////////////////////////////////////////////////////////////////////////////////////

        // 定义委托—状态1-OK
        private delegate void cogStatusOk_1Delegate();

        // 定于所要委托的方法
        private void cogStatusOk_1DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong1.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_1Delegate myDelegate = new cogStatusOk_1Delegate(cogStatusOk_1DelegateMethod);
                // Invoke delegate
                this.wrong1.Invoke(new Action(() =>
                {
                    this.wrong1.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong1.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.correct1.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_1Delegate myDelegate = new cogStatusOk_1Delegate(cogStatusOk_1DelegateMethod);
                // Invoke delegate
                this.correct1.Invoke(new Action(() =>
                {
                    this.correct1.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct1.Visible = true;
            }
        }

        // 定义委托—状态1-NO
        private delegate void cogStatusNo_1Delegate();

        // 定于所要委托的方法
        private void cogStatusNo_1DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong1.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_1Delegate myDelegate = new cogStatusNo_1Delegate(cogStatusNo_1DelegateMethod);
                // Invoke delegate
                this.wrong1.Invoke(new Action(() =>
                {
                    this.wrong1.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong1.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.correct1.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_1Delegate myDelegate = new cogStatusNo_1Delegate(cogStatusNo_1DelegateMethod);
                // Invoke delegate
                this.correct1.Invoke(new Action(() =>
                {
                    this.correct1.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct1.Visible = false;
            }
        }

        // 定义委托—状态2-OK
        private delegate void cogStatusOk_2Delegate();

        // 定于所要委托的方法
        private void cogStatusOk_2DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong2.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_2Delegate myDelegate = new cogStatusOk_2Delegate(cogStatusOk_2DelegateMethod);
                // Invoke delegate
                this.wrong2.Invoke(new Action(() =>
                {
                    this.wrong2.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong2.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.correct2.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_2Delegate myDelegate = new cogStatusOk_2Delegate(cogStatusOk_2DelegateMethod);
                // Invoke delegate
                this.correct2.Invoke(new Action(() =>
                {
                    this.correct2.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct2.Visible = true;
            }
        }

        // 定义委托—状态2-NO
        private delegate void cogStatusNo_2Delegate();

        // 定于所要委托的方法
        private void cogStatusNo_2DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong2.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_2Delegate myDelegate = new cogStatusNo_2Delegate(cogStatusNo_2DelegateMethod);
                // Invoke delegate
                this.wrong2.Invoke(new Action(() =>
                {
                    this.wrong2.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong2.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.correct2.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_2Delegate myDelegate = new cogStatusNo_2Delegate(cogStatusNo_2DelegateMethod);
                // Invoke delegate
                this.correct2.Invoke(new Action(() =>
                {
                    this.correct2.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct2.Visible = false;
            }
        }

        // 定义委托—状态3-OK
        private delegate void cogStatusOk_3Delegate();

        // 定于所要委托的方法
        private void cogStatusOk_3DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong3.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_3Delegate myDelegate = new cogStatusOk_3Delegate(cogStatusOk_3DelegateMethod);
                // Invoke delegate
                this.wrong3.Invoke(new Action(() =>
                {
                    this.wrong3.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong3.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.correct3.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_3Delegate myDelegate = new cogStatusOk_3Delegate(cogStatusOk_3DelegateMethod);
                // Invoke delegate
                this.correct3.Invoke(new Action(() =>
                {
                    this.correct3.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct3.Visible = true;
            }
        }

        // 定义委托—状态3-NO
        private delegate void cogStatusNo_3Delegate();

        // 定于所要委托的方法
        private void cogStatusNo_3DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong3.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_3Delegate myDelegate = new cogStatusNo_3Delegate(cogStatusNo_3DelegateMethod);
                // Invoke delegate
                this.wrong3.Invoke(new Action(() =>
                {
                    this.wrong3.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong3.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.correct3.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_3Delegate myDelegate = new cogStatusNo_3Delegate(cogStatusNo_3DelegateMethod);
                // Invoke delegate
                this.correct3.Invoke(new Action(() =>
                {
                    this.correct3.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct3.Visible = false;
            }
        }

        // 定义委托—状态4-OK
        private delegate void cogStatusOk_4Delegate();

        // 定于所要委托的方法
        private void cogStatusOk_4DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong4.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_4Delegate myDelegate = new cogStatusOk_4Delegate(cogStatusOk_4DelegateMethod);
                // Invoke delegate
                this.wrong4.Invoke(new Action(() =>
                {
                    this.wrong4.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong4.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.correct4.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_4Delegate myDelegate = new cogStatusOk_4Delegate(cogStatusOk_4DelegateMethod);
                // Invoke delegate
                this.correct4.Invoke(new Action(() =>
                {
                    this.correct4.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct4.Visible = true;
            }
        }

        // 定义委托—状态4-NO
        private delegate void cogStatusNo_4Delegate();

        // 定于所要委托的方法
        private void cogStatusNo_4DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong4.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_4Delegate myDelegate = new cogStatusNo_4Delegate(cogStatusNo_4DelegateMethod);
                // Invoke delegate
                this.wrong4.Invoke(new Action(() =>
                {
                    this.wrong4.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong4.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.correct4.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_4Delegate myDelegate = new cogStatusNo_4Delegate(cogStatusNo_4DelegateMethod);
                // Invoke delegate
                this.correct4.Invoke(new Action(() =>
                {
                    this.correct4.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct4.Visible = false;
            }
        }

        // 定义委托—状态5-OK
        private delegate void cogStatusOk_5Delegate();

        // 定于所要委托的方法
        private void cogStatusOk_5DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong5.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_5Delegate myDelegate = new cogStatusOk_5Delegate(cogStatusOk_5DelegateMethod);
                // Invoke delegate
                this.wrong5.Invoke(new Action(() =>
                {
                    this.wrong5.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong5.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.correct5.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_5Delegate myDelegate = new cogStatusOk_5Delegate(cogStatusOk_5DelegateMethod);
                // Invoke delegate
                this.correct5.Invoke(new Action(() =>
                {
                    this.correct5.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct5.Visible = true;
            }
        }

        // 定义委托—状态5-NO
        private delegate void cogStatusNo_5Delegate();

        // 定于所要委托的方法
        private void cogStatusNo_5DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong5.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_5Delegate myDelegate = new cogStatusNo_5Delegate(cogStatusNo_5DelegateMethod);
                // Invoke delegate
                this.wrong5.Invoke(new Action(() =>
                {
                    this.wrong5.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong5.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.correct5.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_5Delegate myDelegate = new cogStatusNo_5Delegate(cogStatusNo_5DelegateMethod);
                // Invoke delegate
                this.correct5.Invoke(new Action(() =>
                {
                    this.correct5.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct5.Visible = false;
            }
        }

        // 定义委托—状态6-OK
        private delegate void cogStatusOk_6Delegate();

        // 定于所要委托的方法
        private void cogStatusOk_6DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong6.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_6Delegate myDelegate = new cogStatusOk_6Delegate(cogStatusOk_6DelegateMethod);
                // Invoke delegate
                this.wrong6.Invoke(new Action(() =>
                {
                    this.wrong6.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong6.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.correct6.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_6Delegate myDelegate = new cogStatusOk_6Delegate(cogStatusOk_6DelegateMethod);
                // Invoke delegate
                this.correct6.Invoke(new Action(() =>
                {
                    this.correct6.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct6.Visible = true;
            }
        }

        // 定义委托—状态6-NO
        private delegate void cogStatusNo_6Delegate();

        // 定于所要委托的方法
        private void cogStatusNo_6DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong6.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_6Delegate myDelegate = new cogStatusNo_6Delegate(cogStatusNo_6DelegateMethod);
                // Invoke delegate
                this.wrong6.Invoke(new Action(() =>
                {
                    this.wrong6.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong6.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.correct6.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_6Delegate myDelegate = new cogStatusNo_6Delegate(cogStatusNo_6DelegateMethod);
                // Invoke delegate
                this.correct6.Invoke(new Action(() =>
                {
                    this.correct6.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct6.Visible = false;
            }
        }

        // 定义委托—状态7-OK
        private delegate void cogStatusOk_7Delegate();

        // 定于所要委托的方法
        private void cogStatusOk_7DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong7.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_7Delegate myDelegate = new cogStatusOk_7Delegate(cogStatusOk_7DelegateMethod);
                // Invoke delegate
                this.wrong7.Invoke(new Action(() =>
                {
                    this.wrong7.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong7.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.correct7.InvokeRequired)
            {
                // 使用委托
                cogStatusOk_7Delegate myDelegate = new cogStatusOk_7Delegate(cogStatusOk_7DelegateMethod);
                // Invoke delegate
                this.correct7.Invoke(new Action(() =>
                {
                    this.correct7.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct7.Visible = true;
            }
        }

        // 定义委托—状态7-NO
        private delegate void cogStatusNo_7Delegate();

        // 定于所要委托的方法
        private void cogStatusNo_7DelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.wrong7.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_7Delegate myDelegate = new cogStatusNo_7Delegate(cogStatusNo_7DelegateMethod);
                // Invoke delegate
                this.wrong7.Invoke(new Action(() =>
                {
                    this.wrong7.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.wrong7.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.correct7.InvokeRequired)
            {
                // 使用委托
                cogStatusNo_7Delegate myDelegate = new cogStatusNo_7Delegate(cogStatusNo_7DelegateMethod);
                // Invoke delegate
                this.correct7.Invoke(new Action(() =>
                {
                    this.correct7.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.correct7.Visible = false;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////
        //
        //                            飞行器命令状态委托   2015/5/14  0:16
        //
        /////////////////////////////////////////////////////////////////////////////////////

        // 飞行器命令状态
        private delegate void richBoxDelegate(string str);

        // 飞行器命令状态
        private void richBoxDelegateMethod(string str)
        {
            // 判断是否达到InvokeRequired
            if (this.richTextBox1.InvokeRequired)
            {
                // 使用委托
                richBoxDelegate myDelegate = new richBoxDelegate(richBoxDelegateMethod);
                // Invoke delegate
                this.richTextBox1.Invoke(new Action(() =>
                {
                    if (this.richTextBox1.Text != "")
                    {
                        this.richTextBox1.Text = this.richTextBox1.Text + '\n' + str;
                    }
                    else
                    {
                        this.richTextBox1.Text = this.richTextBox1.Text + str;
                    }
                }));
            }
            else
            {
                // 没使用InvokeRequired
                if (this.richTextBox1.Text != "")
                {
                    this.richTextBox1.Text = this.richTextBox1.Text + '\n' + str;
                }
                else
                {
                    this.richTextBox1.Text = this.richTextBox1.Text + str;
                }
            }
        }


        // 飞行器命令状态
        private delegate void richBox2Delegate(string str);

        // 飞行器命令状态
        private void richBox2DelegateMethod(string str)
        {
            // 判断是否达到InvokeRequired
            if (this.richTextBox2.InvokeRequired)
            {
                // 使用委托
                richBox2Delegate myDelegate = new richBox2Delegate(richBox2DelegateMethod);
                // Invoke delegate
                this.richTextBox2.Invoke(new Action(() =>
                {
                    if (this.richTextBox2.Text != "")
                    {
                        this.richTextBox2.Text = this.richTextBox2.Text + '\n' + str;
                    }
                    else
                    {
                        this.richTextBox2.Text = this.richTextBox2.Text + str;
                    }
                }));
            }
            else
            {
                // 没使用InvokeRequired
                if (this.richTextBox2.Text != "")
                {
                    this.richTextBox2.Text = this.richTextBox2.Text + '\n' + str;
                }
                else
                {
                    this.richTextBox2.Text = this.richTextBox2.Text + str;
                }
            }
        }

        // 飞行器命令状态
        private delegate void richBox3Delegate(string str);

        // 飞行器命令状态
        private void richBox3DelegateMethod(string str)
        {
            // 判断是否达到InvokeRequired
            if (this.richTextBox3.InvokeRequired)
            {
                // 使用委托
                richBox3Delegate myDelegate = new richBox3Delegate(richBox3DelegateMethod);
                // Invoke delegate
                this.richTextBox3.Invoke(new Action(() =>
                {
                    if (this.richTextBox3.Text != "")
                    {
                        this.richTextBox3.Text = this.richTextBox3.Text + '\n' + str;
                    }
                    else
                    {
                        this.richTextBox3.Text = this.richTextBox2.Text + str;
                    }
                }));
            }
            else
            {
                // 没使用InvokeRequired
                if (this.richTextBox3.Text != "")
                {
                    this.richTextBox3.Text = this.richTextBox3.Text + '\n' + str;
                }
                else
                {
                    this.richTextBox3.Text = this.richTextBox3.Text + str;
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////
        //
        //                            跟踪状态委托   2015/5/5  20:36
        //
        /////////////////////////////////////////////////////////////////////////////////////

        // 定义委托—跟踪状态
        private delegate void followCogStatusDelegate();

        /////////////////////////////////////////////////////////////////////////////////////
        //
        //                            意念状态委托   2015/5/14  0:16
        //
        /////////////////////////////////////////////////////////////////////////////////////

        // 左
        private void followCogStatusLeftDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.cogBarStatus.InvokeRequired)
            {
                // 使用委托
                followCogStatusDelegate myDelegate = new followCogStatusDelegate(followCogStatusLeftDelegateMethod);
                //Invoke delegate
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (x < 103)
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(103, 169);
                        }));
                        break;
                    }
                    else
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(x - 10, y);
                        }));
                        Thread.Sleep(100);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (x < 103)
                    {
                        this.box.Location = new Point(103, 169);
                        break;
                    }
                    else
                    {
                        this.box.Location = new Point(x - 10, y);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        // 右
        private void followCogStatusRightDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.cogBarStatus.InvokeRequired)
            {
                // 使用委托
                followCogStatusDelegate myDelegate = new followCogStatusDelegate(followCogStatusRightDelegateMethod);
                //Invoke delegate
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (x > 303)
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(303, 169);
                        }));
                        break;
                    }
                    else
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(x + 10, y);
                        }));
                        Thread.Sleep(100);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (x > 303)
                    {
                        this.box.Location = new Point(303, 169);
                        break;
                    }
                    else
                    {
                        this.box.Location = new Point(x + 10, y);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        // 下
        private void followCogStatusDropDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.cogBarStatus.InvokeRequired)
            {
                // 使用委托
                followCogStatusDelegate myDelegate = new followCogStatusDelegate(followCogStatusLiftDelegateMethod);
                //Invoke delegate
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (y > 269)
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(203, 269);
                        }));
                        break;
                    }
                    else
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(x , y+10);
                        }));
                        Thread.Sleep(100);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (y > 269)
                    {
                        this.box.Location = new Point(203, 269);
                        break;
                    }
                    else
                    {
                        this.box.Location = new Point(x , y+10);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        // 上
        private void followCogStatusLiftDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.cogBarStatus.InvokeRequired)
            {
                // 使用委托
                followCogStatusDelegate myDelegate = new followCogStatusDelegate(followCogStatusDropDelegateMethod);
                //Invoke delegate
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (y < 69)
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(203, 69);
                        }));
                        break;
                    }
                    else
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(x, y - 10);
                        }));
                        Thread.Sleep(100);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (y < 69)
                    {
                        this.box.Location = new Point(203, 69);
                        break;
                    }
                    else
                    {
                        this.box.Location = new Point(x, y - 10);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        // 前
        private void followCogStatusPushDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.cogBarStatus.InvokeRequired)
            {
                // 使用委托
                followCogStatusDelegate myDelegate = new followCogStatusDelegate(followCogStatusPushDelegateMethod);
                //Invoke delegate
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (w < 71)
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(213, 179);
                            this.box.Size = new Size(71, 71);
                        }));
                        break;
                    }
                    else
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(x + 1, y + 1);
                            this.box.Size = new Size(w - 2, h - 2);
                        }));
                        Thread.Sleep(100);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (w < 71)
                    {
                        this.box.Location = new Point(213, 179);
                        this.box.Size = new Size(71, 71);
                        break;
                    }
                    else
                    {
                        this.box.Location = new Point(x + 1, y + 1);
                        this.box.Size = new Size(w - 2, h - 2);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        // 后
        private void followCogStatusPullDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.cogBarStatus.InvokeRequired)
            {
                // 使用委托
                followCogStatusDelegate myDelegate = new followCogStatusDelegate(followCogStatusPullDelegateMethod);
                //Invoke delegate
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (w > 111)
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(193, 159);
                            this.box.Size = new Size(111, 111);
                        }));
                        break;
                    }
                    else
                    {
                        this.box.Invoke(new Action(() =>
                        {
                            this.box.Location = new Point(x - 1, y - 1);
                            this.box.Size = new Size(w + 2, h + 2);
                        }));
                        Thread.Sleep(100);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    int w = this.box.Size.Width;
                    int h = this.box.Size.Height;
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    if (w > 111)
                    {
                        this.box.Location = new Point(193, 159);
                        this.box.Size = new Size(111, 111);
                        break;
                    }
                    else
                    {
                        this.box.Location = new Point(x - 1, y - 1);
                        this.box.Size = new Size(w + 2, h + 2);
                        Thread.Sleep(100);
                    }
                }
            }
        }

        // 空闲
        private void freeCogStatusDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.box.InvokeRequired)
            {
                // 使用委托
                followCogStatusDelegate myDelegate = new followCogStatusDelegate(freeCogStatusDelegateMethod);
                //Invoke delegate
                for (int i = 0; i < 3; i++)
                {
                    this.box.Invoke(new Action(() =>
                    {
                        int x = this.box.Location.X;
                        int y = this.box.Location.Y;
                        this.box.Location = new Point(x, y + 1);
                    }));
                    Thread.Sleep(180);
                }
                for (int i = 0; i < 3; i++)
                {
                    this.box.Invoke(new Action(() =>
                    {
                        int x = this.box.Location.X;
                        int y = this.box.Location.Y;
                        this.box.Location = new Point(x, y - 1);
                    }));
                    Thread.Sleep(180);
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    this.box.Location = new Point(x, y + 1);
                    Thread.Sleep(100);
                }
                for (int i = 0; i < 5; i++)
                {
                    int x = this.box.Location.X;
                    int y = this.box.Location.Y;
                    this.box.Location = new Point(x, y - 1);
                    Thread.Sleep(100);
                }
            }
        }

        // 还原
        private void followCogStatusResetDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.box.InvokeRequired)
            {
                // 使用委托
                followCogStatusDelegate myDelegate = new followCogStatusDelegate(followCogStatusResetDelegateMethod);
                //Invoke delegate
                this.box.Invoke(new Action(() =>
                {
                    this.box.Location = new Point(203, 169);
                    this.box.Size = new Size(91, 91);
                }));
            }
            else
            {
                    this.box.Location = new Point(203, 169);
                    this.box.Size = new Size(91, 91);
            }
        }
        

        /////////////////////////////////////////////////////////////////////////////////////
        //
        //                            识别状态委托   2015/4/24  13:56
        //
        /////////////////////////////////////////////////////////////////////////////////////

        // 定义委托—识别状态
        private delegate void cogStatusDelegate(string str);

        // 定于所要委托的方法
        private void cogStatusDelegateMethod(string str)
        {
            // 判断是否达到InvokeRequired
            if (this.cogStatus.InvokeRequired)
            {
                // 使用委托
                cogStatusDelegate myDelegate = new cogStatusDelegate(cogStatusDelegateMethod);
                //Invoke delegate
                this.cogStatus.Invoke(myDelegate, str);
            }
            else
            {
                // 没使用InvokeRequired
                cogStatus.Text = str;
            }
        }

        // 定义委托—识别状态进度条
        private delegate void cogBarStatusDelegate(Int32 chargeLevel, Int32 maxChargeLevel);

        // 定于所要委托的方法
        private void cogBarStatusDelegateMethod(Int32 chargeLevel, Int32 maxChargeLevel)
        {
            // 判断是否达到InvokeRequired
            if (this.cogBarStatus.InvokeRequired)
            {
                // 使用委托
                cogBarStatusDelegate myDelegate = new cogBarStatusDelegate(cogBarStatusDelegateMethod);
                //Invoke delegate
                this.cogBarStatus.Invoke(new Action(() =>
                {
                    this.cogBarStatus.Value = chargeLevel;
                    this.cogBarStatus.Maximum = maxChargeLevel;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.electricPower.Value = chargeLevel;
                this.electricPower.Maximum = maxChargeLevel;
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////
        //
        //                            信号强弱委托   2015/4/27  17:56
        //
        /////////////////////////////////////////////////////////////////////////////////////

        // 定义委托—信号强度-强
        private delegate void signalStrengthGoodDelegate();

        // 定于所要委托的方法
        private void signalStrengthGoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.signalStrength1_red.InvokeRequired)
            {
                // 使用委托
                signalStrengthGoodDelegate myDelegate = new signalStrengthGoodDelegate(signalStrengthGoodDelegateMethod);
                // Invoke delegate
                this.signalStrength1_red.Invoke(new Action(() =>
                {
                    this.signalStrength1_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength1_red.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.signalStrength2_red.InvokeRequired)
            {
                // 使用委托
                signalStrengthGoodDelegate myDelegate = new signalStrengthGoodDelegate(signalStrengthGoodDelegateMethod);
                // Invoke delegate
                this.signalStrength2_red.Invoke(new Action(() =>
                {
                    this.signalStrength2_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength2_red.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.signalStrength1_green.InvokeRequired)
            {
                // 使用委托
                signalStrengthGoodDelegate myDelegate = new signalStrengthGoodDelegate(signalStrengthGoodDelegateMethod);
                //Invoke delegate
                this.signalStrength1_green.Invoke(new Action(() =>
                {
                    this.signalStrength1_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength2_green.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.signalStrength2_green.InvokeRequired)
            {
                // 使用委托
                signalStrengthGoodDelegate myDelegate = new signalStrengthGoodDelegate(signalStrengthGoodDelegateMethod);
                //Invoke delegate
                this.signalStrength2_green.Invoke(new Action(() =>
                {
                    this.signalStrength2_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength2_green.Visible = true;
            }
        }

        // 定义委托—信号强度-弱
        private delegate void signalStrengthBadDelegate();

        // 定于所要委托的方法
        private void signalStrengthBadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.signalStrength1_red.InvokeRequired)
            {
                // 使用委托
                signalStrengthBadDelegate myDelegate = new signalStrengthBadDelegate(signalStrengthBadDelegateMethod);
                //Invoke delegate
                this.signalStrength1_red.Invoke(new Action(() =>
                {
                    this.signalStrength1_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength1_red.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.signalStrength2_red.InvokeRequired)
            {
                // 使用委托
                signalStrengthBadDelegate myDelegate = new signalStrengthBadDelegate(signalStrengthBadDelegateMethod);
                //Invoke delegate
                this.signalStrength2_red.Invoke(new Action(() =>
                {
                    this.signalStrength2_red.Visible =true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength2_red.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.signalStrength1_green.InvokeRequired)
            {
                // 使用委托
                signalStrengthBadDelegate myDelegate = new signalStrengthBadDelegate(signalStrengthBadDelegateMethod);
                //Invoke delegate
                this.signalStrength1_green.Invoke(new Action(() =>
                {
                    this.signalStrength1_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength1_green.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.signalStrength2_green.InvokeRequired)
            {
                // 使用委托
                signalStrengthBadDelegate myDelegate = new signalStrengthBadDelegate(signalStrengthBadDelegateMethod);
                //Invoke delegate
                this.signalStrength2_green.Invoke(new Action(() =>
                {
                    this.signalStrength2_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength2_green.Visible = false;
            }
        }

        // 定义委托—信号强度-没信号
        private delegate void signalStrengthNoSignalDelegate();

        // 定于所要委托的方法
        private void signalStrengthNoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.signalStrength1_red.InvokeRequired)
            {
                // 使用委托
                signalStrengthNoSignalDelegate myDelegate = new signalStrengthNoSignalDelegate(signalStrengthNoSignalDelegateMethod);
                //Invoke delegate
                this.signalStrength1_red.Invoke(new Action(() =>
                {
                    this.signalStrength1_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength1_red.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.signalStrength2_red.InvokeRequired)
            {
                // 使用委托
                signalStrengthNoSignalDelegate myDelegate = new signalStrengthNoSignalDelegate(signalStrengthNoSignalDelegateMethod);
                //Invoke delegate
                this.signalStrength2_red.Invoke(new Action(() =>
                {
                    this.signalStrength2_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength2_red.Visible = true;
            }
            // 判断是否达到InvokeRequired
            if (this.signalStrength1_green.InvokeRequired)
            {
                // 使用委托
                signalStrengthNoSignalDelegate myDelegate = new signalStrengthNoSignalDelegate(signalStrengthNoSignalDelegateMethod);
                //Invoke delegate
                this.signalStrength1_green.Invoke(new Action(() =>
                {
                    this.signalStrength1_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength2_green.Visible = false;
            }
            // 判断是否达到InvokeRequired
            if (this.signalStrength2_green.InvokeRequired)
            {
                // 使用委托
                signalStrengthNoSignalDelegate myDelegate = new signalStrengthNoSignalDelegate(signalStrengthNoSignalDelegateMethod);
                //Invoke delegate
                this.signalStrength2_green.Invoke(new Action(() =>
                {
                    this.signalStrength2_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.signalStrength2_green.Visible = false;
            }
        }

        // 定义委托—信号强度状态
        private delegate void signalStrengthStatusDelegate(string str);

        // 定于所要委托的方法
        private void signalStrengthStatusDelegateMethod(string str)
        {
            // 判断是否达到InvokeRequired
            if (this.signalStrengthStatus.InvokeRequired)
            {
                // 使用委托
                signalStrengthStatusDelegate myDelegate = new signalStrengthStatusDelegate(signalStrengthStatusDelegateMethod);
                //Invoke delegate
                this.signalStrengthStatus.Invoke(myDelegate, str);
            }
            else
            {
                // 没使用InvokeRequired
                signalStrengthStatus.Text = str;
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////
        //
        //                            电池电量委托   2015/4/27  15:51
        //
        /////////////////////////////////////////////////////////////////////////////////////

        // 定义委托—电池电量状态
        private delegate void electricPowerStatusDelegate(string str);

        // 定于所要委托的方法
        private void electricPowerStatusDelegateMethod(string str)
        {
            // 判断是否达到InvokeRequired
            if (this.electricPowerStatus.InvokeRequired)
            {
                // 使用委托
                electricPowerStatusDelegate myDelegate = new electricPowerStatusDelegate(electricPowerStatusDelegateMethod);
                //Invoke delegate
                this.electricPowerStatus.Invoke(myDelegate, str);
            }
            else
            {
                // 没使用InvokeRequired
                electricPowerStatus.Text = str;
            }
        }

        // 定义委托—电池电量
        private delegate void electricPowerDelegate(Int32 chargeLevel, Int32 maxChargeLevel);

        // 定于所要委托的方法
        private void electricPowerDelegateMethod(Int32 chargeLevel, Int32 maxChargeLevel)
        {
            // 判断是否达到InvokeRequired
            if (this.electricPower.InvokeRequired)
            {
                // 使用委托
                electricPowerDelegate myDelegate = new electricPowerDelegate(electricPowerDelegateMethod);
                //Invoke delegate
                this.electricPower.Invoke(new Action(() =>
                {
                    this.electricPower.Value = chargeLevel;
                    this.electricPower.Maximum = maxChargeLevel;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.electricPower.Value = chargeLevel;
                this.electricPower.Maximum = maxChargeLevel;
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////
        //
        //                            系统状态委托   2015/4/27  16:34
        //
        /////////////////////////////////////////////////////////////////////////////////////

        // 定义委托—系统状态
        private delegate void contactStatusDelegate(string str);

        // 定于所要委托的方法
        private void contactStatusDelegateMethod(string str)
        {
            // 判断是否达到InvokeRequired
            if (this.contactStatus.InvokeRequired)
            {
                // 使用委托
                contactStatusDelegate myDelegate = new contactStatusDelegate(contactStatusDelegateMethod);
                //Invoke delegate
                this.contactStatus.Invoke(myDelegate, str);
            }
            else
            {
                // 没使用InvokeRequired
                contactStatus.Text = str;
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////
        //
        //                            开始时间委托   2015/4/27  16:34
        //
        /////////////////////////////////////////////////////////////////////////////////////

        // 定义委托—开始时间
        private delegate void starttimeDelegate(string str);

        // 定于所要委托的方法
        private void starttimeDelegateMethod(string str)
        {
            // 判断是否达到InvokeRequired
            if (this.starttime.InvokeRequired)
            {
                // 使用委托
                starttimeDelegate myDelegate = new starttimeDelegate(starttimeDelegateMethod);
                // Invoke delegate
                this.starttime.Invoke(myDelegate, str);
            }
            else
            {
                // 没使用InvokeRequired
                starttime.Text = str;
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////
        //
        //                            耳机触点状态   2015/4/23  13:44
        //
        /////////////////////////////////////////////////////////////////////////////////////

        // 定义委托—P3状态
        private delegate void p3Delegate();

        // 定于所要委托的方法-弱信号
        private void p3BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb11_red.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3BadDelegateMethod);
                //Invoke delegate
                this.pb11_red.Invoke(new Action(() =>
                {
                    this.pb11_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_yellow.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3BadDelegateMethod);
                //Invoke delegate
                this.pb11_yellow.Invoke(new Action(() =>
                {
                    this.pb11_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_green.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3BadDelegateMethod);
                //Invoke delegate
                this.pb11_green.Invoke(new Action(() =>
                {
                    this.pb11_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_orange.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3BadDelegateMethod);
                //Invoke delegate
                this.pb11_orange.Invoke(new Action(() =>
                {
                    this.pb11_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-差信号
        private void p3PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb11_red.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3PoorDelegateMethod);
                //Invoke delegate
                this.pb11_red.Invoke(new Action(() =>
                {
                    this.pb11_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_yellow.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3PoorDelegateMethod);
                //Invoke delegate
                this.pb11_yellow.Invoke(new Action(() =>
                {
                    this.pb11_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_green.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3PoorDelegateMethod);
                //Invoke delegate
                this.pb11_green.Invoke(new Action(() =>
                {
                    this.pb11_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_orange.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3PoorDelegateMethod);
                //Invoke delegate
                this.pb11_orange.Invoke(new Action(() =>
                {
                    this.pb11_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void p3FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb11_red.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3FairDelegateMethod);
                //Invoke delegate
                this.pb11_red.Invoke(new Action(() =>
                {
                    this.pb11_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_yellow.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3FairDelegateMethod);
                //Invoke delegate
                this.pb11_yellow.Invoke(new Action(() =>
                {
                    this.pb11_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_green.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3FairDelegateMethod);
                //Invoke delegate
                this.pb11_green.Invoke(new Action(() =>
                {
                    this.pb11_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_orange.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3FairDelegateMethod);
                //Invoke delegate
                this.pb11_orange.Invoke(new Action(() =>
                {
                    this.pb11_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void p3GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb11_red.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3GoodDelegateMethod);
                //Invoke delegate
                this.pb11_red.Invoke(new Action(() =>
                {
                    this.pb11_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_yellow.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3GoodDelegateMethod);
                //Invoke delegate
                this.pb11_yellow.Invoke(new Action(() =>
                {
                    this.pb11_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_green.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3GoodDelegateMethod);
                //Invoke delegate
                this.pb11_green.Invoke(new Action(() =>
                {
                    this.pb11_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_orange.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3GoodDelegateMethod);
                //Invoke delegate
                this.pb11_orange.Invoke(new Action(() =>
                {
                    this.pb11_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void p3NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb11_red.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb11_red.Invoke(new Action(() =>
                {
                    this.pb11_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_yellow.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb11_yellow.Invoke(new Action(() =>
                {
                    this.pb11_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_green.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb11_green.Invoke(new Action(() =>
                {
                    this.pb11_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb11_orange.InvokeRequired)
            {
                // 使用委托
                p3Delegate myDelegate = new p3Delegate(p3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb11_orange.Invoke(new Action(() =>
                {
                    this.pb11_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb11_orange.Visible = false;
            }
        }

        // 定义委托—P4状态
        private delegate void p4Delegate();

        // 定于所要委托的方法-差信号
        private void p4BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb12_red.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4BadDelegateMethod);
                //Invoke delegate
                this.pb12_red.Invoke(new Action(() =>
                {
                    this.pb12_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_yellow.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4BadDelegateMethod);
                //Invoke delegate
                this.pb12_yellow.Invoke(new Action(() =>
                {
                    this.pb12_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_green.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4BadDelegateMethod);
                //Invoke delegate
                this.pb12_green.Invoke(new Action(() =>
                {
                    this.pb12_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_orange.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4BadDelegateMethod);
                //Invoke delegate
                this.pb12_orange.Invoke(new Action(() =>
                {
                    this.pb12_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void p4PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb12_red.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4PoorDelegateMethod);
                //Invoke delegate
                this.pb12_red.Invoke(new Action(() =>
                {
                    this.pb12_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_yellow.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4PoorDelegateMethod);
                //Invoke delegate
                this.pb12_yellow.Invoke(new Action(() =>
                {
                    this.pb12_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_green.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4PoorDelegateMethod);
                //Invoke delegate
                this.pb12_green.Invoke(new Action(() =>
                {
                    this.pb12_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_orange.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4PoorDelegateMethod);
                //Invoke delegate
                this.pb12_orange.Invoke(new Action(() =>
                {
                    this.pb12_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void p4FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb12_red.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4FairDelegateMethod);
                //Invoke delegate
                this.pb12_red.Invoke(new Action(() =>
                {
                    this.pb12_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_yellow.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4FairDelegateMethod);
                //Invoke delegate
                this.pb12_yellow.Invoke(new Action(() =>
                {
                    this.pb12_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_green.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4FairDelegateMethod);
                //Invoke delegate
                this.pb12_green.Invoke(new Action(() =>
                {
                    this.pb12_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_orange.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4FairDelegateMethod);
                //Invoke delegate
                this.pb12_orange.Invoke(new Action(() =>
                {
                    this.pb12_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void p4GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb12_red.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4GoodDelegateMethod);
                //Invoke delegate
                this.pb12_red.Invoke(new Action(() =>
                {
                    this.pb12_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_yellow.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4GoodDelegateMethod);
                //Invoke delegate
                this.pb12_yellow.Invoke(new Action(() =>
                {
                    this.pb12_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_green.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4GoodDelegateMethod);
                //Invoke delegate
                this.pb12_green.Invoke(new Action(() =>
                {
                    this.pb12_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_orange.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4GoodDelegateMethod);
                //Invoke delegate
                this.pb12_orange.Invoke(new Action(() =>
                {
                    this.pb12_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_orange.Visible = false;
            }
        }

        //define a method which match the above delegae
        private void p4NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb12_red.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb12_red.Invoke(new Action(() =>
                {
                    this.pb12_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_yellow.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb12_yellow.Invoke(new Action(() =>
                {
                    this.pb12_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_green.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb12_green.Invoke(new Action(() =>
                {
                    this.pb12_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb12_orange.InvokeRequired)
            {
                // 使用委托
                p4Delegate myDelegate = new p4Delegate(p4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb12_orange.Invoke(new Action(() =>
                {
                    this.pb12_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb12_orange.Visible = false;
            }
        }

        // 定义委托—af3状态
        private delegate void af3Delegate();

        // 定于所要委托的方法-差信号
        private void af3BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb1_red.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3BadDelegateMethod);
                //Invoke delegate
                this.pb1_red.Invoke(new Action(() =>
                {
                    this.pb1_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_yellow.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3BadDelegateMethod);
                //Invoke delegate
                this.pb1_yellow.Invoke(new Action(() =>
                {
                    this.pb1_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_green.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3BadDelegateMethod);
                //Invoke delegate
                this.pb1_green.Invoke(new Action(() =>
                {
                    this.pb1_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_orange.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3BadDelegateMethod);
                //Invoke delegate
                this.pb1_orange.Invoke(new Action(() =>
                {
                    this.pb1_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void af3PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb1_red.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3PoorDelegateMethod);
                //Invoke delegate
                this.pb1_red.Invoke(new Action(() =>
                {
                    this.pb1_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_yellow.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3PoorDelegateMethod);
                //Invoke delegate
                this.pb1_yellow.Invoke(new Action(() =>
                {
                    this.pb1_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_green.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3PoorDelegateMethod);
                //Invoke delegate
                this.pb1_green.Invoke(new Action(() =>
                {
                    this.pb1_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_orange.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3PoorDelegateMethod);
                //Invoke delegate
                this.pb1_orange.Invoke(new Action(() =>
                {
                    this.pb1_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void af3FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb1_red.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3FairDelegateMethod);
                //Invoke delegate
                this.pb1_red.Invoke(new Action(() =>
                {
                    this.pb1_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_yellow.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3FairDelegateMethod);
                //Invoke delegate
                this.pb1_yellow.Invoke(new Action(() =>
                {
                    this.pb1_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_green.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3FairDelegateMethod);
                //Invoke delegate
                this.pb1_green.Invoke(new Action(() =>
                {
                    this.pb1_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_orange.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3FairDelegateMethod);
                //Invoke delegate
                this.pb1_orange.Invoke(new Action(() =>
                {
                    this.pb1_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void af3GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb1_red.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3GoodDelegateMethod);
                //Invoke delegate
                this.pb1_red.Invoke(new Action(() =>
                {
                    this.pb1_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_yellow.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3GoodDelegateMethod);
                //Invoke delegate
                this.pb1_yellow.Invoke(new Action(() =>
                {
                    this.pb1_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_green.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3GoodDelegateMethod);
                //Invoke delegate
                this.pb1_green.Invoke(new Action(() =>
                {
                    this.pb1_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_orange.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3GoodDelegateMethod);
                //Invoke delegate
                this.pb1_orange.Invoke(new Action(() =>
                {
                    this.pb1_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_orange.Visible = false;
            }
        }

        //define a method which match the above delegae
        private void af3NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb1_red.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb1_red.Invoke(new Action(() =>
                {
                    this.pb1_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_yellow.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb1_yellow.Invoke(new Action(() =>
                {
                    this.pb1_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_green.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb1_green.Invoke(new Action(() =>
                {
                    this.pb1_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb1_orange.InvokeRequired)
            {
                // 使用委托
                af3Delegate myDelegate = new af3Delegate(af3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb1_orange.Invoke(new Action(() =>
                {
                    this.pb1_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb1_orange.Visible = false;
            }
        }

        // 定义委托—f7状态
        private delegate void f7Delegate();

        // 定于所要委托的方法-差信号
        private void f7BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb3_red.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7BadDelegateMethod);
                //Invoke delegate
                this.pb3_red.Invoke(new Action(() =>
                {
                    this.pb3_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_yellow.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7BadDelegateMethod);
                //Invoke delegate
                this.pb3_yellow.Invoke(new Action(() =>
                {
                    this.pb3_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_green.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7BadDelegateMethod);
                //Invoke delegate
                this.pb3_green.Invoke(new Action(() =>
                {
                    this.pb3_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_orange.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7BadDelegateMethod);
                //Invoke delegate
                this.pb3_orange.Invoke(new Action(() =>
                {
                    this.pb3_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void f7PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb3_red.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7PoorDelegateMethod);
                //Invoke delegate
                this.pb3_red.Invoke(new Action(() =>
                {
                    this.pb3_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_yellow.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7PoorDelegateMethod);
                //Invoke delegate
                this.pb3_yellow.Invoke(new Action(() =>
                {
                    this.pb3_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_green.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7PoorDelegateMethod);
                //Invoke delegate
                this.pb3_green.Invoke(new Action(() =>
                {
                    this.pb3_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_orange.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7PoorDelegateMethod);
                //Invoke delegate
                this.pb3_orange.Invoke(new Action(() =>
                {
                    this.pb3_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void f7FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb3_red.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7FairDelegateMethod);
                //Invoke delegate
                this.pb3_red.Invoke(new Action(() =>
                {
                    this.pb3_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_yellow.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7FairDelegateMethod);
                //Invoke delegate
                this.pb3_yellow.Invoke(new Action(() =>
                {
                    this.pb3_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_green.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7FairDelegateMethod);
                //Invoke delegate
                this.pb3_green.Invoke(new Action(() =>
                {
                    this.pb3_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_orange.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7FairDelegateMethod);
                //Invoke delegate
                this.pb3_orange.Invoke(new Action(() =>
                {
                    this.pb3_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void f7GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb3_red.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7GoodDelegateMethod);
                //Invoke delegate
                this.pb3_red.Invoke(new Action(() =>
                {
                    this.pb3_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_yellow.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7GoodDelegateMethod);
                //Invoke delegate
                this.pb3_yellow.Invoke(new Action(() =>
                {
                    this.pb3_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_green.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7GoodDelegateMethod);
                //Invoke delegate
                this.pb3_green.Invoke(new Action(() =>
                {
                    this.pb3_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_orange.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7GoodDelegateMethod);
                //Invoke delegate
                this.pb3_orange.Invoke(new Action(() =>
                {
                    this.pb3_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_orange.Visible = false;
            }
        }

        //define a method which match the above delegae
        private void f7NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb3_red.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb3_red.Invoke(new Action(() =>
                {
                    this.pb3_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_yellow.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb3_yellow.Invoke(new Action(() =>
                {
                    this.pb3_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_green.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb3_green.Invoke(new Action(() =>
                {
                    this.pb3_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb3_orange.InvokeRequired)
            {
                // 使用委托
                f7Delegate myDelegate = new f7Delegate(f7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb3_orange.Invoke(new Action(() =>
                {
                    this.pb3_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb3_orange.Visible = false;
            }
        }

        // 定义委托—f3状态
        private delegate void f3Delegate();

        // 定于所要委托的方法-差信号
        private void f3BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb5_red.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3BadDelegateMethod);
                //Invoke delegate
                this.pb5_red.Invoke(new Action(() =>
                {
                    this.pb5_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_yellow.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3BadDelegateMethod);
                //Invoke delegate
                this.pb5_yellow.Invoke(new Action(() =>
                {
                    this.pb5_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_green.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3BadDelegateMethod);
                //Invoke delegate
                this.pb5_green.Invoke(new Action(() =>
                {
                    this.pb5_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_orange.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3BadDelegateMethod);
                //Invoke delegate
                this.pb5_orange.Invoke(new Action(() =>
                {
                    this.pb5_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void f3PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb5_red.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3PoorDelegateMethod);
                //Invoke delegate
                this.pb5_red.Invoke(new Action(() =>
                {
                    this.pb5_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_yellow.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3PoorDelegateMethod);
                //Invoke delegate
                this.pb5_yellow.Invoke(new Action(() =>
                {
                    this.pb5_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_green.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3PoorDelegateMethod);
                //Invoke delegate
                this.pb5_green.Invoke(new Action(() =>
                {
                    this.pb5_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_orange.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3PoorDelegateMethod);
                //Invoke delegate
                this.pb5_orange.Invoke(new Action(() =>
                {
                    this.pb5_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void f3FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb5_red.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3FairDelegateMethod);
                //Invoke delegate
                this.pb5_red.Invoke(new Action(() =>
                {
                    this.pb5_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_yellow.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3FairDelegateMethod);
                //Invoke delegate
                this.pb5_yellow.Invoke(new Action(() =>
                {
                    this.pb5_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_green.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3FairDelegateMethod);
                //Invoke delegate
                this.pb5_green.Invoke(new Action(() =>
                {
                    this.pb5_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_orange.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3FairDelegateMethod);
                //Invoke delegate
                this.pb5_orange.Invoke(new Action(() =>
                {
                    this.pb5_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void f3GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb5_red.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3GoodDelegateMethod);
                //Invoke delegate
                this.pb5_red.Invoke(new Action(() =>
                {
                    this.pb5_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_yellow.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3GoodDelegateMethod);
                //Invoke delegate
                this.pb5_yellow.Invoke(new Action(() =>
                {
                    this.pb5_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_green.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3GoodDelegateMethod);
                //Invoke delegate
                this.pb5_green.Invoke(new Action(() =>
                {
                    this.pb5_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_orange.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3GoodDelegateMethod);
                //Invoke delegate
                this.pb5_orange.Invoke(new Action(() =>
                {
                    this.pb5_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void f3NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb5_red.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb5_red.Invoke(new Action(() =>
                {
                    this.pb5_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_yellow.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb5_yellow.Invoke(new Action(() =>
                {
                    this.pb5_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_green.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb5_green.Invoke(new Action(() =>
                {
                    this.pb5_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb5_orange.InvokeRequired)
            {
                // 使用委托
                f3Delegate myDelegate = new f3Delegate(f3NoSignalDelegateMethod);
                //Invoke delegate
                this.pb5_orange.Invoke(new Action(() =>
                {
                    this.pb5_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb5_orange.Visible = false;
            }
        }

        // 定义委托—FC5状态
        private delegate void fc5Delegate();

        // 定于所要委托的方法-差信号
        private void fc5BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb7_red.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5BadDelegateMethod);
                //Invoke delegate
                this.pb7_red.Invoke(new Action(() =>
                {
                    this.pb7_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_yellow.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5BadDelegateMethod);
                //Invoke delegate
                this.pb7_yellow.Invoke(new Action(() =>
                {
                    this.pb7_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_green.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5BadDelegateMethod);
                //Invoke delegate
                this.pb7_green.Invoke(new Action(() =>
                {
                    this.pb7_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_orange.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5BadDelegateMethod);
                //Invoke delegate
                this.pb7_orange.Invoke(new Action(() =>
                {
                    this.pb7_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void fc5PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb7_red.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5PoorDelegateMethod);
                //Invoke delegate
                this.pb7_red.Invoke(new Action(() =>
                {
                    this.pb7_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_yellow.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5PoorDelegateMethod);
                //Invoke delegate
                this.pb7_yellow.Invoke(new Action(() =>
                {
                    this.pb7_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_green.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5PoorDelegateMethod);
                //Invoke delegate
                this.pb7_green.Invoke(new Action(() =>
                {
                    this.pb7_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_orange.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5PoorDelegateMethod);
                //Invoke delegate
                this.pb7_orange.Invoke(new Action(() =>
                {
                    this.pb7_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void fc5FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb7_red.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5FairDelegateMethod);
                //Invoke delegate
                this.pb7_red.Invoke(new Action(() =>
                {
                    this.pb7_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_yellow.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5FairDelegateMethod);
                //Invoke delegate
                this.pb7_yellow.Invoke(new Action(() =>
                {
                    this.pb7_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_green.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5FairDelegateMethod);
                //Invoke delegate
                this.pb7_green.Invoke(new Action(() =>
                {
                    this.pb7_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_orange.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5FairDelegateMethod);
                //Invoke delegate
                this.pb7_orange.Invoke(new Action(() =>
                {
                    this.pb7_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void fc5GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb7_red.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5GoodDelegateMethod);
                //Invoke delegate
                this.pb7_red.Invoke(new Action(() =>
                {
                    this.pb7_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_yellow.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5GoodDelegateMethod);
                //Invoke delegate
                this.pb7_yellow.Invoke(new Action(() =>
                {
                    this.pb7_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_green.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5GoodDelegateMethod);
                //Invoke delegate
                this.pb7_green.Invoke(new Action(() =>
                {
                    this.pb7_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_orange.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5GoodDelegateMethod);
                //Invoke delegate
                this.pb7_orange.Invoke(new Action(() =>
                {
                    this.pb7_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void fc5NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb7_red.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5NoSignalDelegateMethod);
                //Invoke delegate
                this.pb7_red.Invoke(new Action(() =>
                {
                    this.pb7_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_yellow.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5NoSignalDelegateMethod);
                //Invoke delegate
                this.pb7_yellow.Invoke(new Action(() =>
                {
                    this.pb7_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_green.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5NoSignalDelegateMethod);
                //Invoke delegate
                this.pb7_green.Invoke(new Action(() =>
                {
                    this.pb7_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb7_orange.InvokeRequired)
            {
                // 使用委托
                fc5Delegate myDelegate = new fc5Delegate(fc5NoSignalDelegateMethod);
                //Invoke delegate
                this.pb7_orange.Invoke(new Action(() =>
                {
                    this.pb7_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb7_orange.Visible = false;
            }
        }

        // 定义委托—T7状态
        private delegate void t7Delegate();

        // 定于所要委托的方法-差信号
        private void t7BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb9_red.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7BadDelegateMethod);
                //Invoke delegate
                this.pb9_red.Invoke(new Action(() =>
                {
                    this.pb9_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_yellow.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7BadDelegateMethod);
                //Invoke delegate
                this.pb9_yellow.Invoke(new Action(() =>
                {
                    this.pb9_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_green.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7BadDelegateMethod);
                //Invoke delegate
                this.pb9_green.Invoke(new Action(() =>
                {
                    this.pb9_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_orange.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7BadDelegateMethod);
                //Invoke delegate
                this.pb9_orange.Invoke(new Action(() =>
                {
                    this.pb9_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void t7PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb9_red.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7PoorDelegateMethod);
                //Invoke delegate
                this.pb9_red.Invoke(new Action(() =>
                {
                    this.pb9_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_yellow.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7PoorDelegateMethod);
                //Invoke delegate
                this.pb9_yellow.Invoke(new Action(() =>
                {
                    this.pb9_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_green.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7PoorDelegateMethod);
                //Invoke delegate
                this.pb9_green.Invoke(new Action(() =>
                {
                    this.pb9_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_orange.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7PoorDelegateMethod);
                //Invoke delegate
                this.pb9_orange.Invoke(new Action(() =>
                {
                    this.pb9_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void t7FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb9_red.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7FairDelegateMethod);
                //Invoke delegate
                this.pb9_red.Invoke(new Action(() =>
                {
                    this.pb9_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_yellow.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7FairDelegateMethod);
                //Invoke delegate
                this.pb9_yellow.Invoke(new Action(() =>
                {
                    this.pb9_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_green.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7FairDelegateMethod);
                //Invoke delegate
                this.pb9_green.Invoke(new Action(() =>
                {
                    this.pb9_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_orange.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7FairDelegateMethod);
                //Invoke delegate
                this.pb9_orange.Invoke(new Action(() =>
                {
                    this.pb9_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void t7GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb9_red.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7GoodDelegateMethod);
                //Invoke delegate
                this.pb9_red.Invoke(new Action(() =>
                {
                    this.pb9_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_yellow.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7GoodDelegateMethod);
                //Invoke delegate
                this.pb9_yellow.Invoke(new Action(() =>
                {
                    this.pb9_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_green.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7GoodDelegateMethod);
                //Invoke delegate
                this.pb9_green.Invoke(new Action(() =>
                {
                    this.pb9_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_orange.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7GoodDelegateMethod);
                //Invoke delegate
                this.pb9_orange.Invoke(new Action(() =>
                {
                    this.pb9_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void t7NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb9_red.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb9_red.Invoke(new Action(() =>
                {
                    this.pb9_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_yellow.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb9_yellow.Invoke(new Action(() =>
                {
                    this.pb9_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_green.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb9_green.Invoke(new Action(() =>
                {
                    this.pb9_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb9_orange.InvokeRequired)
            {
                // 使用委托
                t7Delegate myDelegate = new t7Delegate(t7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb9_orange.Invoke(new Action(() =>
                {
                    this.pb9_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb9_orange.Visible = false;
            }
        }

        // 定义委托—P7状态
        private delegate void p7Delegate();

        // 定于所要委托的方法-差信号
        private void p7BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb13_red.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7BadDelegateMethod);
                //Invoke delegate
                this.pb13_red.Invoke(new Action(() =>
                {
                    this.pb13_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_yellow.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7BadDelegateMethod);
                //Invoke delegate
                this.pb13_yellow.Invoke(new Action(() =>
                {
                    this.pb13_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_green.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7BadDelegateMethod);
                //Invoke delegate
                this.pb13_green.Invoke(new Action(() =>
                {
                    this.pb13_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_orange.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7BadDelegateMethod);
                //Invoke delegate
                this.pb13_orange.Invoke(new Action(() =>
                {
                    this.pb13_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void p7PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb13_red.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7PoorDelegateMethod);
                //Invoke delegate
                this.pb13_red.Invoke(new Action(() =>
                {
                    this.pb13_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_yellow.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7PoorDelegateMethod);
                //Invoke delegate
                this.pb13_yellow.Invoke(new Action(() =>
                {
                    this.pb13_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_green.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7PoorDelegateMethod);
                //Invoke delegate
                this.pb13_green.Invoke(new Action(() =>
                {
                    this.pb13_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_orange.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7PoorDelegateMethod);
                //Invoke delegate
                this.pb13_orange.Invoke(new Action(() =>
                {
                    this.pb13_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void p7FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb13_red.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7FairDelegateMethod);
                //Invoke delegate
                this.pb13_red.Invoke(new Action(() =>
                {
                    this.pb13_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_yellow.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7FairDelegateMethod);
                //Invoke delegate
                this.pb13_yellow.Invoke(new Action(() =>
                {
                    this.pb13_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_green.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7FairDelegateMethod);
                //Invoke delegate
                this.pb13_green.Invoke(new Action(() =>
                {
                    this.pb13_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_orange.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7FairDelegateMethod);
                //Invoke delegate
                this.pb13_orange.Invoke(new Action(() =>
                {
                    this.pb13_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void p7GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb13_red.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7GoodDelegateMethod);
                //Invoke delegate
                this.pb13_red.Invoke(new Action(() =>
                {
                    this.pb13_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_yellow.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7GoodDelegateMethod);
                //Invoke delegate
                this.pb13_yellow.Invoke(new Action(() =>
                {
                    this.pb13_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_green.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7GoodDelegateMethod);
                //Invoke delegate
                this.pb13_green.Invoke(new Action(() =>
                {
                    this.pb13_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_orange.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7GoodDelegateMethod);
                //Invoke delegate
                this.pb13_orange.Invoke(new Action(() =>
                {
                    this.pb13_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void p7NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb13_red.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb13_red.Invoke(new Action(() =>
                {
                    this.pb13_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_yellow.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb13_yellow.Invoke(new Action(() =>
                {
                    this.pb13_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_green.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb13_green.Invoke(new Action(() =>
                {
                    this.pb13_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb13_orange.InvokeRequired)
            {
                // 使用委托
                p7Delegate myDelegate = new p7Delegate(p7NoSignalDelegateMethod);
                //Invoke delegate
                this.pb13_orange.Invoke(new Action(() =>
                {
                    this.pb13_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb13_orange.Visible = false;
            }
        }

        // 定义委托—O1状态
        private delegate void o1Delegate();

        // 定于所要委托的方法-差信号
        private void o1BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb15_red.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1BadDelegateMethod);
                //Invoke delegate
                this.pb15_red.Invoke(new Action(() =>
                {
                    this.pb15_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_yellow.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1BadDelegateMethod);
                //Invoke delegate
                this.pb15_yellow.Invoke(new Action(() =>
                {
                    this.pb15_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_green.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1BadDelegateMethod);
                //Invoke delegate
                this.pb15_green.Invoke(new Action(() =>
                {
                    this.pb15_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_orange.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1BadDelegateMethod);
                //Invoke delegate
                this.pb15_orange.Invoke(new Action(() =>
                {
                    this.pb15_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void o1PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb15_red.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1PoorDelegateMethod);
                //Invoke delegate
                this.pb15_red.Invoke(new Action(() =>
                {
                    this.pb15_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_yellow.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1PoorDelegateMethod);
                //Invoke delegate
                this.pb15_yellow.Invoke(new Action(() =>
                {
                    this.pb15_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_green.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1PoorDelegateMethod);
                //Invoke delegate
                this.pb15_green.Invoke(new Action(() =>
                {
                    this.pb15_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_orange.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1PoorDelegateMethod);
                //Invoke delegate
                this.pb15_orange.Invoke(new Action(() =>
                {
                    this.pb15_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void o1FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb15_red.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1FairDelegateMethod);
                //Invoke delegate
                this.pb15_red.Invoke(new Action(() =>
                {
                    this.pb15_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_yellow.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1FairDelegateMethod);
                //Invoke delegate
                this.pb15_yellow.Invoke(new Action(() =>
                {
                    this.pb15_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_green.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1FairDelegateMethod);
                //Invoke delegate
                this.pb15_green.Invoke(new Action(() =>
                {
                    this.pb15_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_orange.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1FairDelegateMethod);
                //Invoke delegate
                this.pb15_orange.Invoke(new Action(() =>
                {
                    this.pb15_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void o1GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb15_red.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1GoodDelegateMethod);
                //Invoke delegate
                this.pb15_red.Invoke(new Action(() =>
                {
                    this.pb15_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_yellow.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1GoodDelegateMethod);
                //Invoke delegate
                this.pb15_yellow.Invoke(new Action(() =>
                {
                    this.pb15_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_green.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1GoodDelegateMethod);
                //Invoke delegate
                this.pb15_green.Invoke(new Action(() =>
                {
                    this.pb15_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_orange.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1GoodDelegateMethod);
                //Invoke delegate
                this.pb15_orange.Invoke(new Action(() =>
                {
                    this.pb15_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void o1NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb15_red.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1NoSignalDelegateMethod);
                //Invoke delegate
                this.pb15_red.Invoke(new Action(() =>
                {
                    this.pb15_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_yellow.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1NoSignalDelegateMethod);
                //Invoke delegate
                this.pb15_yellow.Invoke(new Action(() =>
                {
                    this.pb15_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_green.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1NoSignalDelegateMethod);
                //Invoke delegate
                this.pb15_green.Invoke(new Action(() =>
                {
                    this.pb15_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb15_orange.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1NoSignalDelegateMethod);
                //Invoke delegate
                this.pb15_orange.Invoke(new Action(() =>
                {
                    this.pb15_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb15_orange.Visible = false;
            }
        }

        // 定义委托—O2状态
        private delegate void o2Delegate();

        // 定于所要委托的方法-差信号
        private void o2BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb16_red.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2BadDelegateMethod);
                //Invoke delegate
                this.pb16_red.Invoke(new Action(() =>
                {
                    this.pb16_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_yellow.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2BadDelegateMethod);
                //Invoke delegate
                this.pb16_yellow.Invoke(new Action(() =>
                {
                    this.pb16_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_green.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2BadDelegateMethod);
                //Invoke delegate
                this.pb16_green.Invoke(new Action(() =>
                {
                    this.pb16_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_orange.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2BadDelegateMethod);
                //Invoke delegate
                this.pb16_orange.Invoke(new Action(() =>
                {
                    this.pb16_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void o2PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb16_red.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2PoorDelegateMethod);
                //Invoke delegate
                this.pb16_red.Invoke(new Action(() =>
                {
                    this.pb16_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_yellow.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2PoorDelegateMethod);
                //Invoke delegate
                this.pb16_yellow.Invoke(new Action(() =>
                {
                    this.pb16_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_green.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2PoorDelegateMethod);
                //Invoke delegate
                this.pb16_green.Invoke(new Action(() =>
                {
                    this.pb16_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_orange.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2PoorDelegateMethod);
                //Invoke delegate
                this.pb16_orange.Invoke(new Action(() =>
                {
                    this.pb16_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void o2FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb16_red.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2FairDelegateMethod);
                //Invoke delegate
                this.pb16_red.Invoke(new Action(() =>
                {
                    this.pb16_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_yellow.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2FairDelegateMethod);
                //Invoke delegate
                this.pb16_yellow.Invoke(new Action(() =>
                {
                    this.pb16_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_green.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2FairDelegateMethod);
                //Invoke delegate
                this.pb16_green.Invoke(new Action(() =>
                {
                    this.pb16_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_orange.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2FairDelegateMethod);
                //Invoke delegate
                this.pb16_orange.Invoke(new Action(() =>
                {
                    this.pb16_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void o2GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb16_red.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2GoodDelegateMethod);
                //Invoke delegate
                this.pb16_red.Invoke(new Action(() =>
                {
                    this.pb16_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_yellow.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2GoodDelegateMethod);
                //Invoke delegate
                this.pb16_yellow.Invoke(new Action(() =>
                {
                    this.pb16_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_green.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2GoodDelegateMethod);
                //Invoke delegate
                this.pb16_green.Invoke(new Action(() =>
                {
                    this.pb16_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_orange.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2GoodDelegateMethod);
                //Invoke delegate
                this.pb16_orange.Invoke(new Action(() =>
                {
                    this.pb16_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void o2NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb16_red.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2NoSignalDelegateMethod);
                //Invoke delegate
                this.pb16_red.Invoke(new Action(() =>
                {
                    this.pb16_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_yellow.InvokeRequired)
            {
                // 使用委托
                o1Delegate myDelegate = new o1Delegate(o1NoSignalDelegateMethod);
                //Invoke delegate
                this.pb16_yellow.Invoke(new Action(() =>
                {
                    this.pb16_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_green.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2NoSignalDelegateMethod);
                //Invoke delegate
                this.pb16_green.Invoke(new Action(() =>
                {
                    this.pb16_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb16_orange.InvokeRequired)
            {
                // 使用委托
                o2Delegate myDelegate = new o2Delegate(o2NoSignalDelegateMethod);
                //Invoke delegate
                this.pb16_orange.Invoke(new Action(() =>
                {
                    this.pb16_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb16_orange.Visible = false;
            }
        }

        // 定义委托—P8状态
        private delegate void p8Delegate();

        // 定于所要委托的方法-差信号
        private void p8BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb14_red.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8BadDelegateMethod);
                //Invoke delegate
                this.pb14_red.Invoke(new Action(() =>
                {
                    this.pb14_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_yellow.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8BadDelegateMethod);
                //Invoke delegate
                this.pb14_yellow.Invoke(new Action(() =>
                {
                    this.pb14_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_green.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8BadDelegateMethod);
                //Invoke delegate
                this.pb14_green.Invoke(new Action(() =>
                {
                    this.pb14_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_orange.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8BadDelegateMethod);
                //Invoke delegate
                this.pb14_orange.Invoke(new Action(() =>
                {
                    this.pb14_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void p8PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb14_red.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8PoorDelegateMethod);
                //Invoke delegate
                this.pb14_red.Invoke(new Action(() =>
                {
                    this.pb14_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_yellow.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8PoorDelegateMethod);
                //Invoke delegate
                this.pb14_yellow.Invoke(new Action(() =>
                {
                    this.pb14_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_green.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8PoorDelegateMethod);
                //Invoke delegate
                this.pb14_green.Invoke(new Action(() =>
                {
                    this.pb14_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_orange.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8PoorDelegateMethod);
                //Invoke delegate
                this.pb14_orange.Invoke(new Action(() =>
                {
                    this.pb14_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void p8FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb14_red.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8FairDelegateMethod);
                //Invoke delegate
                this.pb14_red.Invoke(new Action(() =>
                {
                    this.pb14_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_yellow.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8FairDelegateMethod);
                //Invoke delegate
                this.pb14_yellow.Invoke(new Action(() =>
                {
                    this.pb14_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_green.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8FairDelegateMethod);
                //Invoke delegate
                this.pb14_green.Invoke(new Action(() =>
                {
                    this.pb14_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_orange.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8FairDelegateMethod);
                //Invoke delegate
                this.pb14_orange.Invoke(new Action(() =>
                {
                    this.pb14_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void p8GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb14_red.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8GoodDelegateMethod);
                //Invoke delegate
                this.pb14_red.Invoke(new Action(() =>
                {
                    this.pb14_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_yellow.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8GoodDelegateMethod);
                //Invoke delegate
                this.pb14_yellow.Invoke(new Action(() =>
                {
                    this.pb14_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_green.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8GoodDelegateMethod);
                //Invoke delegate
                this.pb14_green.Invoke(new Action(() =>
                {
                    this.pb14_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_orange.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8GoodDelegateMethod);
                //Invoke delegate
                this.pb14_orange.Invoke(new Action(() =>
                {
                    this.pb14_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void p8NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb14_red.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb14_red.Invoke(new Action(() =>
                {
                    this.pb14_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_yellow.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb14_yellow.Invoke(new Action(() =>
                {
                    this.pb14_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_green.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb14_green.Invoke(new Action(() =>
                {
                    this.pb14_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb14_orange.InvokeRequired)
            {
                // 使用委托
                p8Delegate myDelegate = new p8Delegate(p8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb14_orange.Invoke(new Action(() =>
                {
                    this.pb14_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb14_orange.Visible = false;
            }
        }

        // 定义委托—T8状态
        private delegate void  t8Delegate();

        // 定于所要委托的方法-差信号
        private void t8BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb10_red.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8BadDelegateMethod);
                //Invoke delegate
                this.pb10_red.Invoke(new Action(() =>
                {
                    this.pb10_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_yellow.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8BadDelegateMethod);
                //Invoke delegate
                this.pb10_yellow.Invoke(new Action(() =>
                {
                    this.pb10_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_green.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8BadDelegateMethod);
                //Invoke delegate
                this.pb10_green.Invoke(new Action(() =>
                {
                    this.pb10_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_orange.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8BadDelegateMethod);
                //Invoke delegate
                this.pb10_orange.Invoke(new Action(() =>
                {
                    this.pb10_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void t8PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb10_red.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8PoorDelegateMethod);
                //Invoke delegate
                this.pb10_red.Invoke(new Action(() =>
                {
                    this.pb10_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_yellow.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8PoorDelegateMethod);
                //Invoke delegate
                this.pb10_yellow.Invoke(new Action(() =>
                {
                    this.pb10_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_green.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8PoorDelegateMethod);
                //Invoke delegate
                this.pb10_green.Invoke(new Action(() =>
                {
                    this.pb10_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_orange.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8PoorDelegateMethod);
                //Invoke delegate
                this.pb10_orange.Invoke(new Action(() =>
                {
                    this.pb10_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void t8FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb10_red.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8FairDelegateMethod);
                //Invoke delegate
                this.pb10_red.Invoke(new Action(() =>
                {
                    this.pb10_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_yellow.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8FairDelegateMethod);
                //Invoke delegate
                this.pb10_yellow.Invoke(new Action(() =>
                {
                    this.pb10_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_green.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8FairDelegateMethod);
                //Invoke delegate
                this.pb10_green.Invoke(new Action(() =>
                {
                    this.pb10_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_orange.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8FairDelegateMethod);
                //Invoke delegate
                this.pb10_orange.Invoke(new Action(() =>
                {
                    this.pb10_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void t8GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb10_red.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8GoodDelegateMethod);
                //Invoke delegate
                this.pb10_red.Invoke(new Action(() =>
                {
                    this.pb10_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_yellow.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8GoodDelegateMethod);
                //Invoke delegate
                this.pb10_yellow.Invoke(new Action(() =>
                {
                    this.pb10_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_green.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8GoodDelegateMethod);
                //Invoke delegate
                this.pb10_green.Invoke(new Action(() =>
                {
                    this.pb10_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_orange.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8GoodDelegateMethod);
                //Invoke delegate
                this.pb10_orange.Invoke(new Action(() =>
                {
                    this.pb10_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void t8NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb10_red.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb10_red.Invoke(new Action(() =>
                {
                    this.pb10_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_yellow.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb10_yellow.Invoke(new Action(() =>
                {
                    this.pb10_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_green.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb10_green.Invoke(new Action(() =>
                {
                    this.pb10_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb10_orange.InvokeRequired)
            {
                // 使用委托
                t8Delegate myDelegate = new t8Delegate(t8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb10_orange.Invoke(new Action(() =>
                {
                    this.pb10_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb10_orange.Visible = false;
            }
        }

        // 定义委托—FC6状态
        private delegate void fc6Delegate();

        // 定于所要委托的方法-差信号
        private void fc6BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb8_red.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6BadDelegateMethod);
                //Invoke delegate
                this.pb8_red.Invoke(new Action(() =>
                {
                    this.pb8_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_yellow.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6BadDelegateMethod);
                //Invoke delegate
                this.pb8_yellow.Invoke(new Action(() =>
                {
                    this.pb8_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_green.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6BadDelegateMethod);
                //Invoke delegate
                this.pb8_green.Invoke(new Action(() =>
                {
                    this.pb8_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_orange.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6BadDelegateMethod);
                //Invoke delegate
                this.pb8_orange.Invoke(new Action(() =>
                {
                    this.pb8_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void fc6PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb8_red.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6PoorDelegateMethod);
                //Invoke delegate
                this.pb8_red.Invoke(new Action(() =>
                {
                    this.pb8_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_yellow.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6PoorDelegateMethod);
                //Invoke delegate
                this.pb8_yellow.Invoke(new Action(() =>
                {
                    this.pb8_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_green.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6PoorDelegateMethod);
                //Invoke delegate
                this.pb8_green.Invoke(new Action(() =>
                {
                    this.pb8_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_orange.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6PoorDelegateMethod);
                //Invoke delegate
                this.pb8_orange.Invoke(new Action(() =>
                {
                    this.pb8_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void fc6FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb8_red.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6FairDelegateMethod);
                //Invoke delegate
                this.pb8_red.Invoke(new Action(() =>
                {
                    this.pb8_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_yellow.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6FairDelegateMethod);
                //Invoke delegate
                this.pb8_yellow.Invoke(new Action(() =>
                {
                    this.pb8_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_green.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6FairDelegateMethod);
                //Invoke delegate
                this.pb8_green.Invoke(new Action(() =>
                {
                    this.pb8_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_orange.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6FairDelegateMethod);
                //Invoke delegate
                this.pb8_orange.Invoke(new Action(() =>
                {
                    this.pb8_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void fc6GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb8_red.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6GoodDelegateMethod);
                //Invoke delegate
                this.pb8_red.Invoke(new Action(() =>
                {
                    this.pb8_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_yellow.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6GoodDelegateMethod);
                //Invoke delegate
                this.pb8_yellow.Invoke(new Action(() =>
                {
                    this.pb8_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_green.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6GoodDelegateMethod);
                //Invoke delegate
                this.pb8_green.Invoke(new Action(() =>
                {
                    this.pb8_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_orange.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6GoodDelegateMethod);
                //Invoke delegate
                this.pb8_orange.Invoke(new Action(() =>
                {
                    this.pb8_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void fc6NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb8_red.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6NoSignalDelegateMethod);
                //Invoke delegate
                this.pb8_red.Invoke(new Action(() =>
                {
                    this.pb8_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_yellow.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6NoSignalDelegateMethod);
                //Invoke delegate
                this.pb8_yellow.Invoke(new Action(() =>
                {
                    this.pb8_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_green.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6NoSignalDelegateMethod);
                //Invoke delegate
                this.pb8_green.Invoke(new Action(() =>
                {
                    this.pb8_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb8_orange.InvokeRequired)
            {
                // 使用委托
                fc6Delegate myDelegate = new fc6Delegate(fc6NoSignalDelegateMethod);
                //Invoke delegate
                this.pb8_orange.Invoke(new Action(() =>
                {
                    this.pb8_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb8_orange.Visible = false;
            }
        }

        // 定义委托—F4状态
        private delegate void f4Delegate();

        // 定于所要委托的方法-差信号
        private void f4BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb6_red.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4BadDelegateMethod);
                //Invoke delegate
                this.pb6_red.Invoke(new Action(() =>
                {
                    this.pb6_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_yellow.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4BadDelegateMethod);
                //Invoke delegate
                this.pb6_yellow.Invoke(new Action(() =>
                {
                    this.pb6_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_green.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4BadDelegateMethod);
                //Invoke delegate
                this.pb6_green.Invoke(new Action(() =>
                {
                    this.pb6_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_orange.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4BadDelegateMethod);
                //Invoke delegate
                this.pb6_orange.Invoke(new Action(() =>
                {
                    this.pb6_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void f4PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb6_red.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4PoorDelegateMethod);
                //Invoke delegate
                this.pb6_red.Invoke(new Action(() =>
                {
                    this.pb6_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_yellow.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4PoorDelegateMethod);
                //Invoke delegate
                this.pb6_yellow.Invoke(new Action(() =>
                {
                    this.pb6_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_green.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4PoorDelegateMethod);
                //Invoke delegate
                this.pb6_green.Invoke(new Action(() =>
                {
                    this.pb6_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_orange.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4PoorDelegateMethod);
                //Invoke delegate
                this.pb6_orange.Invoke(new Action(() =>
                {
                    this.pb6_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void f4FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb6_red.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4FairDelegateMethod);
                //Invoke delegate
                this.pb6_red.Invoke(new Action(() =>
                {
                    this.pb6_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_yellow.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4FairDelegateMethod);
                //Invoke delegate
                this.pb6_yellow.Invoke(new Action(() =>
                {
                    this.pb6_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_green.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4FairDelegateMethod);
                //Invoke delegate
                this.pb6_green.Invoke(new Action(() =>
                {
                    this.pb6_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_orange.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4FairDelegateMethod);
                //Invoke delegate
                this.pb6_orange.Invoke(new Action(() =>
                {
                    this.pb6_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void f4GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb6_red.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4GoodDelegateMethod);
                //Invoke delegate
                this.pb6_red.Invoke(new Action(() =>
                {
                    this.pb6_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_yellow.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4GoodDelegateMethod);
                //Invoke delegate
                this.pb6_yellow.Invoke(new Action(() =>
                {
                    this.pb6_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_green.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4GoodDelegateMethod);
                //Invoke delegate
                this.pb6_green.Invoke(new Action(() =>
                {
                    this.pb6_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_orange.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4GoodDelegateMethod);
                //Invoke delegate
                this.pb6_orange.Invoke(new Action(() =>
                {
                    this.pb6_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void f4NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb6_red.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb6_red.Invoke(new Action(() =>
                {
                    this.pb6_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_yellow.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb6_yellow.Invoke(new Action(() =>
                {
                    this.pb6_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_green.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb6_green.Invoke(new Action(() =>
                {
                    this.pb6_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb6_orange.InvokeRequired)
            {
                // 使用委托
                f4Delegate myDelegate = new f4Delegate(f4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb6_orange.Invoke(new Action(() =>
                {
                    this.pb6_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb6_orange.Visible = false;
            }
        }

        // 定义委托—F8状态
        private delegate void f8Delegate();

        // 定于所要委托的方法-差信号
        private void f8BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb4_red.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8BadDelegateMethod);
                //Invoke delegate
                this.pb4_red.Invoke(new Action(() =>
                {
                    this.pb4_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_yellow.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8BadDelegateMethod);
                //Invoke delegate
                this.pb4_yellow.Invoke(new Action(() =>
                {
                    this.pb4_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_green.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8BadDelegateMethod);
                //Invoke delegate
                this.pb4_green.Invoke(new Action(() =>
                {
                    this.pb4_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_orange.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8BadDelegateMethod);
                //Invoke delegate
                this.pb4_orange.Invoke(new Action(() =>
                {
                    this.pb4_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void f8PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb4_red.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8PoorDelegateMethod);
                //Invoke delegate
                this.pb4_red.Invoke(new Action(() =>
                {
                    this.pb4_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_yellow.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8PoorDelegateMethod);
                //Invoke delegate
                this.pb4_yellow.Invoke(new Action(() =>
                {
                    this.pb4_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_green.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8PoorDelegateMethod);
                //Invoke delegate
                this.pb4_green.Invoke(new Action(() =>
                {
                    this.pb4_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_orange.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8PoorDelegateMethod);
                //Invoke delegate
                this.pb4_orange.Invoke(new Action(() =>
                {
                    this.pb4_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void f8FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb4_red.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8FairDelegateMethod);
                //Invoke delegate
                this.pb4_red.Invoke(new Action(() =>
                {
                    this.pb4_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_yellow.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8FairDelegateMethod);
                //Invoke delegate
                this.pb4_yellow.Invoke(new Action(() =>
                {
                    this.pb4_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_green.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8FairDelegateMethod);
                //Invoke delegate
                this.pb4_green.Invoke(new Action(() =>
                {
                    this.pb4_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_orange.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8FairDelegateMethod);
                //Invoke delegate
                this.pb4_orange.Invoke(new Action(() =>
                {
                    this.pb4_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void f8GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb4_red.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8GoodDelegateMethod);
                //Invoke delegate
                this.pb4_red.Invoke(new Action(() =>
                {
                    this.pb4_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_yellow.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8GoodDelegateMethod);
                //Invoke delegate
                this.pb4_yellow.Invoke(new Action(() =>
                {
                    this.pb4_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_green.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8GoodDelegateMethod);
                //Invoke delegate
                this.pb4_green.Invoke(new Action(() =>
                {
                    this.pb4_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_orange.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8GoodDelegateMethod);
                //Invoke delegate
                this.pb4_orange.Invoke(new Action(() =>
                {
                    this.pb4_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void f8NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb4_red.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb4_red.Invoke(new Action(() =>
                {
                    this.pb4_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_yellow.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb4_yellow.Invoke(new Action(() =>
                {
                    this.pb4_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_green.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb4_green.Invoke(new Action(() =>
                {
                    this.pb4_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb4_orange.InvokeRequired)
            {
                // 使用委托
                f8Delegate myDelegate = new f8Delegate(f8NoSignalDelegateMethod);
                //Invoke delegate
                this.pb4_orange.Invoke(new Action(() =>
                {
                    this.pb4_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb4_orange.Visible = false;
            }
        }

        // 定义委托—AF4状态
        private delegate void af4Delegate();

        // 定于所要委托的方法-差信号
        private void af4BadDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb2_red.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4BadDelegateMethod);
                //Invoke delegate
                this.pb2_red.Invoke(new Action(() =>
                {
                    this.pb2_red.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_red.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_yellow.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4BadDelegateMethod);
                //Invoke delegate
                this.pb2_yellow.Invoke(new Action(() =>
                {
                    this.pb2_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_green.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4BadDelegateMethod);
                //Invoke delegate
                this.pb2_green.Invoke(new Action(() =>
                {
                    this.pb2_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_orange.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4BadDelegateMethod);
                //Invoke delegate
                this.pb2_orange.Invoke(new Action(() =>
                {
                    this.pb2_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-弱信号
        private void af4PoorDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb2_red.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4PoorDelegateMethod);
                //Invoke delegate
                this.pb2_red.Invoke(new Action(() =>
                {
                    this.pb2_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_yellow.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4PoorDelegateMethod);
                //Invoke delegate
                this.pb2_yellow.Invoke(new Action(() =>
                {
                    this.pb2_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_green.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4PoorDelegateMethod);
                //Invoke delegate
                this.pb2_green.Invoke(new Action(() =>
                {
                    this.pb2_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_orange.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4PoorDelegateMethod);
                //Invoke delegate
                this.pb2_orange.Invoke(new Action(() =>
                {
                    this.pb2_orange.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_orange.Visible = true;
            }
        }

        // 定于所要委托的方法-一般信号
        private void af4FairDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb2_red.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4FairDelegateMethod);
                //Invoke delegate
                this.pb2_red.Invoke(new Action(() =>
                {
                    this.pb2_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_yellow.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4FairDelegateMethod);
                //Invoke delegate
                this.pb2_yellow.Invoke(new Action(() =>
                {
                    this.pb2_yellow.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_yellow.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_green.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4FairDelegateMethod);
                //Invoke delegate
                this.pb2_green.Invoke(new Action(() =>
                {
                    this.pb2_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_orange.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4FairDelegateMethod);
                //Invoke delegate
                this.pb2_orange.Invoke(new Action(() =>
                {
                    this.pb2_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-强信号
        private void af4GoodDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb2_red.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4GoodDelegateMethod);
                //Invoke delegate
                this.pb2_red.Invoke(new Action(() =>
                {
                    this.pb2_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_yellow.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4GoodDelegateMethod);
                //Invoke delegate
                this.pb2_yellow.Invoke(new Action(() =>
                {
                    this.pb2_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_green.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4GoodDelegateMethod);
                //Invoke delegate
                this.pb2_green.Invoke(new Action(() =>
                {
                    this.pb2_green.Visible = true;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_green.Visible = true;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_orange.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4GoodDelegateMethod);
                //Invoke delegate
                this.pb2_orange.Invoke(new Action(() =>
                {
                    this.pb2_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_orange.Visible = false;
            }
        }

        // 定于所要委托的方法-没信号
        private void af4NoSignalDelegateMethod()
        {
            // 判断是否达到InvokeRequired
            if (this.pb2_red.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb2_red.Invoke(new Action(() =>
                {
                    this.pb2_red.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_red.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_yellow.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb2_yellow.Invoke(new Action(() =>
                {
                    this.pb2_yellow.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_yellow.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_green.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb2_green.Invoke(new Action(() =>
                {
                    this.pb2_green.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_green.Visible = false;
            }

            // 判断是否达到InvokeRequired
            if (this.pb2_orange.InvokeRequired)
            {
                // 使用委托
                af4Delegate myDelegate = new af4Delegate(af4NoSignalDelegateMethod);
                //Invoke delegate
                this.pb2_orange.Invoke(new Action(() =>
                {
                    this.pb2_orange.Visible = false;
                }));
            }
            else
            {
                // 没使用InvokeRequired
                this.pb2_orange.Visible = false;
            }
        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // WIFI连接
        private void button9_Click(object sender, EventArgs e)
        {
            if (isWifiConnected)
            {
                isWifiConnected = false;
                bt_wifiConnect.Text = "连 接";
                socket.Close();

            }
            else
            {
                
                //创建一个Socket
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //连接到指定服务器的指定端口
                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.connect.aspx
                string ip = this.tb_ip.Text;
                int port = Convert.ToInt32(this.tb_port.Text);
                socket.Connect(ip, port);
                if (socket != null)
                {
                    Console.WriteLine("connect to the server");
                    isWifiConnected = true;
                    bt_wifiConnect.Text = "连接中";
                    //实现接受消息的方法

                    //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.beginreceive.aspx
                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);

                    //接受用户输入，将消息发送给服务器端
                    richBoxDelegateMethod("WIFI连接成功");
                }
                else{
                    richBoxDelegateMethod("WIFI连接失败");
                }

                //f = new Thread(plane);
                //f.Start();
            }
        }

        // 飞行器线程
        void plane()
        {
            try
            {
                //创建一个Socket
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //连接到指定服务器的指定端口
                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.connect.aspx
                string ip = this.tb_ip.Text;
                int port = Convert.ToInt32(this.tb_port.Text);
                socket.Connect(ip, port);
                if (socket != null)
                {
                    Console.WriteLine("connect to the server");

                    //实现接受消息的方法

                    //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.beginreceive.aspx
                    //socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);

                    //接受用户输入，将消息发送给服务器端
                    richBoxDelegateMethod("WIFI连接成功");
                }
                else
                {
                    richBoxDelegateMethod("WIFI连接失败");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        static byte[] buffer = new byte[1024];

        public void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                var socket = ar.AsyncState as Socket;

                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
                var length = socket.EndReceive(ar);
                //读取出来消息内容
                var message = Encoding.Unicode.GetString(buffer, 0, length);
                //显示消息
                Console.WriteLine(message);

                //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // 解锁电机
        private void button10_Click(object sender, EventArgs e)
        {
            if (isWifiConnected && socket != null)
            {
                if (fly_unlock == false)
                {
                    try
                    {
                        fly_unlock = true;
                        this.bt_dj.Text = "加锁电机";
                        this.bt_dj.BackColor = Color.Red;
                        richBoxDelegateMethod("电机已经解锁 请注意安全");
                        String command = adding_protocal("PWON");
                        byte[] outputBuffer = Encoding.ASCII.GetBytes(command);
                        socket.Send(outputBuffer);
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    try
                    {
                        fly_unlock = false;
                        this.bt_dj.Text = "解锁电机";
                        this.bt_dj.BackColor = Color.White;
                        richBoxDelegateMethod("电机已经加锁");
                        String command = adding_protocal("PWOF");
                        var outputBuffer = Encoding.ASCII.GetBytes(command);
                        socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private String adding_protocal(String str_input)
        {
            char[] asc_code_array = str_input.ToCharArray();
            int checksume = 0;
            //
            for (int i = 0; i < asc_code_array.Length; i++) checksume += asc_code_array[i];
            checksume = 256 - (checksume & 0xff);
            int checksum_low = checksume & 0x0f;
            int checksum_high = (checksume & 0x0f0) / 16;
            if (checksum_low < 10) checksum_low += 48;
            else checksum_low += 55;
            if (checksum_high < 10) checksum_high += 48;
            else checksum_high += 55;
            String str = ":" + str_input + (char)checksum_high + (char)checksum_low + "/";
            //
            return str;
        }

        // 一键起飞
        private void button20_Click(object sender, EventArgs e)
        {
            if (isWifiConnected && socket != null)
            {
                try
                {
                    richBoxDelegateMethod("使用了一键起飞");
                    String command = adding_protocal("RC64000000E1/");
                    var outputBuffer = Encoding.ASCII.GetBytes(command);
                    socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // 一键降落
        private void button22_Click(object sender, EventArgs e)
        {
            if (isWifiConnected && socket != null)
            {
                try
                {
                    richBoxDelegateMethod("使用了一键降落");
                    String command = adding_protocal("RC00000000");
                    var outputBuffer = Encoding.ASCII.GetBytes(command);
                    socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // 水平校准监听
        private void button11_Click(object sender, EventArgs e)
        {
            if (isWifiConnected && socket != null)
            {
                try
                {
                    richBoxDelegateMethod("水平校准");
                    String command = adding_protocal("CAL30/");
                    var outputBuffer = Encoding.ASCII.GetBytes(command);
                    socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // 前飞按钮监听
        private void button15_Click(object sender, EventArgs e)
        {
            if (isWifiConnected && socket != null)
            {
                try
                {
                    richBoxDelegateMethod("前飞");
                    String command = adding_protocal("RC64040000D0/");
                    var outputBuffer = Encoding.ASCII.GetBytes(command);
                    socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // 后飞
        private void button18_Click(object sender, EventArgs e)
        {
            if (isWifiConnected && socket != null)
            {
                try
                {
                    richBoxDelegateMethod("后飞");
                    String command = adding_protocal("RC64840000D5/");
                    var outputBuffer = Encoding.ASCII.GetBytes(command);
                    socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // 设置高度
        private void button21_Click(object sender, EventArgs e)
        {
            if (isWifiConnected && socket != null)
            {
                try
                {
                    richBoxDelegateMethod("设置飞行高度");
                    String command = adding_protocal("WRHT0088EB/");
                    var outputBuffer = Encoding.ASCII.GetBytes(command);
                    socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // 左飞监听
        private void button16_Click(object sender, EventArgs e)
        {
            zuofei();
        }

        // 左飞
        private void zuofei()
        {
            if (isWifiConnected && socket != null)
            {
                try
                {
                    richBoxDelegateMethod("左飞");
                    if (ear_CraftConnected == true)
                    {
                        richBox3DelegateMethod("左飞");
                    }
                    String command = adding_protocal("RC64000400DD/");
                    var outputBuffer = Encoding.ASCII.GetBytes(command);
                    socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // 右飞监听
        private void button17_Click(object sender, EventArgs e)
        {
            youfei();
        }

        // 右飞
        private void youfei()
        {
            if (isWifiConnected && socket != null)
            {
                try
                {
                    richBoxDelegateMethod("右飞");
                    if (ear_CraftConnected == true)
                    {
                        richBox3DelegateMethod("右飞");
                    }
                    String command = adding_protocal("RC64008400D5/");
                    var outputBuffer = Encoding.ASCII.GetBytes(command);
                    socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        // 回中监听
        private void button19_Click(object sender, EventArgs e)
        {
            huizhong();
        }

        // 回中
        private void huizhong()
        {
            if (isWifiConnected && socket != null)
            {
                try
                {
                    richBoxDelegateMethod("回中");
                    String command = adding_protocal("RC64000000E1/");
                    var outputBuffer = Encoding.ASCII.GetBytes(command);
                    socket.BeginSend(outputBuffer, 0, outputBuffer.Length, SocketFlags.None, null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void label30_Click(object sender, EventArgs e)
        {

        }

        private void cogDataFileWrite(string filename)
        {
            FileStream fs;
            fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryWriter bw = new BinaryWriter(fs);
            //准备不同类型的数据  
            profile = EmoEngine.Instance.GetUserProfile(0);
            //利用Write 方法的多种重载形式写入数据  
            bw.Write(profile.GetBytes());
            fs.Close();
            bw.Close(); 
        }

        private string cogNumFileRead(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open); 
            StreamReader sw = new StreamReader(fs); 
            string str = sw.ReadLine();
            sw.Close();    
            fs.Close();
            return str;
        }

        private void cogNumFileWrite(string filename , string str)
        {
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);    
            StreamWriter sw = new StreamWriter(fs,Encoding.Default);    
            sw.Write(str);    
            sw.Close();    
            fs.Close();
        }

        // 连接耳机和四轴飞行器
        private void button9_Click_1(object sender, EventArgs e)
        {
            if (ear_CraftConnected == false) // 没连接
            {
                ear_CraftConnected = true;
                this.button9.Text = "脑电波耳机与四轴飞行器已连接";
            }
            else
            {
                ear_CraftConnected = false;
                this.button9.Text = "脑电波耳机与四轴飞行器未连接";
            }
        }

        public bool uploadFileByHttp(string webUrl, string localFileName)
        {
            string srcString = "";
            // 检查文件是否存在  
            if (!System.IO.File.Exists(localFileName))
            {
                MessageBox.Show("{0} does not exist!", localFileName);
                return false;
            }
            try
            {
                System.Net.WebClient myWebClient = new System.Net.WebClient();
                byte[] responseData = myWebClient.UploadFile(webUrl, "POST", localFileName);
                srcString = Encoding.UTF8.GetString(responseData);//解码
                MessageBox.Show(srcString);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void DownloadFile(string URL, string filename, System.Windows.Forms.ProgressBar prog)
        {
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;

                if (prog != null)
                {
                    prog.Maximum = (int)totalBytes;
                }

                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    System.Windows.Forms.Application.DoEvents();
                    so.Write(by, 0, osize);

                    if (prog != null)
                    {
                        prog.Value = (int)totalDownloadedByte;
                    }
                    osize = st.Read(by, 0, (int)by.Length);
                }
                so.Close();
                st.Close();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public void DownloadFile(string URL, string filename)
        {
            DownloadFile(URL, filename, null);
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            string dataN = ac + ".emo";
            string numN = ac + ".num";
            string dataNL = @"emo\" + dataN;
            string numNL = @"emo\" + numN;
            DownloadFile("http://localhost/emotive/Public/uploads/" + dataN, dataNL);
            DownloadFile("http://localhost/emotive/Public/uploads/" + numN, numNL);
        }

    }   
}  
