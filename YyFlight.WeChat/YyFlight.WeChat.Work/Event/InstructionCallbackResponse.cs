using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YyFlight.WeChat.Utility;
using YyFlight.WeChat.Work.Enums;

namespace YyFlight.WeChat.Work.Event
{
    /// <summary>
    /// 指令回调响应应答处理
    /// </summary>
    public class InstructionCallbackResponse
    {
        /// <summary>
        /// 响应应答处理
        /// </summary>
        /// <param name="sMsg">解密参数</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="signature">签名</param>
        /// <param name="sToken">企业微信后台，开发者设置的Token</param>
        /// <param name="sEncodingAESKey">开发者设置的EncodingAESKey</param>
        /// <param name="sCorpID">业号corpid是企业号的专属编号（CorpID）</param>
        /// <returns></returns>
        public string ReceiveResponse(string sMsg, string timestamp, string signature, string sToken, string sEncodingAESKey, string sCorpID)
        {
            string responseMessage = "success";//响应内容   
            var xmlDoc = XDocument.Parse(sMsg);//xml数据转化

            //区分普通消息与第三方应用授权推送消息，MsgType有值说明是普通消息，反之则是第三方应用授权推送消息
            if (xmlDoc.Root.Element("MsgType") != null)
            {   
                var msgType = (ResponseMsgType)Enum.Parse(typeof(ResponseMsgType), xmlDoc.Root.Element("MsgType").Value, true);
                switch (msgType)
                {
                    case ResponseMsgType.Text://文本消息
                        responseMessage = ResponseMessageText(xmlDoc, timestamp, signature,sToken,sEncodingAESKey,sCorpID);
                        break;
                    case ResponseMsgType.Image:
                        responseMessage = ResponseMessageImage();
                        break;
                    case ResponseMsgType.Voice:
                        responseMessage = ResponseMessageVoice();
                        break;
                    case ResponseMsgType.Video:
                        responseMessage = ResponseMessageVideo();
                        break;
                    case ResponseMsgType.News:
                        responseMessage = ResponseMessageNews();
                        break;
                }
            }
            else if (xmlDoc.Root.Element("InfoType") != null)
            {
                //第三方回调
                var infoType = (ResponseInfoType)Enum.Parse(typeof(ResponseInfoType), xmlDoc.Root.Element("InfoType").Value, true);

                switch (infoType)
                {
                    case ResponseInfoType.suite_ticket:
                        {
                            //LoggerHelper._.Warn("suite_ticket===>>>>>,进来了，获取到的SuiteTicket票据为" + xmlDoc.Root.Element("SuiteTicket").Value);
                        }
                        break;
                }
            }
            else
            {
                //其他情况
            }

            // result==0表示解密成功，sMsg表示解密之后的明文xml串
            //服务器未正确返回响应字符串 “success”
            return responseMessage;
        }


        #region 相关事件实现

        /// <summary>
        /// 消息文本回复
        /// </summary>
        /// <returns></returns>
        public string ResponseMessageText(XDocument xmlDoc, string timestamp, string nonce,string sToken,string sEncodingAESKey,string sCorpID)
        {
            string FromUserName = xmlDoc.Root.Element("FromUserName").Value;
            string ToUserName = xmlDoc.Root.Element("ToUserName").Value;
            string Content = xmlDoc.Root.Element("Content").Value;

            string xml = "<xml>";
            xml += "<ToUserName><![CDATA[" + ToUserName + "]]></ToUserName>";
            xml += "<FromUserName><![CDATA[" + FromUserName + "]]></FromUserName>";
            xml += "<CreateTime>" + GetCurrentTimeUnix() + "</CreateTime>";
            xml += "<MsgType><![CDATA[text]]></MsgType>";
            xml += "<Content><![CDATA[" + Content + "]]></Content>";
            xml += "</xml>";
            //"" + Content + "0";//回复内容 FuncFlag设置为1的时候，自动星标刚才接收到的消息，适合活动统计使用
            WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(sToken, sEncodingAESKey, sCorpID);
            string sEncryptMsg = "";// 加密后的可以直接回复用户的密文;
            wxcpt.EncryptMsg(xml, timestamp, nonce, ref sEncryptMsg);

            //返回
            return sEncryptMsg;
        }

        /// <summary>
        /// 图片消息
        /// </summary>
        /// <returns></returns>

        public string ResponseMessageImage()
        {
            return "success";
        }

        /// <summary>
        /// 语音消息
        /// </summary>
        /// <returns></returns>
        public string ResponseMessageVoice()
        {
            return "success";
        }

        /// <summary>
        /// 视频消息
        /// </summary>
        /// <returns></returns>
        public string ResponseMessageVideo()
        {
            return "success";
        }

        /// <summary>
        /// 图文消息
        /// </summary>
        /// <returns></returns>
        public string ResponseMessageNews()
        {
            return "success";
        }

        #endregion

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentTimeUnix()
        {
            TimeSpan cha = (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
            long t = (long)cha.TotalSeconds;
            return t.ToString();
        }
    }
}
