using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Pull_PS3_Games_Universal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.uname.Text.CompareTo("") == 0 && this.pass.Text.CompareTo("") == 0 && this.filePath.Text.CompareTo("") == 0)
            {
                this.helplabel.Text = "Please fill in all fields";
            }
            else
            {
                int ret = pullGames(this.uname.Text, this.pass.Text, this.filePath.Text);
                if (ret == 0)
                    this.helplabel.Text = "Success! Check the folder for a file called 'PSPurchaseList'";
                if (ret == 1)
                    this.helplabel.Text = "Failure! Please check your username and password, then retry.";
                if (ret == 2)
                    this.helplabel.Text = "Failure!";
                this.helplabel.Visible = true;

            }

        }

        private int pullGames(string uname, string pword, string fpath)
        {
            string exePath = Application.ExecutablePath;
            string[] path = exePath.Split('\\');
            string cdPath = "";
            for (int i = 0; i < path.Length - 1; i++)
                cdPath += path[i] + "\\";
            ChromeDriver cd;
            try
            {
                cd = new ChromeDriver(cdPath);
            }
            catch (Exception)
            {
                Console.WriteLine("Put chromedriver in: " + cdPath);
                return 2;
            }

            cd = login(cd, uname, pword);

            //Now go to transaction summary page
            System.Threading.Thread.Sleep(2000); // Give the page time to load
            IWebElement e;
            e = cd.FindElementById("transactionHistoryLink");
            e.Click();
            //Now change the date to 2010, since psplus has only been around since then
            System.Threading.Thread.Sleep(2000); // Give the page time to load

            e = cd.FindElementById("largeFilterStartInput");
            e.Clear();
            e.SendKeys("01/01/2010");
            //Now update table
            e = cd.FindElementById("largeFilterUpdateDateFilterButton");
            e.Click();
            System.Threading.Thread.Sleep(2000);


            List<string> AllYourGames = new List<string>();
            try
            {
                AllYourGames = pullGamesHelper(cd);
                File.WriteAllLines(fpath + "\\PSPurchaseList.txt", AllYourGames);
                return 0;
            }
            catch (Exception)
            {
                return 2;
            }
        }


        private ChromeDriver login(ChromeDriver cd, string uname, string pword)
        {
            cd.Url = "https://account.sonyentertainmentnetwork.com/login.action";
            cd.Navigate();
            IWebElement e = cd.FindElementById("signInInput_SignInID");
            e.SendKeys(uname);
            e = cd.FindElementById("signInInput_Password");
            e.SendKeys(pword);
            e = cd.FindElementById("signInButton");
            e.Click();
            return cd;
        }

        private List<string> pullGamesHelper(ChromeDriver cd)
        {
            List<string> AllYourGames = new List<string>();
            IWebElement e;
            string purchaseTitle = "";
            bool anotherRow = true;
            int index = 0;
            while (anotherRow)
            {
                index++;
                try
                {
                    e = cd.FindElementById("transactionDetailsRow-" + index);
                    e.Click();
                    System.Threading.Thread.Sleep(1000);

                    try
                    {
                        ReadOnlyCollection<IWebElement> roc = cd.FindElements(By.Id("itemTitle-0"));
                        foreach (IWebElement iwe in roc)
                        {
                            purchaseTitle = iwe.Text;
                            AllYourGames.Add(purchaseTitle);
                        }
                        cd.Navigate().Back();
                        System.Threading.Thread.Sleep(1000);
                    }
                    catch (Exception)
                    {
                        purchaseTitle = "This wasn't a game/movie";
                        cd.Navigate().Back();
                        System.Threading.Thread.Sleep(1000);
                    }
                    //AllYourGames.Add(purchaseTitle);
                }
                catch (Exception)
                {
                    Console.WriteLine("The program has finished gathering the data...");
                    anotherRow = false;
                }

            }
            cd.Close();
            cd.Dispose();
            return AllYourGames;
        }


        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void folderPath_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.filePath.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }
}
