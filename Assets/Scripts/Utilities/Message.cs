using System;
using System.Collections.Generic;

[Serializable]
public class Message
{
    public string senderName;
    public string text;
    public List<string> recipientIds;   //this will include the person that sends the message

    public Message(string senderName, string text, List<string> recipientIds)
    {
        this.senderName = senderName;
        this.text = text;
        this.recipientIds = new List<string>(recipientIds);
    }

}
