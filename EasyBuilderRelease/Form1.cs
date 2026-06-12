using System.Diagnostics;
using System.Text;

namespace EasyBuilderRelease
{
    public partial class Form1 : Form
    {
        private const string ReleaseFolderPrefix = "COMERCIO_FACIL_RELEASE_";
        private const string ReleaseGitPathspec = "COMERCIO_FACIL_RELEASE_*";
        private const string DefaultBlazorProjectPath = @"C:\Users\PEDRO\Desktop\EasyPRO\EasyLoginBase\EasyLoginBase.WebAppBlazor\EasyLoginBase.WebAppBlazor.csproj";
        private const string DefaultWindowsFormsProjectPath = @"C:\Users\PEDRO\Desktop\EasyPRO\EasyModuloPontoVendaDesktop\EasyModuloPontoVendaDesktop\EasyModuloPontoVendaDesktop.csproj";
        private readonly ReleaseRunner _releaseRunner = new();
        private readonly List<ReleaseItem> _releaseItems = [];
        private readonly LastReleaseStore _lastReleaseStore = new(GetLocalDataDirectory());
        private bool _isRunning;
        private bool _isGitRunning;
        private string? _lastReleaseRoot;
        private string? _gitRoot;
        private TabControl? _mainTabControl;
        private ListView? _gitCommitListView;
        private TextBox? _gitRepositoryTextBox;
        private TextBox? _gitCommitDetailsTextBox;
        private Label? _gitStatusLabel;
        private Button? _gitRefreshButton;
        private Button? _gitCheckoutButton;
        private Button? _gitCheckoutMainButton;
        private Label? _blazorLabel;
        private TextBox? _blazorProjectTextBox;
        private Button? _blazorButton;
        private Label? _windowsFormsLabel;
        private TextBox? _windowsFormsProjectTextBox;
        private Button? _windowsFormsButton;
        private TextBox? _windowsFormsLogTextBox;
        private TextBox? _blazorLogTextBox;
        private TextBox? _summaryLogTextBox;

        public Form1()
        {
            InitializeComponent();
            ConfigureMainTabs();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConfigureLogTextBoxes();
            LoadLastReleaseItems();
            LoadDefaultReleaseProjects();
            RefreshReleaseList();
            AppendSummary("Selecione os projetos .csproj e clique em Release em fila ou Release paralelo.");
            _ = RefreshGitHistoryAsync();
        }

        private void ConfigureMainTabs()
        {
            var releaseControls = Controls
                .Cast<Control>()
                .Where(control => control is not TabControl)
                .ToList();

            _mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Name = "tabControlMain"
            };

            var releaseTabPage = new TabPage
            {
                Name = "tabPageReleases",
                Text = "Releases"
            };

            var gitTabPage = new TabPage
            {
                Name = "tabPageGit",
                Text = "Git"
            };

            Controls.Clear();
            Controls.Add(_mainTabControl);
            _mainTabControl.TabPages.Add(releaseTabPage);
            _mainTabControl.TabPages.Add(gitTabPage);

            foreach (var control in releaseControls)
            {
                releaseTabPage.Controls.Add(control);
            }

            ConfigureAdditionalReleaseControls(releaseTabPage);
            ConfigureGitTab(gitTabPage);
        }

