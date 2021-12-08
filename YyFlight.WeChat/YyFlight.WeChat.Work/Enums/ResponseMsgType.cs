using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YyFlight.WeChat.Work.Enums
{
    /// <summary>
    /// 普通消息响应类型
    /// </summary>
    public enum ResponseMsgType
    {
        /// <summary>
        /// 文本消息
        /// </summary>
        Text = 0,
        /// <summary>
        /// 图文消息
        /// </summary>
        News = 1,
        /// <summary>
        /// 图片消息
        /// </summary>
        Image = 3,
        /// <summary>
        /// 语音消息
        /// </summary>
        Voice = 4,
        /// <summary>
        /// 视频消息
        /// </summary>
        Video = 5
    }
}
