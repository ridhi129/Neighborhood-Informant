using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using GoogleMaps.LocationServices;

namespace LogicTier
{
  public class DataAPI
  {
    /**
     * return all listings in database as a list of Listing objects
     **/
    public IReadOnlyList<Listing> search(int zipCode, string type, decimal min, decimal max, int minBeds, int maxBeds, int minSq, int maxSq, double minBaths, double maxBaths)
    {
      List<Listing> listings = new List<Listing>();

      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
        var query = from listing in NIData.Listings select listing;
        if(zipCode == 0) // no zip code
        {
          if(type == null || type.Equals("")) // no type
          {
            query = from listing in NIData.Listings
                    where ((listing.Price >= min) && (listing.Price <= max) && (listing.SqFeet >= minSq) && (listing.SqFeet <= maxSq)
                          && (listing.NoBed >= minBeds) && (listing.NoBed <= maxBeds) && (listing.NoBath >= minBaths) && (listing.NoBath <= maxBaths))
                    select listing;
          }
          else // has type
          {
            query = from listing in NIData.Listings
                    where ((listing.Price >= min) && (listing.Price <= max) && (listing.SqFeet >= minSq) && (listing.SqFeet <= maxSq)
                          && (listing.NoBed >= minBeds) && (listing.NoBed <= maxBeds) && (listing.NoBath >= minBaths) && (listing.NoBath <= maxBaths)
                          && (listing.Type.Equals(type)))
                    select listing;
          }
        }
        else // has zip code
        {
          if (type == null || type.Equals("")) // no type
          {
            query = from listing in NIData.Listings
                    where ((listing.Price >= min) && (listing.Price <= max) && (listing.SqFeet >= minSq) && (listing.SqFeet <= maxSq)
                          && (listing.NoBed >= minBeds) && (listing.NoBed <= maxBeds) && (listing.NoBath >= minBaths) && (listing.NoBath <= maxBaths)
                          && (listing.ZipCode == zipCode))
                    select listing;
          }
          else // has type
          {
            query = from listing in NIData.Listings
                    where ((listing.Price >= min) && (listing.Price <= max) && (listing.SqFeet >= minSq) && (listing.SqFeet <= maxSq)
                          && (listing.NoBed >= minBeds) && (listing.NoBed <= maxBeds) && (listing.NoBath >= minBaths) && (listing.NoBath <= maxBaths)
                          && (listing.ZipCode == zipCode) && (listing.Type.Equals(type)))
                    select listing;
          }
        }

        foreach (var listing in query)
        {
          Listing s = new Listing(listing.ListingID, listing.UserID, listing.StAddr, listing.ZipCode, listing.SqFeet, listing.NoBath, listing.NoBed, listing.Type, listing.Price, listing.Picture);
          listings.Add(s);
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return listings;
    }

    /**
     * return all listings
     **/
    public IReadOnlyList<Listing> allListings()
    {
      return search((int)0, "", (decimal)0.0, (decimal)1000000000.0,1,10,0,10000000,1,10);
    }

    /**
     * return User object correspondin to the uName and password parameters, null if such a user does not exist in the database
     **/
    public User login(string uName, string pass)
    {
      uName = uName.Replace("'", "''");
      pass = pass.Replace("'", "''");
      User loggedIn = null;
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

        var query = from user in NIData.Users
                    where (user.UserName.Equals(uName) && user.Password.Equals(pass))
                    select user;

        // now create business objects from the Station entities, and build a list
        // to return:
        foreach (var user in query)
        {
          var test = user.Seller;
          loggedIn = new User(user.UserID, user.UserName, user.Password, user.FirstName, user.LastName, user.Email, (user.Seller == 1));
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return loggedIn;
    }

    /**
     * return all listings posted by the User given
     **/
    public DataTable listingsFromUser(User u)
    {
      DataTable listings = new DataTable();
      listings.Columns.Add("ListingID");
      listings.Columns.Add("StAddr");
      listings.Columns.Add("ZipCode");
      listings.Columns.Add("SqFeet");
      listings.Columns.Add("NoBath");
      listings.Columns.Add("NoBed");
      listings.Columns.Add("Type");
      listings.Columns.Add("Price");
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

        var query = from listing in NIData.Listings
                    where listing.UserID.Equals(u.UserID)
                    select listing;
        
        foreach(var listing in query)
        {
          listings.Rows.Add(listing.ListingID, listing.StAddr, listing.ZipCode, listing.SqFeet, listing.NoBath, listing.NoBed, listing.Type, listing.Price);
        }
        
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return listings;
    }

    /**
     * Create a new user account in database, return true on success, false on failure
     **/
    public bool createAccount(string uName, string pass, string fName, string lName, string email, string phone, byte seller)
    {
      DataTier.User newUser = new DataTier.User();
      newUser.UserName = uName.Replace("'", "''");
      newUser.Password = pass.Replace("'", "''");
      newUser.FirstName = fName.Replace("'", "''");
      newUser.LastName = lName.Replace("'", "''");
      newUser.Email = email.Replace("'", "''");
      newUser.MobileNo = phone.Replace("'", "''");
      newUser.Seller = seller;

      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
        NIData.Users.InsertOnSubmit(newUser);
        NIData.SubmitChanges();
      }
      catch (Exception ex)
      {
        return false;
      }
      return true;
    }

    /**
     * return the Listing corresponding to the given street address, null if such a listing does not exist
     **/
    public Listing getListingFromAddress(string addr)
    {
      Listing selected = null;
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

        var query = from listing in NIData.Listings
                    where listing.StAddr.Equals(addr)
                    select listing;

        // now create business objects from the Station entities, and build a list
        // to return:
        foreach (var listing in query)
        {
          selected = new Listing(listing.ListingID, listing.UserID, listing.StAddr, listing.ZipCode, listing.SqFeet, listing.NoBath, listing.NoBed, listing.Type, listing.Price, listing.Picture);
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }
      return selected;
    }

    public Listing getListingFromID(int id)
    {
      Listing selected = null;
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

        var query = from listing in NIData.Listings
                    where listing.ListingID == id
                    select listing;

        // now create business objects from the Station entities, and build a list
        // to return:
        foreach (var listing in query)
        {
          selected = new Listing(listing.ListingID, listing.UserID, listing.StAddr, listing.ZipCode, listing.SqFeet, listing.NoBath, listing.NoBed, listing.Type, listing.Price, listing.Picture);
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }
      return selected;
    }

    /**
     * retun the user with given userID
     **/
    public User getUser(int userID)
    {
      User retval = null;
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

        var query = from user in NIData.Users
                    where (user.UserID == userID)
                    select user;

        // now create business objects from the Station entities, and build a list
        // to return:
        foreach (var user in query)
        {
          var test = user.Seller;
          retval = new User(user.UserID, user.UserName, user.Password, user.FirstName, user.LastName, user.Email, (user.Seller == 1));
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return retval;
    }

    public bool postListing(User u, string stAddr, int zipCode, int sqFeet, float noBath, int noBed, string type, decimal price, string picture)
    {
      DataTier.Listing newListing = new DataTier.Listing();
      newListing.UserID = u.UserID;
      newListing.StAddr= stAddr.Replace("'", "''");
      newListing.ZipCode = zipCode;
      newListing.SqFeet = sqFeet;
      newListing.NoBath = noBath;
      newListing.NoBed = noBed;
      newListing.Type = type;
      newListing.Price = price;
      newListing.Picture = picture;

      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
        NIData.Listings.InsertOnSubmit(newListing);
        NIData.SubmitChanges();
      }
      catch (Exception ex)
      {
        return false;
      }
      return true;
    }

    public bool sendMessage(string message, User sender, int reciever, int threadid)
    {
      DataTier.Message newMessage = new DataTier.Message();
      newMessage.SenderID = sender.UserID;
      newMessage.ReceiverID = reciever;
      newMessage.MessageBody = message;
      newMessage.ThreadID = threadid;

      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
        NIData.Messages.InsertOnSubmit(newMessage);
        NIData.SubmitChanges();
      }
      catch (Exception ex)
      {
        return false;
      }
      return true;
    }

