using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YyFlight.WeChat.Work.Enums
{
    /// <summary>
    /// 第三方应用授权推送消息类型
    /// </summary>
    public enum ResponseInfoType
    {
        /// <summary>
        /// 推送suite_ticket 企业微信服务器会定时（每十分钟）推送ticket。ticket会实时变更，并用于后续接口的调用。
        /// </summary>
        suite_ticket = 1,

        /// <summary>
        /// 授权成功通知
        /// </summary>
        create_auth = 2,

        /// <summary>
        /// 成员通知事件
        /// </summary>
        change_contact = 3
    }
}
