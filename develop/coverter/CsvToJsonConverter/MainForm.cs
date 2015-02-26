using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvToJsonConverter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;

        }

        string JsonSafe(string src)
        {
            return src.Replace("'", "\\'");
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            var pathes = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            var path = pathes[0];
            var file = System.IO.File.ReadAllBytes(path);
            var text = System.Text.UTF8Encoding.UTF8.GetString(file);
            var csv = CSVReader.SplitCsvGrid(text);

            try
            {
                var builder = new StringBuilder();
                for(int i=1 ; i<csv.GetLength(1); ++i)
                {
                    var cell = csv[0, i];
                    if (string.IsNullOrEmpty(cell))
                    {
                        continue;
                    }

                    // ファイル名を分解
                    var filename = System.IO.Path.GetFileNameWithoutExtension(cell);

                    // 括弧とじ検索
                    var pivot = filename.IndexOf("）");
                    var head = filename.Substring(0, pivot);
                    var title = filename.Substring(pivot + 1);

                    var spacePosition = head.LastIndexOf(" ");
                    var productName = head.Substring(1, spacePosition - 1);
                    var categoryName = head.Substring(spacePosition + 1);

                    var artist = csv[1, i];
                    var year = csv[2, i];

                    builder.Append("{title:'");
                    builder.Append(JsonSafe(title));
                    builder.Append("', product:'");
                    builder.Append(JsonSafe(productName));
                    builder.Append("', category:'");
                    builder.Append(JsonSafe(categoryName));
                    builder.Append("', artist:'");
                    builder.Append(JsonSafe(artist));
                    builder.Append("', year:'");
                    builder.Append(JsonSafe(year));
                    builder.Append("'}, \r\n");
                }

                var directory =System.IO.Path.GetDirectoryName(path);
                var exportFilename = System.IO.Path.GetFileNameWithoutExtension(path) + "_json.txt";
                var exportPath = directory + System.IO.Path.DirectorySeparatorChar + exportFilename;

                var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(builder.ToString());
                System.IO.File.WriteAllBytes(exportPath, bytes);

                MessageBox.Show(
                    exportPath,
                    "完了",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "エラーが発生しました",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                System.Console.WriteLine(ex.Message);
            }
        }
    }
}
