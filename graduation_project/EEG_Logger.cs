using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emotiv;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;


namespace graduation_project
{
    public class EEG_Logger
    {
        // 通过EmoEngine类去访问EDK
        EmoEngine engine;
        // 用户ID是用来唯一的识别用户的耳机
        int userID = -1;
        // 输出的原始文件名
        string originalFilename = "originalsingnal.csv";
        // 输出的原始文件名
        string afterDbFilename = "afterdbsingnal.csv"; 

        private bool isConnected = false;

        private bool isCognitivTrainingStarted   = false;
        private bool isCognitivTrainingSucceeded = false;
        private bool isCognitivTrainingCompleted = false;
        private bool isCognitivTrainingRejected  = false;

        static System.IO.StreamWriter cogLog = new System.IO.StreamWriter("cogLog.log");

        EdkDll.EE_EEG_ContactQuality_t[] contactQualityFromAllChannels;
        EdkDll.EE_SignalStrength_t signalStrength;
        Int32 numContactQualityChannels;
        Single timeFromStart;
        Int32 chargeLevel;
        Int32 maxChargeLevel;
        int cog = 0;

        
        public EEG_Logger()
        {
            // 创建engine 
            engine = EmoEngine.Instance;
            engine.UserAdded += 
                new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
            engine.EmoStateUpdated += 
                new EmoEngine.EmoStateUpdatedEventHandler(engine_EmoStateUpdated);
            engine.EmoEngineEmoStateUpdated += 
                new EmoEngine.EmoEngineEmoStateUpdatedEventHandler(engine_EmoEngineEmoStateUpdated);
            engine.CognitivEmoStateUpdated +=
                new EmoEngine.CognitivEmoStateUpdatedEventHandler(engine_CognitivEmoStateUpdated);
            engine.CognitivTrainingStarted +=
               new EmoEngine.CognitivTrainingStartedEventEventHandler(engine_CognitivTrainingStarted);
            engine.CognitivTrainingSucceeded +=
                new EmoEngine.CognitivTrainingSucceededEventHandler(engine_CognitivTrainingSucceeded);
            engine.CognitivTrainingCompleted +=
                new EmoEngine.CognitivTrainingCompletedEventHandler(engine_CognitivTrainingCompleted);
            engine.CognitivTrainingRejected +=
                new EmoEngine.CognitivTrainingRejectedEventHandler(engine_CognitivTrainingRejected);
            // engine.EmoEngineConnected += new EmoEngine.EmoEngineConnectedEventHandler(engine_Connected_Event);
            //engine.EmoEngineDisconnected += new EmoEngine.EmoEngineDisconnectedEventHandler(engine_DisConnected_Event);
            // 连接耳机            
            engine.Connect();

            // 创建输出文件的列头名称
            WriteHeader();
        }

        void engine_CognitivEmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
        {
            EmoState es = e.emoState;

            Single timeFromStart = es.GetTimeFromStart();

            EdkDll.EE_CognitivAction_t cogAction = es.CognitivGetCurrentAction();
            Single power = es.CognitivGetCurrentActionPower();
            Boolean isActive = es.CognitivIsActive();

            if (cogAction == EdkDll.EE_CognitivAction_t.COG_LIFT && isActive == true && power > 0.5)
            {
                cog = 1;
            }
            else if (cogAction == EdkDll.EE_CognitivAction_t.COG_DROP && isActive == true && power > 0.5)
            {
                cog = 2;
            }
            else if (cogAction == EdkDll.EE_CognitivAction_t.COG_PUSH && isActive == true && power > 0.5)
            {
                cog = 3;
            }
            else if (cogAction == EdkDll.EE_CognitivAction_t.COG_PULL && isActive == true && power > 0.5)
            {
                cog = 4;
            }
            else if (cogAction == EdkDll.EE_CognitivAction_t.COG_LEFT && isActive == true && power > 0.5)
            {
                cog = 5;
            }
            else if (cogAction == EdkDll.EE_CognitivAction_t.COG_RIGHT && isActive == true && power > 0.5)
            {
                cog = 6;
            }

            cogLog.WriteLine(
                "{0},{1},{2},{3}",
                timeFromStart,
                cogAction, power, isActive);
            cogLog.Flush();
        }

