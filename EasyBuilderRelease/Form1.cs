using System.Text;

namespace EasyBuilderRelease
{
    public partial class Form1 : Form
    {
        private readonly ReleaseRunner _releaseRunner = new();
        private readonly List<ReleaseItem> _releaseItems = [];
        private bool _isRunning;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureLogTextBoxes();
            RefreshReleaseList();
            AppendSummary("Selecione os projetos .csproj e clique em Builder Release.");
        }

        private void ConfigureLogTextBoxes()
        {
            textBox4.Text = "Log MAUI Android";
            textBox5.Text = "Log MAUI Windows";
            textBox6.Text = "Log API";
            textBox7.Text = "Log Servidor Impressao / Resumo";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddProject(ReleaseKind.MauiAndroid, textBox1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AddProject(ReleaseKind.MauiWindows, textBox2);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AddProject(ReleaseKind.Api, textBox3);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            AddProject(ReleaseKind.PrintServer, textBox8);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await ExecuteReleaseAsync(runParallel: true);
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            await ExecuteReleaseAsync(runParallel: false);
        }

        private async Task ExecuteReleaseAsync(bool runParallel)
        {
            if (_isRunning)
            {
                return;
            }

            if (_releaseItems.Count == 0)
            {
                MessageBox.Show(
                    "Adicione ao menos um projeto para gerar release.",
                    "Easy Builder Release",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            _isRunning = true;
            SetControlsEnabled(false);
            ClearLogs();

            var releaseRoot = CreateReleaseRoot();
            var logsDirectory = Path.Combine(releaseRoot, "logs");
            Directory.CreateDirectory(logsDirectory);

            AppendSummary($"Release: {releaseRoot}");
            AppendSummary(runParallel
                ? "Executando builds em paralelo..."
                : "Executando builds em fila...");

            foreach (var item in _releaseItems)
            {
                item.OutputDirectory = Path.Combine(releaseRoot, item.Kind.OutputFolderName());
                item.Status = "Aguardando";
                item.ExitCode = null;
            }

            RefreshReleaseList();

            var results = new List<ReleaseRunResult>();
            try
            {
                if (runParallel)
                {
                    var tasks = _releaseItems
                        .Select(item => Task.Run(() => RunReleaseItemAsync(item, logsDirectory)))
                        .ToArray();

                    results.AddRange(await Task.WhenAll(tasks));
                }
                else
                {
                    foreach (var item in _releaseItems)
                    {
                        results.Add(await RunReleaseItemAsync(item, logsDirectory));
                    }
                }
            }
            finally
            {
                _isRunning = false;
                SetControlsEnabled(true);
            }

            WriteManifest(releaseRoot, results);
            AppendSummary($"Manifesto: {Path.Combine(releaseRoot, "manifesto-release.txt")}");
            AppendSummary(results.All(result => result.Succeeded)
                ? "Release finalizado com sucesso."
                : "Release finalizado com falhas. Veja os logs.");
        }

        private async Task<ReleaseRunResult> RunReleaseItemAsync(ReleaseItem item, string logsDirectory)
        {
            UpdateItemStatus(item, "Executando");
            var result = await _releaseRunner.RunAsync(
                item,
                logsDirectory,
                GetLogWriter(item.Kind),
                AppendSummary);
            UpdateItemStatus(item, result.Succeeded ? "Concluido" : "Falhou");
            return result;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }

        private void removerSelecionadoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RemoveSelectedItem();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedItem();
                e.Handled = true;
            }
        }

        private void AddProject(ReleaseKind kind, TextBox targetTextBox)
        {
            if (_isRunning)
            {
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Title = $"Selecionar projeto {kind.DisplayName()}",
                Filter = "Projetos C# (*.csproj)|*.csproj",
                InitialDirectory = Directory.Exists(@"C:\Users\PEDRO\Desktop\EasyPRO")
                    ? @"C:\Users\PEDRO\Desktop\EasyPRO"
                    : Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var validation = _releaseRunner.ValidateProject(kind, dialog.FileName);
            if (!validation.IsValid)
            {
                MessageBox.Show(
                    validation.Message,
                    "Projeto invalido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var existing = _releaseItems.FirstOrDefault(item => item.Kind == kind);
            if (existing is null)
            {
                _releaseItems.Add(new ReleaseItem(kind, dialog.FileName));
            }
            else
            {
                existing.ProjectFile = dialog.FileName;
                existing.OutputDirectory = kind.OutputFolderName();
                existing.Status = "Pronto";
                existing.ExitCode = null;
            }

            targetTextBox.Text = dialog.FileName;
            RefreshReleaseList();
            AppendSummary($"{kind.DisplayName()}: projeto adicionado.");
        }

        private void RemoveSelectedItem()
        {
            if (_isRunning || listView1.SelectedItems.Count == 0)
            {
                return;
            }

            if (listView1.SelectedItems[0].Tag is not ReleaseItem item)
            {
                return;
            }

            _releaseItems.Remove(item);

            switch (item.Kind)
            {
                case ReleaseKind.MauiAndroid:
                    textBox1.Clear();
                    break;
                case ReleaseKind.MauiWindows:
                    textBox2.Clear();
                    break;
                case ReleaseKind.Api:
                    textBox3.Clear();
                    break;
                case ReleaseKind.PrintServer:
                    textBox8.Clear();
                    break;
            }

            RefreshReleaseList();
            AppendSummary($"{item.Kind.DisplayName()}: removido da lista.");
        }

        private void RefreshReleaseList()
        {
            listView1.BeginUpdate();
            try
            {
                listView1.Items.Clear();
                foreach (var item in _releaseItems)
                {
                    var listItem = new ListViewItem(item.Kind.DisplayName())
                    {
                        Tag = item
                    };
                    listItem.SubItems.Add(item.ProjectFile);
                    listItem.SubItems.Add(item.OutputDirectory);
                    listItem.SubItems.Add(item.Status);
                    listView1.Items.Add(listItem);
                }

                foreach (ColumnHeader column in listView1.Columns)
                {
                    column.Width = -2;
                }
            }
            finally
            {
                listView1.EndUpdate();
            }
        }

        private void UpdateItemStatus(ReleaseItem item, string status)
        {
            item.Status = status;
            BeginInvokeIfNeeded(RefreshReleaseList);
        }

        private Action<string> GetLogWriter(ReleaseKind kind) => kind switch
        {
            ReleaseKind.MauiAndroid => line => AppendLog(textBox4, line),
            ReleaseKind.MauiWindows => line => AppendLog(textBox5, line),
            ReleaseKind.Api => line => AppendLog(textBox6, line),
            ReleaseKind.PrintServer => line => AppendLog(textBox7, line),
            _ => AppendSummary
        };

        private void ClearLogs()
        {
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            textBox7.Clear();
        }

        private void AppendSummary(string line)
        {
            AppendLog(textBox7, $"[{DateTime.Now:HH:mm:ss}] {line}");
        }

        private void AppendLog(TextBox textBox, string line)
        {
            BeginInvokeIfNeeded(() =>
            {
                textBox.AppendText(line);
                textBox.AppendText(Environment.NewLine);
                textBox.SelectionStart = textBox.TextLength;
                textBox.ScrollToCaret();
            });
        }

        private void BeginInvokeIfNeeded(Action action)
        {
            if (IsDisposed)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(action);
            }
            else
            {
                action();
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            button1.Enabled = enabled;
            button2.Enabled = enabled;
            button3.Enabled = enabled;
            button4.Enabled = enabled;
            button5.Enabled = enabled;
            button6.Enabled = enabled;
            button7.Enabled = enabled;
            listView1.Enabled = enabled;
        }

        private string CreateReleaseRoot()
        {
            var baseDirectory = FindProjectRoot() ?? AppContext.BaseDirectory;
            var baseName = $"COMERCIO_FACIL_RELEASE_{DateTime.Now:yyyy-MM-dd_HHmmss}";
            var releaseRoot = Path.Combine(baseDirectory, baseName);

            if (!Directory.Exists(releaseRoot))
            {
                Directory.CreateDirectory(releaseRoot);
                return releaseRoot;
            }

            for (var index = 2; index <= 99; index++)
            {
                var candidate = $"{releaseRoot}_{index}";
                if (!Directory.Exists(candidate))
                {
                    Directory.CreateDirectory(candidate);
                    return candidate;
                }
            }

            throw new InvalidOperationException("Nao foi possivel criar uma pasta de release unica.");
        }

        private static string? FindProjectRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "EasyBuilderRelease.slnx")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private void WriteManifest(string releaseRoot, IReadOnlyCollection<ReleaseRunResult> results)
        {
            var manifest = Path.Combine(releaseRoot, "manifesto-release.txt");
            var builder = new StringBuilder();
            builder.AppendLine("COMERCIO FACIL - RELEASE");
            builder.AppendLine($"Gerado em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            builder.AppendLine($"Pasta: {releaseRoot}");
            builder.AppendLine();
            builder.AppendLine("Itens:");

            foreach (var result in results.OrderBy(result => result.Item.Kind))
            {
                builder.AppendLine($"- {result.Item.Kind.DisplayName()}");
                builder.AppendLine($"  Projeto: {result.Item.ProjectFile}");
                builder.AppendLine($"  Saida: {result.Item.OutputDirectory}");
                builder.AppendLine($"  Log: {result.LogFile}");
                builder.AppendLine($"  Comando: {result.CommandText}");
                builder.AppendLine($"  ExitCode: {result.ExitCode}");
                builder.AppendLine($"  Status: {(result.Succeeded ? "Concluido" : "Falhou")}");
            }

            File.WriteAllText(manifest, builder.ToString(), new UTF8Encoding(false));
        }
    }
}
