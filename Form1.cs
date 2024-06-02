using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace ChatGPTAppSqlServerAdvWithQuery
{
    public partial class Form1 : Form
    {
        private readonly ChatGPTService _chatGPTService;

        public Form1()
        {
            InitializeComponent();

            // قراءة مفتاح API وسلسلة الاتصال من App.config
            string apiKey = ConfigurationManager.AppSettings["OpenAI_API_Key"];
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            _chatGPTService = new ChatGPTService(apiKey, connectionString);
        }

        private async void buttonSend_Click(object sender, EventArgs e)
        {
            string prompt = textBoxPrompt.Text;
            string response = await _chatGPTService.SendMessageAsync(prompt);
            textBoxResponse.Text = response;
        }

        private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isOnline = comboBoxMode.SelectedItem.ToString() == "Online";
            _chatGPTService.SetMode(isOnline);
        }
    }
}
