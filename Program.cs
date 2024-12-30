using Newtonsoft.Json;
using Timer = System.Windows.Forms.Timer;

namespace CryptoTicker {
	public class MainForm : Form {
		private const int CardHeight = 90;
		private const int CardMargin = 5;
		private const int PaddingTop = 35;
		private const int PaddingBottom = 45;
		private const int PaddingLeft = 10;
		private const int PaddingRight = 25;
		private const int MenuHeight = 30;
		private const int FormWidth = 350;

		private FlowLayoutPanel cryptoPanel;
		private Timer refreshTimer;
		private readonly string configPath = "config.json";
		private string[] tokens;
		private ToolStripMenuItem topMostMenuItem;

		public MainForm() {
			InitializeConfig();
			InitializeComponents();
			InitializeCryptoCards();
			RefreshPricesAsync();
		}

		private void InitializeConfig() {
			if (!File.Exists(configPath)) {
				var defaultConfig = new { Tokens = new[] { "bitcoin", "ethereum", "solana" } };
				File.WriteAllText(configPath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
			}

			string configContent = File.ReadAllText(configPath);
			dynamic? config = JsonConvert.DeserializeObject<dynamic>(configContent);
			tokens = config.Tokens.ToObject<string[]>();
		}

		private void InitializeComponents() {
			this.Text = "Crypto Ticker";
			this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - FormWidth, 0);
			this.Size = new Size(FormWidth, 400); // Initial size, will adjust dynamically
			this.BackColor = Color.FromArgb(30, 30, 30);

			MenuStrip menuStrip = new MenuStrip();
			ToolStripMenuItem settingsMenu = new ToolStripMenuItem("Settings") {
				ForeColor = Color.LightGray
			};
			topMostMenuItem = new ToolStripMenuItem("Always on Top", null, ToggleTopMost) { CheckOnClick = true };
			settingsMenu.DropDownItems.Add(topMostMenuItem);
			menuStrip.Items.Add(settingsMenu);
			this.MainMenuStrip = menuStrip;
			this.Controls.Add(menuStrip);
			this.MainMenuStrip.BackColor = Color.FromArgb(50, 50, 50);
			this.MainMenuStrip.ForeColor = Color.White;

			this.TopMost = topMostMenuItem.Checked;

			cryptoPanel = new FlowLayoutPanel {
				Dock = DockStyle.Fill,
				FlowDirection = FlowDirection.TopDown,
				WrapContents = false,
				Padding = new Padding(PaddingLeft, PaddingTop, PaddingRight, PaddingBottom),
				Location = new Point(0, menuStrip.Height),
				BackColor = Color.FromArgb(40, 40, 40),
				AutoScroll = false
			};

			this.Controls.Add(cryptoPanel);

			refreshTimer = new Timer { Interval = 1000 * 60 };
			refreshTimer.Tick += async (s, e) => await RefreshPricesAsync();
			refreshTimer.Start();
		}

		private async Task RefreshPricesAsync() {
			try {
				foreach (Control control in cryptoPanel.Controls) {
					if (control is Panel panel && panel.Tag is CryptoData tokenData) {
						UpdateCryptoCard(panel, await FetchCryptoPriceAsync(tokenData.Name));
					}
				}
			} catch (Exception ex) {
				cryptoPanel.Controls.Clear();

				CryptoData[] placeholderTokens = new[] {
					new CryptoData { Name = "bitcoin", Price = 98000, Change = 5.3m },
					new CryptoData { Name = "ethereum", Price = 3500, Change = -2.1m },
					new CryptoData { Name = "solana", Price = 150, Change = 3.8m }
				};

				foreach (CryptoData? token in placeholderTokens) {
					cryptoPanel.Controls.Add(CreateCryptoCard(token));
				}

				cryptoPanel.Controls.Add(new Label {
					Text = $"Error fetching prices\nUsing placeholder data for now.\nError: {ex.Message}",
					AutoSize = true,
					ForeColor = Color.Red,
					Location = new Point(10, 10)
				});
			}
		}

		private async Task<CryptoData> FetchCryptoPriceAsync(string token) {
			string apiUrl = $"https://api.coingecko.com/api/v3/simple/price?ids={token}&vs_currencies=usd&include_24hr_change=true";

			using HttpClient client = new HttpClient();
			string response = await client.GetStringAsync(apiUrl);
			dynamic? data = JsonConvert.DeserializeObject<dynamic>(response);

			return new CryptoData {
				Name = token,
				Price = (decimal)data[token].usd,
				Change = (decimal)data[token].usd_24h_change
			};
		}

		private void UpdateCryptoCard(Panel panel, CryptoData token) {
			foreach (Control control in panel.Controls) {
				if (control is Label label) {
					if (label.Tag?.ToString() == "price") {
						label.Text = $"Price: ${token.Price:F2}";
					} else if (label.Tag?.ToString() == "change") {
						label.Text = $"24h Change: {token.Change:+0.00;-0.00}% (24h)";
						label.ForeColor = token.Change >= 0 ? Color.Green : Color.Red;
					}
				}
			}
		}

		private void InitializeCryptoCards() {
			foreach (string token in tokens) {
				CryptoData tokenData = new CryptoData { Name = token };
				cryptoPanel.Controls.Add(CreateCryptoCard(tokenData));
			}

			AdjustFormHeight();
		}

		private void AdjustFormHeight() {
			int totalHeight = (tokens.Length * CardHeight) + PaddingTop + PaddingBottom + MenuHeight;

			this.Size = new Size(FormWidth, totalHeight);
			this.MinimumSize = new Size(FormWidth, totalHeight);
		}

		private Panel CreateCryptoCard(CryptoData token) {
			Panel panel = new Panel {
				Size = new Size(FormWidth - PaddingLeft - PaddingRight, CardHeight),
				Margin = new Padding(0, CardMargin, 0, CardMargin),
				BorderStyle = BorderStyle.FixedSingle,
				Tag = token,
				BackColor = Color.FromArgb(50, 50, 50)
			};

			Label nameLabel = new Label {
				Text = char.ToUpper(token.Name[0]) + token.Name.Substring(1),
				Font = new Font("Arial", 12, FontStyle.Bold),
				Location = new Point(10, 10),
				AutoSize = true,
				ForeColor = Color.White
			};

			Label priceLabel = new Label {
				Text = $"Price: ${token.Price:F2}",
				Font = new Font("Arial", 10, FontStyle.Regular),
				Location = new Point(10, 40),
				AutoSize = true,
				Tag = "price",
				ForeColor = Color.White
			};

			Label changeLabel = new Label {
				Text = $"24h Change: {token.Change:+0.00;-0.00}% (24h)",
				Font = new Font("Arial", 10, FontStyle.Regular),
				ForeColor = token.Change >= 0 ? Color.Green : Color.Red,
				Location = new Point(10, 60),
				AutoSize = true,
				Tag = "change"
			};

			panel.Controls.Add(nameLabel);
			panel.Controls.Add(priceLabel);
			panel.Controls.Add(changeLabel);

			return panel;
		}

		private void ToggleTopMost(object sender, EventArgs e) {
			this.TopMost = topMostMenuItem.Checked;
		}

		[STAThread]
		private static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		private class CryptoData {
			public string Name { get; set; }
			public decimal Price { get; set; }
			public decimal Change { get; set; }
		}
	}
}