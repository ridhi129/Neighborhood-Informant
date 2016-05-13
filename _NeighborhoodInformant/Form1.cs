using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using GoogleMaps.LocationServices;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace NeighborhoodInformant
{
  public partial class Form1 : Form
  {
    string details, exportFile;
    LogicTier.DataAPI database;
    LogicTier.User currUser;
    LogicTier.Listing currListing;
    LogicTier.Message currMessage;
    Panel prevPage;
    public Form1()
    {
      //set up UI
      InitializeComponent();
      panelRegistration.Hide();
      panelLogin.Hide();
      panelSearch.Hide();
      panelListing.Hide();
      panelSellerHome.Hide();
      panelMap.Hide();
      panelBuyerHome.Hide();
      panelSendMessage.Hide();
      panelMessage.Hide();
      panelSettings.Hide();

      comboBoxMin.SelectedIndex = 0;
      comboBoxMax.SelectedIndex = 14;
      comboBoxSqMin.SelectedIndex = 0;
      comboBoxSqMax.SelectedIndex = 10;
      comboBoxSearchBathsMin.SelectedIndex = 0;
      comboBoxSearchBathsMax.SelectedIndex = 18;
      comboBoxSearchBedsMin.SelectedIndex = 0;
      comboBoxSearchBedsMax.SelectedIndex = 9;
      //comboBoxRType.SelectedIndex = 0;


      //set up data export file
      exportFile = null; 
      //set up database and user info
      database = new LogicTier.DataAPI();
      currUser = null;
      currListing = null;
      currMessage = null;
      prevPage = panelHome;
      pictureBoxSearch.Image = Image.FromFile(@"D:\Downloads\NeighborhoodInformant\NeighborhoodInformant\bin\Debug\homeforsale.jpg");
      pictureBoxMap.Image = Image.FromFile(@"D:\Downloads\NeighborhoodInformant\NeighborhoodInformant\bin\Debug\homeforsale.jpg");    
    }

    /**
     * Create account button on registration page
     **/
    private void buttonCreateAccount_Click(object sender, EventArgs e)
    {
      // get input values
      string uName, pass, fName, lName, email, phone;
      byte seller;
      uName = txtRUName.Text;
      pass = txtRPass.Text;
      fName = txtRFName.Text;
      lName = txtRLName.Text;
      email = txtREmail.Text;
      phone = maskedTextRPhone.Text;
      if(rRButtonBuyer.Checked){
        seller = 0;
      }else{
        seller = 1;
      }
   
      //error checking
      if((uName.Length == 0)||(fName.Length == 0)||(lName.Length == 0)||(pass.Length == 0)||(email.Length == 0)||(!maskedTextRPhone.MaskCompleted)){
        MessageBox.Show("Please fill in all fields");
      }
      else
      {
        if(database.createAccount(uName, pass, fName, lName, email, phone, seller))
        {
          MessageBox.Show("Account successfully created");
          clearRegistration();
          panelLogin.Show();
          panelRegistration.Hide();
          prevPage = panelRegistration;
        }
        else
        {
          MessageBox.Show("Must choose a unique User Name, Email, and Phone Number");
        }
      }
    }

    /**
     * Login Button on login page
     *
     **/
    private void buttonLogin_Click(object sender, EventArgs e)
    {
      currUser = database.login(txtLUName.Text, txtLPass.Text);
      if(currUser == null)
      {
        MessageBox.Show("Invalid user name and/or password. Please check your credentials!");
      }
      else
      {
        login();
        clearLogin();
        if(currUser.Seller)// seller
        {
          buttonSeller.Enabled = true;
          if(prevPage == panelSearch)// coming from search page
          {
            panelSearch.Show();
          }
          else// coming from home page
          {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = database.listingsFromUser(currUser);
            dataGridView1.AutoGenerateColumns = false;
            panelSellerHome.Show();
          }
        }
        else// buyer
        {
          if (prevPage == panelSearch)// coming from search page
          {
            panelSearch.Show();
          }
          else// coming from home page
          {
            panelBuyerHome.Show();
          }
        }
        panelLogin.Hide();
        prevPage = panelLogin;
      }
    }

    /**
     * account creation button on login page
     **/
    private void buttonToACPage_Click(object sender, EventArgs e)
    {
      panelLogin.Hide();
      prevPage = panelLogin;
      panelRegistration.Show();
    }

    /**
     * Login button on home page
     **/
    private void buttonGoToLogin_Click(object sender, EventArgs e)
    {
      panelHome.Hide();
      prevPage = panelHome;
      panelLogin.Show();
    }

    /**
     * Seller button on home page
     **/
    private void buttonSeller_Click(object sender, EventArgs e)
    {
      rRButtonSeller.Checked = true;
      if (currUser != null)
      {
        if (currUser.Seller)
        {
          panelHome.Hide();
          dataGridView1.DataSource = null;
          dataGridView1.DataSource = database.listingsFromUser(currUser);
          dataGridView1.AutoGenerateColumns = false;
          panelSellerHome.Show();
        }
        else
        {
          MessageBox.Show("You are not logged into a seller account");
        }
      }
      else
      {
        panelHome.Hide();
        panelLogin.Show();
      }
      prevPage = panelHome;
    }

    /**
     * buyer button on home page
     **/
    private void buttonBuyer_Click(object sender, EventArgs e)
    {
      panelBuyerHome.Show();
      panelHome.Hide();
      prevPage = panelHome;
    }

    /**
     * Post listing button on listings page
     **/
    private void buttonPost_Click(object sender, EventArgs e)
    {
      string stAddr, type, picture;
      int zipCode, sqFeet, noBed; 
      float noBath;
      decimal price;
      try
      { 
        stAddr = txtAddr.Text;
        type = comboBoxRType.SelectedItem.ToString();
        zipCode = Int32.Parse(maskedTextZip.Text);
        sqFeet = Int32.Parse(txtSqFeet.Text);
        noBed = Int32.Parse(comboBoxBedRooms.SelectedItem.ToString());
        noBath = float.Parse(comboBoxBathRooms.SelectedItem.ToString());
        price = decimal.Parse(txtPrice.Text);
        picture = textBoxPhoto.Text;
         //error checking
        if ((stAddr.Length == 0) || (type.Length == 0) || (zipCode == 0) || (sqFeet == 0) || (noBed == 0) || (noBath == 0) || (price == 0))
        {
          MessageBox.Show("Please fill in all fields");
        }
        else
        {
          if(database.postListing(currUser, stAddr, zipCode, sqFeet, noBath, noBed, type, price, picture))
          {
            MessageBox.Show("Listing Posted!");
            clearListing();
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = database.listingsFromUser(currUser);
            dataGridView1.AutoGenerateColumns = false;
            panelSellerHome.Show();
            panelListing.Hide();
            prevPage = panelListing;
          }
          else
          {
            MessageBox.Show("Error: that address has already been posted");
          }
        }
      }
      catch(Exception ex)
      {
        MessageBox.Show("Please fill in all fields");
      }
    }

    /**
     * Search button on search page
     **/
    private void buttonSearch_Click(object sender, EventArgs e)
    {
      try
      {
        int zipCode = 0;
        String type = "";
        if (maskedTextBoxSearchZip.MaskCompleted)
          zipCode = Int32.Parse(maskedTextBoxSearchZip.Text);
        if(comboBoxType.SelectedIndex >= 0)
         type = comboBoxType.SelectedItem.ToString();
        decimal minPrice = decimal.Parse(comboBoxMin.SelectedItem.ToString());
        decimal maxPrice = decimal.Parse(comboBoxMax.SelectedItem.ToString());
        int minSq = (int)decimal.Parse(comboBoxSqMin.SelectedItem.ToString());
        int maxSq = (int)decimal.Parse(comboBoxSqMax.SelectedItem.ToString());
        int minBed = (int)decimal.Parse(comboBoxSearchBedsMin.SelectedItem.ToString());
        int maxBed = (int)decimal.Parse(comboBoxSearchBedsMax.SelectedItem.ToString());
        double minBath = double.Parse(comboBoxSearchBathsMin.SelectedItem.ToString());
        double maxBath = double.Parse(comboBoxSearchBathsMax.SelectedItem.ToString());
        if (maxPrice == 800000)
          maxPrice = (decimal)100000000.00;
        if (maxSq == 5000)
          maxSq = 10000000;

        IReadOnlyList<LogicTier.Listing> listings = database.search(zipCode, type, minPrice, maxPrice, minBed, maxBed, minSq, maxSq, minBath, maxBath);


        listBoxListings.Items.Clear();
        listBoxDetails.Items.Clear();
        buttonExport.Enabled = false;
        btnMessageSearch.Enabled = false;
        pictureBoxSearch.Image = Image.FromFile(@"D:\Downloads\NeighborhoodInformant\NeighborhoodInformant\bin\Debug\homeforsale.jpg");
        currListing = null;
        foreach(var listing in listings)
        {
          listBoxListings.Items.Add(listing.StAddr);
        }
   
      }
      catch(Exception ex)
      {
        MessageBox.Show(ex.Message, "this one");

      }
    }

    /**
     * select new listing on search page
     **/
    private void listBoxListings_SelectedIndexChanged(object sender, EventArgs e)
    {
      int i = listBoxListings.SelectedIndex;
      listBoxDetails.Items.Clear();
      buttonExport.Enabled = false;
      btnMessageSearch.Enabled = false;
      pictureBoxSearch.Image = Image.FromFile(@"D:\Downloads\NeighborhoodInformant\NeighborhoodInformant\bin\Debug\homeforsale.jpg");
      currListing = null;
      details = null;
      if(i >= 0)
      {
        string addr = listBoxListings.GetItemText(listBoxListings.SelectedItem);
        LogicTier.Listing listing = database.getListingFromAddress(addr);
        currListing = listing;
        listBoxDetails.Items.Add(listing.fullAddress());
        listBoxDetails.Items.Add("Size: " + listing.SqFeet + " square feet");
        listBoxDetails.Items.Add("Type: " + listing.Type);
        listBoxDetails.Items.Add("Bedrooms: " + listing.NoBed);
        listBoxDetails.Items.Add("Bathrooms: " + listing.NoBath);
        decimal price = decimal.Parse(listing.Price.ToString());
        string priceStr = String.Format("Price: {0:C2}", price);
        listBoxDetails.Items.Add(priceStr);
        if (!string.IsNullOrEmpty(listing.Picture))
        {
          pictureBoxSearch.Image = new Bitmap(listing.Picture);
        }
        details = listing.StAddr.ToString() + "," + listing.ZipCode.ToString() + "," + listing.Type.ToString() + "," + listing.SqFeet.ToString()
                    + "," + listing.NoBed.ToString() + "," + listing.NoBath.ToString() + "," + listing.Price.ToString() + "\n";
        buttonExport.Enabled = true;
        btnMessageSearch.Enabled = true;
        if(currUser != null)
        {
          //LogicTier.User seller = database.getUser(listing.UserID);
          //listBoxDetails.Items.Add("Seller contact: " + seller.Email);
          int c = 0;
          listBoxDetails.Items.Add("Schools: ");
          IReadOnlyList<String> schools = database.getSchools(listing.ZipCode);
          foreach(var school in schools)
          {
            c++;
            listBoxDetails.Items.Add("  "+school);
          }
          if(c == 0)
            listBoxDetails.Items.Add("  None");
          
          c = 0;
          listBoxDetails.Items.Add("Hospitals: ");
          IReadOnlyList<String> hospitals = database.getHospitals(listing.ZipCode);
          foreach (var hospital in hospitals)
          {
            c++;
            listBoxDetails.Items.Add("  " + hospital);
          }
          if (c == 0)
            listBoxDetails.Items.Add("  None");
          listBoxDetails.Items.Add("Police Stations: ");
          c = 0;
          IReadOnlyList<String> stations = database.getPoliceStations(listing.ZipCode);
          foreach (var station in stations)
          {
            c++;
            listBoxDetails.Items.Add("  " + station);
          }
          if (c == 0)
            listBoxDetails.Items.Add("  None");
          listBoxDetails.Items.Add("Crime Rate:");
          listBoxDetails.Items.Add("  "+ String.Format("{0:0.##}", database.getCrimeRate(listing.ZipCode)));
        }

      }
    }

    /**
     * Back button on listings page
     **/
    private void button1_Click(object sender, EventArgs e)
    {
      clearListing();
      panelListing.Hide();
      prevPage = panelListing;
      panelSellerHome.Show();
    }

    /**
     * back button on registration page
     **/
    private void button2_Click(object sender, EventArgs e)
    {
      clearRegistration();
      panelRegistration.Hide();
      prevPage = panelRegistration;
      panelLogin.Show();
    }

    /**
     * Add listing button on seller page
     **/
    private void btnAdd_Click(object sender, EventArgs e)
    {
      panelSellerHome.Hide();
      prevPage = panelSellerHome;
      panelListing.Show();
    }


    /**
     * Log off button on seller page
     **/
    private void btnLogOff_Click(object sender, EventArgs e)
    {
      logoff();
      panelHome.Show();
      panelSellerHome.Hide();
      prevPage = panelSellerHome;
    }

    /**
     * login button from search page
     **/
    private void button2_Click_1(object sender, EventArgs e)
    {
        prevPage = panelSearch;
        panelLogin.Show();
        panelSearch.Hide();
    }

    /**
     * log off button on search page
     **/
    private void btnSearchLogOff_Click(object sender, EventArgs e)
    {
      logoff();
      panelHome.Show();
      panelSearch.Hide();
      prevPage = panelSearch;
    }

    
    /**
     * back button on search page
     **/
    private void btnSearchBack2_Click(object sender, EventArgs e)
    {
        panelSearch.Hide();
        prevPage = panelSearch;
        panelBuyerHome.Show();
    }

    /**
     * export data button on search page
     **/
    private void buttonExport_Click(object sender, EventArgs e)
    {
      if(details.Length != 0)
      {
        if(exportFile == null)
        {
          chooseFile();
        }
        if(exportFile != null)
        {
          System.IO.File.AppendAllText(exportFile, details);
          MessageBox.Show("Data saved to file");
        }
      }
      else
      {
        buttonExport.Enabled = false;
      }
    }

    /**
     * back button on seller page
     **/
    private void buttonSellerBack_Click(object sender, EventArgs e)
    {
      panelHome.Show();
      panelSellerHome.Hide();
      prevPage = panelSellerHome;
    }

    /**
     * search button from buyer home page
     **/
    private void buttonToSearchPage_Click(object sender, EventArgs e)
    {
      panelSearch.Show();
      panelBuyerHome.Hide();
      prevPage = panelBuyerHome;
    }

    /**
     * Map button from buyer home
     **/
    private void buttonToMap_Click(object sender, EventArgs e)
    {
      listBoxMapDetails.Items.Clear();
      pictureBoxMap.Image = Image.FromFile(@"D:\Downloads\NeighborhoodInformant\NeighborhoodInformant\bin\Debug\homeforsale.jpg");
      buttonMapMessage.Enabled = false;
      buttonMapExport.Enabled = false;
      gMap.MapProvider = GMap.NET.MapProviders.BingMapProvider.Instance;
      GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
      gMap.SetPositionByKeywords("Chicago, Illinois");
      gMap.DragButton = MouseButtons.Left;
      GMap.NET.WindowsForms.GMapOverlay markersOverlay = new GMap.NET.WindowsForms.GMapOverlay("markers");
      gMap.Overlays.Add(markersOverlay);
      gMap.Overlays.Remove(markersOverlay);
      gMap.Overlays.Clear();
      IReadOnlyList<LogicTier.Listing> listings = database.allListings();
      foreach(var listing in listings)
      {
        var address = listing.fullAddress();

        var locationService = new GoogleLocationService();
        try
        {
          var point = locationService.GetLatLongFromAddress(address);
          var latitude = point.Latitude;
          var longitude = point.Longitude;

          GMap.NET.WindowsForms.Markers.GMarkerGoogle marker = new GMap.NET.WindowsForms.Markers.GMarkerGoogle(new GMap.NET.PointLatLng(latitude, longitude), GMap.NET.WindowsForms.Markers.GMarkerGoogleType.green);
          marker.ToolTipText = listing.StAddr;
          markersOverlay.Markers.Add(marker);
          
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message, "Error");
        }
      }
      gMap.Overlays.Add(markersOverlay);
      
      panelMap.Show();
      panelBuyerHome.Hide();
      prevPage = panelBuyerHome;
    }


    /**
     * export button on map page
     */
    private void buttonMapExport_Click(object sender, EventArgs e)
    {
      if (details.Length != 0)
      {
        if (exportFile == null)
        {
          chooseFile();
        }
        if (exportFile != null)
        {
          System.IO.File.AppendAllText(exportFile, details);
          MessageBox.Show("Data saved to file");
        }
      }
      else
      {
        buttonMapExport.Enabled = false;
      }
    }

    /**
     * Map marker click
     **/
    private void gMap_OnMarkerClick(GMap.NET.WindowsForms.GMapMarker item, MouseEventArgs e)
    {
      pictureBoxMap.Image = Image.FromFile(@"D:\Downloads\NeighborhoodInformant\NeighborhoodInformant\bin\Debug\homeforsale.jpg");
      LogicTier.Listing listing =  database.getListingFromAddress(item.ToolTipText);
      currListing = listing;
      listBoxMapDetails.Items.Clear();
      listBoxMapDetails.Items.Add(listing.fullAddress());
      listBoxMapDetails.Items.Add("Size: " + listing.SqFeet + " square feet");
      listBoxMapDetails.Items.Add("Type: " + listing.Type);
      listBoxMapDetails.Items.Add("Bedrooms: " + listing.NoBed);
      listBoxMapDetails.Items.Add("Bathrooms: " + listing.NoBath);
      decimal price = decimal.Parse(listing.Price.ToString());
      string priceStr = String.Format("Price: {0:C2}", price);
      listBoxMapDetails.Items.Add(priceStr);
      if (!string.IsNullOrEmpty(listing.Picture))
      {
        pictureBoxMap.Image = new Bitmap(listing.Picture);
      }
      buttonMapMessage.Enabled = true;
      details = listing.StAddr.ToString() + "," + listing.ZipCode.ToString() + "," + listing.Type.ToString() + "," + listing.SqFeet.ToString()
                    + "," + listing.NoBed.ToString() + "," + listing.NoBath.ToString() + "," + listing.Price.ToString() + "\n";
      buttonMapExport.Enabled = true;
      if(currUser != null)
      {
          int c = 0;
          listBoxMapDetails.Items.Add("Schools: ");
          IReadOnlyList<String> schools = database.getSchools(listing.ZipCode);
          foreach(var school in schools)
          {
            c++;
            listBoxMapDetails.Items.Add("  "+school);
          }
          if(c == 0)
            listBoxMapDetails.Items.Add("  None");
          c = 0;
          listBoxMapDetails.Items.Add("Hospitals: ");
          IReadOnlyList<String> hospitals = database.getHospitals(listing.ZipCode);
          foreach (var hospital in hospitals)
          {
            c++;
            listBoxMapDetails.Items.Add("  " + hospital);
          }
          if (c == 0)
            listBoxMapDetails.Items.Add("  None");
          listBoxMapDetails.Items.Add("Police Stations: ");
          c = 0;
          IReadOnlyList<String> stations = database.getPoliceStations(listing.ZipCode);
          foreach (var station in stations)
          {
            c++;
            listBoxMapDetails.Items.Add("  " + station);
          }
          if (c == 0)
            listBoxMapDetails.Items.Add("  None");
          listBoxMapDetails.Items.Add("Crime Rate:");
          listBoxMapDetails.Items.Add("  "+ String.Format("{0:0.##}", database.getCrimeRate(listing.ZipCode)));
      }
    }

    /**
     * back button on map page
     **/
    private void buttonMapBack_Click(object sender, EventArgs e)
    {
      panelBuyerHome.Show();
      panelMap.Hide();
      prevPage = panelMap;
    }

    /**
     * back button from buyer home
     **/
    private void buttonBuyerBack_Click(object sender, EventArgs e)
    {
      panelHome.Show();
      panelBuyerHome.Hide();
      prevPage = panelBuyerHome;
    }

    /**
     * log off from buyer home
     **/
    private void buttonBuyerLogoff_Click(object sender, EventArgs e)
    {
      logoff();
      panelHome.Show();
      panelBuyerHome.Hide();
      prevPage = panelBuyerHome;
    }

    private void buttonLogFile_Click(object sender, EventArgs e)
    {
      chooseFile();
    }

    private void chooseFile()
    {
      SaveFileDialog openFileDialog = new SaveFileDialog();

      openFileDialog.Filter = "Data Files (.csv)|*.csv|All Files (*.*)|*.*";
      openFileDialog.FilterIndex = 1;
      openFileDialog.OverwritePrompt = false;

      var userClickedOK = openFileDialog.ShowDialog();

      if (userClickedOK == DialogResult.OK)
      {
        exportFile = openFileDialog.FileName;
        if (!File.Exists(exportFile) || ((new FileInfo(exportFile).Length) == 0))
        {
          File.WriteAllText(exportFile, "ADDRESS,ZIP CODE,TYPE,SIZE,BEDROOMS,BATHROOMS,PRICE\n");
        }
      }
    }

    private string choosePicture()
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();

      openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
      openFileDialog.FilterIndex = 1;

      var userClickedOK = openFileDialog.ShowDialog();

      if (userClickedOK == DialogResult.OK)
      {
        var size = new FileInfo(openFileDialog.FileName).Length;
        if (size < 5000000)
        {
          return openFileDialog.FileName;
        }
        else
        {
          MessageBox.Show("File too Large.");
        }
      }
      return null;
    }

    /**
     * message seller button from search page
     **/
    private void btnMessageSearch_Click(object sender, EventArgs e)
    {
      panelSendMessage.Show();
      panelSearch.Hide();
      prevPage = panelSearch;
    }

    /**
     * set curser to begining of the maskedtextbox
     **/
    private void maskedTextRPhone_MouseClick(object sender, MouseEventArgs e)
    {
      maskedTextRPhone.Select(0,0);
      maskedTextZip.Select(0,0);
      maskedTextBoxSearchZip.Select(0,0);
    }

    /*
     * back button from send message page
     **/
    private void buttonSendMessageBack_Click(object sender, EventArgs e)
    {
      prevPage.Show();
      panelSendMessage.Hide();
      txtMessage.Clear();
      prevPage = panelSendMessage;
    }

    /**
     * send message button on send message page
     **/
    private void buttonSendMessageSend_Click(object sender, EventArgs e)
    {
      string message = txtMessage.Text;
      if(database.sendMessage(message, currUser, currListing.UserID, currListing.ListingID))
      {
        MessageBox.Show("Message Sent!");
        txtMessage.Clear();
        panelSearch.Show();
        panelSendMessage.Hide();
        prevPage = panelSendMessage;
      }
      else
      {
        MessageBox.Show("Error: Could not send message");
      }
    }

    private void logoff()
    {
      currUser = null;
      btnMessageSearch.Visible = false;
      btnSearchLogin.Visible = true;
      btnSearchLogOff.Visible = false;
      buttonGoToLogin.Enabled = true;
      buttonToMessages.Enabled = false;
      buttonMapMessage.Visible = false;
      buttonBuyerLogoff.Visible = false;
      buttonSeller.Enabled = false;
      buttonBToSettings.Visible = false;
    }

    private void login()
    {
      btnMessageSearch.Visible = true;
      btnSearchLogin.Visible = false;
      btnSearchLogOff.Visible = true;
      buttonGoToLogin.Enabled = false;
      buttonToMessages.Enabled = true;
      buttonMapMessage.Visible = true;
      buttonBuyerLogoff.Visible = true;
      buttonBToSettings.Visible = true;
      clearMessage();
    }

    private void clearLogin()
    {
      txtLPass.Clear();
      txtLUName.Clear();
    }

    private void clearListing()
    {
      txtSqFeet.Clear();
      txtPrice.Clear();
      txtAddr.Clear();
      comboBoxRType.SelectedIndex = -1;
      comboBoxBedRooms.SelectedIndex = -1;
      comboBoxBathRooms.SelectedIndex = -1;
      maskedTextZip.Clear();
      textBoxPhoto.Clear();
    }

    private void clearRegistration()
    {
      maskedTextRPhone.Clear();
      txtRUName.Clear();
      txtRPass.Clear();
      txtRFName.Clear();
      txtRLName.Clear();
      txtREmail.Clear();
      rRButtonBuyer.Select();
    }

    private void clearMessage()
    {
      textBoxMessageDisplay.Clear();
      textBoxMessageRespond.Clear();
      buttonReply.Enabled = false;
    }

    private void buttonMessageBack_Click(object sender, EventArgs e)
    {
      clearMessage();
      prevPage.Show();
      panelMessage.Hide();
      prevPage = panelMessage;
    }

    private void buttonToMessages_Click(object sender, EventArgs e)
    {
      buttonReply.Enabled = false;
      listView1.Items.Clear();
      IReadOnlyList<LogicTier.Message> messages = database.getMessages(currUser);
      foreach (var message in messages)
      {
        ListViewItem nLine = new ListViewItem();
        string addr = "**Listing Removed**";
        try
        {
          addr = database.getListing(message.ThreadID).StAddr;
        }
        catch (Exception ex)
        {
          // do nothing
        }
        nLine.Text = database.getUser(message.SenderID).UserName + ": " + addr;
        nLine.Tag = message.MessageID;
        listView1.Items.Add(nLine);
      }
      panelMessage.Show();
      panelBuyerHome.Hide();
      prevPage = panelBuyerHome;
    }

    private void buttonReply_Click(object sender, EventArgs e)
    {
      string message = textBoxMessageRespond.Text;
      if (database.sendMessage(message, currUser, currMessage.SenderID, currMessage.ThreadID))
      {
        MessageBox.Show("Message Sent!");
        clearMessage();
      }
      else
      {
        MessageBox.Show("Error: Could not send message");
      }
    }

    private void buttonChoosePhoto_Click(object sender, EventArgs e)
    {
      textBoxPhoto.Text = choosePicture();
    }

    private void buttonMapMessage_Click(object sender, EventArgs e)
    {
      panelSendMessage.Show();
      panelMap.Hide();
      prevPage = panelMap;
    }

    private void buttonLoginBack_Click(object sender, EventArgs e)
    {
      panelLogin.Hide();
      prevPage = panelLogin;
      clearLogin();
      panelHome.Show();
    }

    private void buttonSellerToMessages_Click(object sender, EventArgs e)
    {
      buttonReply.Enabled = false;
      listView1.Items.Clear();
      IReadOnlyList<LogicTier.Message> messages = database.getMessages(currUser);
      foreach (var message in messages)
      {
        ListViewItem nLine = new ListViewItem();
        string addr = "**Listing Removed**";
        try
        {
          addr = database.getListing(message.ThreadID).StAddr;
        }
        catch(Exception ex)
        {
          // do nothing
        }
        nLine.Text = database.getUser(message.SenderID).UserName + ": " + addr;
        nLine.Tag = message.MessageID;
        listView1.Items.Add(nLine);
      }
      panelMessage.Show();
      panelSellerHome.Hide();
      prevPage = panelSellerHome;
    }

    private void buttonChangeUName_Click(object sender, EventArgs e)
    {
      string nUName = textBoxSUName.Text;
      if(string.IsNullOrEmpty(nUName))
      {
        MessageBox.Show("Error: Please enter a vaild user name");
      }
      else
      {
        if(database.changeUserName(currUser, nUName))
        {
          MessageBox.Show("User Name has been changed to: " + nUName);
          currUser = database.login(nUName, currUser.Password);
        }
        else
        {
          MessageBox.Show("Error: Please enter a vaild user name");
        }
      }
    }

    private void buttonBToSettings_Click(object sender, EventArgs e)
    {
      panelSettings.Show();
      panelBuyerHome.Hide();
      prevPage = panelBuyerHome;
    }

    private void buttonSettingsBack_Click(object sender, EventArgs e)
    {
      panelSettings.Hide();
      prevPage.Show();
      textBoxChangePass.Clear();
      textBoxSUName.Clear();
      prevPage = panelSettings;
    }

    private void buttonChangePassword_Click(object sender, EventArgs e)
    {
      string nPass = textBoxChangePass.Text;

      if (string.IsNullOrEmpty(nPass))
      {
        MessageBox.Show("Error: Please enter a vaild password");
      }
      else
      {
        if (database.changePassword(currUser, nPass))
        {
          MessageBox.Show("Password has been changed");
          currUser = database.login(currUser.UserName, nPass);
        }
        else
        {
          MessageBox.Show("Error: Please enter a vaild password");
        }
      }
    }

    private void buttonSellerSettings_Click(object sender, EventArgs e)
    {
      panelSettings.Show();
      panelSellerHome.Hide();
      prevPage = panelSellerHome;
    }

    private void buttonLogFile_Click_1(object sender, EventArgs e)
    {
      chooseFile();
    }


    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      string userName = txtLUName.Text;

      if (string.IsNullOrEmpty(userName))
      {
        MessageBox.Show("Please enter a User Name");
      }
      else
      {
        LogicTier.User u = database.getUser(userName);
        if (u == null)
        {
          MessageBox.Show("Invalid User Name");
        }
        else
        {
          var fromAddress = new MailAddress("neighborhoodinformantbot@gmail.com", "Neighborhood Informant");
          try
          {
            var toAddress = new MailAddress(u.Email, u.FirstName + " " + u.LastName);
            const string fromPassword = "ni123456";
            const string subject = "Neighborhood Informant Password Reset";
            const string body = "Your password for has been changed to 123";
            var smtp = new SmtpClient
            {
              Host = "smtp.gmail.com",
              Port = 587,
              EnableSsl = true,
              DeliveryMethod = SmtpDeliveryMethod.Network,
              UseDefaultCredentials = false,
              Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
              Timeout = 20000
            };
            try
            {
              using (var message = new MailMessage(fromAddress, toAddress)
              {
                Subject = subject,
                Body = body
              })
              {
                smtp.Send(message);
              }
              database.changePassword(u, "123");
              MessageBox.Show("Password has been reset and emailed to " + u.Email);
            }
            catch (Exception ex)
            {
              MessageBox.Show("Unable to send email to " + u.Email);
            }
          }
          catch(Exception ex)
          {
            MessageBox.Show("Unable to send email to " + u.Email);
          }
        }
      }
    }

    private void button1_Click_1(object sender, EventArgs e)
    {
      var selected = dataGridView1.CurrentRow;
      if (selected == null)
      {
        MessageBox.Show("No listing to remove");
      }
      else
      {
        string addr = dataGridView1.CurrentRow.Cells[1].Value.ToString();
        try
        {
          if(database.removeListing(addr))
          {
            MessageBox.Show("Listing for " + addr + " removed");
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = database.listingsFromUser(currUser);
            dataGridView1.AutoGenerateColumns = false;
          }
          else
          {
            MessageBox.Show("Error: could not remove listing");
          }
        }
        catch(Exception ex)
        {
          MessageBox.Show("Error: could not remove listing");
        }
      }
      
    }

    private void listView1_SelectedIndexChanged(object sender, EventArgs e)
    {
      if(listView1.SelectedItems.Count > 0)
      {
        int mid = Int32.Parse(listView1.SelectedItems[0].Tag.ToString());
        textBoxMessageDisplay.Clear();
        buttonReply.Enabled = false;
        btnMessageSearch.Enabled = false;
        currMessage = null;
        LogicTier.Message message = database.getMessage(mid);
        currMessage = message;
        textBoxMessageDisplay.Text = message.MessageBody;
        buttonReply.Enabled = true;
      }
    }

    private void buttonDeleteMessage_Click(object sender, EventArgs e)
    {
      if (listView1.SelectedItems.Count > 0)
      {
        int mid = Int32.Parse(listView1.SelectedItems[0].Tag.ToString());
        textBoxMessageDisplay.Clear();
        buttonReply.Enabled = false;
        btnMessageSearch.Enabled = false;
        currMessage = null;
        database.deleteMessage(mid);

        listView1.Items.Clear();
        IReadOnlyList<LogicTier.Message> messages = database.getMessages(currUser);
        foreach (var message in messages)
        {
          ListViewItem nLine = new ListViewItem();
          string addr = "**Listing Removed**";
          try
          {
            addr = database.getListing(message.ThreadID).StAddr;
          }
          catch (Exception ex)
          {
            // do nothing
          }
          nLine.Text = database.getUser(message.SenderID).UserName + ": " + addr;
          nLine.Tag = message.MessageID;
          listView1.Items.Add(nLine);
        }
      }
      else
      {
        MessageBox.Show("No message to delete");
      }
    }

    private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
      LogicTier.Listing selected = database.getListingFromID(Int32.Parse(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString()));
      string newval = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
      if (dataGridView1.Columns[e.ColumnIndex].Name == "ListingID")
      {
        MessageBox.Show("Error: you can not change the listing id");
        reloadListings();
      }
      else if (dataGridView1.Columns[e.ColumnIndex].Name == "StAddr")
      {
        if(database.changeAddr(selected, newval))
        {
          MessageBox.Show("Address changed");
        }
        else
        {
          MessageBox.Show("Error: Invalid entry");
          reloadListings();
        }
      }
      else if(dataGridView1.Columns[e.ColumnIndex].Name == "ZipCode")
      {
        try
        {
          if (database.changeZip(selected, Int32.Parse(newval)))
          {
            MessageBox.Show("Zip code changed");
          }
          else
          {
            MessageBox.Show("Error: Invalid entry");
            reloadListings();
          }
        }
        catch(Exception ex)
        {
          MessageBox.Show("Error: Invalid entry");
          reloadListings();
        }
      }
      else if (dataGridView1.Columns[e.ColumnIndex].Name == "SqFeet")
      {
        try
        {
          int newsize = Int32.Parse(newval);
          if (database.changeSize(selected, newsize))
          {
            MessageBox.Show("Sq feet changed");
          }
          else
          {
            MessageBox.Show("Error: Invalid entry");
            reloadListings();
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Invalid entry");
          reloadListings();
        }
      }
      else if (dataGridView1.Columns[e.ColumnIndex].Name == "NoBath")
      {
        try
        {
          float newbaths = float.Parse(newval);
          if ((newbaths > 0) && (newbaths <= 10) && ((newbaths % .5) == 0) && database.changeBaths(selected, newbaths))
          {
            MessageBox.Show("Number of bathrooms changed");
          }
          else
          {
            MessageBox.Show("Error: Invalid entry");
            reloadListings();
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Invalid entry");
          reloadListings();
        }
      }
      else if (dataGridView1.Columns[e.ColumnIndex].Name == "NoBed")
      {
        try
        {
          int newbeds = Int32.Parse(newval);
          if ((newbeds > 0) && (newbeds <= 10) && database.changeBeds(selected, newbeds))
          {
            MessageBox.Show("Number of bedrooms changed changed");
          }
          else
          {
            MessageBox.Show("Error: Invalid entry");
            reloadListings();
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Invalid entry");
          reloadListings();
        }
      }
      else if (dataGridView1.Columns[e.ColumnIndex].Name == "Type")
      {
        try
        {
          if ((newval.Equals("Condo") || newval.Equals("Appartment") || newval.Equals("House") || newval.Equals("Townhome")) && database.changeType(selected, newval))
          {
            MessageBox.Show("Type of listing changed");
          }
          else
          {
            MessageBox.Show("Error: Invalid entry");
            reloadListings();
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Invalid entry");
          reloadListings();
        }
      }
      else if (dataGridView1.Columns[e.ColumnIndex].Name == "Price")
      {
        try
        {
          decimal newprice = Decimal.Parse(newval);
          if (database.changePrice(selected, newprice))
          {
            MessageBox.Show("Price changed");
          }
          else
          {
            MessageBox.Show("Error: Invalid entry");
            reloadListings();
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error: Invalid entry");
          reloadListings();
        }
      }
    }

    void reloadListings()
    {
      dataGridView1.DataSource = null;
      dataGridView1.DataSource = database.listingsFromUser(currUser);
      dataGridView1.AutoGenerateColumns = false;
    }
    






  }//class
}//namespace
