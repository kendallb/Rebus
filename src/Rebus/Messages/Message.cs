using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rebus.Messages
{
    /// <summary>
    /// Message wrapper object that may contain a collection of headers and multiple logical messages.
    /// </summary>
    [Serializable]
    public class Message
    {
        public Message()
        {
            Headers = new Dictionary<string, object>();
        }

        /// <summary>
        /// Headers of this message. May include metadata like e.g. the address of the sender.
        /// </summary>
        public IDictionary<string, object> Headers { get; set; }

        /// <summary>
        /// Collection of logical messages that are contained within this transport message.
        /// </summary>
        public object[] Messages { get; set; }

        /// <summary>
        /// Gets the string header with the specified key or null if the given key is not present
        /// or is not a string, Lookup names of pre-defined keys via <see cref="Headers"/>.
        /// </summary>
        public string GetHeader(string key)
        {
            if (!Headers.ContainsKey(key))
                return null;

            if (!(Headers[key] is string))
                return null;
            
            return (string)Headers[key];
        }

        /// <summary>
        /// Gets some kind of headline that somehow describes this message. May be used by the queue
        /// infrastructure to somehow label a message.
        /// </summary>
        public string GetLabel()
        {
            if (Messages == null || Messages.Length == 0)
                return "Empty Message";

            return string.Join(" + ", Messages.Select(GetStringRepresentation));
        }

        static string GetStringRepresentation(object m)
        {
            if (!(m is string)) return m.GetType().FullName;
            
            var s = (string) m;

            if (string.IsNullOrWhiteSpace(s)) return "(empty string)";

            using(var reader = new StringReader(s))
            {
                string line;
                while (string.IsNullOrWhiteSpace((line = reader.ReadLine()))) ;
                line = Sanitize(line);
                if (line.Length > 20) return line.Substring(0, 20) + "(...)";
                var nextLine = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(nextLine)) return line;
                return line + "(...)";
            }
        }

        static string Sanitize(string line)
        {
            return new string(line.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray()).Trim();
        }
    }
}