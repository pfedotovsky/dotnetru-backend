using System.IO;
using System.Xml.Serialization;

namespace PushNotifications
{
    public static class StringExtensions
    {
        public static T Deserialize<T>(this string input)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));

            using (StringReader sr = new StringReader(input))
            {
                return (T)ser.Deserialize(sr);
            }
        }
    }
}
