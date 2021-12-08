using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml.Linq;
using YyFlight.WeChat.Utility;
using YyFlight.WeChat.Work.Event;

namespace YyFlight.WeChat.Controllers;
public class EnterpriseCallbackController : Controller
{

    //企业微信后台开发者设置的token, corpID, EncodingAESKey
    private readonly string sToken = "追逐时光者";//企业微信后台，开发者设置的Token
    private readonly string sCorpID = "追逐时光者";//企业号corpid是企业号的专属编号（CorpID）[不同场景含义不同，详见文档说明（ToUserName：企业微信的CorpID，当为第三方应用回调事件时，CorpID的内容为suiteid）]
    private readonly string sEncodingAESKey = "追逐时光者";//企业微信后台，开发者设置的EncodingAESKey


    /// <summary>
    /// 处理企业号的信息
    /// get:数据回调URL验证;
    /// post:指令回调URL验证;
    /// </summary>
    public ActionResult EtWechatCommunication()
    {
        string httpMethod = Request.Method.ToUpper();

        if (httpMethod == "POST")
        {
            var bodyStream = Request.Body;
            var currentRequest = Request.HttpContext;
            //获取请求中的xml数据
            string postString = GetXMLParameters(currentRequest);

            string responseContent = "响应失败，未获取到xml中的请求参数";
            if (!string.IsNullOrEmpty(postString))
            {
                //指令响应回调
                responseContent = CommandCallback(currentRequest, postString);
            }

            return Content(responseContent);
        }
        else
        {
            return EtWachatCheckVerifyURL();
        }
    }


    /// <summary>
    /// 数据回调URL验证
    /// </summary>
    /// <returns></returns>
    public ActionResult EtWachatCheckVerifyURL()
    {
        string signature = HttpContext.Request.Query["msg_signature"];//微信加密签名，msg_signature结合了企业填写的token、请求中的timestamp、nonce参数、加密的消息体
        string timestamp = HttpContext.Request.Query["timestamp"];//时间戳
        string nonce = HttpContext.Request.Query["nonce"];//随机数
        string httpMethod = HttpContext.Request.Method.ToUpper();

        if (httpMethod == "GET")//验证回调URL(注意：企业回调的url-该url不做任何的业务逻辑，仅仅微信查看是否可以调通)
        {
            try
            {
                //在1秒内响应GET请求，响应内容为上一步得到的明文消息内容decryptEchoString（不能加引号，不能带bom头，不能带换行符）
                string echostr = HttpContext.Request.Query["echostr"];//加密的随机字符串，以msg_encrypt格式提供。需要解密并返回echostr明文，解密后有random、msg_len、msg、$CorpID四个字段，其中msg即为echostr明文

                if (!IsNullOrWhiteSpace(signature) && !IsNullOrWhiteSpace(timestamp) && !IsNullOrWhiteSpace(nonce) && !IsNullOrWhiteSpace(echostr))
                {
                    string decryptEchoString = string.Empty;
                    if (CheckVerifyURL(sToken, signature, timestamp, nonce, sCorpID, sEncodingAESKey, echostr, ref decryptEchoString))
                    {
                        if (!string.IsNullOrEmpty(decryptEchoString))
                        {
                            //必须要返回解密之后的明文
                            return Content(decryptEchoString);
                        }
                    }
                }
                else
                {
                    return Content("fail");
                }
            }
            catch (Exception ex)
            {
                return Content($"fail_ErrorMessage{ex.Message}");
            }
        }

        return Content("fail");
    }