        public void setCog(int cog)
        {
            this.cog = cog;
        }

        public int getCog()
        {
            return this.cog;
        }

        public void setIsCognitivTrainingCompleted()
        {
            isCognitivTrainingCompleted = false;
        }

        public bool getIsCognitivTrainingStarted()
        {
            return isCognitivTrainingStarted;
        }

        public bool getIsCognitivTrainingSucceeded()
        {
            return isCognitivTrainingSucceeded;
        }

        public bool getIsCognitivTrainingCompleted()
        {
            return isCognitivTrainingCompleted;
        }

        public bool getIsCognitivTrainingRejected()
        {
            return isCognitivTrainingRejected;
        }


        void engine_CognitivTrainingStarted(object sender, EmoEngineEventArgs e)
        {
            Console.WriteLine("Start Cognitiv Training");
            isCognitivTrainingStarted = true;
        }

        void engine_CognitivTrainingSucceeded(object sender, EmoEngineEventArgs e)
        {
            isCognitivTrainingStarted = false;
            isCognitivTrainingSucceeded = true;
            Console.WriteLine("Cognitiv Training Success. (A)ccept/Reject?");
            //if ( MessageBox.Show("Cognitiv Training Success. (A)ccept/Reject?","确定信息",
            //    MessageBoxButtons.OKCancel) == DialogResult.OK )
            //{
                Console.WriteLine("Accept!!!");
                //MessageBox.Show("Accept!!!");
                EmoEngine.Instance.CognitivSetTrainingControl(0, EdkDll.EE_CognitivTrainingControl_t.COG_ACCEPT);
            //}
            //else
            //{
             //   EmoEngine.Instance.CognitivSetTrainingControl(0, EdkDll.EE_CognitivTrainingControl_t.COG_REJECT);
           // }
        }

        void engine_CognitivTrainingCompleted(object sender, EmoEngineEventArgs e)
        {
            isCognitivTrainingSucceeded = false;
            isCognitivTrainingCompleted = true;
            Console.WriteLine("Cognitiv Training Completed.");
        }

        void engine_CognitivTrainingRejected(object sender, EmoEngineEventArgs e)
        {
            isCognitivTrainingRejected = true;
            Console.WriteLine("Cognitiv Training Rejected.");
        }

        public bool getConnectedStatus()
        {
            return isConnected;
        }

        void engine_DisConnected_Event(object sender, EmoEngineEventArgs e)
        {
            isConnected = false;
        }

        void engine_Connected_Event(object sender, EmoEngineEventArgs e)
        {
            isConnected = true;
        }

        public void disconnected()
        {
            isConnected = false;
        }

        void engine_EmoEngineEmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
        {
            EmoState es = e.emoState;
            if (es.GetHeadsetOn() == 1)
            {
                isConnected = true;
            }
            else
            {
                isConnected = false;
            }
        }
        


        void engine_EmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
        {
            EmoState es = e.emoState;
            //Console.WriteLine("{0} ; excitement: {1} ", e.userId, es.AffectivGetEngagementBoredomScore());
            //EdkDll.EE_SignalStrength_t strength = es.GetWirelessSignalStatus();
            //String dd = strength.ToString();
            //int i = es.GetNumContactQualityChannels();
            //es.GetContactQualityFromAllChannels();
            if (es.GetHeadsetOn() == 1)
            {
                isConnected = true;
                this.timeFromStart = es.GetTimeFromStart();
                this.numContactQualityChannels = es.GetNumContactQualityChannels();
                this.contactQualityFromAllChannels = es.GetContactQualityFromAllChannels();
                this.signalStrength = es.GetWirelessSignalStatus();
                this.chargeLevel = 0;
                this.maxChargeLevel = 0;
                es.GetBatteryChargeLevel(out chargeLevel, out maxChargeLevel);
            }
            else
            {
                isConnected = false;
            }
        }

        public EdkDll.EE_EEG_ContactQuality_t[] getContactQualityFromAllChannels()
        {
            return contactQualityFromAllChannels;
        }

        public int getSignalStrength()
        {
            return (int)signalStrength;
        }

