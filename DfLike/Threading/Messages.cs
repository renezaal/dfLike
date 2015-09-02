using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfLike.Threading
{
    public static class Messages
    {
        private static ConcurrentDictionary<string, ConcurrentQueue<Message>> _messages = new ConcurrentDictionary<string, ConcurrentQueue<Message>>();
        public static ConcurrentQueue<Message> SetMessageReceiver(string receiverName)
        {
            return _messages.GetOrAdd(receiverName, new ConcurrentQueue<Message>());
        }
        public static ConcurrentQueue<Message> GetMessages(string receiverName)
        {
            ConcurrentQueue<Message> getValue = new ConcurrentQueue<Message>();
            bool success = _messages.TryGetValue(receiverName, out getValue);
            return success ? getValue : null;
        }
        public static string[] GetMessageReceivers() { return _messages.Keys.ToArray(); }
    }
}
