﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory = null!;
        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _sessionFactory = sessionFactory;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnConnectCompleted;
                args.RemoteEndPoint = endPoint;
                args.UserToken = socket;

                RegisterConnectAsync(args);
            }
        }

        void RegisterConnectAsync(SocketAsyncEventArgs args)
        {
            if (args.UserToken is not Socket socket)
            {
                Debug.WriteLine("UserToken is not a Socket");
                return;
            }

            bool pending = socket.ConnectAsync(args);
            if (!pending)
                OnConnectCompleted(null, args);
        }

        void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                Debug.WriteLine($"OnConnectCompleted Failed. {args.SocketError} {args.RemoteEndPoint}");
                return;
            }

            if (args.ConnectSocket == null || args.RemoteEndPoint == null)
            {
                Debug.WriteLine("ConnectSocket is null or not connected");
                return;
            }

            Session session = _sessionFactory.Invoke();
            session.Initialize(args.ConnectSocket);
            session.OnConnected(args.RemoteEndPoint);
        }
    }
}
