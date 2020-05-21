using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Model;
using NSA;

public class Equity
{
    
    

    private int _messageId;
    private System.Net.Sockets.TcpClient _tcpClient;
    private System.Net.Sockets.TcpClient[] _clientSocket;
    private System.Net.Sockets.NetworkStream _stream;

    private decimal _tokenRegistrationId;
    private DateTime _tokenRegStartTime;

    public List<Candle> allDaa
    {
        get;
        set;
    }

    public double Low
    {
        get;
        set;
    }

    public double Volume
    {
        get;
        set;
    }

    public double High
    {
        get;
        set;
    }

    public double Open
    {
        get;
        set;
    }

    public double Close
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    }

    public StockData todaysLevel
    {
        get;
        set;
    }
    public double Gap
    {
        get;
        set;
    }

    public int backTestDay
    {
        get;
        set;
    }

    public int Category
    {
        get;
        set;
    }

    public int TestAtMinute
    {
        get;
        set;
    }

    public DateTime TransactionTradingDate
    {
        get;
        set;
    }

    public double MaxRisk
    {
        get;
        set;
    }

    public decimal TokenRegistrationId
    {
        get
        {
            return _tokenRegistrationId;
        }
        set
        {
            _tokenRegistrationId = value;
        }
    }


    public DateTime TokenRegStartTime
    {
        get
        {
            return _tokenRegStartTime;
        }
        set
        {
            _tokenRegStartTime = value;
        }
    }


    public System.Net.Sockets.TcpClient TCPClient
    {
        get
        {
            return _tcpClient;
        }
        set
        {
            _tcpClient = value;
        }
    }


    public int MessageId
    {
        get
        {
            return _messageId;
        }
        set
        {
            _messageId = value;
        }
    }


    public System.Net.Sockets.TcpClient[] ClientSocket
    {
        get
        {
            return _clientSocket;
        }
        set
        {
            _clientSocket = value;
        }
    }


    public System.Net.Sockets.NetworkStream Stream
    {
        get
        {
            return _stream;
        }
        set
        {
            _stream = value;
        }
    }








}
