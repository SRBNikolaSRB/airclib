/* OutputByDataType.cs
 * airclib example script.
 * See license for copyrights.
 */

switch (irc.GetDataType(data))
{
    case DataType.NULL:
        break;
    case DataType.MSGTYPE_ACTION:
        ActionData ad = irc.GetAction(data);
        Console.WriteLine(ad.Sender + " " + ad.Action);
        break;
    case DataType.MSGTYPE_SERVER:
        ServerData sdata = irc.ReadServerData(data);
        Console.WriteLine(sdata.Message);
        break;
    case DataType.MSGTYPE_DEFAULT:
        Console.WriteLine(data);
        break;
    case DataType.MSGTYPE_CHANNEL:
    case DataType.MSGTYPE_USER:
        PrivmsgData msg = irc.ReadPrivmsg(data);
        Console.WriteLine(irc.ReadNick(msg.Sender) + " : " + irc.ReadOnlyMessage(msg.Message));
        break;
}