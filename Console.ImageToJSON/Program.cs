//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Lifeprojects.de">
//     Class: Program
//     Copyright © Lifeprojects.de 2025
// </copyright>
//
// <author>Gerhard Ahrens - Lifeprojects.de</author>
// <email>developer@lifeprojects.de</email>
// <date>30.04.2025 08:04:06</date>
//
// <summary>
// Konsolen Applikation mit Menü
// </summary>
//-----------------------------------------------------------------------

namespace Console.ImageToJSON
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Windows.Controls;
    using System.Xml.Linq;

    public class Program
    {
        private static void Main(string[] args)
        {
            do
            {
                Console.Clear();
                Console.WriteLine("1. Image in JSON Datei schreiben/Lesen (über Base64)");
                Console.WriteLine("2. Image in JSON Datei mit JsonConverter");
                Console.WriteLine("X. Beenden");

                Console.WriteLine("Wählen Sie einen Menüpunkt oder 'x' für beenden");
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.X)
                {
                    Environment.Exit(0);
                }
                else
                {
                    if (key == ConsoleKey.D1)
                    {
                        MenuPoint1();
                    }
                    else if (key == ConsoleKey.D2)
                    {
                        MenuPoint2();
                    }
                }
            }
            while (true);
        }

        private static void MenuPoint1()
        {
            Console.Clear();

            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string demoDataPath = Path.Combine(new DirectoryInfo(currentDirectory).Parent.Parent.Parent.FullName, "DemoData");
            string demoDataImage = Path.Combine(new DirectoryInfo(currentDirectory).Parent.Parent.Parent.FullName, "DemoData", "Demo.png");

            if (File.Exists(demoDataImage))
            {
                Base64 base64String = ImageToBase64(demoDataImage);

                Contact contact = new Contact("Gerhard","Ahrens",base64String.Value);

                string jsonString = JsonSerializer.Serialize<Contact>(contact);

                File.WriteAllText($"{demoDataPath}\\Demo.json", jsonString);
                Console.WriteLine("JSON Datei geschrieben!");
            }

            if (File.Exists($"{demoDataPath}\\Demo.json"))
            {
                Console.WriteLine("JSON Datei lesen!");
                string jsonString = File.ReadAllText($"{demoDataPath}\\Demo.json");
                Contact contact = JsonSerializer.Deserialize<Contact>(jsonString);
                System.Drawing.Image photo = Base64ToImage(contact.PhotoBase64);
                photo.Save($"{demoDataPath}\\DemoSave.png", System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"Image-Datei aus Base64 gelesen und unter {demoDataPath}\\DemoSave.png gespeichert");
            }

            Console.WriteLine("Mit einer belibigen Taste zurück zum Menü!");
            Console.ReadKey();
        }

        private static void MenuPoint2()
        {
            Console.Clear();

            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string demoDataPath = Path.Combine(new DirectoryInfo(currentDirectory).Parent.Parent.Parent.FullName, "DemoData");
            string demoDataImage = Path.Combine(new DirectoryInfo(currentDirectory).Parent.Parent.Parent.FullName, "DemoData", "Demo.png");

            if (File.Exists(demoDataImage))
            {
                ContactConv contact = new ContactConv("Gerhard", "Ahrens");
                contact.PhotoPath = demoDataImage;

                string jsonString = JsonSerializer.Serialize<ContactConv>(contact);
                File.WriteAllText($"{demoDataPath}\\DemoConv.json", jsonString);
                Console.WriteLine("JSON Datei geschrieben!");
            }

            if (File.Exists($"{demoDataPath}\\DemoConv.json"))
            {
                Console.WriteLine("JSON Datei lesen!");
                string jsonString = File.ReadAllText($"{demoDataPath}\\DemoConv.json");
                ContactConv contact = JsonSerializer.Deserialize<ContactConv>(jsonString);
                System.Drawing.Image photo = Base64ToImage(contact.PhotoBase64);
                photo.Save($"{demoDataPath}\\DemoSave.png", System.Drawing.Imaging.ImageFormat.Png);
                Console.WriteLine($"Image-Datei aus Base64 gelesen und unter {demoDataPath}\\DemoSave.png gespeichert");
            }

            Console.WriteLine("Mit einer belibigen Taste zurück zum Menü!");
            Console.ReadKey();
        }

        #region Image Hilfsmethoden
        public static Base64 ImageToBase64(string imageFile)
        {
            System.Drawing.Image img = System.Drawing.Image.FromFile(imageFile);
            if (File.Exists(imageFile) == true)
            {
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, img.RawFormat);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }

            return new Base64(string.Empty);
        }

        public static System.Drawing.Image Base64ToImage(Base64 base64String)
        {
            System.Drawing.Image result = null;
            if (base64String.IsNullOrEmpty == false)
            {
                byte[] imageBytes = System.Convert.FromBase64String(base64String.Value);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    result = System.Drawing.Image.FromStream(ms);
                }
            }

            return result;
        }
        #endregion Image Hilfsmethoden
    }

    [Serializable]
    public class Contact
    {
        public Contact(string firstName, string lastName, string photoBase64 = "")
        {
            this.Firstname = firstName;
            this.Lastname = lastName;
            this.PhotoBase64 = photoBase64;
        }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string PhotoBase64 { get; set; }
    }

    [Serializable]
    [JsonConverter(typeof(ContactJsonConverter))]
    public class ContactConv
    {
        public ContactConv(string firstName, string lastName, string photoBase64 = "")
        {
            this.Firstname = firstName;
            this.Lastname = lastName;
            this.PhotoBase64 = photoBase64;
        }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string PhotoPath { get; set; }

        public string PhotoBase64 { get; set; }
    }

    #region JsonConverter
    public class ContactJsonConverter : JsonConverter<ContactConv>
    {
        public override ContactConv Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            string firstName = string.Empty;
            string lastName = string.Empty;
            string photoPath = string.Empty;
            string photoBase64 = string.Empty;

            using (var jsonDocument = JsonDocument.ParseValue(ref reader))
            {
                firstName = jsonDocument.RootElement.GetProperty("Firstname").GetString();
                lastName = jsonDocument.RootElement.GetProperty("Lastname").GetString();
                photoPath = jsonDocument.RootElement.GetProperty("PhotoPath").GetString();
                photoBase64 = jsonDocument.RootElement.GetProperty("PhotoBase64").GetString();
            }

            ContactConv contact = new ContactConv(firstName, lastName, photoBase64);
            contact.PhotoPath = photoPath;

            return contact;
        }

        public override void Write(Utf8JsonWriter writer, ContactConv contact, JsonSerializerOptions options)
        {
            string fullName = $"{contact.Firstname} {contact.Lastname}";
            string firstName = contact.Firstname;
            string lastName = contact.Lastname;
            string photoPath = contact.PhotoPath;
            string photoBase64 = string.Empty;

            if (File.Exists(contact.PhotoPath) == true)
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(contact.PhotoPath);
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, img.RawFormat);
                    photoBase64 = Convert.ToBase64String(ms.ToArray());
                }
            }

            writer.WriteStartObject();
            writer.WriteString(nameof(contact.Firstname), firstName);
            writer.WriteString(nameof(contact.Lastname), lastName);
            writer.WriteString(nameof(contact.PhotoPath), photoPath);
            writer.WriteString(nameof(contact.PhotoBase64), photoBase64);
            writer.WriteEndObject();
        }
    }
    #endregion JsonConverter

    #region Implementation of Type Base64
    public struct Base64 : IEquatable<Base64>, IComparable<Base64>
    {
        private readonly string _value;

        public Base64(string value)
        {
            this._value = value;
        }

        public string Value
        {
            get
            {
                return this._value;
            }
        }

        public static string Default
        {
            get { return string.Empty; }
        }

        public bool IsNullOrEmpty
        {
            get { return string.IsNullOrEmpty(this.Value); }
        }

        public int Length
        {
            get
            {
                if (string.IsNullOrEmpty(this.Value) == true)
                {
                    return 0;
                }
                else
                {
                    return this.Value.Length;
                }
            }
        }

        public bool IsNull
        {
            get
            {
                if (this.Value == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static implicit operator Base64(string value)
        {
            return new Base64(value);
        }

        public static implicit operator Base64(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                throw new ArgumentException("Der Wert für byte[] muß <> null bzw. größer 0 sein.");
            }
            else
            {
                string fromByteArray = Convert.ToBase64String(value);
                return new Base64(fromByteArray);
            }
        }

        public static bool operator ==(Base64 left, Base64 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Base64 left, Base64 right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
            hashCode.Add(this.Value);
            return hashCode.ToHashCode();
        }

        #region Implementation of IEquatable<Base64>
        public override bool Equals(object obj)
        {
            if ((obj is Base64) == false)
            {
                return false;
            }

            Base64 other = (Base64)obj;
            return Equals(other);
        }

        public bool Equals(Base64 other)
        {
            return this.Value == other.Value;
        }
        #endregion Implementation of IEquatable<Base64>

        #region Konvertierung nach To...
        public override string ToString()
        {
            return this.Value.ToString();
        }

        public byte[] ToByteArray()
        {
            if (string.IsNullOrEmpty(this.Value) == false)
            {
                return System.Convert.FromBase64String(this.Value);
            }
            else
            {
                return null;
            }
        }
        #endregion Konvertierung nach To...

        #region Implementation of IComparable<Base64>

        public int CompareTo(Base64 other)
        {
            int valueCompare = this.Value.CompareTo(other.Value);

            return valueCompare;
        }
        #endregion Implementation of IComparable<Base64>

    }
    #endregion Implementation of Type Base64
}
