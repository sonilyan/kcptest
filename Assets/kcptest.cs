using System;
using UnityEngine;
using System.Buffers;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Text;
using System.Threading.Tasks;

public class kcptest : MonoBehaviour,IKcpCallback
{
    private Kcp _kcp;
    private UdpClient _udpClient;
    private IPEndPoint iep;
    
    IPEndPoint remote = null;

    private void Awake()
    {
        _kcp = new Kcp(666,this);
        
        _kcp.NoDelay(1, 10, 2, 1);//fast
        _kcp.WndSize(64, 64);
        _kcp.SetMtu(512);
        //https://github.com/xtaci/kcptun/issues/185
        
        _udpClient = new UdpClient(62001);
        
        UdpClient xxx = new UdpClient();
        
        iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 62001);

        StartCoroutine(test_send());
        StartCoroutine(test_recv());

        StartRecv();
    }
    
    async void StartRecv()
    {
        while (true)
        {
            var tmp = await _udpClient.ReceiveAsync();
            _kcp.Input(tmp.Buffer);
            //Debug.Log($"{tmp.RemoteEndPoint.Address}|{tmp.RemoteEndPoint.Port}");
        }
    }

    public void Output(IMemoryOwner<byte> buffer, int avalidLength)
    {
        _udpClient.Send(buffer.Memory.ToArray(), avalidLength, iep);
    }

    private void FixedUpdate()
    {
        _kcp.Update(DateTime.UtcNow);
    }

    IEnumerator test_send()
    {
        int i = 0;
        while (true)
        {
            yield return new WaitForSeconds(1);

            var tmp = Encoding.ASCII.GetBytes($"this is a test 666666666666666666666666666666666666666666 {i++}");

            _kcp.Send(tmp);
        }
    }

    IEnumerator test_recv()
    {
        while (true)
        {
            byte[] xxx = new byte[256];
            int size = _kcp.Recv(xxx);
            if (size > 0)
            {
                Debug.Log(Encoding.UTF8.GetString(xxx));
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private void OnDestroy()
    {
        _kcp.Dispose();
    }
}
