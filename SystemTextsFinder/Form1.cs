using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SystemTextsFinder
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                string filename = openFileDialog1.FileName;
                LoadFile(filename);
            }
        }

        void LoadFile(string filename)
        {
            XDocument xdoc = XDocument.Load(filename);

            IEnumerable<string> languages = xdoc
                .Descendants("Name")
                .Where(el => el.Attribute("lang") != null)
                .Select(el => el.Attribute("lang").Value)
                .Distinct()
                .OrderBy(t => t);

            dataGridView1.Columns.Clear();

            var texts = xdoc
                .Descendants("EnovaSystemText")
                .OrderBy(el => el.Element("Identifier").Value);

            DataTable dt = new DataTable();

            dt.Columns.Add("Identifier");
            foreach (string lang in languages)
            {
                dt.Columns.Add(lang);
            }


            foreach (var text in texts)
            {
                DataRow dr = dt.NewRow();
                dr["Identifier"] = text.Element("Identifier").Value;
                foreach (var lang in text.Elements("Name"))
                {
                    string l = lang.Attribute("lang").Value;
                    dr[l] = lang.Value;
                }
                dt.Rows.Add(dr);
            }

            dataGridView1.DataSource = dt;

            dataGridView1.Columns["Identifier"].Width = 250;
            foreach (string lang in languages)
            {
                dataGridView1.Columns[lang].Width = 250;
            }

            return;
        }

        private void buttonCheckIfUsed_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> foundIdentifiers = new Dictionary<string, string>();

            DataTable dt = (DataTable)dataGridView1.DataSource;
            if (dt != null)
            {
                IEnumerable<string> identifiers = dt.AsEnumerable().Select(r => (string)r["Identifier"]);

                string path = textBox1.Text;

                string[] sourcefiles =
                    Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories)
                    .Union(Directory.GetFiles(path, "*.as?x", SearchOption.AllDirectories))
                    .ToArray();

                if (MessageBox.Show("Searching " + sourcefiles.Length + " files...", "Parse files", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                long rowcount = 0;
                long matches = 0;

                foreach (string sourcefile in sourcefiles)
                {
                    string[] rows = File.ReadAllLines(sourcefile);
                    rowcount += rows.Length;

                    foreach (string row in rows)
                    {
                        foreach (string identifier in identifiers)
                        {
                            if (row.Contains(identifier))
                            {
                                if (foundIdentifiers.ContainsKey(identifier))
                                {
                                    foundIdentifiers[identifier] += ", " + sourcefile;
                                }
                                else
                                {
                                    foundIdentifiers[identifier] = sourcefile;
                                }
                                matches++;
                            }
                        }
                    }
                }

                if (!dt.Columns.Contains("InSourceCode"))
                {
                    dt.Columns.Add("InSourceCode");
                }

                int foundIdentifiersCount = 0;

                foreach (DataRow dr in dt.Rows)
                {
                    string identifier = (string)dr["Identifier"];
                    if (foundIdentifiers.ContainsKey(identifier))
                    {
                        dr["InSourceCode"] = foundIdentifiers[identifier];
                        foundIdentifiersCount++;
                    }
                    else
                    {
                        dr["InSourceCode"] = null;
                    }
                }

                MessageBox.Show(
                    "Rowcount: " + rowcount + Environment.NewLine +
                    "Matches: " + matches + Environment.NewLine +
                    "Missing identifiers: " + (dt.Rows.Count - foundIdentifiersCount) + "/" + dt.Rows.Count);
            }
        }
    }
}
