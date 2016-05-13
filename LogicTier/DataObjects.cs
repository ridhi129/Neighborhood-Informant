using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTier
{
  public class User
  {
    public int UserID { get; private set; }
    public string UserName { get; private set; }
    public string Password { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public Boolean Seller { get; private set; }
    public User(int id, string userName, string password, string first, string last, string email, Boolean seller)
    {
      UserID = id;
      UserName = userName;
      Password = password;
      FirstName = first;
      LastName = last;
      Email = email;
      Seller = seller;
    }
  }

  public class Listing
  {
    public int ListingID { get; private set; }
    public int UserID { get; private set; }
    public string StAddr { get; private set; }
    public int ZipCode { get; private set; }
    public int SqFeet { get; private set; }
    public double NoBath { get; private set; }
    public int NoBed { get; private set; }
    public string Type { get; private set; }
    public decimal Price { get; private set; }
    public string Picture { get; private set; }
    public Listing(int id, int userID, string stAddr, int zipCode, int sqFeet, double noBath, int noBed, string type, decimal price, string picture)
    {
      ListingID = id;
      UserID = userID;
      StAddr = stAddr;
      ZipCode = zipCode;
      SqFeet = sqFeet;
      NoBath = noBath;
      NoBed = noBed;
      Type = type;
      Price = price;
      Picture = picture;
    }

    public string fullAddress()
    {
      return StAddr + " Chicago, Il " + ZipCode.ToString();
    }
  }

  public class Message
  {
    public int MessageID { get; private set; }
    public int SenderID { get; private set; }
    public int ReceiverID { get; private set; }
    public int ThreadID { get; private set; }
    public string MessageBody { get; private set; }
    public Message(int messageID, int senderID, int receiverID, int threadID, string messageBody)
    {
      MessageID = messageID;
      SenderID = senderID;
      ReceiverID = receiverID;
      ThreadID = threadID;
      MessageBody = messageBody;
    }

  }
}
