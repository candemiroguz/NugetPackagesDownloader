using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using CsvHelper;
using System.Formats.Asn1;

class Program
{
    static async Task Main()
    {
        try
        {
            var client = new HttpClient();
            int take = 100; // Everytime take a 100
            int skip = 0;   // Start Count
            bool hasMoreData = true;
            List<string> allPackages = new List<string>();

            Console.WriteLine(" NuGet paketleri listeleniyor...");

            while (hasMoreData)
            {
                string queryUrl = $"https://api-v2v3search-0.nuget.org/query?take={take}&skip={skip}";
                Console.WriteLine($"Fetch Url: {queryUrl}");

                HttpResponseMessage baseResponse = await client.GetAsync(queryUrl);
                if (!baseResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($" Hata: {baseResponse.StatusCode}");
                    break;
                }

                string baseJsonResponse = await baseResponse.Content.ReadAsStringAsync();
                using JsonDocument baseDoc = JsonDocument.Parse(baseJsonResponse);
                JsonElement baseData = baseDoc.RootElement.GetProperty("data");

                int count = 0;
                foreach (JsonElement package in baseData.EnumerateArray())
                {
                    var packages = package.GetProperty("versions");

                    foreach (var version in packages.EnumerateArray())
                    {
                        using JsonDocument versionData = JsonDocument.Parse(version.ToString());
                        JsonElement idDoc = versionData.RootElement.GetProperty("@id");
                        JsonElement versionName = versionData.RootElement.GetProperty("version");
                        var idUrl = idDoc.ToString();

                        HttpResponseMessage contentResponse = await client.GetAsync(idUrl);
                        if (!contentResponse.IsSuccessStatusCode)
                        {
                            Console.WriteLine($" Hata: {contentResponse.StatusCode}");
                            continue;
                        }

                        string contextJsonResponse = await contentResponse.Content.ReadAsStringAsync();
                        using JsonDocument contentDoc = JsonDocument.Parse(contextJsonResponse);
                        JsonElement contentData = contentDoc.RootElement.GetProperty("packageContent");

                        string packageContentUrl = contentData.ToString();

                        allPackages.Add(packageContentUrl);
                        count++;

                        Console.WriteLine($" {packageContentUrl} listeye eklendi");
                    }
                }

                if (count < take)
                {
                    hasMoreData = false;
                }
                else
                {
                    skip += take;
                }
            }

            // CSV'ye Kaydet
            string csvFilePath = "nupkg_list.csv";
            using (var writer = new StreamWriter(csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField("Package URL");
                csv.NextRecord();

                foreach (var url in allPackages)
                {
                    csv.WriteField(url);
                    csv.NextRecord();
                }
            }
            Console.WriteLine($" {csvFilePath} dosyası oluşturuldu!");

            //  İndirme klasörü oluştur
            string downloadDirectoryBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "NugetPackages");
            if (!Directory.Exists(downloadDirectoryBase))
            {
                Directory.CreateDirectory(downloadDirectoryBase);
            }

            Console.WriteLine(" NuGet paketleri indiriliyor...");

            //  Paketleri indir
            using (HttpClient downloadClient = new HttpClient())
            {
                foreach (var url in allPackages)
                {
                    string packageName = url.Split("/")[4];
                    string versionName = url.Split("/")[5];
                    string downloadDirectory = Path.Combine(downloadDirectoryBase, packageName, versionName);
                    string fileName = Path.GetFileName(url);
                    string filePath = Path.Combine(downloadDirectory, fileName);

                    Console.WriteLine($" İndiriliyor: {url}");

                    try
                    {
                        byte[] fileBytes = await downloadClient.GetByteArrayAsync(url);
                        await File.WriteAllBytesAsync(filePath, fileBytes);
                        Console.WriteLine($" Kaydedildi: {filePath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" Hata: {ex.Message}");
                    }
                }
            }

        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($" HTTP Hatası: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Genel Hata: {ex.Message}");
        }

        Console.WriteLine(" Tüm paketler indirildi!");
    }
}
