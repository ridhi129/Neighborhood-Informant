CREATE TABLE Users(
  UserID      INT IDENTITY(1,1) PRIMARY KEY,
  UserName    NVARCHAR(64)  UNIQUE NOT NULL,
  Password    NVARCHAR(64)  NOT NULL,
  FirstName   NVARCHAR(64)  NOT NULL,
  LastName    NVARCHAR(64)  NOT NULL,
  Email       NVARCHAR(320)  UNIQUE NOT NULL,
  MobileNo    NVARCHAR(15) UNIQUE,
  Seller      TINYINT NOT NULL
);

CREATE TABLE Listings(
  ListingID     INT IDENTITY(1,1) PRIMARY KEY,
  UserID        INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
  StAddr        NVARCHAR(128)  UNIQUE NOT NULL,
  zipCode       INT NOT NULL,
  SqFeet        INT NOT NULL,
  NoBath        FLOAT NOT NULL,
  NoBed         INT NOT NULL,
  Type          NVARCHAR(32) NOT NULL,
  Price         MONEY NOT NULL
);