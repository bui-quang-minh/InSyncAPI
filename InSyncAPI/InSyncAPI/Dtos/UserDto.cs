using Microsoft.OData.Edm;
using System.Net.Mail;

namespace InSyncAPI.Dtos
{
    public class Data
    {
        public string? Birthday { get; set; }
        public long? Created_At { get; set; }
        public EmailAddresses[]? Email_Addresses { get; set; }
        public string[]? External_Accounts { get; set; }
        public string? First_Name { get; set; } = "";
        public string? Gender { get; set; }
        public string? Id { get; set; }
        public string? Image_Url { get; set; }
        public string? Last_Name { get; set; } = "";
        public long? Last_Sign_In_At { get; set; }
        public string? Object { get; set; }
        public bool? Password_Enabled { get; set; }
        public string[]? Phone_Numbers { get; set; };
        public string? Primary_Email_Address_Id { get; set; }
        public string? Primary_Phone_Number_Id { get; set; }
        public string? Primary_Web3_Wallet_Id { get; set; }
        public Dictionary<string, object>? Private_Metadata { get; set; }
        public string? Profile_Image_Url { get; set; } = "";
        public Dictionary<string, object>? Public_Metadata { get; set; }
        public bool? Two_Factor_Enabled { get; set; }
        public Dictionary<string, object>? Unsafe_Metadata { get; set; }
        public long? Updated_At { get; set; }
        public object? Username { get; set; }
        public string[]? Web3_Wallets { get; set; }
    }
    public class EmailAddresses
    {
        public string Email_Address { get; set; }
        public string Id { get; set; }
        public object[] Linked_To { get; set; }
        public string Object { get; set; }
        public Verification Verification { get; set; }
    }
    public class Verification
    {
        public string Status { get; set; }
        public string Strategy { get; set; }
    }
    public class CreateUserDto
    {
        public Data Data { get; set; }
        public string Object { get; set; }
        public string Type { get; set; }
    }
    public class UpdateUserDto
    {
        public Data Data { get; set; }
        public string Object { get; set; }
        public string Type { get; set; }
    }
}
