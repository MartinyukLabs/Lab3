using Lab3;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

class Program
{
    static void Main()
    {
        Third();
    }
    /////////////////////////////////////////1
    public static void First()
    {
        string csvFilePath = "transactions.csv";
        string dateFormat = "yyyy-MM-dd";
        int batchSize = 10;

        Func<string, DateTime> getDate = (line) =>
        {
            string[] fields = line.Split(',');
            return DateTime.ParseExact(fields[0], dateFormat, null);
        };

        Func<string, double> getAmount = (line) =>
        {
            string[] fields = line.Split(',');
            return double.Parse(fields[1]);
        };

        Action<DateTime, double> printTotalAmount = (date, totalAmount) =>
        {
            Console.WriteLine($"{date.ToString(dateFormat)}: {totalAmount}");
        };

        int page = 1;
        List<string> lines = File.ReadAllLines(csvFilePath).ToList();
        List<Transaction> transactions = new List<Transaction>();
        for (int i = 0; i < lines.Count; i++)
        {
            DateTime currentDay = getDate(lines[i]);
            Transaction transaction = new Transaction();
            if (!transactions.Any(tr => tr.DateTime == currentDay))
                transactions.Add(new Transaction() { DateTime = currentDay });
            transaction = transactions.FirstOrDefault(tr => tr.DateTime == currentDay);

            double amount = getAmount(lines[i]);
            if (transaction.DateTime == currentDay)
                transaction.Amount += amount;

            if (i % 10 == 0 && i != 0)
            {
                WriteCsv(csvFilePath, $"transactions{page}.csv", dateFormat, i - 10);
                page++;
            }
        }
        if (lines.Count - 1 % 10 != 0)
            WriteCsv(csvFilePath, $"transactions{page}.csv", dateFormat, (page - 1) * 10);
        foreach (var transaction in transactions)
            printTotalAmount(transaction.DateTime, transaction.Amount);
    }
    static void WriteCsv(string dataPath, string resultFilePath, string dateFormat, int start)
    {
        var lines = File.ReadAllLines(dataPath);
        int end = start + 10 > lines.Length - 1 ? lines.Length - 1 : start + 10;
        double currentTotal = 0;
        List<string> result = new List<string>();

        for (int i = start; i < end; i++)
            result.Add(lines[i]);

        File.WriteAllLines(resultFilePath, result);
    }
    /////////////////////////////////////////2
    public static void Second()
    {
        string path = "products";
        for (int i = 1; i <= 10; i++)
        {
            string fileName = path + $"{i}.json";
            if (!File.Exists(fileName))
            {
                List<Product> products = new List<Product>
                {
                    new Product { Name = "Product1", Category = "electronics", Price = 200.0m },
                    new Product { Name = "Product2", Category = "clothing", Price = 100.0m },
                    new Product { Name = "Product3", Category = "books", Price = 50.0m },
                    new Product { Name = "Product4", Category = "electronics", Price = 400.0m },
                    new Product { Name = "Product5", Category = "clothing", Price = 300.0m }
                };
                string json = JsonConvert.SerializeObject(products);
                File.WriteAllText(fileName, json);
            }
        }
        
        string categoryFilter = "electronics";
        decimal priceFilter = 250.0m;

        Func<Product, bool> filter = p => p.Category == categoryFilter && p.Price <= priceFilter;

        for (int i = 1; i <= 10; i++)
        {
            string fileName = path + $"{i}.json";
            if (File.Exists(fileName))
            {
                string json = File.ReadAllText(fileName);
                List<Product> products = JsonConvert.DeserializeObject<List<Product>>(json);
                List<Product> filteredProducts = products.Where(filter).ToList();

                Action<Product> display = p => Console.WriteLine($"{p.Name} ({p.Category}): {p.Price:C}");

                Console.WriteLine();
                Console.WriteLine($"Filtered products in {fileName}:");
                foreach (Product product in filteredProducts)
                    display(product);
            }
        }

        Console.ReadLine();
    }
    /////////////////////////////////////////3
    public static void Third()
    {
        string path = "pictures";
        List<Bitmap> images = LoadImages(path);

        Func<Bitmap, Bitmap> operation1 = InvertColors;
        Func<Bitmap, Bitmap> operation2 = AddBorder;
        Action<Bitmap> displayImage = DisplayImage;

        foreach (Bitmap image in images)
        {
            Bitmap result = operation1(image);
            result = operation2(result);
            displayImage(result);
        }
    }
    static List<Bitmap> LoadImages(string path)
    {
        List<Bitmap> images = new List<Bitmap>();
        List<string> files = Directory.GetFiles(path, "*.jpg").ToList();
        files.AddRange(Directory.GetFiles(path, "*.png"));
        foreach (string file in files)
        {
            Bitmap image = new Bitmap(file);
            images.Add(image);
        }
        return images;
    }
    static Bitmap InvertColors(Bitmap image)
    {
        Bitmap result = new Bitmap(image.Width, image.Height);
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color color = image.GetPixel(x, y);
                int red = 255 - color.R;
                int green = 255 - color.G;
                int blue = 255 - color.B;
                result.SetPixel(x, y, Color.FromArgb(red, green, blue));
            }
        }
        return result;
    }
    static Bitmap AddBorder(Bitmap image)
    {
        Bitmap result = new Bitmap(image.Width + 10, image.Height + 10);
        using (Graphics g = Graphics.FromImage(result))
        {
            g.FillRectangle(Brushes.White, 0, 0, result.Width, result.Height);
            g.DrawImage(image, new Point(5, 5));
        }
        return result;
    }
    static int[] cColors = { 0x000000, 0x000080, 0x008000, 0x008080, 0x800000, 0x800080, 0x808000, 0xC0C0C0, 0x808080, 0x0000FF, 0x00FF00, 0x00FFFF, 0xFF0000, 0xFF00FF, 0xFFFF00, 0xFFFFFF };

    public static void ConsoleWritePixel(Color cValue)
    {
        Color[] cTable = cColors.Select(x => Color.FromArgb(x)).ToArray();
        char[] rList = new char[] { (char)9617, (char)9618, (char)9619, (char)9608 }; // 1/4, 2/4, 3/4, 4/4
        int[] bestHit = new int[] { 0, 0, 4, int.MaxValue }; //ForeColor, BackColor, Symbol, Score

        for (int rChar = rList.Length; rChar > 0; rChar--)
        {
            for (int cFore = 0; cFore < cTable.Length; cFore++)
            {
                for (int cBack = 0; cBack < cTable.Length; cBack++)
                {
                    int R = (cTable[cFore].R * rChar + cTable[cBack].R * (rList.Length - rChar)) / rList.Length;
                    int G = (cTable[cFore].G * rChar + cTable[cBack].G * (rList.Length - rChar)) / rList.Length;
                    int B = (cTable[cFore].B * rChar + cTable[cBack].B * (rList.Length - rChar)) / rList.Length;
                    int iScore = (cValue.R - R) * (cValue.R - R) + (cValue.G - G) * (cValue.G - G) + (cValue.B - B) * (cValue.B - B);
                    if (!(rChar > 1 && rChar < 4 && iScore > 50000)) // rule out too weird combinations
                    {
                        if (iScore < bestHit[3])
                        {
                            bestHit[3] = iScore; //Score
                            bestHit[0] = cFore;  //ForeColor
                            bestHit[1] = cBack;  //BackColor
                            bestHit[2] = rChar;  //Symbol
                        }
                    }
                }
            }
        }
        Console.ForegroundColor = (ConsoleColor)bestHit[0];
        Console.BackgroundColor = (ConsoleColor)bestHit[1];
        Console.Write(rList[bestHit[2] - 1]);
    }


    public static void DisplayImage(Bitmap source)
    {
        int sMax = 39;
        decimal percent = Math.Min(decimal.Divide(sMax, source.Width), decimal.Divide(sMax, source.Height));
        Size dSize = new Size((int)(source.Width * percent), (int)(source.Height * percent));
        Bitmap bmpMax = new Bitmap(source, dSize.Width * 2, dSize.Height);
        for (int i = 0; i < dSize.Height; i++)
        {
            for (int j = 0; j < dSize.Width; j++)
            {
                ConsoleWritePixel(bmpMax.GetPixel(j * 2, i));
                ConsoleWritePixel(bmpMax.GetPixel(j * 2 + 1, i));
            }
            System.Console.WriteLine();
        }
        Console.ResetColor();
    }
    /////////////////////////////////////////4
    public static void Fourth()
    {

    }
}
