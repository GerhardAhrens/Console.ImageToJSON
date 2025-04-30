# ImageToJSON

![NET](https://img.shields.io/badge/NET-8.0-green.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
![VS2022](https://img.shields.io/badge/Visual%20Studio-2022-white.svg)
![Version](https://img.shields.io/badge/Version-1.0.2025.0-yellow.svg)]

JSON Dateien können ubnter NET Core nur mit primitiven Datentypen geschrieben und gelesen werden. Komplexere Datentypen wie z.B ein Image, muss über den Umweg eines Base64 kodierten String verwendet werden.

```csharp
Base64 base64String = string.Empty;

string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
string demoDataPath = Path.Combine(new DirectoryInfo(currentDirectory).Parent.Parent.Parent.FullName, "DemoData");
string demoDataImage = Path.Combine(new DirectoryInfo(currentDirectory).Parent.Parent.Parent.FullName, "DemoData", "Demo.png");

if (File.Exists(demoDataImage))
{
    base64String = ImageToBase64(demoDataImage);

    Contact contact = new Contact("Gerhard",base64String.Value);

    string jsonString = JsonSerializer.Serialize<Contact>(contact);

    File.WriteAllText($"{demoDataPath}\\Demo.json", jsonString);
    Console.WriteLine("JSON Datei geschrieben!");
}

if (File.Exists($"{demoDataPath}\\Demo.json"))
{
    Console.WriteLine("JSON Datei gelesen!");
    string jsonString = File.ReadAllText($"{demoDataPath}\\Demo.json");

    Contact contact = JsonSerializer.Deserialize<Contact>(jsonString);

    System.Drawing.Image photo = Base64ToImage(contact.Photo);
    photo.Save($"{demoDataPath}\\DemoSave.png", System.Drawing.Imaging.ImageFormat.Png);
    Console.WriteLine($"Image-Datei aus Base64 gelesen und unter {demoDataPath}\\DemoSave.png gespeichert");
}
```

**Hinweis:** Der Typ **Base64** ist ein eigener Custom DataTyp der ebenfalls in das Projekt mit hinzugefügt ist.
