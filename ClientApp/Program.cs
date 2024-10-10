using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FileTransferClient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private const string baseUrl = "https://localhost:7108/api/File";

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            while (true)
            {
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1. Загрузить файл");
                Console.WriteLine("2. Скачивать файл");
                Console.WriteLine("3. Выйти");
                Console.Write("Ваш выбор: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await UploadFile();
                        break;
                    case "2":
                        await DownloadFile();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Некорректный выбор. Попробуйте снова.");
                        break;
                }
            }
        }

        private static async Task UploadFile()
        {
            Console.Write("Введите путь к файлу для загрузки: ");
            var filePath = Console.ReadLine();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл не найден. Пожалуйста, проверьте путь.");
                return;
            }

            try
            {
                using (var form = new MultipartFormDataContent())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var streamContent = new StreamContent(fileStream);
                        form.Add(streamContent, "file", Path.GetFileName(filePath));

                        var response = await client.PostAsync($"{baseUrl}/upload", form);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Файл успешно загружен.");
                        }
                        else
                        {
                            Console.WriteLine($"Ошибка при загрузке файла: {response.ReasonPhrase}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при загрузке файла: {ex.Message}");
            }
        }

        private static async Task DownloadFile()
        {
            Console.Write("Введите имя файла для скачивания: ");
            var fileName = Console.ReadLine();

            try
            {
                var response = await client.GetAsync($"{baseUrl}/download/{fileName}");

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    var savePath = Path.Combine(@"C:\Users\Sun\Downloads", fileName);

                    // Асинхронное сохранение файла
                    using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await fileStream.WriteAsync(data, 0, data.Length);
                    }
                    Console.WriteLine($"Файл {fileName} успешно скачан и сохранен в текущей директории.");
                }
                else
                {
                    Console.WriteLine($"Ошибка при скачивании файла: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при скачивании файла: {ex.Message}");
            }
        }
    }
}
