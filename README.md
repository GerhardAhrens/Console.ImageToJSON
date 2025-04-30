# ImageToJSON

![NET](https://img.shields.io/badge/NET-8.0-green.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
![VS2022](https://img.shields.io/badge/Visual%20Studio-2022-white.svg)
![Version](https://img.shields.io/badge/Version-1.0.2025.0-yellow.svg)]

JSON Dateien können unter NET Core nur mit primitiven Datentypen geschrieben und gelesen werden. Komplexere Datentypen wie z.B ein Image, muss über den Umweg eines Base64 kodierten String verwendet werden.

## Ohne JsonConverter

```csharp
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
```

**Hinweis:** Der Typ **Base64** ist ein eigener Custom DataTyp der ebenfalls in das Projekt mit hinzugefügt ist.

## Mit JsonConverter

Eine weitere Möglichkeit ist der Einsatz von einem JsonConverter um die notwendige Funktionalität an die passenden Stelle implementieren zu können.
In dem Beispiel sind nun nur noch der Source geschrieben, der zu Serialisieren bzw. DeSerialisieren notwendig ist.
Die eigendliche Programmierung zur Behandlung des Images wird im Converter implementiert.

```csharp
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
```

### Der JsonConverter dazu

```csharp
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
```