    /// <summary>
    /// 验证URL有效性
    /// </summary>
    /// <param name="token">企业微信后台，开发者设置的Token</param>
    /// <param name="signature">签名串，对应URL参数的msg_signature</param>
    /// <param name="timestamp">时间戳</param>
    /// <param name="nonce">随机数</param>
    /// <param name="corpId">ToUserName为企业号的CorpID</param>
    /// <param name="encodingAESKey">企业微信后台，开发者设置的EncodingAESKey</param>
    /// <param name="echostr">随机串，对应URL参数的echostr</param>
    /// <param name="retEchostr">解密之后的echostr，当return返回0时有效</param>
    /// <returns></returns>
    private bool CheckVerifyURL(string token, string signature, string timestamp, string nonce, string corpId, string encodingAESKey, string echostr, ref string retEchostr)
    {
        WXBizMsgCrypt wxcpt = new WXBizMsgCrypt(token, encodingAESKey, corpId);

        int result = wxcpt.VerifyURL(signature, timestamp, nonce, echostr, ref retEchostr);

        if (result != 0)
        {
            return false;//FAIL
        }

        //result==0表示验证成功、retEchostr参数表示明文
        //用户需要将retEchostr作为get请求的返回参数、返回给企业微信号
        return true;
    }

    /// <summary>
    /// 指令响应回调
    /// </summary>
    /// <param name="Request"></param>
    /// <param name="postString">post请求的xml参数</param>
    /// <returns></returns>
    private string CommandCallback(HttpContext httpContext, string postString)
    {
        string signature = httpContext.Request.Query["msg_signature"];//微信加密签名，msg_signature结合了企业填写的token、请求中的timestamp、nonce参数、加密的消息体
        string timestamp = httpContext.Request.Query["timestamp"];//时间戳
        string nonce = httpContext.Request.Query["nonce"];//随机数                             
        var xmlDoc = XDocument.Parse(postString);//xml数据转化

        try
        {
            //https://work.weixin.qq.com/api/doc/90001/90143/90613
            //在发生授权、通讯录变更、ticket变化等事件时，企业微信服务器会向应用的“指令回调URL”推送相应的事件消息。
            //消息结构体将使用创建应用时的EncodingAESKey进行加密（特别注意, 在第三方回调事件中使用加解密算法，receiveid的内容为suiteid），请参考接收消息解析数据包。 本章节的回调事件，服务商在收到推送后都必须直接返回字符串 “success”，若返回值不是 “success”，企业微信会把返回内容当作错误信息。
            if (xmlDoc.Root.Element("Encrypt") != null)
            {
                //将post请求的数据进行xml解析，并将<Encrypt> 标签的内容进行解密，解密出来的明文即是用户回复消息的明文
                //接收并读取POST过来的XML文件流
                string decryptionParame = string.Empty;  // 解析之后的明文

                // 注意注意:sCorpID
                // @param sReceiveId: 不同场景含义不同，详见文档说明（[消息加密时为 CorpId]ToUserName：企业微信的CorpID，当为第三方应用回调事件时，CorpID的内容为suiteid）

                WXBizMsgCrypt crypt = new WXBizMsgCrypt(sToken, sEncodingAESKey, xmlDoc.Root.Element("ToUserName").Value);
                var result = crypt.DecryptMsg(signature, timestamp, nonce, postString, ref decryptionParame);

                if (result != 0)
                {
                    return "fial";
                }

                //响应应答处理
                return new InstructionCallbackResponse().ReceiveResponse(decryptionParame, timestamp, signature,sToken, sEncodingAESKey, sCorpID);
            }
        }
        catch (Exception ex)
        {
            //LoggerHelper._.Debug("异常：" + ex.Message);
        }

        return "fail";
    }


    /// <summary>
    /// 验证是否为空
    /// </summary>
    /// <param name="strParame">验证参数</param>
    /// <returns></returns>
    private bool IsNullOrWhiteSpace(string strParame)
    {
        if (string.IsNullOrWhiteSpace(strParame))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 获取post请求中的xml参数
    /// </summary>
    /// <returns></returns>
    private string GetXMLParameters(HttpContext httpContext)
    {
        string replyMsg;
        using (Stream stream = httpContext.Request.Body) //Request.InputStream .net fx版本写法
        {
            Byte[] postBytes = new Byte[stream.Length];
            stream.Read(postBytes, 0, (Int32)stream.Length);
            replyMsg = Encoding.UTF8.GetString(postBytes);
        }
        return replyMsg;
    }

}