    public IReadOnlyList<Message> getMessages(User u)
    {
      List<Message> messages = new List<Message>();

      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
        var query = from message in NIData.Messages
                    where message.ReceiverID == u.UserID
                    select message;

  

        foreach (var message in query)
        {
          Message s = new Message(message.MessageID, message.SenderID, message.ReceiverID, message.ThreadID, message.MessageBody);
          messages.Add(s);
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return messages;
    }

    public Message getMessage(int messageID)
    {
      Message retval = null;
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

        var query = from message in NIData.Messages
                    where (message.MessageID == messageID)
                    select message;

        // now create business objects from the Station entities, and build a list
        // to return:
        foreach (var message in query)
        {
          retval = new Message(message.MessageID, message.SenderID, message.ReceiverID, message.ThreadID, message.MessageBody);
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return retval;
    }

    public IReadOnlyList<String> getSchools(int zip)
    {
      List<String> schools = new List<String>();
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
        var query = (from school in NIData.NEIGHBORHOOD_SCHOOLs
                    where school.SchoolZipCode == zip
                    select school).Take(5);
        foreach (var school in query)
        {
          schools.Add(school.SchoolName);
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }
      return schools;
    }

    public IReadOnlyList<String> getHospitals(int zip)
    {
      List<String> hospitals = new List<String>();
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
        var query = (from hospital in NIData.NEIGHBORHOOD_HOSPITALs
                     where hospital.HospitalZipCode == zip
                     select hospital).Take(5);
        foreach (var hospital in query)
        {
          hospitals.Add(hospital.HospitalName);
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }
      return hospitals;
    }

    public IReadOnlyList<String> getPoliceStations(int zip)
    {
      List<String> policeStations = new List<String>();
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
        var query = (from policeStation in NIData.NEIGHBORHOOD_POLICESTATIONs
                     where policeStation.StationZipCode == zip
                     select policeStation).Take(5);
        foreach (var policeStation in query)
        {
          policeStations.Add(policeStation.StationAdress);
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }
      return policeStations;
    }

    public double getCrimeRate(int zip)
    {
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
        var query =  from rate in NIData.NEIGHBORHOOD_CRIMERATEs
                     where rate.CrimeZipCodes == zip
                     select rate.CrimeRate;
        return query.Average();
      }
      catch (Exception ex)
      {
        return 0;
      }
    }

    public bool changeUserName(User u, string nUName)
    {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

        var query = from user in NIData.Users
                    where (user.UserID == u.UserID)
                    select user;

        // now create business objects from the Station entities, and build a list
        // to return:
        foreach (var user in query)
        {
          user.UserName = nUName;
        }


        try
        {
          NIData.SubmitChanges();
          return true;
        }
        catch(Exception ex)
        {
          return false;
        }
    }

    public bool changePassword(User u, string pass)
    {
      DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

      var query = from user in NIData.Users
                  where (user.UserID == u.UserID)
                  select user;

      // now create business objects from the Station entities, and build a list
      // to return:
      foreach (var user in query)
      {
        user.Password = pass;
      }
      try
      {
        NIData.SubmitChanges();
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public User getUser(string uName)
    {
      uName = uName.Replace("'", "''");
      User loggedIn = null;
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

        var query = from user in NIData.Users
                    where user.UserName.Equals(uName)
                    select user;

        // now create business objects from the Station entities, and build a list
        // to return:
        foreach (var user in query)
        {
          var test = user.Seller;
          loggedIn = new User(user.UserID, user.UserName, user.Password, user.FirstName, user.LastName, user.Email, (user.Seller == 1));
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }

      return loggedIn;
    }

    public bool removeListing(string addr)
    {
      DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
      var deleteOrderDetails = from listing in NIData.Listings
                               where listing.StAddr.Equals(addr)
                               select listing;

      foreach (var detail in deleteOrderDetails)
      {
        NIData.Listings.DeleteOnSubmit(detail);
      }

      try
      {
        NIData.SubmitChanges();
        return true;
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        return false;
        // Provide for exceptions.
      }
    }

    public Listing getListing(int listid)
    {
      Listing selected = null;
      try
      {
        DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

        var query = from listing in NIData.Listings
                    where listing.ListingID == listid
                    select listing;

        // now create business objects from the Station entities, and build a list
        // to return:
        foreach (var listing in query)
        {
          selected = new Listing(listing.ListingID, listing.UserID, listing.StAddr, listing.ZipCode, listing.SqFeet, listing.NoBath, listing.NoBed, listing.Type, listing.Price, listing.Picture);
        }
      }
      catch (Exception ex)
      {
        string msg = string.Format("Error: '{0}'", ex.Message);
        throw new ApplicationException(msg);
      }
      return selected;
    }

    public bool deleteMessage(int mid)
    {
      DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();
      var deleteMessages = from message in NIData.Messages
                               where message.MessageID == mid
                               select message;

      foreach (var message in deleteMessages)
      {
        NIData.Messages.DeleteOnSubmit(message);
      }

      try
      {
        NIData.SubmitChanges();
        return true;
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        return false;
        // Provide for exceptions.
      }
    }

    public bool changeAddr(Listing l, string newAddr)
    {
      newAddr = newAddr.Replace("'","''");
      DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

      var query = from listing in NIData.Listings
                  where (listing.ListingID == l.ListingID)
                  select listing;

      foreach (var listing in query)
      {
        listing.StAddr = newAddr;
      }


      try
      {
        NIData.SubmitChanges();
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool changeZip(Listing l, int newZip)
    {
      DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

      var query = from listing in NIData.Listings
                  where (listing.ListingID == l.ListingID)
                  select listing;

      foreach (var listing in query)
      {
        listing.ZipCode = newZip;
      }


      try
      {
        NIData.SubmitChanges();
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool changeSize(Listing l, int newSize)
    {
      DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

      var query = from listing in NIData.Listings
                  where (listing.ListingID == l.ListingID)
                  select listing;

      foreach (var listing in query)
      {
        listing.SqFeet = newSize;
      }


      try
      {
        NIData.SubmitChanges();
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool changeBaths(Listing l, float newbaths)
    {
      DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

      var query = from listing in NIData.Listings
                  where (listing.ListingID == l.ListingID)
                  select listing;

      foreach (var listing in query)
      {
        listing.NoBath = newbaths;
      }

      try
      {
        NIData.SubmitChanges();
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool changeBeds(Listing l, int newbeds)
    {
      DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

      var query = from listing in NIData.Listings
                  where (listing.ListingID == l.ListingID)
                  select listing;

      foreach (var listing in query)
      {
        listing.NoBed = newbeds;
      }

      try
      {
        NIData.SubmitChanges();
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool changePrice(Listing l, decimal newprice)
    {
      DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

      var query = from listing in NIData.Listings
                  where (listing.ListingID == l.ListingID)
                  select listing;

      foreach (var listing in query)
      {
        listing.Price = newprice;
      }

      try
      {
        NIData.SubmitChanges();
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public bool changeType(Listing l, string newtype)
    {
      DataTier.UsersDBDataContext NIData = new DataTier.UsersDBDataContext();

      var query = from listing in NIData.Listings
                  where (listing.ListingID == l.ListingID)
                  select listing;

      foreach (var listing in query)
      {
        listing.Type = newtype;
      }


      try
      {
        NIData.SubmitChanges();
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }
  }// class DataAPI
}// namespace LogicTier