        private void ConfigureAdditionalReleaseControls(TabPage releaseTabPage)
        {
            const int left = 26;
            const int textLeft = 26;
            const int buttonLeft = 475;
            const int textWidth = 443;
            const int buttonWidth = 142;
            const int rowHeight = 51;
            const int labelTop = 31;
            const int textOffset = 18;
            const int blazorRow = 3;
            const int windowsFormsRow = 4;
            const int printServerRow = 5;

            _blazorLabel = new Label
            {
                AutoSize = true,
                Location = new Point(left, labelTop + (rowHeight * blazorRow)),
                Name = "labelBlazorWeb",
                Text = "Blazor WebApp"
            };

            _blazorProjectTextBox = new TextBox
            {
                Location = new Point(textLeft, _blazorLabel.Top + textOffset),
                Name = "textBoxBlazorWeb",
                ReadOnly = true,
                Size = new Size(textWidth, 23)
            };

            _blazorButton = new Button
            {
                Location = new Point(buttonLeft, _blazorProjectTextBox.Top),
                Name = "buttonBlazorWeb",
                Size = new Size(buttonWidth, 23),
                Text = "Adicionar Blazor",
                UseVisualStyleBackColor = true
            };
            _blazorButton.Click += (_, _) => AddProject(ReleaseKind.BlazorWeb, _blazorProjectTextBox!);

            _windowsFormsLabel = new Label
            {
                AutoSize = true,
                Location = new Point(left, labelTop + (rowHeight * windowsFormsRow)),
                Name = "labelWindowsForms",
                Text = "Windows Forms"
            };

            _windowsFormsProjectTextBox = new TextBox
            {
                Location = new Point(textLeft, _windowsFormsLabel.Top + textOffset),
                Name = "textBoxWindowsForms",
                ReadOnly = true,
                Size = new Size(textWidth, 23)
            };

            _windowsFormsButton = new Button
            {
                Location = new Point(buttonLeft, _windowsFormsProjectTextBox.Top),
                Name = "buttonWindowsForms",
                Size = new Size(buttonWidth, 23),
                Text = "Adicionar WinForms",
                UseVisualStyleBackColor = true
            };
            _windowsFormsButton.Click += (_, _) => AddProject(ReleaseKind.WindowsForms, _windowsFormsProjectTextBox!);

            label4.Location = new Point(left, labelTop + (rowHeight * printServerRow));
            textBox8.Location = new Point(textLeft, label4.Top + textOffset);
            button7.Location = new Point(buttonLeft, textBox8.Top);

            listView1.Location = new Point(left, textBox8.Bottom + 8);
            var totalHeight = Math.Max(128, button5.Top - listView1.Top - 6);
            listView1.Size = new Size(591, totalHeight - 110);

            _summaryLogTextBox = new TextBox
            {
                Location = new Point(left, listView1.Bottom + 8),
                Size = new Size(591, 102),
                Font = new Font("Consolas", 9F),
                Multiline = true,
                Name = "textBoxSummaryLog",
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                BackColor = Color.Black,
                ForeColor = Color.Lime
            };
            releaseTabPage.Controls.Add(_summaryLogTextBox);

            releaseTabPage.Controls.Add(_blazorLabel);
            releaseTabPage.Controls.Add(_blazorProjectTextBox);
            releaseTabPage.Controls.Add(_blazorButton);
            releaseTabPage.Controls.Add(_windowsFormsLabel);
            releaseTabPage.Controls.Add(_windowsFormsProjectTextBox);
            releaseTabPage.Controls.Add(_windowsFormsButton);

            ConfigureReleaseLogPanel();
        }