        public int getNumContactQualityChannelsChargeLevel()
        {
            return numContactQualityChannels;
        }

        public float getTimeFromStart()
        {
            return (float)timeFromStart;
        }

        public int getChargeLevel()
        {
            return chargeLevel;
        }

        public int getMaxChargeLevel()
        {
            return maxChargeLevel;
        }

        public void engine_UserAdded_Event(object sender, EmoEngineEventArgs e)
        {
            Console.WriteLine("User Added Event has occured");

            // 记录用户id
            userID = (int)e.userId;

            // enable data aquisition for this user.
            engine.DataAcquisitionEnable((uint)userID, true);
            
            // 请求1秒的缓存区数据
            engine.EE_DataSetBufferSizeInSec(1); 

        }

        public bool judgeOnContact()
        {
            // 处理任何等待事件
            engine.ProcessEvents(100);

            // 如果用户没连接好，就停止运行
            if ((int)userID == -1)
                return false;

            return true;
        }

        public void Run()
        {
            // 处理任何等待事件
            engine.ProcessEvents(1000);

            // 如果用户没连接好，就停止运行
            if ((int)userID == -1)
                return;

            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = engine.GetData((uint)userID);


            if (data == null)
            {
                return;
            }

            int _bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;

            Console.WriteLine("Writing " + _bufferSize.ToString() + " lines of data ");

            // 打开存放原始数据的文件 
            TextWriter originalFile = new StreamWriter(originalFilename, true);

            // 打开存放滤波后数据的文件
            TextWriter afterDbFile = new StreamWriter(afterDbFilename, true);

            bool flag = false;

            for (int i = 0; i < _bufferSize; i++)
            {
                // 开始写数据
                foreach (EdkDll.EE_DataChannel_t channel in data.Keys)
                {
                    originalFile.Write(data[channel][i] + ",");
                    if (channel.Equals(EdkDll.EE_DataChannel_t.FC5))
                    {
                        afterDbFile.Write(data[channel][i] + ",");
                        flag = true;
                    }
                    if (channel.Equals(EdkDll.EE_DataChannel_t.FC6))
                    {
                        afterDbFile.Write(data[channel][i] + ",");
                        flag = true;
                    }
                }
                if (flag)
                {
                    afterDbFile.WriteLine("");
                    flag = false;
                }
                originalFile.WriteLine("");

            }
            originalFile.Close();
            afterDbFile.Close();

        }



        /*
        public void getOriginalSignal()
        {
            // 处理任何等待事件
            engine.ProcessEvents();

            // 如果用户没连接好，就停止运行
            if ((int)userID == -1)
                return;

            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = engine.GetData((uint)userID);


            if (data == null)
            {
                return;
            }

            int _bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;

            Console.WriteLine("Writing " + _bufferSize.ToString() + " lines of data ");

            // 把数据写进文件 
            TextWriter file = new StreamWriter(filename, true);

            for (int i = 0; i < _bufferSize; i++)
            {
                // 开始写数据
                foreach (EdkDll.EE_DataChannel_t channel in data.Keys)
                    file.Write(data[channel][i] + ",");
                file.WriteLine("");

            }
            file.Close();
            return data;
        }*/

        public void WriteHeader()
        {
            TextWriter originalFile = new StreamWriter(originalFilename, false);
            TextWriter afterDbFile = new StreamWriter(afterDbFilename, false);

            // 原始文件列头标题
            string originalHeader = "COUNTER,INTERPOLATED,RAW_CQ,AF3,F7,F3, FC5, T7, P7, O1, O2,P8" +
                            ", T8, FC6, F4,F8, AF4,GYROX, GYROY, TIMESTAMP, ES_TIMESTAMP" +  
                            "FUNC_ID, FUNC_VALUE, MARKER, SYNC_SIGNAL,";

            // 滤波后文件列头标题
            string afterDbHeader = " FC5, FC6,";

            // 文件列头标题
            originalFile.WriteLine(originalHeader);
            afterDbFile.WriteLine(afterDbHeader);

            // 关闭文件
            originalFile.Close();
            afterDbFile.Close();
        }

    }
}
