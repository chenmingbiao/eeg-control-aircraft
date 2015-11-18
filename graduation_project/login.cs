using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Web.Script.Serialization;


namespace graduation_project
{
    public partial class login : Form
    {
        string account;
        string password;


        public login()
        {
            InitializeComponent();
            label5.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            account = tb_account.Text.Trim().ToString();
            password = tb_password.Text.Trim().ToString();
            //Post post = new Post();
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("account={0}&password={1}", account, password);
            //string result = post.PostLogin("http://localhost/emotive/index.php/Index/login", sb.ToString());
            //MessageBox.Show(result);

            string strURL = "http://localhost/emotive/index.php/Index/login.html?" + sb;
            System.Net.HttpWebRequest request;
            // 创建一个HTTP请求
            request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
            //request.Method="get";
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string responseText = myreader.ReadToEnd();
            myreader.Close();
            string[] str = json(responseText);
            if (str[2] == "SUCCESS")
            {
                label5.Visible = true;
                this.label5.Text = "登录成功.....";
          
                main mainForm = new main(str[0],str[1]);
                mainForm.Show();
                this.Hide();
            }
            else
            {
                label5.Visible = true;
                this.label5.Text = "账号或密码错误";
            }
            
        }

        private string[] json(string str)
        {
            int j = 0;
            string[] data = new string[5];
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == ':')
                {
                    i=i+2;
                    for (; str[i]!='"'; i++)
                    {
                        data[j] += str[i];
                    }
                    j++;
                }
            }
            return data;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
  
}
