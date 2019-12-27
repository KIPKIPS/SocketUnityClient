using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour {
    public string ipAddress = "10.18.59.166";//IP地址
    public int port = 7788;//端口号
    public InputField InputField;//输入框组件
    private Socket clientSocket;
    private Text chatDetail;//显示已发送消息列表的文本框组件
    public float timeStart;//弹出框的开始时间
    public float timeEnd;//弹出框的结束时间
    public Rect windowRect = new Rect(95, 125, 120, 75);//弹出框显示的位置
    public bool isShow = false;//弹出框的显示
    public bool isShowConnectedDetail = false;//弹出框的显示
    public string text;//输入文本
    public Thread t;//接收消息的线程
    public byte[] data=new byte[1024];//接收消息的数据容器
    public string message = "";
    void Awake() {
        InputField = GameObject.Find("Input").GetComponent<InputField>();//获取输入框组件
        chatDetail = GameObject.Find("chatDetail").GetComponent<Text>();
    }
    void Start() {
        ConnectToServer();//建立连接
    }
    // Update is called once per frame
    void Update() {
        timeEnd = Time.time;//更新弹出框显示的时间
        //如果弹窗在显示并且超过两秒
        if (timeEnd - timeStart >= 2&&isShow) {
            isShow = false;//关闭弹出框
            Debug.Log("Shutdown this window");
        }

        if (clientSocket.Poll(10,SelectMode.SelectRead)) {
            isShowConnectedDetail = true;//显示服务器未响应消息框
            //InputField.readOnly = true;//禁用输入(无法输入)
            InputField.DeactivateInputField();//禁用输入(光标无法选中)
        }

        if (message!="") {
            chatDetail.text += message + "\n";
            message = "";
        }

    }
    void ConnectToServer() {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //与服务器建立连接
        clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));

        //创建一个线程用来接收消息
        t=new Thread(ReceiveMessage);
        t.Start();//开启线程
    }
    public void ReceiveMessage() {
        while (true) {
            if (clientSocket.Poll(10, SelectMode.SelectRead)) {
                break;
            }
            int length = clientSocket.Receive(data);
            message = Encoding.UTF8.GetString(data, 0, length);
        }
    }
    //绘制窗口
    void OnGUI() {
        if (isShow==true) {
            windowRect = GUI.Window(0, windowRect, DoMyWindow,"提示信息\n\n输入文本为空");
        }
        if (isShowConnectedDetail == true) {
            windowRect = GUI.Window(0, windowRect, DoMyWindow, "提示信息\n\n服务器未响应");
        }
    }
    //窗口事件
    void DoMyWindow(int windowID) {
    }
    public void OnSendButton() {
        if (InputField.text!="") {
            Send(InputField.text);//发送文本
            chatDetail.text += InputField.text + "\n";//原有文本基础上换行
            InputField.text = ""; //清空输入框文字
        }
        else {
            isShow = true;
            timeStart = Time.time;
        }
    }
    void Send(string message) {
        byte[] data = Encoding.UTF8.GetBytes(message);//string转byte
        text = message;
        clientSocket.Send(data);//Socket发送byte数据包
    }
    public void Destroy() {
        clientSocket.Shutdown(SocketShutdown.Both);//接收和发送全部关闭
        clientSocket.Close();
    }
    //接收消息的方法,用来循环接收消息
}
