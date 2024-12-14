using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using iTextSharp.text.pdf;
using Xceed.Words.NET;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;
using QRCoder;

namespace Baymatov
{
    public partial class Form1 : Form
    {
        private BookCollection bookCollection = new BookCollection();
        private string currentUser;
        public Form1(string user)
        {
            InitializeComponent();
            currentUser = user;
            UpdateButtonVisibility();
        }

        public class BookCollection
        {
            private List<Book> books = new List<Book>();

            public void AddBook(Book book)
            {
                books.Add(book);
            }

            public void RemoveBook(Guid id)
            {
                var book = books.FirstOrDefault(b => b.Id == id);
                if (book != null)
                {
                    books.Remove(book);
                }
            }

            public void AddBookFromPdf(string filePath, string title, string author, int year)
            {
                Book newBook = new Book(title, author, year, filePath);
                books.Add(newBook);
            }

            public void AddBookFromDocx(string filePath, string title, string author, int year)
            {
                Book newBook = new Book(title, author, year, filePath);
                books.Add(newBook);
            }

            public void ConvertBookToPdf(Book book, string outputFilePath)
            {
                using (FileStream stream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Document doc = new Document();
                    PdfWriter.GetInstance(doc, stream);
                    doc.Open();
                    doc.Add(new Paragraph($"Title: {book.Title}"));
                    doc.Add(new Paragraph($"Author: {book.Author}"));
                    doc.Add(new Paragraph($"Year: {book.Year}"));
                    doc.Close();
                }
            }

            public void ConvertBookToDocx(Book book, string outputFilePath)
            {
                using (DocX document = DocX.Create(outputFilePath))
                {
                    document.InsertParagraph($"Title: {book.Title}");
                    document.InsertParagraph($"Author: {book.Author}");
                    document.InsertParagraph($"Year: {book.Year}");
                    document.Save();
                }
            }




            public List<Book> SearchBooks(string query)
            {
                return books.Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || b.Author.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            public List<Book> GetAllBooks()
            {
                return books;
            }

            public void ImportFromFile(string filePath)
            {
                string json = File.ReadAllText(filePath);
                var importedBooks = JsonConvert.DeserializeObject<List<Book>>(json);
                if (importedBooks != null)
                {
                    books.AddRange(importedBooks);
                }
            }


            public void ExportToFile(string filePath)
            {
                string json = JsonConvert.SerializeObject(books, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }

        }



        public class Book
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public int Year { get; set; }
            public string FilePath { get; set; }

            public Book(string title, string author, int year, string filePath)
            {
                Id = Guid.NewGuid();
                Title = title;
                Author = author;
                Year = year;
                FilePath = filePath;
            }

            public override string ToString()
            {
                return $"{Title} by {Author} ({Year})";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Добавление новой книги
            string input = textBox1.Text;
            string[] parts = input.Split(',');
            if (parts.Length == 3)
            {
                string title = parts[0].Trim();
                string author = parts[1].Trim();
                if (int.TryParse(parts[2].Trim(), out int year))
                {
                    string filePath = ""; // Добавьте путь к файлу или оставьте пустым, если он не требуется
                    Book newBook = new Book(title, author, year, filePath);
                    bookCollection.AddBook(newBook);
                    UpdateListBox();
                    ClearInputFields();
                }
                else
                {
                    MessageBox.Show("Неверный формат года.");
                }
            }
            else
            {
                MessageBox.Show("Введите данные в формате: Название, Автор, Год");
            }
        }


        private void UpdateListBox()
        {
            listBox1.Items.Clear();
            foreach (var book in bookCollection.GetAllBooks())
            {
                listBox1.Items.Add(book);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Этот метод можно использовать для отладки
            if (listBox1.SelectedItem is Book selectedBook)
            {
                Console.WriteLine($"Выбранная книга: {selectedBook.Title} от {selectedBook.Author}");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            // Удаление книги по идентификатору
            if (listBox1.SelectedItem is Book selectedBook)
            {
                MessageBox.Show($"Удаление книги: {selectedBook.Title} от {selectedBook.Author}");
                bookCollection.RemoveBook(selectedBook.Id);
                UpdateListBox();
            }
            else
            {
                MessageBox.Show("Выберите книгу для удаления.");
            }
        }

        private void ClearInputFields()
        {
            textBox1.Clear();
            textBox2.Clear();
        }
        private void UpdateButtonVisibility()
        {
            if (currentUser == "user")
            {
                button2.Visible = false;
            }
        }
        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {
            {
                // Поиск книги по названию или автору
                string query = textBox2.Text;
                var results = bookCollection.SearchBooks(query);
                listBox1.Items.Clear();
                foreach (var book in results)
                {
                    listBox1.Items.Add(book);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    bookCollection.ExportToFile(saveFileDialog.FileName);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    bookCollection.ImportFromFile(openFileDialog.FileName);
                    UpdateListBox();
                }
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string title = "Title"; // Замените на реальное значение
                    string author = "Author"; // Замените на реальное значение
                    int year = 2023; // Замените на реальное значение
                    bookCollection.AddBookFromPdf(openFileDialog.FileName, title, author, year);
                    UpdateListBox();
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "DOCX files (*.docx)|*.docx|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string title = "Title"; // Замените на реальное значение
                    string author = "Author"; // Замените на реальное значение
                    int year = 2023; // Замените на реальное значение
                    bookCollection.AddBookFromDocx(openFileDialog.FileName, title, author, year);
                    UpdateListBox();
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is Book selectedBook)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        bookCollection.ConvertBookToPdf(selectedBook, saveFileDialog.FileName);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для конвертации.");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is Book selectedBook)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "DOCX files (*.docx)|*.docx|All files (*.*)|*.*";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        bookCollection.ConvertBookToDocx(selectedBook, saveFileDialog.FileName);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для конвертации.");
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem is Book selectedBook)
            {
                string bookInfo = $"Title: {selectedBook.Title}\nAuthor: {selectedBook.Author}\nYear: {selectedBook.Year}";
                using (QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator())
                {
                    QRCoder.QRCodeData qrCodeData = qrGenerator.CreateQrCode(bookInfo, QRCoder.QRCodeGenerator.ECCLevel.Q);
                    using (QRCoder.PngByteQRCode qrCode = new QRCoder.PngByteQRCode(qrCodeData))
                    {
                        byte[] qrCodeBytes = qrCode.GetGraphic(20);
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(qrCodeBytes))
                        {
                            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(ms))
                            {
                                using (Form qrForm = new Form())
                                {
                                    qrForm.Text = "QR Code";
                                    qrForm.Width = bitmap.Width + 20;
                                    qrForm.Height = bitmap.Height + 40;
                                    PictureBox pictureBox = new PictureBox
                                    {
                                        Image = bitmap,
                                        SizeMode = PictureBoxSizeMode.AutoSize,
                                        Location = new System.Drawing.Point(10, 10)
                                    };
                                    qrForm.Controls.Add(pictureBox);
                                    qrForm.ShowDialog();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для генерации QR-кода.");
            }
        }
    }
}