        private void ConfigureReleaseLogPanel()
        {
            _blazorLogTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F),
                Multiline = true,
                Name = "textBoxBlazorLog",
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false
            };

            _windowsFormsLogTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F),
                Multiline = true,
                Name = "textBoxWindowsFormsLog",
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false
            };

            tableLayoutPanel1.SuspendLayout();
            try
            {
                tableLayoutPanel1.RowCount = 6;
                tableLayoutPanel1.RowStyles.Clear();
                for (int i = 0; i < 6; i++)
                {
                    tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 6f));
                }

                tableLayoutPanel1.Controls.Clear();
                tableLayoutPanel1.Controls.Add(textBox4, 0, 0); // MAUI Android
                tableLayoutPanel1.Controls.Add(textBox5, 0, 1); // MAUI Windows
                tableLayoutPanel1.Controls.Add(textBox6, 0, 2); // API
                tableLayoutPanel1.Controls.Add(_blazorLogTextBox, 0, 3); // Blazor WebApp
                tableLayoutPanel1.Controls.Add(_windowsFormsLogTextBox, 0, 4); // Windows Forms
                tableLayoutPanel1.Controls.Add(textBox7, 0, 5); // Print Server
            }
            finally
            {
                tableLayoutPanel1.ResumeLayout();
            }
        }

        private void ConfigureGitTab(TabPage gitTabPage)
        {
            var titleLabel = new Label
            {
                AutoSize = true,
                Font = new Font(Font, FontStyle.Bold),
                Location = new Point(18, 18),
                Name = "labelGitTitle",
                Text = "Gerenciador Git"
            };

            _gitRepositoryTextBox = new TextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(18, 42),
                Name = "textBoxGitRepository",
                ReadOnly = true,
                Size = new Size(740, 23)
            };

            _gitRefreshButton = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(776, 41),
                Name = "buttonGitRefresh",
                Size = new Size(140, 25),
                Text = "Atualizar historico",
                UseVisualStyleBackColor = true
            };
            _gitRefreshButton.Click += gitRefreshButton_Click;

            _gitCheckoutButton = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Enabled = false,
                Location = new Point(922, 41),
                Name = "buttonGitCheckout",
                Size = new Size(160, 25),
                Text = "Checkout selecionado",
                UseVisualStyleBackColor = true
            };
            _gitCheckoutButton.Click += gitCheckoutButton_Click;

            _gitCheckoutMainButton = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(1088, 41),
                Name = "buttonGitCheckoutMain",
                Size = new Size(160, 25),
                Text = "Checkout main",
                UseVisualStyleBackColor = true
            };
            _gitCheckoutMainButton.Click += gitCheckoutMainButton_Click;

            _gitCommitListView = new ListView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                Location = new Point(18, 82),
                MultiSelect = false,
                Name = "listViewGitCommits",
                Size = new Size(720, 520),
                UseCompatibleStateImageBehavior = false,
                View = View.Details
            };
            _gitCommitListView.Columns.Add("Hash", 82);
            _gitCommitListView.Columns.Add("Data", 150);
            _gitCommitListView.Columns.Add("Autor", 160);
            _gitCommitListView.Columns.Add("Refs", 180);
            _gitCommitListView.Columns.Add("Mensagem", 420);
            _gitCommitListView.SelectedIndexChanged += gitCommitListView_SelectedIndexChanged;
            _gitCommitListView.DoubleClick += gitCommitListView_DoubleClick;

            _gitCommitDetailsTextBox = new TextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Consolas", 9F),
                Location = new Point(754, 82),
                Multiline = true,
                Name = "textBoxGitCommitDetails",
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Size = new Size(494, 520),
                WordWrap = false
            };

            _gitStatusLabel = new Label
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoEllipsis = true,
                Location = new Point(18, 616),
                Name = "labelGitStatus",
                Size = new Size(1230, 24),
                Text = "Carregando historico Git..."
            };

            gitTabPage.Controls.Add(titleLabel);
            gitTabPage.Controls.Add(_gitRepositoryTextBox);
            gitTabPage.Controls.Add(_gitRefreshButton);
            gitTabPage.Controls.Add(_gitCheckoutButton);
            gitTabPage.Controls.Add(_gitCheckoutMainButton);
            gitTabPage.Controls.Add(_gitCommitListView);
            gitTabPage.Controls.Add(_gitCommitDetailsTextBox);
            gitTabPage.Controls.Add(_gitStatusLabel);

            gitTabPage.Resize += (_, _) => LayoutGitTab(gitTabPage);
            LayoutGitTab(gitTabPage);
        }

        private void LayoutGitTab(TabPage gitTabPage)
        {
            if (_gitRepositoryTextBox is null
                || _gitRefreshButton is null
                || _gitCheckoutButton is null
                || _gitCheckoutMainButton is null
                || _gitCommitListView is null
                || _gitCommitDetailsTextBox is null
                || _gitStatusLabel is null)
            {
                return;
            }

            var width = Math.Max(gitTabPage.ClientSize.Width, 1000);
            var height = Math.Max(gitTabPage.ClientSize.Height, 500);
            const int margin = 18;
            const int gap = 14;
            const int buttonTop = 41;
            const int contentTop = 82;
            const int statusHeight = 24;
            const int buttonWidth = 160;
            const int refreshButtonWidth = 140;
            var contentBottom = height - margin - statusHeight - 10;
            var contentHeight = Math.Max(260, contentBottom - contentTop);
            var listWidth = Math.Min(720, Math.Max(520, (width - (margin * 2) - gap) / 2));
            var detailLeft = margin + listWidth + gap;
            var detailWidth = Math.Max(320, width - detailLeft - margin);

            _gitCheckoutMainButton.Location = new Point(width - margin - buttonWidth, buttonTop);
            _gitCheckoutMainButton.Size = new Size(buttonWidth, 25);

            _gitCheckoutButton.Location = new Point(_gitCheckoutMainButton.Left - gap - buttonWidth, buttonTop);
            _gitCheckoutButton.Size = new Size(buttonWidth, 25);

            _gitRefreshButton.Location = new Point(_gitCheckoutButton.Left - gap - refreshButtonWidth, buttonTop);
            _gitRefreshButton.Size = new Size(refreshButtonWidth, 25);

            _gitRepositoryTextBox.Location = new Point(margin, 42);
            _gitRepositoryTextBox.Size = new Size(Math.Max(260, _gitRefreshButton.Left - margin - gap), 23);

            _gitCommitListView.Location = new Point(margin, contentTop);
            _gitCommitListView.Size = new Size(listWidth, contentHeight);

            _gitCommitDetailsTextBox.Location = new Point(detailLeft, contentTop);
            _gitCommitDetailsTextBox.Size = new Size(detailWidth, contentHeight);

            _gitStatusLabel.Location = new Point(margin, height - margin - statusHeight);
            _gitStatusLabel.Size = new Size(width - (margin * 2), statusHeight);
        }

        private void ConfigureLogTextBoxes()
        {
            foreach (var textBox in GetReleaseLogTextBoxes())
            {
                StyleLogTextBox(textBox);
            }

            ResetLogTextBoxes();
        }

        private IEnumerable<TextBox> GetReleaseLogTextBoxes()
        {
            yield return textBox4;
            yield return textBox5;
            yield return textBox6;

            if (_blazorLogTextBox is not null)
            {
                yield return _blazorLogTextBox;
            }

            if (_windowsFormsLogTextBox is not null)
            {
                yield return _windowsFormsLogTextBox;
            }

            yield return textBox7;
        }

        private static void StyleLogTextBox(TextBox textBox)
        {
            textBox.BackColor = Color.Black;
            textBox.ForeColor = Color.Lime;
            textBox.Font = new Font("Consolas", 9F);
        }

        private void ResetLogTextBoxes()
        {
            ResetLog(textBox4, "Log MAUI Android");
            ResetLog(textBox5, "Log MAUI Windows");
            ResetLog(textBox6, "Log API");

            if (_blazorLogTextBox is not null)
            {
                ResetLog(_blazorLogTextBox, "Log Blazor WebApp");
            }

            if (_windowsFormsLogTextBox is not null)
            {
                ResetLog(_windowsFormsLogTextBox, "Log Windows Forms");
            }

            ResetLog(textBox7, "Log Servidor Impressao");

            if (_summaryLogTextBox is not null)
            {
                ResetLog(_summaryLogTextBox, "Resumo (Concluidos)");
            }
        }

        private static void ResetLog(TextBox textBox, string title)
        {
            textBox.Text = $"== {title} =={Environment.NewLine}";
            textBox.SelectionStart = textBox.TextLength;
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

            string releaseRoot;
            try
            {
                releaseRoot = CreateReleaseRoot();
            }
            catch (Exception ex)
            {
                _isRunning = false;
                SetControlsEnabled(true);
                AppendSummary($"Nao foi possivel preparar a pasta de release: {ex.Message}");
                return;
            }

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
            SaveLastReleaseItems(releaseRoot);
            AppendSummary($"Manifesto: {Path.Combine(releaseRoot, "manifesto-release.txt")}");
            if (results.All(result => result.Succeeded))
            {
                AppendSummary("Release finalizado com sucesso.");
                await CommitReleaseAsync(releaseRoot);
            }
            else
            {
                AppendSummary("Release finalizado com falhas. Commit nao executado.");
            }
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
            SaveLastReleaseItems();
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
                case ReleaseKind.BlazorWeb:
                    _blazorProjectTextBox?.Clear();
                    break;
                case ReleaseKind.WindowsForms:
                    _windowsFormsProjectTextBox?.Clear();
                    break;
            }

            RefreshReleaseList();
            SaveLastReleaseItems();
            AppendSummary($"{item.Kind.DisplayName()}: removido da lista.");
        }

        private void LoadLastReleaseItems()
        {
            LastReleaseSettings settings;
            try
            {
                settings = _lastReleaseStore.Load();
            }
            catch (Exception ex)
            {
                AppendSummary($"Aviso: nao foi possivel carregar o ultimo release: {ex.Message}");
                return;
            }

            _lastReleaseRoot = settings.LastReleaseRoot;

            var loaded = 0;
            var skipped = 0;

            foreach (var savedProject in settings.Projects)
            {
                if (!Enum.TryParse<ReleaseKind>(savedProject.Kind, ignoreCase: true, out var kind)
                    || string.IsNullOrWhiteSpace(savedProject.ProjectFile)
                    || _releaseItems.Any(item => item.Kind == kind))
                {
                    skipped++;
                    continue;
                }

                var validation = _releaseRunner.ValidateProject(kind, savedProject.ProjectFile);
                if (!validation.IsValid)
                {
                    skipped++;
                    AppendSummary($"{kind.DisplayName()}: projeto salvo ignorado. {validation.Message}");
                    continue;
                }

                _releaseItems.Add(new ReleaseItem(kind, savedProject.ProjectFile));
                SetProjectTextBox(kind, savedProject.ProjectFile);
                loaded++;
            }

            if (loaded > 0)
            {
                AppendSummary($"{loaded} projeto(s) carregado(s) do ultimo uso.");
            }

            if (!string.IsNullOrWhiteSpace(_lastReleaseRoot))
            {
                AppendSummary($"Ultimo release: {_lastReleaseRoot}");
            }

            if (skipped > 0)
            {
                AppendSummary($"{skipped} projeto(s) salvo(s) ignorado(s).");
            }
        }

        private void LoadDefaultReleaseProjects()
        {
            var addedAny = false;
            addedAny |= AddDefaultReleaseProject(ReleaseKind.BlazorWeb, DefaultBlazorProjectPath);
            addedAny |= AddDefaultReleaseProject(ReleaseKind.WindowsForms, DefaultWindowsFormsProjectPath);

            if (addedAny)
            {
                SaveLastReleaseItems();
            }
        }

        private bool AddDefaultReleaseProject(ReleaseKind kind, string projectFile)
        {
            if (_releaseItems.Any(item => item.Kind == kind))
            {
                return false;
            }

            if (!File.Exists(projectFile))
            {
                AppendSummary($"{kind.DisplayName()}: caminho padrao nao encontrado: {projectFile}");
                return false;
            }

            var validation = _releaseRunner.ValidateProject(kind, projectFile);
            if (!validation.IsValid)
            {
                AppendSummary($"{kind.DisplayName()}: projeto padrao ignorado. {validation.Message}");
                return false;
            }

            _releaseItems.Add(new ReleaseItem(kind, projectFile));
            SetProjectTextBox(kind, projectFile);
            AppendSummary($"{kind.DisplayName()}: projeto padrao carregado.");
            return true;
        }

        private void SaveLastReleaseItems(string? lastReleaseRoot = null)
        {
            if (!string.IsNullOrWhiteSpace(lastReleaseRoot))
            {
                _lastReleaseRoot = lastReleaseRoot;
            }

            var settings = new LastReleaseSettings
            {
                SavedAt = DateTime.Now,
                LastReleaseRoot = _lastReleaseRoot,
                Projects = _releaseItems
                    .OrderBy(item => item.Kind)
                    .Select(item => new LastReleaseProject
                    {
                        Kind = item.Kind.ToString(),
                        ProjectFile = item.ProjectFile
                    })
                    .ToList()
            };

            try
            {
                _lastReleaseStore.Save(settings);
            }
            catch (Exception ex)
            {
                AppendSummary($"Aviso: nao foi possivel salvar o ultimo release: {ex.Message}");
            }
        }

        private void SetProjectTextBox(ReleaseKind kind, string value)
        {
            switch (kind)
            {
                case ReleaseKind.MauiAndroid:
                    textBox1.Text = value;
                    break;
                case ReleaseKind.MauiWindows:
                    textBox2.Text = value;
                    break;
                case ReleaseKind.Api:
                    textBox3.Text = value;
                    break;
                case ReleaseKind.PrintServer:
                    textBox8.Text = value;
                    break;
                case ReleaseKind.BlazorWeb:
                    if (_blazorProjectTextBox is not null)
                    {
                        _blazorProjectTextBox.Text = value;
                    }
                    break;
                case ReleaseKind.WindowsForms:
                    if (_windowsFormsProjectTextBox is not null)
                    {
                        _windowsFormsProjectTextBox.Text = value;
                    }
                    break;
            }
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
            ReleaseKind.BlazorWeb => line => AppendLog(_blazorLogTextBox ?? textBox6, line),
            ReleaseKind.WindowsForms => line => AppendLog(_windowsFormsLogTextBox ?? textBox7, line),
            ReleaseKind.PrintServer => line => AppendLog(textBox7, line),
            _ => AppendSummary
        };

        private void ClearLogs()
        {
            ResetLogTextBoxes();
        }

        private void AppendSummary(string line)
        {
            if (_summaryLogTextBox is not null)
            {
                AppendLog(_summaryLogTextBox, $"[{DateTime.Now:HH:mm:ss}] {line}");
            }
            else
            {
                AppendLog(textBox7, $"[{DateTime.Now:HH:mm:ss}] {line}");
            }
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
            if (_blazorButton is not null)
            {
                _blazorButton.Enabled = enabled;
            }
            if (_windowsFormsButton is not null)
            {
                _windowsFormsButton.Enabled = enabled;
            }
            listView1.Enabled = enabled;
            SetGitControlsEnabled(enabled);
        }

        private async void gitRefreshButton_Click(object? sender, EventArgs e)
        {
            await RefreshGitHistoryAsync();
        }

        private async void gitCommitListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateGitSelectionButtons();
            await ShowSelectedCommitDetailsAsync();
        }

        private async void gitCommitListView_DoubleClick(object? sender, EventArgs e)
        {
            await CheckoutSelectedCommitAsync();
        }

        private async void gitCheckoutButton_Click(object? sender, EventArgs e)
        {
            await CheckoutSelectedCommitAsync();
        }

        private async void gitCheckoutMainButton_Click(object? sender, EventArgs e)
        {
            await CheckoutMainAsync();
        }

        private async Task RefreshGitHistoryAsync()
        {
            if (_isGitRunning)
            {
                return;
            }

            _isGitRunning = true;
            SetGitControlsEnabled(false);
            SetGitStatus("Carregando historico Git...");

            try
            {
                var gitRoot = await GetGitRootAsync(GetLocalDataDirectory());
                if (gitRoot is null)
                {
                    _gitRoot = null;
                    SetGitRepositoryText("Repositorio Git nao encontrado.");
                    SetGitDetails("Nao foi possivel encontrar um repositorio Git para este aplicativo.");
                    SetGitStatus("Repositorio Git nao encontrado.");
                    RefreshGitCommitList([], null);
                    return;
                }

                _gitRoot = gitRoot;
                SetGitRepositoryText(gitRoot);

                var branchResult = await RunGitAsync(gitRoot, ["branch", "--show-current"]);
                var headResult = await RunGitAsync(gitRoot, ["rev-parse", "HEAD"]);
                var logResult = await RunGitAsync(gitRoot, [
                    "log",
                    "--all",
                    "--date=iso-local",
                    "--pretty=format:%H%x1f%h%x1f%ad%x1f%an%x1f%D%x1f%s%x1e"
                ]);

                if (logResult.ExitCode != 0)
                {
                    SetGitDetails(logResult.Output);
                    SetGitStatus($"Falha ao carregar historico Git (exit code {logResult.ExitCode}).");
                    RefreshGitCommitList([], null);
                    return;
                }

                var commits = ParseGitLog(logResult.Output);
                var head = headResult.ExitCode == 0 ? headResult.Output.Trim() : null;
                var branch = branchResult.ExitCode == 0 && !string.IsNullOrWhiteSpace(branchResult.Output)
                    ? branchResult.Output.Trim()
                    : "HEAD destacado";

                RefreshGitCommitList(commits, head);
                SetGitStatus($"Branch: {branch} | Commits carregados: {commits.Count}");

                if (commits.Count == 0)
                {
                    SetGitDetails("Nenhum commit encontrado.");
                }
            }
            finally
            {
                _isGitRunning = false;
                SetGitControlsEnabled(!_isRunning);
            }
        }

        private void RefreshGitCommitList(IReadOnlyList<GitCommitInfo> commits, string? currentHead)
        {
            if (_gitCommitListView is null)
            {
                return;
            }

            _gitCommitListView.BeginUpdate();
            try
            {
                _gitCommitListView.Items.Clear();

                foreach (var commit in commits)
                {
                    var refs = commit.Decorations;
                    var isCurrentHead = string.Equals(commit.FullHash, currentHead, StringComparison.OrdinalIgnoreCase);
                    if (isCurrentHead && !refs.Contains("HEAD", StringComparison.OrdinalIgnoreCase))
                    {
                        refs = string.IsNullOrWhiteSpace(refs) ? "HEAD" : $"HEAD, {refs}";
                    }

                    var listItem = new ListViewItem(commit.ShortHash)
                    {
                        Tag = commit
                    };
                    listItem.SubItems.Add(commit.DateText);
                    listItem.SubItems.Add(commit.Author);
                    listItem.SubItems.Add(refs);
                    listItem.SubItems.Add(commit.Subject);

                    if (isCurrentHead)
                    {
                        listItem.Font = new Font(_gitCommitListView.Font, FontStyle.Bold);
                    }

                    _gitCommitListView.Items.Add(listItem);
                }

                foreach (ColumnHeader column in _gitCommitListView.Columns)
                {
                    column.Width = -2;
                }
            }
            finally
            {
                _gitCommitListView.EndUpdate();
            }

            UpdateGitSelectionButtons();
        }

        private async Task ShowSelectedCommitDetailsAsync()
        {
            var commit = GetSelectedGitCommit();
            if (commit is null)
            {
                return;
            }

            var gitRoot = await EnsureGitRootAsync();
            if (gitRoot is null)
            {
                return;
            }

            SetGitDetails("Carregando detalhes do commit...");
            var result = await RunGitAsync(gitRoot, [
                "show",
                "--stat",
                "--name-status",
                "--format=fuller",
                "--no-ext-diff",
                commit.FullHash
            ]);

            SetGitDetails(result.ExitCode == 0
                ? result.Output
                : $"Falha ao carregar detalhes do commit.\r\n\r\n{result.Output}");
        }

        private async Task CheckoutSelectedCommitAsync()
        {
            if (_isRunning || _isGitRunning)
            {
                return;
            }

            var commit = GetSelectedGitCommit();
            if (commit is null)
            {
                MessageBox.Show(
                    "Selecione um commit no historico.",
                    "Easy Builder Release",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var gitRoot = await EnsureGitRootAsync();
            if (gitRoot is null || !await EnsureCleanGitWorktreeAsync(gitRoot))
            {
                return;
            }

            var confirmation = MessageBox.Show(
                $"Fazer checkout do commit {commit.ShortHash}?\r\n\r\n{commit.Subject}\r\n\r\nO repositorio ficara em HEAD destacado.",
                "Confirmar checkout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            await RunGitCheckoutAsync(gitRoot, ["checkout", "--detach", commit.FullHash], $"Checkout realizado: {commit.ShortHash}");
        }

        private async Task CheckoutMainAsync()
        {
            if (_isRunning || _isGitRunning)
            {
                return;
            }

            var gitRoot = await EnsureGitRootAsync();
            if (gitRoot is null || !await EnsureCleanGitWorktreeAsync(gitRoot))
            {
                return;
            }

            var confirmation = MessageBox.Show(
                "Fazer checkout da branch main?",
                "Confirmar checkout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            await RunGitCheckoutAsync(gitRoot, ["checkout", "main"], "Checkout realizado: main");
        }

        private async Task RunGitCheckoutAsync(string gitRoot, IReadOnlyList<string> arguments, string successMessage)
        {
            _isGitRunning = true;
            SetGitControlsEnabled(false);
            SetGitStatus("Executando checkout...");

            try
            {
                var result = await RunGitAsync(gitRoot, arguments);
                SetGitDetails(result.Output);

                if (result.ExitCode == 0)
                {
                    AppendSummary($"Git: {successMessage}.");
                    SetGitStatus(successMessage);
                }
                else
                {
                    AppendSummary($"Git: checkout falhou (exit code {result.ExitCode}).");
                    SetGitStatus($"Checkout falhou (exit code {result.ExitCode}).");
                }
            }
            finally
            {
                _isGitRunning = false;
                SetGitControlsEnabled(!_isRunning);
            }

            await RefreshGitHistoryAsync();
        }

        private async Task<bool> EnsureCleanGitWorktreeAsync(string gitRoot)
        {
            var statusResult = await RunGitAsync(gitRoot, ["status", "--porcelain"]);
            if (statusResult.ExitCode != 0)
            {
                SetGitDetails(statusResult.Output);
                SetGitStatus($"Nao foi possivel verificar status Git (exit code {statusResult.ExitCode}).");
                return false;
            }

            if (string.IsNullOrWhiteSpace(statusResult.Output))
            {
                return true;
            }

            SetGitDetails($"Existem alteracoes pendentes. Commit ou descarte antes de fazer checkout.\r\n\r\n{statusResult.Output}");
            SetGitStatus("Checkout bloqueado: existem alteracoes pendentes.");

            MessageBox.Show(
                "Existem alteracoes pendentes no repositorio. Faca commit, push ou descarte essas alteracoes antes de fazer checkout.",
                "Checkout bloqueado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        private async Task<string?> EnsureGitRootAsync()
        {
            if (!string.IsNullOrWhiteSpace(_gitRoot) && Directory.Exists(_gitRoot))
            {
                return _gitRoot;
            }

            _gitRoot = await GetGitRootAsync(GetLocalDataDirectory());
            if (_gitRoot is null)
            {
                SetGitStatus("Repositorio Git nao encontrado.");
            }

            return _gitRoot;
        }

        private GitCommitInfo? GetSelectedGitCommit()
        {
            if (_gitCommitListView is null || _gitCommitListView.SelectedItems.Count == 0)
            {
                return null;
            }

            return _gitCommitListView.SelectedItems[0].Tag as GitCommitInfo;
        }

        private void SetGitControlsEnabled(bool enabled)
        {
            var canUse = enabled && !_isRunning && !_isGitRunning;

            if (_gitRefreshButton is not null)
            {
                _gitRefreshButton.Enabled = canUse;
            }

            if (_gitCheckoutMainButton is not null)
            {
                _gitCheckoutMainButton.Enabled = canUse;
            }

            if (_gitCommitListView is not null)
            {
                _gitCommitListView.Enabled = canUse;
            }

            UpdateGitSelectionButtons();
        }

        private void UpdateGitSelectionButtons()
        {
            if (_gitCheckoutButton is null)
            {
                return;
            }

            _gitCheckoutButton.Enabled = !_isRunning
                && !_isGitRunning
                && GetSelectedGitCommit() is not null;
        }

        private void SetGitRepositoryText(string text)
        {
            if (_gitRepositoryTextBox is not null)
            {
                _gitRepositoryTextBox.Text = text;
            }
        }

        private void SetGitDetails(string text)
        {
            if (_gitCommitDetailsTextBox is null)
            {
                return;
            }

            _gitCommitDetailsTextBox.Text = text;
            _gitCommitDetailsTextBox.SelectionStart = 0;
            _gitCommitDetailsTextBox.ScrollToCaret();
        }

        private void SetGitStatus(string text)
        {
            if (_gitStatusLabel is not null)
            {
                _gitStatusLabel.Text = text;
            }
        }

        private static List<GitCommitInfo> ParseGitLog(string output)
        {
            var commits = new List<GitCommitInfo>();
            var records = output.Split('\u001e', StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawRecord in records)
            {
                var record = rawRecord.Trim('\r', '\n');
                if (string.IsNullOrWhiteSpace(record))
                {
                    continue;
                }

                var fields = record.Split('\u001f');
                if (fields.Length < 6)
                {
                    continue;
                }

                commits.Add(new GitCommitInfo(
                    fields[0],
                    fields[1],
                    fields[2],
                    fields[3],
                    fields[4],
                    fields[5]));
            }

            return commits;
        }

        private string CreateReleaseRoot()
        {
            var baseDirectory = FindProjectRoot() ?? AppContext.BaseDirectory;
            var removedReleaseCount = DeleteExistingReleaseDirectories(baseDirectory);
            if (removedReleaseCount > 0)
            {
                AppendSummary($"{removedReleaseCount} pasta(s) de release antiga(s) removida(s).");
            }

            var baseName = $"{ReleaseFolderPrefix}{DateTime.Now:yyyy-MM-dd_HHmmss}";
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

        private static int DeleteExistingReleaseDirectories(string baseDirectory)
        {
            if (!Directory.Exists(baseDirectory))
            {
                return 0;
            }

            var releaseDirectories = Directory
                .EnumerateDirectories(baseDirectory, $"{ReleaseFolderPrefix}*", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var releaseDirectory in releaseDirectories)
            {
                Directory.Delete(releaseDirectory, recursive: true);
            }

            return releaseDirectories.Count;
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

        private static string GetLocalDataDirectory()
        {
            return FindProjectRoot() ?? AppContext.BaseDirectory;
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

        private async Task CommitReleaseAsync(string releaseRoot)
        {
            var gitRoot = await GetGitRootAsync(releaseRoot);
            if (gitRoot is null)
            {
                AppendSummary("Commit nao executado: pasta de release nao esta dentro de um repositorio Git.");
                return;
            }

            var message = $"EasyPro Releases {DateTime.Now:dd-MM-yyyy}";

            AppendSummary($"Git add: {ReleaseGitPathspec}");
            var addResult = await RunGitAsync(gitRoot, ["add", "-A", "-f", "--", ReleaseGitPathspec]);
            AppendSummary(addResult.Output);

            if (addResult.ExitCode != 0)
            {
                AppendSummary($"Commit nao executado: git add falhou (exit code {addResult.ExitCode}).");
                return;
            }

            var diffResult = await RunGitAsync(gitRoot, ["diff", "--cached", "--quiet", "--", ReleaseGitPathspec]);
            if (diffResult.ExitCode == 0)
            {
                AppendSummary("Commit nao executado: nao ha arquivos novos/alterados no release.");
                return;
            }

            if (diffResult.ExitCode > 1)
            {
                AppendSummary($"Commit nao executado: git diff falhou (exit code {diffResult.ExitCode}).");
                AppendSummary(diffResult.Output);
                return;
            }

            AppendSummary($"Git commit: {message}");
            var commitResult = await RunGitAsync(gitRoot, ["commit", "-m", message, "--", ReleaseGitPathspec]);
            AppendSummary(commitResult.Output);

            if (commitResult.ExitCode != 0)
            {
                AppendSummary($"Commit falhou (exit code {commitResult.ExitCode}).");
                return;
            }

            AppendSummary("Commit criado com sucesso.");
            AppendSummary("Git push: enviando commits para o remoto configurado...");

            var pushResult = await RunGitAsync(gitRoot, ["push"]);
            AppendSummary(pushResult.Output);
            AppendSummary(pushResult.ExitCode == 0
                ? "Push concluido com sucesso."
                : $"Push falhou (exit code {pushResult.ExitCode}).");
        }

        private static async Task<string?> GetGitRootAsync(string startDirectory)
        {
            var result = await RunGitAsync(startDirectory, ["rev-parse", "--show-toplevel"]);
            if (result.ExitCode != 0)
            {
                return null;
            }

            return result.Output
                .Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()
                ?.Trim();
        }

        private static async Task<GitResult> RunGitAsync(string workingDirectory, IReadOnlyList<string> arguments)
        {
            var output = new StringBuilder();
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            try
            {
                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var standardOutput = process.StandardOutput.ReadToEndAsync();
                var standardError = process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                output.Append(await standardOutput);
                output.Append(await standardError);

                return new GitResult(process.ExitCode, output.ToString().Trim());
            }
            catch (Exception ex)
            {
                return new GitResult(-1, ex.Message);
            }
        }

        private sealed record GitResult(int ExitCode, string Output);

        private sealed record GitCommitInfo(
            string FullHash,
            string ShortHash,
            string DateText,
            string Author,
            string Decorations,
            string Subject);
    }
}
