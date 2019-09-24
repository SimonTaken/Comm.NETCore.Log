using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Comm.NETCore.Log
{
    /// <summary>
    /// 企业应用框架的日志类
    /// </summary>
    /// <remarks>此日志类提供高性能的日志记录实现。
    /// 当调用Write方法时不会造成线程阻塞,而是立即完成方法调用,因此调用线程不用等待日志写入文件之后才返回。</remarks>
    public class Log : IDisposable
    {
        //public  IConfiguration Configuration { get; }        
        //日志对象的缓存队列
        private static Queue<Msg> _msgs;
        //日志文件保存的路径
        //
        private static string _path = AppContext.BaseDirectory + "\\Logs";
        //日志文件保存的等级
        private static string LogGrade = "Debug,Unknown,Information,Warning,Error,Exception,Success";// Configuration["CommLogs:LogGrade"];
        //日志保留天数-默认31天
        private static string LeaveCount = "100";// Configuration["CommLogs:LeaveCount"];// ConfigurationManager.AppSettings["LeaveCount"] ?? "31";
        //日志写入线程的控制标记
        private static bool _state;
        //日志记录的类型
        //private static LogType _type;
        private static string LogTypeStr = "Daily";//Configuration["CommLogs:LogFormat"];//  ConfigurationManager.AppSettings["LogFormat"] ?? "Daily";
        //日志文件生命周期的时间标记
        private static DateTime _timeSign;
        //日志文件写入流对象
        private static StreamWriter _writer;

        /// <summary>
        /// 创建日志对象的新实例，采用默认当前程序位置作为日志路径和默认的每日日志文件类型记录日志
        /// </summary>
        private Log()
            //string aaaa = "F:\\notebook\\haha\\";//路径的正确写法 
            : this(_path)
        {
            //Configuration = new ConfigurationBuilder()
            //.Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })
            //.Build();
            //LogGrade = Configuration.GetSection("CommLogs")["LogGrade"];
            //LeaveCount = Configuration["CommLogs:LeaveCount"];
            //LogTypeStr = Configuration["CommLogs:LogFormat"];
        }
        //private Log(IConfiguration configuration)
        ////string aaaa = "F:\\notebook\\haha\\";//路径的正确写法 
        ////: this(_path)
        //{
        //    Configuration = configuration;          
        //}

        #region 创建日志对象的新实例，根据指定的日志文件路径和指定的日志文件创建类型
        /// <summary>
        /// 创建日志对象的新实例，根据指定的日志文件路径和指定的日志文件创建类型
        /// </summary>
        /// <param name="p">日志文件保存路径</param>
        private Log(string p)
        {
            if (_msgs == null)
            {
                _state = true;
                _path = p;
                //_type = t;
                _msgs = new Queue<Msg>();
                var thread = new Thread(Work);
                thread.Start();
            }
        }
        #endregion

        #region 日志文件写入线程执行的方法
        //日志文件写入线程执行的方法
        private void Work()
        {
            while (true)
            {
                //判断队列中是否存在待写入的日志
                if (_msgs.Count > 0)
                {
                    Msg msg = null;
                    lock (_msgs)
                    {
                        msg = _msgs.Dequeue();
                    }
                    if (msg != null)
                    {
                        FileWrite(msg);
                    }
                }
                else
                {
                    //判断是否已经发出终止日志并关闭的消息
                    if (_state)
                    {
                        Thread.Sleep(1);
                    }
                    else
                    {
                        FileClose();
                    }
                }
            }
        }
        #endregion

        #region 通过判断文件的到期时间标记将决定是否创建新文件
        //根据日志类型获取日志文件名，并同时创建文件到期的时间标记
        //通过判断文件的到期时间标记将决定是否创建新文件。
        private string GetFilename()
        {
            DateTime now = DateTime.Now;
            string format = "";
            switch (LogTypeStr)
            {
                case "Daily":
                    _timeSign = new DateTime(now.Year, now.Month, now.Day);
                    _timeSign = _timeSign.AddDays(1);
                    format = "yyyyMMdd'.log'";
                    break;
                case "Weekly":
                    _timeSign = new DateTime(now.Year, now.Month, now.Day);
                    _timeSign = _timeSign.AddDays(7);
                    format = "yyyyMMdd'.log'";
                    break;
                case "Monthly":
                    _timeSign = new DateTime(now.Year, now.Month, 1);
                    _timeSign = _timeSign.AddMonths(1);
                    format = "yyyyMM'.log'";
                    break;
                case "Annually":
                    _timeSign = new DateTime(now.Year, 1, 1);
                    _timeSign = _timeSign.AddYears(1);
                    format = "yyyy'.log'";
                    break;
                default:
                    _timeSign = new DateTime(now.Year, now.Month, now.Day);
                    _timeSign = _timeSign.AddDays(1);
                    format = "yyyyMMdd'.log'";
                    break;

            }
            return format;
        }
        #endregion

        #region 写入日志文本到文件的方法
        //写入日志文本到文件的方法
        private void FileWrite(Msg msg)
        {
            try
            {
                #region 创建日志文件夹
                if (!Directory.Exists(_path))//如果不存在就创建file文件夹 
                {
                    Directory.CreateDirectory(_path);//创建该文件夹 
                }
                #endregion

                if (_writer == null)
                {
                    FileOpen();
                }
                else
                {
                    //判断文件到期标志，如果当前文件到期则关闭当前文件创建新的日志文件
                    if (DateTime.Now >= _timeSign)
                    {
                        FileClose();
                        FileOpen();
                    }
                }
                if (_writer != null)
                {
                    _writer.Write(msg.Datetime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "==>");
                    _writer.Write("[" + msg.Type + "]:");
                    _writer.WriteLine(msg.Text);
                    _writer.Flush();
                }
            }
            catch (Exception e)
            {
                Console.Out.Write(e);
            }
        }
        #endregion

        #region 打开文件准备写入
        //打开文件准备写入
        private void FileOpen()
        {
            DateTime now = DateTime.Now;
            string format = GetFilename();
            _writer = new StreamWriter(_path + "\\" + now.ToString(format), true, Encoding.UTF8);
            ClearLog();
        }
        #endregion

        #region 关闭打开的日志文件
        /// <summary>
        /// 关闭打开的日志文件
        /// </summary>
        private void FileClose()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Close();
                _writer.Dispose();
                _writer = null;
            }
        }
        #endregion

        #region 写入新日志，根据指定的日志对象Msg
        /// <summary>
        /// 写入新日志，根据指定的日志对象Msg
        /// </summary>
        /// <param name="msg">日志内容对象</param>
        private static void Write(Msg msg)
        {
            if (LogGrade.Contains(msg.Type.ToString()))
            {
                lock (_msgs)
                {
                    _msgs.Enqueue(msg);
                }
            }
        }
        #endregion

        #region 写入新日志，根据指定的日志内容和信息类型，采用当前时间为日志时间写入新日志
        /// <summary>
        /// 写入新日志，根据指定的日志内容和信息类型，采用当前时间为日志时间写入新日志
        /// <example>
        /// 需要在Config文件中配置信息：
        /// <code>
        /// <!--Log日志配置信息-->
        /// <!--Log类型，web:为web程序的相对路径，例如：Logs；local：绝对路径，例如：D:\\log\\Blood；空：本地程序的相对路径，例如：Logs-->
        /// <add key="LogType" value="local"/>
        /// <!--日志保存时间，按天计算.为0则不清理-->
        /// <add key = "LeaveCount" value="31"/>
        ///<!--记录日志路径:web:logs Web下面的路径 空：当前目录  local： D:\\log\\SimonWeb-->
        /// <add key="LogPath" value="D:\\log\\Test"/>
        /// <!--记录消息类型；格式:Debug,Unknown,Information,Warning,Error,Exception,Success -->
        ///<add key="LogGrade" value="Unknown"/>
        ///<!--Daily:每天记录一个日志文件格式 yyyyMMdd.log;Weekly:每周记录一个日志文件格式 yyyyMMdd.log;Monthly:每月记录一个日志文件yyyyMM.log;Annually:每年记录一个日志文件 yyyy.log-->
        ///<add key="LogFormat" value="Daily"/>
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="text">日志内容</param>
        /// <param name="type">信息类型</param>
        public static void Write(string text, MsgType type)
        {
            var log = new Log();
            Write(new Msg(text, type));
        }
        #endregion

        #region 清理过期文件
        private void ClearLog()
        {
            int myLeaveCount = 30;
            int.TryParse(LeaveCount, out myLeaveCount);
            if (myLeaveCount == 0)
            {
                return;
            }
            DateTime now = DateTime.Now;
            try
            {
                FileInfo f;
                TimeSpan t = new TimeSpan();
                string[] files = Directory.GetFiles(_path, "*.log", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    f = new FileInfo(file);
                    t = now - f.CreationTime;
                    if (t.Days > myLeaveCount)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return;
        }
        #endregion

        #region IDisposable 成员
        /// <summary>
        /// 销毁日志对象
        /// </summary>
        public void Dispose()
        {
            _state = false;
        }
        #endregion
    }
}
