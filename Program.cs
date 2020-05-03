using System;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace instabot
{
    class Program
    {
        //Takes seconds and converts to milliseconds and adds randomness to seem more organic
        public static int RandomSec(int n)
        {
            Random random = new Random();
            return random.Next((n-1)*1000,((n+1)*1000)+1);
        }
        //Has an n percent chance to return true
        public static bool Odds(int n)
        {
            if(n != 0){
                return new Random().Next(100) <= n;
            }
            else{
                return false;
            }
        }        
        //Takes the webdriver, email and password of account to login to instagram
        static void Login(IWebDriver driver, string email, string password, int delay)
        {
            Console.WriteLine("Loading instagram...");
            driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/");
            
            System.Threading.Thread.Sleep(Program.RandomSec(delay*3));
            Console.WriteLine("Done!\nEntering credentials...");

            driver.FindElement(By.Name("username")).SendKeys(email);
            driver.FindElement(By.Name("password")).SendKeys(password + Keys.Return);

            //driver.FindElement(By.XPath("//div[text()='Log In']")).Click();
            //driver.FindElement(By.XPath("//div[@label='Log In']")).Click();
            //div[1]/div/form/div[4]/button

            System.Threading.Thread.Sleep(Program.RandomSec(delay*5));
            Console.WriteLine(driver.Url);
            try { driver.FindElement(By.XPath("//button[text()='Not Now']")).Click();}
            catch{}
            System.Threading.Thread.Sleep(Program.RandomSec(delay*1));
            Console.WriteLine("Done!");
        }

        static void Sync()
        {
            if(DateTime.Now.Second!=0){System.Threading.Thread.Sleep((60-DateTime.Now.Second)*1000);} // Line up seconds to zero
            Console.WriteLine(DateTime.Now.Second);
            while(true)
            {
                if(DateTime.Now.Minute==0||DateTime.Now.Minute==15||DateTime.Now.Minute==30||DateTime.Now.Minute==45){break;}
                Console.WriteLine(DateTime.Now.Minute);
                System.Threading.Thread.Sleep(60000);
            }
        }

        //Takes name and content of config file, creates web driver logs in to account 
        //and then begins the main loop for given account.
        static void Start(string[] config, string name)
        {
            string configName = name.Remove(0,6); // Name of user
            string configEmail = config[0].Remove(0,6); // Email of user
            string configPassword = config[1].Remove(0,9); // Password
            string[] configTags = config[2].Remove(0,5).Split(','); // List if tags used to explore
            string[] configComments = config[3].Remove(0,9).Split(','); // List of possible comments left
            int configSkipRate = 100-int.Parse(config[4].Remove(0,9)); // Percentage that a post will be skipped
            int configFollowRate = int.Parse(config[5].Remove(0,11)); // Percentage that a posts creator will be followed
            int configCommentRate = int.Parse(config[6].Remove(0,12)); // Percentage that a post will be commented on
            bool configUnfollow = bool.Parse(config[7].Remove(0,9)); // If the account should automatically unfollow users
            int configMaxLike = 40; // Max amount of posts that can be liked in 15 minutes without getting banned
            int configMaxFollow = 15; // Max amount of people that can be followed in 15 minutes without getting banned
            int configUnfollowAmount = 15; // How many people should be unfollowed every 15 minutes (Max 15)
            int configUnfollowStart = 30; // The next nth(configUnfollowAmount) people after this number will be unfollowed
            int configDelay = 3; // Delay multiplier (More=Slower)
            int configTimeout = 10; // Timeout period
            int configUnfollowMinimum = 200; // Minimum amount of people being followed before the clear function is used
            int configTagsPerCycle = 4; // How many tags should be explored in a single cycle
            bool configNocturnal = false;

            Console.WriteLine("Loading driver...");

            //FirefoxOptions firefoxOptions = new FirefoxOptions();
            //firefoxOptions.AddArguments("-headless");
            ChromeOptions chromeOptions = new ChromeOptions();
            //TODO
            chromeOptions.AddArguments("headless");
            chromeOptions.AddArguments("window-size=1920,1080");

            IWebDriver configDriver = new ChromeDriver(chromeOptions);
            //IWebDriver configDriver = new FirefoxDriver(firefoxOptions);

            configDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(configTimeout);

            Console.WriteLine("Done!");

            Login(configDriver, configEmail, configPassword, configDelay); //Login occurs here before loop

            Sync();

            Console.WriteLine("Beginning main loop...");

            while(true)
            {
                if(configNocturnal == false)
                {
                    if(DateTime.Now.Hour < 2 || (DateTime.Now.Hour == 2 && DateTime.Now.Minute <= 30))
                    {
                        decimal i = Convert.ToDecimal(DateTime.Now.Hour+(DateTime.Now.Minute / 60));
                        System.Threading.Thread.Sleep(Convert.ToInt32((2.5M - i) * 3600000));
                    }
                    else if(DateTime.Now.Hour < 7 && DateTime.Now.Hour > 3 || (DateTime.Now.Hour == 3 && DateTime.Now.Minute >= 30))
                    {
                        decimal i = Convert.ToDecimal(DateTime.Now.Hour+(DateTime.Now.Minute / 60));
                        System.Threading.Thread.Sleep(Convert.ToInt32((7M - i) * 3600000));
                    }
                    else if(DateTime.Now.Hour == 12 && DateTime.Now.Minute >= 30)
                    {
                        decimal i = Convert.ToDecimal(DateTime.Now.Hour+(DateTime.Now.Minute / 60));
                        System.Threading.Thread.Sleep(Convert.ToInt32((13M - i) * 3600000));
                    }
                    else if(DateTime.Now.Hour == 18 && DateTime.Now.Minute >= 30)
                    {
                        decimal i = Convert.ToDecimal(DateTime.Now.Hour+(DateTime.Now.Minute / 60));
                        System.Threading.Thread.Sleep(Convert.ToInt32((19M - i) * 3600000));
                    }
                    else if(DateTime.Now.Hour >= 23)
                    {
                        decimal i = Convert.ToDecimal(DateTime.Now.Hour+(DateTime.Now.Minute / 60));
                        System.Threading.Thread.Sleep(Convert.ToInt32((24M - i + 2.5M) * 3600000));
                    }
                }

                long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond; // Gets the time in ticks and converts to ms for measurement
                Console.WriteLine("Beginning cycle...");
                Cycle.InteractCycles(configDriver, configTags, configName, configComments, configSkipRate, configFollowRate,configCommentRate, configDelay, configUnfollow, configMaxLike, configMaxFollow, configUnfollowAmount, configUnfollowStart, configUnfollowMinimum, configTagsPerCycle); // Completes one cycle
                Console.WriteLine("Done cycle in "+(((DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond)-startTime)/1000)+" seconds!");
            }
        }
        //Reads contents of users folder and creates a thread for each user
        static void Main(string[] args)
        {    
            string[] names = Directory.GetFiles(@"users");
            foreach(string name in names)
            {
                string[] lines = System.IO.File.ReadAllLines(name);
                Thread thread = new Thread(() => Start(lines, name)); 
                thread.Start();
            }           
        }
    }
}