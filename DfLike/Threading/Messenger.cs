using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.Threading
{
    public static class Messenger
    {
        private static Dictionary<string, List<ConcurrentQueue<Message>>> _channeledOutgoingMessages = new Dictionary<string, List<ConcurrentQueue<Message>>>();
        private static Dictionary<object, List<ConcurrentQueue<Message>>> _instanceBoundOutgoingMessages = new Dictionary<object, List<ConcurrentQueue<Message>>>();
        private static Dictionary<Type, List<ConcurrentQueue<Message>>> _typeBoundOutgoingMessages = new Dictionary<Type, List<ConcurrentQueue<Message>>>();

        private static ConcurrentQueue<IncomingMessage> _incomingMessages = new ConcurrentQueue<IncomingMessage>();

        private static ConcurrentQueue<ChannelListener> _newChannelListeners = new ConcurrentQueue<ChannelListener>();
        private static ConcurrentQueue<InstanceListener> _newInstanceListeners = new ConcurrentQueue<InstanceListener>();
        private static ConcurrentQueue<TypeListener> _newTypeListeners = new ConcurrentQueue<TypeListener>();

        private static ConcurrentQueue<ChannelListener> _channelListenersScheduledForRemoval = new ConcurrentQueue<ChannelListener>();
        private static ConcurrentQueue<InstanceListener> _instanceListenersScheduledForRemoval = new ConcurrentQueue<InstanceListener>();
        private static ConcurrentQueue<TypeListener> _typeListenersScheduledForRemoval = new ConcurrentQueue<TypeListener>();
        
        private class IncomingMessage
        {
            internal IncomingMessage(object sender, string channel, Message message) { Channel = channel; Message = message; Sender = sender; }
            internal readonly object Sender;
            internal readonly string Channel;
            internal readonly Message Message;
        }
        private class ChannelListener
        {
            internal ChannelListener(string channel, ConcurrentQueue<Message> queue) { Channel = channel; Queue = queue; }
            internal readonly string Channel;
            internal readonly ConcurrentQueue<Message> Queue;
        }
        private class InstanceListener
        {
            internal InstanceListener(object instance, ConcurrentQueue<Message> queue) { Instance = instance; Queue = queue; }
            internal readonly object Instance;
            internal readonly ConcurrentQueue<Message> Queue;
        }
        private class TypeListener
        {
            internal TypeListener(Type type, ConcurrentQueue<Message> queue) { Type = type; Queue = queue; }
            internal readonly Type Type;
            internal readonly ConcurrentQueue<Message> Queue;
        }

        public static ConcurrentQueue<Message> AddListenerToChannel(string channelName)
        {
            ConcurrentQueue<Message> messageQueue = new ConcurrentQueue<Message>();
            _newChannelListeners.Enqueue(new ChannelListener(channelName, messageQueue));
            return messageQueue;
        }
        public static ConcurrentQueue<Message> AddListenerToInstance(object instance)
        {
            ConcurrentQueue<Message> messageQueue = new ConcurrentQueue<Message>();
            _newInstanceListeners.Enqueue(new InstanceListener(instance, messageQueue));
            return messageQueue;
        }
        public static ConcurrentQueue<Message> AddListenerToType(Type type)
        {
            ConcurrentQueue<Message> messageQueue = new ConcurrentQueue<Message>();
            _newTypeListeners.Enqueue(new TypeListener(type, messageQueue));
            return messageQueue;
        }

        public static void RemoveListenerFromChannel(string channelName, ConcurrentQueue<Message> listener)
        {
            _channelListenersScheduledForRemoval.Enqueue(new ChannelListener(channelName, listener));
        }
        public static void RemoveListenerFromInstance(object instance, ConcurrentQueue<Message> listener)
        {
            _instanceListenersScheduledForRemoval.Enqueue(new InstanceListener(instance, listener));
        }
        public static void RemoveListenerFromType(Type type, ConcurrentQueue<Message> listener)
        {
            _typeListenersScheduledForRemoval.Enqueue(new TypeListener(type, listener));
        }

        public static void SendMessage(object sender, string channel, Message message)
        {
            _incomingMessages.Enqueue(new IncomingMessage(sender, channel, message));
        }

        private static readonly object _processMessagesLock = new object();
        internal static void ProcessMessages()
        {
            lock (_processMessagesLock)
            {
                while (!_newChannelListeners.IsEmpty)
                {
                    ChannelListener listener = null;
                    for (int i = 0; i < 10; i++)
                    {
                        if (_newChannelListeners.TryDequeue(out listener))
                        {
                            break;
                        }
                    }
                    if (listener == null)
                    {
                        break;
                    }
                    string channel = listener.Channel;
                    if (!_channeledOutgoingMessages.ContainsKey(channel))
                    {
                        _channeledOutgoingMessages[channel] = new List<ConcurrentQueue<Message>>(1);
                    }
                    _channeledOutgoingMessages[channel].Add(listener.Queue);
                }

                while (!_newInstanceListeners.IsEmpty)
                {
                    InstanceListener listener = null;
                    for (int i = 0; i < 10; i++)
                    {
                        if (_newInstanceListeners.TryDequeue(out listener))
                        {
                            break;
                        }
                    }
                    if (listener == null)
                    {
                        break;
                    }
                    object instance = listener.Instance;
                    if (!_instanceBoundOutgoingMessages.ContainsKey(instance))
                    {
                        _instanceBoundOutgoingMessages[instance] = new List<ConcurrentQueue<Message>>(1);
                    }
                    _instanceBoundOutgoingMessages[instance].Add(listener.Queue);
                }

                while (!_newTypeListeners.IsEmpty)
                {
                    TypeListener listener = null;
                    for (int i = 0; i < 10; i++)
                    {
                        if (_newTypeListeners.TryDequeue(out listener))
                        {
                            break;
                        }
                    }
                    if (listener == null)
                    {
                        break;
                    }
                    Type type = listener.Type;
                    if (!_typeBoundOutgoingMessages.ContainsKey(type))
                    {
                        _typeBoundOutgoingMessages[type] = new List<ConcurrentQueue<Message>>(1);
                    }
                    _typeBoundOutgoingMessages[type].Add(listener.Queue);
                }

                while (!_channelListenersScheduledForRemoval.IsEmpty)
                {
                    ChannelListener listener = null;
                    for (int i = 0; i < 10; i++)
                    {
                        if (_channelListenersScheduledForRemoval.TryDequeue(out listener))
                        {
                            break;
                        }
                    }
                    if (listener == null)
                    {
                        break;
                    }
                    string channel = listener.Channel;
                    if (_channeledOutgoingMessages.ContainsKey(channel))
                    {
                        List<ConcurrentQueue<Message>> messageQueueList = _channeledOutgoingMessages[channel];
                        messageQueueList.Remove(listener.Queue);
                        if (messageQueueList.Count == 0)
                        {
                            _channeledOutgoingMessages.Remove(channel);
                        }
                    }
                }

                while (!_instanceListenersScheduledForRemoval.IsEmpty)
                {
                    InstanceListener listener = null;
                    for (int i = 0; i < 10; i++)
                    {
                        if (_instanceListenersScheduledForRemoval.TryDequeue(out listener))
                        {
                            break;
                        }
                    }
                    if (listener == null)
                    {
                        break;
                    }
                    object instance = listener.Instance;
                    if (_instanceBoundOutgoingMessages.ContainsKey(instance))
                    {
                        List<ConcurrentQueue<Message>> messageQueueList = _instanceBoundOutgoingMessages[instance];
                        messageQueueList.Remove(listener.Queue);
                        if (messageQueueList.Count == 0)
                        {
                            _instanceBoundOutgoingMessages.Remove(instance);
                        }
                    }
                }

                while (!_typeListenersScheduledForRemoval.IsEmpty)
                {
                    TypeListener listener = null;
                    for (int i = 0; i < 10; i++)
                    {
                        if (_typeListenersScheduledForRemoval.TryDequeue(out listener))
                        {
                            break;
                        }
                    }
                    if (listener == null)
                    {
                        break;
                    }
                    Type type = listener.Type;
                    if (_typeBoundOutgoingMessages.ContainsKey(type))
                    {
                        List<ConcurrentQueue<Message>> messageQueueList = _typeBoundOutgoingMessages[type];
                        messageQueueList.Remove(listener.Queue);
                        if (messageQueueList.Count == 0)
                        {
                            _typeBoundOutgoingMessages.Remove(type);
                        }
                    }
                }

                while (!_incomingMessages.IsEmpty)
                {
                    IncomingMessage message = null;
                    for (int i = 0; i < 5; i++)
                    {
                        if (_incomingMessages.TryDequeue(out message))
                        {
                            break;
                        }
                    }
                    if (message == null)
                    {
                        break;
                    }
                    if (message.Message == null)
                    {
                        continue;
                    }

                    string channel = message.Channel;
                    if (_channeledOutgoingMessages.ContainsKey(channel))
                    {
                        List<ConcurrentQueue<Message>> messageQueueList = _channeledOutgoingMessages[channel];
                        int length = messageQueueList.Count;
                        for (int i = 0; i < length; i++)
                        {
                            messageQueueList[i].Enqueue(message.Message);
                        }
                    }

                    object instance = message.Sender;
                    if (_instanceBoundOutgoingMessages.ContainsKey(instance))
                    {
                        List<ConcurrentQueue<Message>> messageQueueList = _instanceBoundOutgoingMessages[instance];
                        int length = messageQueueList.Count;
                        for (int i = 0; i < length; i++)
                        {
                            messageQueueList[i].Enqueue(message.Message);
                        }
                    }

                    Type type = message.Sender.GetType();
                    if (_typeBoundOutgoingMessages.ContainsKey(type))
                    {
                        List<ConcurrentQueue<Message>> messageQueueList = _typeBoundOutgoingMessages[type];
                        int length = messageQueueList.Count;
                        for (int i = 0; i < length; i++)
                        {
                            messageQueueList[i].Enqueue(message.Message);
                        }
                    }
                }
            }
        }
    }
}
