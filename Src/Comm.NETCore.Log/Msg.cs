using System;
using System.Collections.Generic;
using System.Text;

namespace Comm.NETCore.Log
{
    /// <summary>
    /// 表示一个日志记录的对象
    /// </summary>
    internal class Msg
    {
        //日志记录的时间
        //日志记录的内容
        //日志记录的类型

        /// <summary>
        /// 创建新的日志记录实例;日志记录的内容为空,消息类型为MsgType.Unknown,日志时间为当前时间
        /// </summary>
        private Msg()
            : this("", MsgType.Unknown)
        {
        }

        /// <summary>
        /// 创建新的日志记录实例;日志事件为当前时间
        /// </summary>
        /// <param name="t">日志记录的文本内容</param>
        /// <param name="p">日志记录的消息类型</param>
        internal Msg(string t, MsgType p)
            : this(DateTime.Now, t, p)
        {
        }

        /// <summary>
        /// 创建新的日志记录实例;
        /// </summary>
        /// <param name="dt">日志记录的时间</param>
        /// <param name="t">日志记录的文本内容</param>
        /// <param name="p">日志记录的消息类型</param>
        private Msg(DateTime dt, string t, MsgType p)
        {
            Datetime = dt;
            Type = p;
            Text = t;
        }

        /// <summary>
        /// 获取或设置日志记录的时间
        /// </summary>
        internal DateTime Datetime { get; set; }

        /// <summary>
        /// 获取或设置日志记录的文本内容
        /// </summary>
        internal string Text { get; set; }

        /// <summary>
        /// 获取或设置日志记录的消息类型
        /// </summary>
        internal MsgType Type { get; set; }

        /// <summary>
        /// 转换时间
        /// </summary>
        /// <returns></returns>
        private new string ToString()
        {
            return Datetime.ToString() + "\t" + Text + "\n";
        }
    }

    /// <summary>
    /// 日志消息类型的枚举
    /// </summary>
    public enum MsgType
    {
        /// <summary>
        /// 指示调试信息类型的日志记录
        /// </summary>
        Debug,

        /// <summary>
        /// 指示未知信息类型的日志记录
        /// </summary>
        Unknown,

        /// <summary>
        /// 指示普通信息类型的日志记录
        /// </summary>
        Information,

        /// <summary>
        /// 指示警告信息类型的日志记录
        /// </summary>
        Warning,

        /// <summary>
        /// 指示错误信息类型的日志记录
        /// </summary>
        Error,

        /// <summary>
        /// Try捕捉的错误
        /// </summary>
        Exception,

        /// <summary>
        /// 指示成功信息类型的日志记录
        /// </summary>
        Success
    }
}
