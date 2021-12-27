using System.IO;
using System.Runtime.Serialization.Formatters.Binary;








public class Json  {

    public static byte[] EnCodeBytes(object msg)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter fmt = new BinaryFormatter();
            fmt.Serialize(ms, msg);
            return ms.GetBuffer();
        }
    }

    public static T DeCode<T>(byte[] bytes)
    {
        using (MemoryStream ms = new MemoryStream(bytes))
        {
            BinaryFormatter fmt = new BinaryFormatter();
            return (T)fmt.Deserialize(ms);
        }
    }
}